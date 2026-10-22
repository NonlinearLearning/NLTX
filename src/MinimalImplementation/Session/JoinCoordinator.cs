using System;
using System.Collections.Generic;
using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Player;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;
using PlayerEntity = MinimalImplementation.Server.Player.Player;

namespace MinimalImplementation.Server.Session;

/// <summary>
/// Commits the 6/8 join stages and orders the provider-owned synchronization batch.
/// </summary>
public sealed class JoinCoordinator
{
  private static readonly SessionStage[] WorldDataStages =
  [
    SessionStage.AwaitPlayerData,
    SessionStage.AwaitSectionRequest,
    SessionStage.Synchronizing
  ];

  private static readonly SessionStage[] SectionStages = [SessionStage.AwaitSectionRequest];

  private readonly SessionAdmissionCoordinator _sessions;
  private readonly PlayerOwner _players;

  public JoinCoordinator(
    SessionAdmissionCoordinator sessions,
    PlayerOwner players)
  {
    ArgumentNullException.ThrowIfNull(sessions);
    ArgumentNullException.ThrowIfNull(players);
    _sessions = sessions;
    _players = players;
  }

  /// <summary>Accepts packet 6 after authenticating its player and preparing delivery.</summary>
  /// <remarks>
  /// Call on the serialized commit loop. The optional callback runs after player validation and
  /// before session progression. It must synchronously encode all output before initializing
  /// section delivery state, and leave delivery state unchanged on rejection. It must not mutate
  /// the supplied projections/packet or other owners, perform I/O, or reenter the session owner.
  /// Applied allows session progression; rejection publishes no projections and consumes no sequence.
  /// Reserved commit versions may have gaps. This does not make network submission transactional.
  /// </remarks>
  public JoinStepResult HandleWorldDataRequest(
    InboundPacketEnvelope envelope,
    WorldDataPacket worldData,
    Func<IReadOnlyList<SessionPacketProjection>, OwnerCommitResult>? prepareDelivery = null)
  {
    ArgumentNullException.ThrowIfNull(envelope);
    ArgumentNullException.ThrowIfNull(worldData);

    IReadOnlyList<SessionPacketProjection> projections = Array.Empty<SessionPacketProjection>();
    OwnerCommitResult result = _sessions.ExecuteAuthenticatedPlayerPacketAndTransition(
      envelope,
      SessionAdmissionCoordinator.RequestWorldDataMessageId,
      WorldDataStages,
      GetWorldDataNextStage(envelope.Epoch),
      binding =>
      {
        if (!_players.TryGetForBinding(binding, out PlayerEntity? player) || player is null)
        {
          return OwnerCommitResult.Rejected(
            ProtocolErrorCode.StaleEntity,
            "The authenticated player is no longer available.");
        }

        CommitVersion version = _sessions.AllocateCommitVersion();
        IReadOnlyList<SessionPacketProjection> preparedProjections =
          Array.AsReadOnly<SessionPacketProjection>([
            new SessionPacketProjection(
              SessionAdmissionCoordinator.WorldDataMessageId,
              worldData,
              [envelope.Epoch],
              null,
              version)
          ]);
        OwnerCommitResult? prepared = prepareDelivery?.Invoke(preparedProjections);
        if (prepared is not null && prepared.Status != OwnerCommitStatus.Applied)
        {
          return prepared;
        }

        projections = preparedProjections;
        return OwnerCommitResult.Applied(version);
      });

    return result.Status == OwnerCommitStatus.Applied
      ? JoinStepResult.Applied(result, projections)
      : JoinStepResult.Rejected(result);
  }

  /// <summary>Captures and prepares packet-8 synchronization before session progression.</summary>
  /// <remarks>
  /// Call on the serialized commit loop. Capture runs only after authenticated player validation;
  /// the caller owns any transfer resources it opens and must retire them on rejection or exception.
  /// The optional preparation callback synchronously encodes/captures the complete proposed output.
  /// It must not mutate inputs or owners, perform I/O, or reenter the session owner. Only Applied
  /// permits session progression and output publication. Reserved commit versions may have gaps.
  /// This does not roll back capture effects or make network submission transactional.
  /// </remarks>
  public JoinStepResult HandleSpawnTileData(
    InboundPacketEnvelope envelope,
    SpawnTileDataPacket request,
    Func<SessionBinding, SpawnTileDataPacket, JoinSynchronizationBatch> captureBatch,
    Func<IReadOnlyList<SessionPacketProjection>, OwnerCommitResult>? prepareOutput = null)
  {
    ArgumentNullException.ThrowIfNull(envelope);
    ArgumentNullException.ThrowIfNull(request);
    ArgumentNullException.ThrowIfNull(captureBatch);

    JoinSynchronizationBatch? batch = null;
    IReadOnlyList<SessionPacketProjection> projections = Array.Empty<SessionPacketProjection>();
    OwnerCommitResult result = _sessions.ExecuteAuthenticatedPlayerPacketAndTransition(
      envelope,
      SessionAdmissionCoordinator.SpawnTileDataMessageId,
      SectionStages,
      SessionStage.Synchronizing,
      binding =>
      {
        if (!_players.TryGetForBinding(binding, out PlayerEntity? player) || player is null)
        {
          return OwnerCommitResult.Rejected(
            ProtocolErrorCode.StaleEntity,
            "The authenticated player is no longer available.");
        }

        JoinSynchronizationBatch captured = captureBatch(binding, request);
        if (captured is null) {
          return OwnerCommitResult.Rejected(
            ProtocolErrorCode.NotReady,
            "The join synchronization batch is not ready.");
        }

        CommitVersion version = _sessions.AllocateCommitVersion();
        var preparedProjections = new List<SessionPacketProjection>(captured.Packets.Count + 2) {
          new SessionPacketProjection(
            SessionAdmissionCoordinator.StatusTextSizeMessageId,
            captured.Progress,
            [envelope.Epoch],
            null,
            version)
        };
        foreach (JoinBatchPacket packet in captured.Packets) {
          preparedProjections.Add(new SessionPacketProjection(
            packet.MessageId,
            packet.Packet,
            [envelope.Epoch],
            null,
            version));
        }

        // 49 is the final synchronization barrier; packet 12 handles later activation.
        preparedProjections.Add(new SessionPacketProjection(
          SessionAdmissionCoordinator.InitialSpawnMessageId,
          new InitialSpawnPacket(),
          [envelope.Epoch],
          null,
          version));
        IReadOnlyList<SessionPacketProjection> preparedOutput = preparedProjections.AsReadOnly();
        OwnerCommitResult? prepared = prepareOutput?.Invoke(preparedOutput);
        if (prepared is not null && prepared.Status != OwnerCommitStatus.Applied) {
          return prepared;
        }

        batch = captured;
        projections = preparedOutput;
        return OwnerCommitResult.Applied(version);
      });

    return result.Status == OwnerCommitStatus.Applied && batch is not null
      ? JoinStepResult.Applied(result, projections, batch)
      : JoinStepResult.Rejected(result);
  }

  private SessionStage? GetWorldDataNextStage(SessionEpoch epoch)
  {
    return _sessions.TryGetStage(epoch, out SessionStage stage)
      && stage == SessionStage.AwaitPlayerData
      ? SessionStage.AwaitSectionRequest
      : null;
  }
}
