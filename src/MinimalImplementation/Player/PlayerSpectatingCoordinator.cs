using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Identity;
using MinimalImplementation.Server.Session;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace MinimalImplementation.Server.Player;

/// <summary>Owns 150 selection, target revalidation and ordered CheckSection preparation.</summary>
public sealed class PlayerSpectatingCoordinator {
  private sealed record Candidate(
    SessionBinding Binding,
    bool Active,
    bool Dead,
    int DeadTicks,
    PacketVector2 Position,
    PlayerSpectatingGeometry Geometry);

  public const int LingerAfterDeathTicks = 180;

  private readonly PlayerOwner _players;
  private readonly SessionAdmissionCoordinator _sessions;
  private readonly Func<Player, PlayerSpectatingGeometry> _captureGeometry;
  private readonly Func<SessionBinding, EntityHandle, PacketVector2,
    IPlayerSpectatingSectionPreparation> _prepareSections;

  public PlayerSpectatingCoordinator(
      PlayerOwner players,
      SessionAdmissionCoordinator sessions,
      Func<Player, PlayerSpectatingGeometry> captureGeometry,
      Func<SessionBinding, EntityHandle, PacketVector2,
        IPlayerSpectatingSectionPreparation> prepareSections) {
    ArgumentNullException.ThrowIfNull(players);
    ArgumentNullException.ThrowIfNull(sessions);
    ArgumentNullException.ThrowIfNull(captureGeometry);
    ArgumentNullException.ThrowIfNull(prepareSections);
    _players = players;
    _sessions = sessions;
    _captureGeometry = captureGeometry;
    _prepareSections = prepareSections;
  }

  public PlayerSpectatingCommit Handle(
      InboundPacketEnvelope envelope, SpectatePlayerPacket packet) {
    ArgumentNullException.ThrowIfNull(envelope);
    ArgumentNullException.ThrowIfNull(packet);
    if (packet.TargetPlayer < 0) {
      return HandleExplicitStop(_players, _sessions, envelope, packet);
    }

    short requestedSlot = packet.TargetPlayer;
    PlayerSpectatingCommit? committed = null;
    OwnerCommitResult gate = _sessions.ExecuteAuthenticatedPlayerPacket(
      envelope, SessionAdmissionCoordinator.SpectatePlayerMessageId,
      [SessionStage.Active], binding => {
        if (!_players.TryGetForBinding(binding, out Player? player) || player is null) {
          return OwnerCommitResult.Rejected(ProtocolErrorCode.StaleEntity,
            "The spectating player is no longer active.");
        }

        if (requestedSlot >= byte.MaxValue) {
          return OwnerCommitResult.Rejected(ProtocolErrorCode.Forbidden,
            "The spectating target is outside the Version4 player slots.");
        }

        bool self = requestedSlot == binding.PlayerSlot;
        Candidate[] roster = self ? [] : CaptureRoster();
        Candidate? requested = roster.FirstOrDefault(
          candidate => candidate.Binding.PlayerSlot == requestedSlot);
        EntityHandle? requestedHandle = self ? null : requested?.Binding.PlayerHandle;
        if ((self || requested is not null)
            && player.Spectating.Target == requestedHandle) {
          committed = new PlayerSpectatingCommit(
            OwnerCommitResult.Applied(_players.ReserveCommitVersion()), player.Spectating, []);
          return committed.Result;
        }

        Candidate? selected = null;
        if (!self) {
          // SetOrRequestSpectating assigns the requested slot before CanSpectate. A requested
          // dead player is therefore eligible only during its existing 180-tick linger.
          selected = requested is not null && CanKeep(requested) ? requested : null;
          if (selected is null) {
            PacketVector2 origin = requested?.Geometry.CameraPosition
              ?? player.Spectating.CameraPosition ?? player.Movement.Position;
            selected = FindClosest(roster, binding.PlayerHandle, origin);
          }
        }

        IReadOnlyList<SessionEpoch> recipients = _sessions.CaptureActiveSessionEpochs();
        OwnerCommitResult prepared = Prepare(binding, selected, recipients, true,
          out PreparedPlayerSpectatingChange? change);
        if (prepared.Status != OwnerCommitStatus.Applied || change is null) {
          return prepared;
        }

        CommitVersion version = _players.ReserveCommitVersion();
        var projections = new List<SessionPacketProjection>();
        change.AppendProjections(version, projections);
        committed = new PlayerSpectatingCommit(
          OwnerCommitResult.Applied(version), change.State, projections);
        change.CommitSections();
        _players.CommitPreparedSpectating(player, change.State, version);
        return committed.Result;
      });

    return committed ?? new PlayerSpectatingCommit(gate, PlayerSpectating.None, []);
  }

