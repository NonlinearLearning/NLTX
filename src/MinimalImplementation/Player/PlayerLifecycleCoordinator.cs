using System;
using System.Collections.Generic;
using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Session;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace MinimalImplementation.Server.Player;

/// <summary>Commits player spawn/death edges and creates their typed network projections.</summary>
public sealed class PlayerLifecycleCoordinator
{
  private static readonly SessionStage[] SpawnStages =
  [
    SessionStage.Synchronizing,
    SessionStage.Active
  ];

  private readonly SessionAdmissionCoordinator _sessions;
  private readonly PlayerOwner _players;
  private readonly Func<SessionBinding, SyncProjectileTrackersPacket>? _captureProjectileTrackers;

  /// <summary>
  /// The optional tracker capture must read current owner state without mutation and return an
  /// independently owned packet that is valid for outbound encoding before spawn commits.
  /// </summary>
  public PlayerLifecycleCoordinator(
    SessionAdmissionCoordinator sessions,
    PlayerOwner players,
    Func<SessionBinding, SyncProjectileTrackersPacket>? captureProjectileTrackers = null)
  {
    ArgumentNullException.ThrowIfNull(sessions);
    ArgumentNullException.ThrowIfNull(players);
    _sessions = sessions;
    _players = players;
    _captureProjectileTrackers = captureProjectileTrackers;
  }

  /// <summary>
  /// Prepares all spawn projections before player writes and session activation. The synchronous
  /// callback may encode or capture output; it must not mutate inputs or owners, perform I/O,
  /// or reenter lifecycle operations. Rejection preserves player state, stage and accepted sequence.
  /// Output may be submitted only after this method returns Applied.
  /// </summary>
  public PlayerLifecycleResult ApplySpawn(
    InboundPacketEnvelope envelope,
    PlayerSpawnPacket packet,
    bool sendGameplayHostProjection,
    Func<IReadOnlyList<SessionPacketProjection>, OwnerCommitResult>? prepareOutput = null,
    PacketVector2? fallbackSpawnPosition = null)
  {
    ArgumentNullException.ThrowIfNull(envelope);
    ArgumentNullException.ThrowIfNull(packet);
    bool completingInitialJoin = _sessions.TryGetStage(
      envelope.Epoch,
      out SessionStage currentStage)
      && currentStage == SessionStage.Synchronizing;
    SessionStage? nextStage = completingInitialJoin ? SessionStage.Active : null;
    IReadOnlyList<SessionPacketProjection> projections =
      Array.Empty<SessionPacketProjection>();
    OwnerCommitResult result = _sessions.ExecuteAuthenticatedPlayerPacketAndTransition(
      envelope,
      SessionAdmissionCoordinator.PlayerSpawnMessageId,
      SpawnStages,
      nextStage,
      binding =>
      {
        bool announceConnection = completingInitialJoin &&
          !_sessions.HasConnectionAnnouncement(binding.Epoch);
        IReadOnlyList<SessionEpoch> recipients =
          _sessions.CaptureActiveRecipients(envelope.Epoch);
        var existingSnapshots = new List<(
          PlayerJoinSnapshot Player, SyncProjectileTrackersPacket? Trackers)>();
        SyncProjectileTrackersPacket? joiningTrackers = null;
        if (completingInitialJoin) {
          try {
            foreach (SessionEpoch epoch in recipients) {
              if (!_sessions.TryGetBinding(epoch, out SessionBinding? other) || other is null ||
                  !_players.TryCaptureJoinSnapshot(other, out PlayerJoinSnapshot? existing) ||
                  existing is null) {
                return OwnerCommitResult.Rejected(ProtocolErrorCode.NotReady,
                  "An active player's snapshot is unavailable.");
              }

              existingSnapshots.Add((existing, CaptureProjectileTrackers(other)));
            }

            if (recipients.Count > 0) {
              joiningTrackers = CaptureProjectileTrackers(binding);
            }
          } catch (InvalidOperationException exception) {
            return OwnerCommitResult.Rejected(ProtocolErrorCode.NotReady, exception.Message);
          }
        }

        OwnerCommitResult PrepareSpawnOutput(
            PlayerJoinSnapshot joining, PlayerSpawnMutation proposed) {
          CommitVersion version = proposed.Result.Version!.Value;
          var projectionList = new List<SessionPacketProjection>();
          if (completingInitialJoin) {
            AddPlayerProjections(joining, recipients, envelope.Epoch,
              version, joiningTrackers, projectionList);
            foreach (var existing in existingSnapshots) {
              AddPlayerProjections(existing.Player, [binding.Epoch], null,
                version, existing.Trackers, projectionList);
            }

            if (announceConnection && recipients.Count > 0) {
              projectionList.Add(new SessionPacketProjection(
                SessionAdmissionCoordinator.NetModulesMessageId,
                new NetModulesPacket {
                  ModuleId = (ushort)Packet82ModuleId.Text,
                  Data = new Packet82TextData(byte.MaxValue,
                    NetworkText.Key("LegacyMultiplayer.19",
                      NetworkText.Literal(joining.Appearance.Name)),
                    255, 240, 20)
                },
                recipients, envelope.Epoch, version));
            }
          }

          if (completingInitialJoin && sendGameplayHostProjection &&
              joining.CountsAsHostForGameplay) {
            projectionList.Add(new SessionPacketProjection(
              SessionAdmissionCoordinator.SetCountsAsHostForGameplayMessageId,
              new SetCountsAsHostForGameplayPacket {
                Player = binding.PlayerSlot,
                CountsAsHost = true
              },
              [envelope.Epoch], null, version));
          }

          if (recipients.Count > 0) {
            projectionList.Add(new SessionPacketProjection(
              SessionAdmissionCoordinator.PlayerSpawnMessageId,
              proposed.Packet!, recipients, envelope.Epoch, version));
          }

          if (completingInitialJoin) {
            projectionList.Add(new SessionPacketProjection(
              SessionAdmissionCoordinator.FinishedConnectingMessageId,
              new FinishedConnectingToServerPacket(), [envelope.Epoch], null, version));
          }

          IReadOnlyList<SessionPacketProjection> captured = projectionList.AsReadOnly();
          OwnerCommitResult prepared = prepareOutput?.Invoke(captured)
            ?? OwnerCommitResult.Applied(version);
          if (prepared.Status == OwnerCommitStatus.Applied) {
            projections = captured;
          }

          return prepared;
        }

        PlayerSpawnMutation spawn = _players.ApplySpawn(
            binding, packet, PrepareSpawnOutput, fallbackSpawnPosition);
        if (spawn.Result.Status == OwnerCommitStatus.Applied && announceConnection) {
          _sessions.RecordConnectionAnnouncement(binding.Epoch);
        }

        return spawn.Result;
      });

    return new PlayerLifecycleResult(
      result,
      projections);
  }

