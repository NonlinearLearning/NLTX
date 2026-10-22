using System.Threading;
using MinimalImplementation.Combat.Identity;
using MinimalImplementation.Contracts;

namespace MinimalImplementation.Combat.Npcs;

/// <summary>
/// Coordinates the supported core NPC AI and Activity commits on the NPC owner thread.
/// This is not a world tick scheduler and does not own NPC state.
/// </summary>
public sealed class NpcTickBatchOwner {
  private readonly NpcManager _npcs;
  private int _isExecuting;

  public NpcTickBatchOwner(NpcManager npcs) {
    ArgumentNullException.ThrowIfNull(npcs);
    _npcs = npcs;
  }

  public NpcTickBatchResult Execute(NpcTickBatchRequest request) {
    ArgumentNullException.ThrowIfNull(request);
    if (Interlocked.CompareExchange(ref _isExecuting, 1, 0) != 0) {
      OwnerCommitResult conflict = OwnerCommitResult.Rejected(
          ProtocolErrorCode.Conflict, "NPC tick batch execution cannot reenter itself.");
      return CreateRejected(request.Frame, conflict, Array.Empty<NpcView>());
    }

    try {
      return ExecuteCore(request);
    } finally {
      Volatile.Write(ref _isExecuting, 0);
    }
  }