  /// <summary>
  /// Handles the negative-target receive branch without geometry or section providers.
  /// Output preparation may encode projections, but must not send or mutate owner state.
  /// Nonnegative targets require the full coordinator and are rejected here as NotReady.
  /// </summary>
  public static PlayerSpectatingCommit HandleExplicitStop(
      PlayerOwner players,
      SessionAdmissionCoordinator sessions,
      InboundPacketEnvelope envelope,
      SpectatePlayerPacket packet,
      Func<IReadOnlyList<SessionPacketProjection>, OwnerCommitResult>? prepareOutput = null) {
    ArgumentNullException.ThrowIfNull(players);
    ArgumentNullException.ThrowIfNull(sessions);
    ArgumentNullException.ThrowIfNull(envelope);
    ArgumentNullException.ThrowIfNull(packet);
    short requestedSlot = packet.TargetPlayer;
    PlayerSpectatingCommit? committed = null;
    OwnerCommitResult gate = sessions.ExecuteAuthenticatedPlayerPacket(
      envelope, SessionAdmissionCoordinator.SpectatePlayerMessageId,
      [SessionStage.Active], binding => {
        if (!players.TryGetForBinding(binding, out Player? player) || player is null) {
          return OwnerCommitResult.Rejected(ProtocolErrorCode.StaleEntity,
            "The spectating player is no longer active.");
        }

        if (requestedSlot >= 0) {
          return OwnerCommitResult.Rejected(ProtocolErrorCode.NotReady,
            "Packet 150 target selection requires authoritative geometry and section preparation.");
        }

        CommitVersion version = players.ReserveCommitVersion();
        var change = new PreparedPlayerSpectatingChange(
          binding, PlayerSpectating.None, requestedSlot, null, [],
          sessions.CaptureActiveRecipients(binding.Epoch), true);
        var projections = new List<SessionPacketProjection>();
        change.AppendProjections(version, projections);
        var prepared = new PlayerSpectatingCommit(
          OwnerCommitResult.Applied(version), change.State, projections);
        OwnerCommitResult? outputValidation = prepareOutput?.Invoke(prepared.Projections);
        if (outputValidation is not null && outputValidation.Status != OwnerCommitStatus.Applied) {
          return outputValidation;
        }

        players.CommitPreparedSpectating(player, change.State, version);
        committed = prepared;
        return committed.Result;
      });

    return committed ?? new PlayerSpectatingCommit(gate, PlayerSpectating.None, []);
  }

  /// <summary>
  /// Called after the target's session starts closing, while its visual geometry is still
  /// available. A failed section preparation is returned and retains the old handle and camera
  /// position for the next tick's retry; it can never resolve to a reused player slot.
  /// </summary>
  public IReadOnlyDictionary<EntityHandle, PlayerSpectatingCommit> RepairAfterClose(
      EntityHandle closingTarget) {
    TryRepairAfterClose(closingTarget, null, out var repairs);
    return repairs;
  }

