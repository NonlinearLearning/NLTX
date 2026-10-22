using MinimalImplementation.Combat.Projectiles;
using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Protocol;
using MinimalImplementation.Server.Session;
using MinimalImplementation.Server.World;
using Terraria.NetWork.Prototype.PacketDesignCompiler;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;
using PlayerEntity = MinimalImplementation.Server.Player.Player;

namespace MinimalImplementation.Server.Host;

public sealed partial class Version4InboundRouter {
  private sealed record PendingJoinSynchronization(
    WorldSectionSendBatch Sections,
    IReadOnlyList<OutboundPacketIntent> BeforeSections,
    IReadOnlyList<OutboundPacketIntent> AfterSections,
    OutboundPacketIntent InitialSpawn);

  private string? HandleInitialJoin(
      InboundPacketEnvelope envelope,
      SpawnTileDataPacket request,
      Guid transferId,
      out PendingJoinSynchronization? pendingJoin) {
    pendingJoin = null;
    if (_worldSectionRequests is null || _captureWorldDataSupplement is null ||
        !_composition.WorldOwner.IsReady) {
      return "Packet 8 is NotReady: authoritative world and section capture are required.";
    }

    WorldSectionSendBatch? sections = null;
    IReadOnlyList<OutboundPacketIntent> joinSnapshots = Array.Empty<OutboundPacketIntent>();
    PendingJoinSynchronization? preparedJoin = null;
    try {
      OwnerCommitResult PrepareJoinOutput(IReadOnlyList<SessionPacketProjection> projections) {
        if (sections is null) {
          return OwnerCommitResult.Rejected(ProtocolErrorCode.NotReady,
            "Packet 8 section capture did not produce a transfer.");
        }

        OwnerCommitResult encodedResult = CaptureSessionOutputIntents(
          SessionAdmissionCoordinator.SpawnTileDataMessageId, projections,
          out IReadOnlyList<OutboundPacketIntent> encoded);
        if (encodedResult.Status != OwnerCommitStatus.Applied) {
          return encodedResult;
        }

        OutboundPacketIntent worldData = encoded.Single(intent => intent.MessageId == 7);
        OutboundPacketIntent progress = encoded.Single(intent => intent.MessageId == 9);
        OutboundPacketIntent initialSpawn = encoded.Single(intent => intent.MessageId == 49);
        OutboundPacketIntent[] afterSections = encoded
          .Where(intent => intent.MessageId is not (7 or 9 or 49))
          .Concat(joinSnapshots).ToArray();
        preparedJoin = new PendingJoinSynchronization(
          sections, [worldData, progress], afterSections, initialSpawn);
        return OwnerCommitResult.Applied(CommitVersion.Initial);
      }

      JoinStepResult joined = _composition.SessionPacketHandlers.HandleSpawnTileData(
        envelope,
        request,
        (binding, packet) => CaptureJoinBatch(
          binding, packet, transferId, envelope.ReceivedAt, out sections, out joinSnapshots),
        PrepareJoinOutput);
      if (!joined.IsApplied || preparedJoin is null) {
        return joined.Result.Detail ?? "Packet 8 join synchronization was rejected.";
      }

      pendingJoin = preparedJoin;
      return null;
    } catch (Exception exception) when (exception is ArgumentException or
        InvalidDataException or InvalidOperationException or NotSupportedException) {
      return $"Packet 8 synchronization capture was rejected: {exception.Message}";
    } finally {
      if (pendingJoin is null && sections is not null) {
        _worldSectionRequests.RetireTransfer(sections.Token);
      }
    }
  }