  private NpcTickBatchResult ExecuteCore(NpcTickBatchRequest request) {
    NpcTickFrameContext frame = request.Frame;
    if (request.MotionSnapshot.WorldEpoch != frame.WorldEpoch ||
        request.MotionSnapshot.WorldCommitVersion != frame.WorldCommitVersion) {
      OwnerCommitResult mismatch = OwnerCommitResult.Rejected(
          ProtocolErrorCode.Conflict,
          "The NPC motion adapter does not belong to the frame's captured world revision.");
      return CreateRejected(frame, mismatch, Array.Empty<NpcView>());
    }
    if (!_npcs.IsReadyForCurrentWorld) {
      OwnerCommitResult notReady = OwnerCommitResult.Rejected(
          ProtocolErrorCode.NotReady,
          "NPC manager must clear its invalidated world scope before ticking.");
      return CreateRejected(frame, notReady, Array.Empty<NpcView>());
    }

    IReadOnlyList<NpcView> captured = _npcs.Capture();
    if (!_npcs.IsReadyForCurrentWorld) {
      OwnerCommitResult notReady = OwnerCommitResult.Rejected(
          ProtocolErrorCode.NotReady,
          "NPC manager world scope changed during capture; retry after world cleanup.");
      return CreateRejected(frame, notReady, captured);
    }
    if (!IsWorldCurrent(request)) {
      OwnerCommitResult stale = StaleWorldResult();
      return CreateRejected(frame, stale, captured);
    }
    Dictionary<CombatEntityIdentity, NpcSlimeTickInput> slimes =
        request.Slimes.ToDictionary(input => input.Entity);
    Dictionary<CombatEntityIdentity, NpcEyeTickInput> eyes =
        request.Eyes.ToDictionary(input => input.Entity);
    Dictionary<CombatEntityIdentity, NpcZombieTickInput> zombies =
        request.Zombies.ToDictionary(input => input.Entity);
    var guides = request.Guides.ToDictionary(input => input.Entity);
    bool invalidGuides = captured.Any(npc => npc.Type == 22 && npc.Life > 0 &&
        (!guides.TryGetValue(npc.Entity, out var input) ||
        input.SourceNpcRevision != npc.Revision)) ||
        guides.Keys.Any(entity => !captured.Any(npc => npc.Entity == entity && npc.Type == 22));
    if (invalidGuides ||
        !InputsMatchCapture(captured, slimes, eyes, zombies, out string? inputError)) {
      OwnerCommitResult invalid = OwnerCommitResult.Rejected(
          ProtocolErrorCode.Conflict,
          "NPC tick inputs do not match the captured entities and revisions.");
      return CreateRejected(frame, invalid, captured);
    }

    var entries = new List<NpcTickEntryResult>(captured.Count);
    var commits = new List<NpcTickCommitRecord>(captured.Count * 2);
    var latestByEntity = new Dictionary<CombatEntityIdentity, NpcView>();
    OwnerCommitResult? stopReason = null;
    int currentIndex = -1;
    OwnerCommitResult? currentAiResult = null;
    OwnerCommitResult? currentActivityResult = null;

    try {
      for (int index = 0; index < captured.Count; index++) {
        currentIndex = index;
        currentAiResult = null;
        currentActivityResult = null;
        NpcView npc = captured[index];
        if (npc.Life <= 0) {
          entries.Add(new NpcTickEntryResult(
              npc, NpcTickEntryResult.Disposition.SkippedDead, null, null, null,
              "Dead NPCs are handled by the death owner, not this AI batch."));
          continue;
        }
        if (npc.Type is not (1 or 2 or 3 or 22)) {
          stopReason = OwnerCommitResult.Rejected(
              ProtocolErrorCode.NotReady, "This batch owns only NPC types 1, 2, 3, and 22.");
          entries.Add(new NpcTickEntryResult(
              npc, NpcTickEntryResult.Disposition.UnsupportedType, stopReason, null, null,
              stopReason.Detail));
          AppendNotProcessed(captured, index + 1, entries, stopReason);
          break;
        }

        if (!IsWorldCurrent(request)) {
          stopReason = StaleWorldResult();
          entries.Add(new NpcTickEntryResult(
              npc, NpcTickEntryResult.Disposition.Rejected, stopReason, null, null,
              stopReason.Detail));
          AppendNotProcessed(captured, index + 1, entries, stopReason);
          break;
        }

        NpcPreparation<NpcAiPlan> preparation = npc.Type == 22
            ? _npcs.PrepareGuideTick(guides[npc.Entity],
                request.MotionSnapshot, request.AiRandom)
            : PrepareAi(request, npc, slimes, eyes, zombies);
        if (!preparation.IsPrepared || preparation.Plan is null) {
          stopReason = preparation.Result;
          entries.Add(new NpcTickEntryResult(
              npc, NpcTickEntryResult.Disposition.Rejected, preparation.Result, null, null,
              preparation.Result.Detail));
          AppendNotProcessed(captured, index + 1, entries, stopReason);
          break;
        }

        OwnerCommitResult validation = _npcs.Validate(preparation.Plan);
        if (validation.Status != OwnerCommitStatus.Applied) {
          stopReason = validation;
          entries.Add(new NpcTickEntryResult(
              npc, NpcTickEntryResult.Disposition.Rejected, validation, null, null,
              validation.Detail));
          AppendNotProcessed(captured, index + 1, entries, stopReason);
          break;
        }
        if (!IsWorldCurrent(request)) {
          stopReason = StaleWorldResult();
          entries.Add(new NpcTickEntryResult(
              npc, NpcTickEntryResult.Disposition.Rejected, stopReason, null, null,
              stopReason.Detail));
          AppendNotProcessed(captured, index + 1, entries, stopReason);
          break;
        }

        (OwnerCommitResult aiCommit, NpcView? aiNpc) = _npcs.Commit(preparation.Plan);
        currentAiResult = aiCommit;
        if (aiCommit.Status != OwnerCommitStatus.Applied) {
          stopReason = aiCommit;
          entries.Add(new NpcTickEntryResult(
              npc, NpcTickEntryResult.Disposition.Rejected, aiCommit, null, null,
              aiCommit.Detail));
          AppendNotProcessed(captured, index + 1, entries, stopReason);
          break;
        }
        if (aiNpc is null) {
          throw new InvalidOperationException("Successful NPC AI commit returned no snapshot.");
        }
        latestByEntity[npc.Entity] = aiNpc;
        commits.Add(new NpcTickCommitRecord(aiNpc, NpcTickCommitRecord.CommitStage.Ai));

        // Town NPCs are exempt from NPC.CheckActive's ordinary monster despawn branch.
        if (npc.IsTownNpc) {
          entries.Add(new NpcTickEntryResult(npc, NpcTickEntryResult.Disposition.Committed,
              aiCommit, null, aiNpc, null));
          continue;
        }

        NpcPreparation<NpcActivityPlan> activity = _npcs.PrepareActivity(
            npc.Entity, aiNpc.Revision, frame.Players);
        if (!activity.IsPrepared || activity.Plan is null) {
          stopReason = activity.Result;
          entries.Add(new NpcTickEntryResult(
              npc, NpcTickEntryResult.Disposition.Rejected, aiCommit, stopReason, aiNpc,
              stopReason.Detail));
          AppendNotProcessed(captured, index + 1, entries, stopReason);
          break;
        }

        OwnerCommitResult activityValidation = _npcs.Validate(activity.Plan);
        if (activityValidation.Status != OwnerCommitStatus.Applied) {
          stopReason = activityValidation;
          entries.Add(new NpcTickEntryResult(
              npc, NpcTickEntryResult.Disposition.Rejected, aiCommit, stopReason, aiNpc,
              stopReason.Detail));
          AppendNotProcessed(captured, index + 1, entries, stopReason);
          break;
        }
        NpcActivityCommitResult activityCommit = _npcs.Commit(activity.Plan);
        currentActivityResult = activityCommit.Result;
        if (activityCommit.Result.Status != OwnerCommitStatus.Applied) {
          stopReason = activityCommit.Result;
          entries.Add(new NpcTickEntryResult(
              npc, NpcTickEntryResult.Disposition.Rejected, aiCommit, stopReason, aiNpc,
              stopReason.Detail));
          AppendNotProcessed(captured, index + 1, entries, stopReason);
          break;
        }
        if (activityCommit.Npc is null) {
          throw new InvalidOperationException("Activity commit returned no snapshot.");
        }

        NpcView activityNpc = activityCommit.Npc;
        latestByEntity[npc.Entity] = activityNpc;
        commits.Add(new NpcTickCommitRecord(
            activityNpc, NpcTickCommitRecord.CommitStage.Activity,
            activityCommit.NearbyPlayerSlots));
        entries.Add(new NpcTickEntryResult(
            npc, NpcTickEntryResult.Disposition.Committed, aiCommit, activityCommit.Result,
            activityNpc, null));
      }
    } catch (Exception exception) {
      if (exception is OutOfMemoryException) {
        throw;
      }
      if (currentIndex < 0) {
        throw;
      }

      NpcView interruptedNpc = captured[currentIndex];
      latestByEntity.TryGetValue(interruptedNpc.Entity, out NpcView? latestCommittedNpc);
      entries.Add(new NpcTickEntryResult(
          interruptedNpc, NpcTickEntryResult.Disposition.Interrupted,
          currentAiResult, currentActivityResult, latestCommittedNpc,
          "NPC tick execution threw; inspect the attached exception."));
      OwnerCommitResult interrupted = OwnerCommitResult.Rejected(
          ProtocolErrorCode.NotReady, "NPC tick execution was interrupted by an exception.");
      AppendNotProcessed(captured, currentIndex + 1, entries, interrupted);
      NpcTickBatchResult partialResult = new(
          frame, interrupted, entries, commits, CreatePacket23Snapshots(latestByEntity),
          captured.Count, false);
      if (exception is OperationCanceledException cancellationException) {
        throw new NpcTickBatchCanceledException(
            interruptedNpc.Entity, partialResult, cancellationException);
      }
      throw new NpcTickBatchExecutionException(interruptedNpc.Entity, partialResult, exception);
    }

    OwnerCommitResult result = stopReason ?? OwnerCommitResult.Applied(_npcs.CommitVersion);
    return new NpcTickBatchResult(
        frame, result, entries, commits, CreatePacket23Snapshots(latestByEntity), captured.Count,
        stopReason is null && entries.All(entry =>
            entry.Status is NpcTickEntryResult.Disposition.Committed or
                NpcTickEntryResult.Disposition.SkippedDead));
  }