  /// <summary>
  /// Captures repair output before section or observer writes. The callback may prepare output
  /// and release the closing target's dependencies; it must preserve observers and their prepared
  /// sections, avoid input mutation and I/O, and not reenter session handling. Rejection exposes
  /// no repairs and may leave reserved player version gaps.
  /// </summary>
  internal OwnerCommitResult TryRepairAfterClose(
      EntityHandle closingTarget,
      Func<IReadOnlyDictionary<EntityHandle, PlayerSpectatingCommit>, OwnerCommitResult>?
        prepareClose,
      out IReadOnlyDictionary<EntityHandle, PlayerSpectatingCommit> repairs) {
    repairs = new ReadOnlyDictionary<EntityHandle, PlayerSpectatingCommit>(
      new Dictionary<EntityHandle, PlayerSpectatingCommit>());
    PlayerSpectatingTickPreparation preparation = PrepareTick(closingTarget);
    var results = new Dictionary<EntityHandle, PlayerSpectatingCommit>();
    var prepared = new List<(
      Player Player, PreparedPlayerSpectatingChange Change, CommitVersion Version)>();
    foreach (var entry in preparation.Changes) {
      if (!_players.TryGet(entry.Key, out Player? player) || player is null) {
        throw new InvalidOperationException("A prepared spectating observer was retired during capture.");
      }

      CommitVersion version = _players.ReserveCommitVersion();
      var projections = new List<SessionPacketProjection>();
      entry.Value.AppendProjections(version, projections);
      results.Add(entry.Key, new PlayerSpectatingCommit(
        OwnerCommitResult.Applied(version), entry.Value.State, projections));
      prepared.Add((player, entry.Value, version));
    }

    foreach (var entry in preparation.Rejections) {
      _players.TryGet(entry.Key, out Player? player);
      results.Add(entry.Key, new PlayerSpectatingCommit(
        entry.Value, player?.Spectating ?? PlayerSpectating.None, []));
    }

    var captured = new ReadOnlyDictionary<EntityHandle, PlayerSpectatingCommit>(results);
    OwnerCommitResult? validation = prepareClose?.Invoke(captured);
    if (validation is not null && validation.Status != OwnerCommitStatus.Applied) {
      return validation;
    }

    foreach (var entry in prepared) {
      entry.Change.CommitSections();
      _players.CommitPreparedSpectating(entry.Player, entry.Change.State, entry.Version);
    }

    repairs = captured;
    return OwnerCommitResult.Applied(CommitVersion.Initial);
  }

  /// <summary>
  /// Captures all target/geometry facts and validates all section participants before tick writes.
  /// Failed arrangements keep their old relation and are reported by PlayerUpdateResult.
  /// </summary>
  internal PlayerSpectatingTickPreparation PrepareTick(EntityHandle? closingTarget = null) {
    Candidate[] roster = CaptureRoster(closingTarget);
    var byHandle = roster.ToDictionary(candidate => candidate.Binding.PlayerHandle);
    SessionEpoch[] recipients = roster.Where(candidate => candidate.Active)
      .Select(candidate => candidate.Binding.Epoch).ToArray();
    var changes = new Dictionary<EntityHandle, PreparedPlayerSpectatingChange>();
    var rejected = new Dictionary<EntityHandle, OwnerCommitResult>();
    foreach (Candidate observer in roster) {
      if (!observer.Active
          || !_players.TryGetForBinding(observer.Binding, out Player? player) || player is null
          || player.Spectating.Target is not EntityHandle target
          || closingTarget is EntityHandle closing && target != closing) {
        continue;
      }

      byHandle.TryGetValue(target, out Candidate? current);
      if (current is not null && target != observer.Binding.PlayerHandle && CanKeep(current)) {
        changes.Add(player.Handle, new PreparedPlayerSpectatingChange(observer.Binding,
          new PlayerSpectating(target, current.Geometry.CameraPosition),
          current.Binding.PlayerSlot, null, [], recipients, false));
        continue;
      }

      PacketVector2 origin = current?.Geometry.CameraPosition
        ?? player.Spectating.CameraPosition ?? observer.Position;
      Candidate? selected = FindClosest(roster, observer.Binding.PlayerHandle, origin);
      OwnerCommitResult result = Prepare(observer.Binding, selected, recipients, true,
        out PreparedPlayerSpectatingChange? change);
      if (result.Status == OwnerCommitStatus.Applied && change is not null) {
        changes.Add(player.Handle, change);
      } else {
        rejected.Add(player.Handle, result);
      }
    }

    return new PlayerSpectatingTickPreparation(changes, rejected);
  }