  private JoinSynchronizationBatch CaptureJoinBatch(
      SessionBinding binding,
      SpawnTileDataPacket request,
      Guid transferId,
      DateTimeOffset capturedAt,
      out WorldSectionSendBatch sections,
      out IReadOnlyList<OutboundPacketIntent> joinSnapshots) {
    WorldSnapshot world = _composition.WorldOwner.CaptureSnapshot();
    if (!IsValidSpawnRequest(world.Header.Geometry, request)) {
      throw new ArgumentException("The requested spawn coordinates or team are invalid.");
    }

    WorldTeamSpawnFacts teamSpawns = world.Header.TeamSpawns;
    if (!teamSpawns.IsReady) {
      throw new InvalidOperationException("The published world has no team-spawn facts.");
    }

    var extraSpawns = new List<TileCoordinate>();
    if (teamSpawns.TeamBasedSpawns && request.Team != 0 &&
        request.Team < teamSpawns.ExtraSpawnPoints.Count) {
      extraSpawns.Add(teamSpawns.ExtraSpawnPoints[request.Team]);
    }

    TileCoordinate? selectedSpawn = request.X == -1 && request.Y == -1
      ? null
      : new TileCoordinate(request.X, request.Y);
    IReadOnlyList<ProjectileView> projectiles = CaptureJoinProjectiles();
    IReadOnlyList<WorldSectionCoordinate> portalSections = CapturePortalSections(world, projectiles);
    WorldDataPacketSupplementSnapshot supplement = _captureWorldDataSupplement!.Invoke(world);
    var packets = new List<JoinBatchPacket> {
      new(7, _worldDataProducer.CreatePacket(world, supplement))
    };

    // Validate provider packets before opening the transfer; complete output precedes admission.
    foreach (JoinBatchPacket packet in packets) {
      _composition.Version4WireProfile.Encode(WireDirection.ServerToClient, packet.Packet);
    }

    var snapshots = new List<OutboundPacketIntent>(
      CapturePlayerJoinIntents(binding.Epoch, projectiles));
    snapshots.AddRange(CaptureJoinSnapshotIntents(binding.Epoch, capturedAt));
    joinSnapshots = snapshots.AsReadOnly();
    sections = _worldSectionRequests!.CaptureInitialBaseline(
      binding.Epoch, transferId, world, selectedSpawn, extraSpawns, portalSections);
    return new JoinSynchronizationBatch(
      checked((long)world.WorldEpoch),
      new StatusTextSizePacket {
        Value = sections.Sections.Count,
        Text = NetworkText.Literal("Synchronizing"),
        Flags = 0
      },
      packets);
  }

  private IReadOnlyList<OutboundPacketIntent> CaptureJoinSnapshotIntents(
      SessionEpoch target, DateTimeOffset capturedAt) {
    long worldScope = _composition.IdentityRegistry.CurrentWorldScope;
    if (worldScope <= 0 || _composition.MechanismWorldScope != worldScope ||
        _composition.TeleportPylonRegistry.WorldScope != worldScope) {
      throw new InvalidOperationException("Pylon owners have not joined the current world scope.");
    }

    // These producers encode read-only owner snapshots in the authenticated commit callback.
    // Join delivery does not acknowledge the separate periodic dirty-snapshot ledger.
    var intents = new List<OutboundPacketIntent>(CaptureWorldItemJoinIntents(target, capturedAt));
    foreach (var npc in _composition.NpcManager.Capture()) {
      intents.Add(_composition.NpcNetwork.CreateSync(npc, [target], initial: true));
    }
    intents.Add(_composition.BannerState.CreateFullStateIntent(target));
    intents.Add(new WorldAlignmentPacketProducer(
      _composition.WorldOwner, _composition.Version4WireProfile).CreateIntent([target]));
    intents.AddRange(_composition.BestiaryState.CreateSnapshotIntents(
      target, BestiaryNpcCatalog.NetIds));
    intents.AddRange(CaptureCreativeJoinIntents(target));
    intents.AddRange(_composition.TeleportPylonProducer.CreateJoinIntents(
      _composition.TeleportPylonRegistry.Capture(),
      worldScope,
      target,
      new CommitVersion(_composition.TeleportPylonRegistry.Revision)));
    return intents.AsReadOnly();
  }

  private IReadOnlyList<ProjectileView> CaptureJoinProjectiles() {
    // The combat owners are created together in this scope. Capture() returns an empty list
    // for stale owners, so establish readiness before interpreting an empty projectile snapshot.
    long worldScope = _composition.IdentityRegistry.CurrentWorldScope;
    if (worldScope <= 0 || _composition.CombatWorldScope != worldScope) {
      throw new InvalidOperationException("Combat owners have not joined the current world scope.");
    }

    IReadOnlyList<ProjectileView> projectiles = _composition.ProjectileManager.Capture();
    foreach (ProjectileView projectile in projectiles) {
      if (projectile.Entity.WorldScope != worldScope ||
          !_composition.CombatIdentityAuthority.IsActive(projectile.Entity)) {
        throw new InvalidOperationException("A join projectile has a stale entity identity.");
      }
    }

    return projectiles;
  }