  /// <summary>
  /// Prepares the canonical death relay before modeled player state changes. The caller supplies
  /// the authoritative death decision and timer; this method does not implement death policy.
  /// The synchronous callback may encode or capture output but must not mutate inputs or owners,
  /// perform I/O, or reenter lifecycle operations. Submit output only after Applied is returned.
  /// </summary>
  public PlayerLifecycleResult ApplyDeath(
    InboundPacketEnvelope envelope,
    PlayerDeathV2Packet packet,
    int respawnTimer,
    bool pvpDeath,
    Func<IReadOnlyList<SessionPacketProjection>, OwnerCommitResult>? prepareOutput = null)
  {
    ArgumentNullException.ThrowIfNull(envelope);
    ArgumentNullException.ThrowIfNull(packet);
    if (respawnTimer < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(respawnTimer));
    }

    IReadOnlyList<SessionPacketProjection> projections = Array.Empty<SessionPacketProjection>();
    OwnerCommitResult result = _sessions.ExecuteAuthenticatedPlayerPacket(
      envelope,
      SessionAdmissionCoordinator.PlayerDeathMessageId,
      [SessionStage.Active],
      binding =>
      {
        OwnerCommitResult PrepareDeathOutput(CommitVersion version) {
          IReadOnlyList<SessionEpoch> recipients =
            _sessions.CaptureActiveRecipients(envelope.Epoch);
          IReadOnlyList<SessionPacketProjection> captured = recipients.Count == 0 ? [] :
            Array.AsReadOnly(new[] {
              new SessionPacketProjection(
                SessionAdmissionCoordinator.PlayerDeathMessageId,
                new PlayerDeathV2Packet {
                  Player = binding.PlayerSlot,
                  Reason = packet.Reason,
                  Damage = packet.Damage,
                  DirectionCode = packet.DirectionCode,
                  DeathFlags = pvpDeath ? (byte)1 : (byte)0
                },
                recipients,
                envelope.Epoch,
                version)
            });
          OwnerCommitResult prepared = prepareOutput?.Invoke(captured)
            ?? OwnerCommitResult.Applied(version);
          if (prepared.Status == OwnerCommitStatus.Applied) {
            projections = captured;
          }

          return prepared;
        }

        return _players.ApplyDeathState(binding, respawnTimer, pvpDeath, PrepareDeathOutput);
      });

    return new PlayerLifecycleResult(result, projections);
  }

  private SyncProjectileTrackersPacket? CaptureProjectileTrackers(SessionBinding binding) {
    SyncProjectileTrackersPacket? packet = _captureProjectileTrackers?.Invoke(binding);
    if (packet is not null && packet.Player != binding.PlayerSlot) {
      throw new InvalidOperationException(
        "Projectile trackers must belong to the authenticated player.");
    }

    return packet;
  }

  private static void AddPlayerProjections(
      PlayerJoinSnapshot snapshot,
      IReadOnlyList<SessionEpoch> recipients,
      SessionEpoch? ignored,
      CommitVersion version,
      SyncProjectileTrackersPacket? projectileTrackers,
      List<SessionPacketProjection> projections) {
    if (recipients.Count == 0) {
      return;
    }

    foreach (PlayerPacketProjection packet in PlayerPacketProjection.From(
        snapshot, projectileTrackers)) {
      projections.Add(new SessionPacketProjection(
        packet.MessageId, packet.Packet, recipients, ignored, version));
    }
  }
}

public sealed record PlayerLifecycleResult(
  OwnerCommitResult Result,
  IReadOnlyList<SessionPacketProjection> Projections);