  private NpcPreparation<NpcAiPlan> PrepareAi(
      NpcTickBatchRequest request,
      NpcView npc,
      IReadOnlyDictionary<CombatEntityIdentity, NpcSlimeTickInput> slimes,
      IReadOnlyDictionary<CombatEntityIdentity, NpcEyeTickInput> eyes,
      IReadOnlyDictionary<CombatEntityIdentity, NpcZombieTickInput> zombies) {
    switch (npc.Type) {
      case 1: {
        NpcSlimeTickInput input = slimes[npc.Entity];
        var tick = new NpcSlimeTickRequest(
            npc.Entity, npc.Revision, input.Value, input.Environment,
            input.Confused, request.Frame.Players);
        return _npcs.PrepareSlimeTick(tick, request.MotionSnapshot, request.AiRandom);
      }
      case 2: {
        NpcEyeTickInput input = eyes[npc.Entity];
        var tick = new NpcEyeTickRequest(
            npc.Entity, npc.Revision, input.IsDay, input.WorldSurfaceTiles,
            input.Confused, request.Frame.Players);
        return _npcs.PrepareEyeTick(tick, request.MotionSnapshot);
      }
      case 3: {
        NpcZombieTickInput input = zombies[npc.Entity];
        var tick = new NpcZombieTickRequest(
            npc.Entity, npc.Revision, input.Environment, input.Confused,
            request.Frame.Players);
        return _npcs.PrepareZombieTick(tick, request.MotionSnapshot);
      }
      default:
        throw new InvalidOperationException("Only core NPC types are dispatched by this owner.");
    }
  }