  private Candidate[] CaptureRoster(EntityHandle? closingTarget = null) {
    var roster = new List<Candidate>();
    foreach (Player player in _players.CapturePlayers()) {
      if (!_sessions.TryGetBinding(player.Epoch, out SessionBinding? binding) || binding is null
          || binding.PlayerHandle != player.Handle) {
        continue;
      }

      PlayerSpectatingGeometry geometry = _captureGeometry.Invoke(player);
      if (!IsFinite(geometry.Center) || !IsFinite(geometry.CameraPosition)) {
        throw new InvalidOperationException("The captured spectating geometry must be finite.");
      }

      bool active = _sessions.TryGetStage(player.Epoch, out SessionStage stage)
        && stage == SessionStage.Active && player.Handle != closingTarget;
      roster.Add(new Candidate(binding, active, player.IsDead, player.DeadTicks,
        player.Movement.Position, geometry));
    }

    return roster.OrderBy(candidate => candidate.Binding.PlayerSlot).ToArray();
  }

  private OwnerCommitResult Prepare(
      SessionBinding binding,
      Candidate? selected,
      IReadOnlyList<SessionEpoch> recipients,
      bool project,
      out PreparedPlayerSpectatingChange? change) {
    change = null;
    IPlayerSpectatingSectionPreparation? sections = null;
    JoinBatchPacket[] packets = [];
    PlayerSpectating state = PlayerSpectating.None;
    short targetSlot = -1;
    if (selected is not null) {
      sections = _prepareSections.Invoke(binding, selected.Binding.PlayerHandle, selected.Position);
      if (sections is null) {
        return OwnerCommitResult.Rejected(ProtocolErrorCode.NotReady,
          "No spectating section arrangement was prepared.");
      }

      if (sections.Epoch != binding.Epoch || sections.Target != selected.Binding.PlayerHandle) {
        return OwnerCommitResult.Rejected(ProtocolErrorCode.Conflict,
          "The spectating section arrangement belongs to another player or target.");
      }

      packets = sections.SectionPackets.ToArray();
      if (packets.Any(packet => packet is null || packet.MessageId is not (10 or 11))) {
        return OwnerCommitResult.Rejected(ProtocolErrorCode.Conflict,
          "The spectating section arrangement contains invalid output.");
      }

      OwnerCommitResult validation = sections.Validate();
      if (validation.Status != OwnerCommitStatus.Applied) {
        return validation;
      }

      targetSlot = selected.Binding.PlayerSlot;
      state = new PlayerSpectating(selected.Binding.PlayerHandle, selected.Geometry.CameraPosition);
    }

    change = new PreparedPlayerSpectatingChange(
      binding, state, targetSlot, sections, packets, recipients, project);
    return OwnerCommitResult.Applied(CommitVersion.Initial);
  }

  private static Candidate? FindClosest(
      IReadOnlyList<Candidate> roster, EntityHandle observer, PacketVector2 origin) {
    Candidate? closest = null;
    float distance = float.MaxValue;
    foreach (Candidate candidate in roster) {
      if (!candidate.Active || candidate.Dead || candidate.Binding.PlayerHandle == observer) {
        continue;
      }

      float x = candidate.Geometry.Center.X - origin.X;
      float y = candidate.Geometry.Center.Y - origin.Y;
      float candidateDistance = x * x + y * y;
      if (candidateDistance < distance) {
        closest = candidate;
        distance = candidateDistance;
      }
    }

    return closest;
  }

  private static bool CanKeep(Candidate candidate) {
    return candidate.Active && (!candidate.Dead || candidate.DeadTicks < LingerAfterDeathTicks);
  }

  private static bool IsFinite(PacketVector2 value) {
    return float.IsFinite(value.X) && float.IsFinite(value.Y);
  }
}

internal sealed record PlayerSpectatingTickPreparation(
  IReadOnlyDictionary<EntityHandle, PreparedPlayerSpectatingChange> Changes,
  IReadOnlyDictionary<EntityHandle, OwnerCommitResult> Rejections);