  private static IReadOnlyList<WorldSectionCoordinate> CapturePortalSections(
      WorldSnapshot world, IReadOnlyList<ProjectileView> projectiles) {
    var sections = new HashSet<WorldSectionCoordinate>();
    WorldGeometry geometry = world.Header.Geometry;
    foreach (ProjectileView projectile in projectiles) {
      if (projectile.Type is not (601 or 602)) {
        continue;
      }

      int tileX = (int)((projectile.Position.X + projectile.Width / 2f) / 16f);
      int tileY = (int)((projectile.Position.Y + projectile.Height / 2f) / 16f);
      int sectionX = tileX / WorldGeometry.SectionWidth;
      int sectionY = tileY / WorldGeometry.SectionHeight;
      for (int y = Math.Max(0, sectionY - 1);
          y <= Math.Min(geometry.SectionCountY - 1, sectionY + 1); y++) {
        for (int x = Math.Max(0, sectionX - 1);
            x <= Math.Min(geometry.SectionCountX - 1, sectionX + 1); x++) {
          sections.Add(new WorldSectionCoordinate(x, y));
        }
      }
    }

    return sections.OrderBy(section => section.Y).ThenBy(section => section.X).ToArray();
  }

  private string? HandleSectionRequest(
      InboundPacketEnvelope envelope,
      RequestSectionPacket packet,
      out WorldSectionSendBatch? pendingRequest) {
    pendingRequest = null;
    if (_worldSectionRequests is null) {
      return "Packet 159 is NotReady: authoritative section attachment capture is required.";
    }

    WorldSectionSendBatch? captured = null;
    OwnerCommitResult result = _composition.SessionAdmission.ExecuteAuthenticatedPlayerPacket(
      envelope,
      expectedMessageId: 159,
      allowedStages: [SessionStage.Active],
      commit: _ => {
        try {
          captured = _worldSectionRequests.CaptureRequestedBaseline(
            envelope.Epoch, packet.SectionX, packet.SectionY);
          return OwnerCommitResult.Applied(
            _composition.SessionAdmission.AllocateCommitVersion());
        } catch (Exception exception) when (exception is ArgumentException or
            InvalidDataException or InvalidOperationException or NotSupportedException) {
          return OwnerCommitResult.Rejected(ProtocolErrorCode.NotReady, exception.Message);
        }
      });
    if (result.Status == OwnerCommitStatus.Applied) {
      pendingRequest = captured;
      return null;
    }

    return result.Detail ?? "Packet 159 section request was rejected.";
  }

  private bool CanSendSections(WorldSectionSendBatch batch, SessionStage expectedStage) {
    return _worldSectionRequests!.IsCurrentTransfer(batch.Token) &&
      _composition.SessionAdmission.TryGetStage(batch.Token.SessionEpoch, out SessionStage stage) &&
      stage == expectedStage;
  }

  private string? CaptureMovementSections(
      SessionEpoch session, out WorldSectionSendBatch? sections) {
    sections = null;
    if (_worldSectionRequests is null ||
        !_composition.Players.TryGet(session, out PlayerEntity? player) || player is null) {
      return "Movement section capture requires an active player and world sender.";
    }

    try {
      sections = _worldSectionRequests.CaptureMovementBaseline(
        session, player.Movement.Position.X, player.Movement.Position.Y);
      return null;
    } catch (Exception exception) when (exception is ArgumentException or
        InvalidDataException or InvalidOperationException or NotSupportedException) {
      return $"Movement section capture was rejected: {exception.Message}";
    }
  }

  private async ValueTask<bool> SendMovementSectionsAsync(
      WorldSectionSendBatch batch, CancellationToken cancellationToken) {
    bool delivered = false;
    try {
      Version4EncodedPacket progress = _composition.Version4WireProfile.Encode(
        WireDirection.ServerToClient, new StatusTextSizePacket {
          Value = batch.Sections.Count,
          Text = NetworkText.Literal("Receiving tile data"),
          Flags = 0
        });
      var intent = new OutboundPacketIntent([batch.Token.SessionEpoch], null,
        progress.MessageId, progress.Payload, batch.Sections[0].Version);
      delivered = await SendJoinPacketAsync(intent, cancellationToken).ConfigureAwait(false) &&
        await SendSectionBatchAsync(batch, SessionStage.Active, cancellationToken)
          .ConfigureAwait(false);
      if (delivered) {
        ServerLog.Info("server.sections.movement",
          $"epoch={batch.Token.SessionEpoch.Value} count={batch.Sections.Count} " +
          $"sections={string.Join(",", batch.Sections.Select(section => section.Coordinate))}");
      }

      return delivered;
    } finally {
      if (!delivered) {
        await RetireSectionTransferAsync(batch).ConfigureAwait(false);
      }
    }
  }