  private static bool InputsMatchCapture(
      IReadOnlyList<NpcView> captured,
      IReadOnlyDictionary<CombatEntityIdentity, NpcSlimeTickInput> slimes,
      IReadOnlyDictionary<CombatEntityIdentity, NpcEyeTickInput> eyes,
      IReadOnlyDictionary<CombatEntityIdentity, NpcZombieTickInput> zombies,
      out string? detail) {
    var expected = new Dictionary<CombatEntityIdentity, short>();
    var livingCoreNpcs = new HashSet<CombatEntityIdentity>();
    foreach (NpcView npc in captured) {
      if (npc.Type is 1 or 2 or 3) {
        expected.Add(npc.Entity, npc.Type);
        if (npc.Life > 0) {
          livingCoreNpcs.Add(npc.Entity);
        }
      }
    }

    foreach ((CombatEntityIdentity entity, short type) in expected) {
      if (!livingCoreNpcs.Contains(entity)) {
        continue;
      }
      bool present = type switch {
        1 => slimes.ContainsKey(entity),
        2 => eyes.ContainsKey(entity),
        3 => zombies.ContainsKey(entity),
        _ => false
      };
      if (!present) {
        detail = $"The frame is missing typed AI facts for NPC generation {entity.Generation}.";
        return false;
      }
    }

    if (slimes.Keys.Any(entity => !expected.TryGetValue(entity, out short type) || type != 1) ||
        eyes.Keys.Any(entity => !expected.TryGetValue(entity, out short type) || type != 2) ||
        zombies.Keys.Any(entity => !expected.TryGetValue(entity, out short type) || type != 3)) {
      detail = "Typed facts contain an entity absent from capture or bound to the wrong type.";
      return false;
    }

    foreach (NpcView npc in captured) {
      bool staleSlime = npc.Type == 1 &&
          slimes.TryGetValue(npc.Entity, out NpcSlimeTickInput? slime) &&
          (slime is null || slime.SourceNpcRevision != npc.Revision);
      bool staleEye = npc.Type == 2 &&
          eyes.TryGetValue(npc.Entity, out NpcEyeTickInput? eye) &&
          (eye is null || eye.SourceNpcRevision != npc.Revision);
      bool staleZombie = npc.Type == 3 &&
          zombies.TryGetValue(npc.Entity, out NpcZombieTickInput? zombie) &&
          (zombie is null || zombie.SourceNpcRevision != npc.Revision);
      if (staleSlime || staleEye || staleZombie) {
        detail = "NPC tick facts were captured from a different NPC revision.";
        return false;
      }
    }

    detail = null;
    return true;
  }

  private bool IsWorldCurrent(NpcTickBatchRequest request) {
    return request.MotionSnapshot.WorldEpoch == request.Frame.WorldEpoch &&
        request.MotionSnapshot.WorldCommitVersion == request.Frame.WorldCommitVersion &&
        request.WorldRevisionGuard.IsCurrentWorldRevision(
            request.Frame.WorldEpoch, request.Frame.WorldCommitVersion);
  }

  private static OwnerCommitResult StaleWorldResult() {
    return new OwnerCommitResult(
        OwnerCommitStatus.Stale, null, ProtocolErrorCode.Conflict,
        "The captured world revision is stale; discard this frame and prepare a new batch.");
  }

  private static void AppendNotProcessed(
      IReadOnlyList<NpcView> captured,
      int startIndex,
      List<NpcTickEntryResult> entries,
      OwnerCommitResult reason) {
    for (int index = startIndex; index < captured.Count; index++) {
      entries.Add(new NpcTickEntryResult(
          captured[index], NpcTickEntryResult.Disposition.NotProcessed,
          null, null, null, reason.Detail));
    }
  }

  private static NpcTickBatchResult CreateRejected(
      NpcTickFrameContext frame,
      OwnerCommitResult result,
      IReadOnlyList<NpcView> captured) {
    NpcTickEntryResult[] entries = captured.Select(npc => new NpcTickEntryResult(
        npc, NpcTickEntryResult.Disposition.NotProcessed,
        null, null, null, result.Detail)).ToArray();
    return new NpcTickBatchResult(
        frame, result, entries, Array.Empty<NpcTickCommitRecord>(),
        Array.Empty<NpcView>(), captured.Count, false);
  }

  private static NpcView[] CreatePacket23Snapshots(
      IReadOnlyDictionary<CombatEntityIdentity, NpcView> latestByEntity) {
    return latestByEntity.Values
        .Where(view => view.IsDirty)
        .OrderBy(view => view.CommitVersion.Value)
        .ToArray();
  }
}