  private async ValueTask<bool> SendJoinSynchronizationAsync(
      PendingJoinSynchronization join, CancellationToken cancellationToken) {
    bool completed = false;
    try {
      foreach (OutboundPacketIntent intent in join.BeforeSections) {
        if (!await SendJoinPacketAsync(intent, cancellationToken).ConfigureAwait(false)) {
          return false;
        }
      }

      if (!await SendSectionBatchAsync(
          join.Sections, SessionStage.Synchronizing, cancellationToken).ConfigureAwait(false)) {
        return false;
      }

      foreach (OutboundPacketIntent intent in join.AfterSections) {
        if (!await SendJoinPacketAsync(intent, cancellationToken).ConfigureAwait(false)) {
          return false;
        }
      }

      bool canComplete = false;
      await _composition.CommitLoop.SubmitAsync(() => {
        canComplete = CanSendSections(join.Sections, SessionStage.Synchronizing);
      }, cancellationToken).ConfigureAwait(false);
      if (!canComplete ||
          !await SendJoinPacketAsync(join.InitialSpawn, cancellationToken).ConfigureAwait(false)) {
        return false;
      }

      // This marker can only follow the successful packet-49 transport receipt.
      await _composition.CommitLoop.SubmitAsync(() => {
        completed = CanSendSections(join.Sections, SessionStage.Synchronizing) &&
          _worldSectionRequests!.CompleteInitialSync(join.Sections);
      }).ConfigureAwait(false);
      return completed;
    } finally {
      if (!completed) {
        await RetireSectionTransferAsync(join.Sections).ConfigureAwait(false);
      }
    }
  }

  private async ValueTask<bool> SendSectionBatchAsync(
      WorldSectionSendBatch batch,
      SessionStage expectedStage,
      CancellationToken cancellationToken) {
    bool delivered = false;
    try {
      bool canSend = false;
      await _composition.CommitLoop.SubmitAsync(() => {
        canSend = CanSendSections(batch, expectedStage);
      }, cancellationToken).ConfigureAwait(false);
      if (!canSend) {
        return false;
      }

      foreach (WorldSectionSendBatch.Section section in batch.Sections) {
        var receipts = new List<SendReceipt>(section.Fragments.Count);
        foreach (OutboundPacketIntent fragment in section.Fragments) {
          IReadOnlyList<SendReceipt> submitted = await SendWithReceiptsAsync(
            fragment, cancellationToken).ConfigureAwait(false);
          receipts.AddRange(submitted);
          if (!IsSuccessfulSubmission(submitted, batch.Token.SessionEpoch)) {
            break;
          }
        }

        bool settled = false;
        await _composition.CommitLoop.SubmitAsync(() => {
          settled = CanSendSections(batch, expectedStage) &&
            _worldSectionRequests!.RecordDelivery(batch, section, receipts);
        }).ConfigureAwait(false);
        if (!settled) {
          return false;
        }
      }

      delivered = true;
      return true;
    } finally {
      if (!delivered) {
        await RetireSectionTransferAsync(batch).ConfigureAwait(false);
      }
    }
  }

  private async ValueTask RetireSectionTransferAsync(WorldSectionSendBatch batch) {
    await _composition.CommitLoop.SubmitAsync(() => {
      _worldSectionRequests!.RetireTransfer(batch.Token);
    }).ConfigureAwait(false);
  }

  private async ValueTask<bool> SendJoinPacketAsync(
      OutboundPacketIntent intent, CancellationToken cancellationToken) {
    IReadOnlyList<SendReceipt> receipts = await SendWithReceiptsAsync(
      intent, cancellationToken).ConfigureAwait(false);
    return intent.TargetEpochs.Count == 1 &&
      IsSuccessfulSubmission(receipts, intent.TargetEpochs[0]);
  }

  private static bool IsSuccessfulSubmission(
      IReadOnlyList<SendReceipt> receipts, SessionEpoch target) {
    return receipts.Count == 1 && receipts[0].Epoch == target &&
      receipts[0].Certainty is TransportSendCertainty.Queued or TransportSendCertainty.LocallySent;
  }
}
