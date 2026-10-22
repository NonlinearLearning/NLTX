using System;
using System.Collections.Generic;
using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Player;
using MinimalImplementation.Server.Protocol;
using Terraria.NetWork.Prototype.PacketDesignCompiler;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace MinimalImplementation.Server.Session;

/// <summary>
/// Thin packet-to-owner adapters. Frame parsing and TCP ownership remain in P01.
/// </summary>
public sealed class SessionPacketHandlers
{
  private readonly SessionAdmissionCoordinator _admission;
  private readonly SessionHostAuthorization _hostAuthorization;
  private readonly JoinCoordinator _join;
  private readonly SessionPingHandler _ping;

  public SessionPacketHandlers(
    SessionAdmissionCoordinator admission,
    SessionHostAuthorization hostAuthorization,
    JoinCoordinator join)
  {
    ArgumentNullException.ThrowIfNull(admission);
    ArgumentNullException.ThrowIfNull(hostAuthorization);
    ArgumentNullException.ThrowIfNull(join);
    _admission = admission;
    _hostAuthorization = hostAuthorization;
    _join = join;
    _ping = new SessionPingHandler(admission);
  }

  public SessionAdmissionPreparation HandleHello(
    InboundPacketEnvelope envelope,
    HelloPacket packet,
    bool isBanned)
  {
    return _admission.PrepareHello(envelope, packet, isBanned);
  }

  public SessionAdmissionPreparation HandlePassword(
    InboundPacketEnvelope envelope,
    SendPasswordPacket packet)
  {
    return _admission.PreparePassword(envelope, packet);
  }

  public SessionTransitionCommitResult CommitAdmission(
    SessionAdmissionPreparation preparation,
    Version4WireProfile wireProfile,
    bool enableDebugCommands = false)
  {
    ArgumentNullException.ThrowIfNull(preparation);
    ArgumentNullException.ThrowIfNull(wireProfile);
    if (!preparation.IsReady || preparation.Transition is null)
    {
      return SessionTransitionCommitResult.Failed(
        preparation.Rejection
          ?? OwnerCommitResult.Rejected(
            ProtocolErrorCode.NotReady,
            "The admission preparation did not produce a transition."));
    }

    if (preparation.ResponsePacket is null || preparation.ResponseMessageId is not byte)
    {
      return SessionTransitionCommitResult.Failed(
        OwnerCommitResult.Rejected(
          ProtocolErrorCode.NotReady,
          "The admission response packet is missing."));
    }

    Version4EncodedPacket encoded = wireProfile.Encode(
      WireDirection.ServerToClient,
      preparation.ResponsePacket,
      enableDebugCommands);
    if (encoded.MessageId != preparation.ResponseMessageId.Value)
    {
      return SessionTransitionCommitResult.Failed(
        OwnerCommitResult.Rejected(
          ProtocolErrorCode.Conflict,
          "The prepared admission response does not match its packet id."));
    }

    return _admission.Commit(preparation.Transition, encoded.Payload);
  }

  public OwnerCommitResult HandleHostToken(
    InboundPacketEnvelope envelope,
    HostTokenPacket packet)
  {
    return _hostAuthorization.Apply(envelope, packet);
  }

  public SessionPingResult HandlePing(
      InboundPacketEnvelope envelope,
      PingPacket packet,
      Func<IReadOnlyList<SessionPacketProjection>, OwnerCommitResult>? prepareOutput = null)
  {
    return _ping.Handle(envelope, packet, prepareOutput);
  }

  /// <inheritdoc cref="JoinCoordinator.HandleWorldDataRequest"/>
  public JoinStepResult HandleWorldDataRequest(
    InboundPacketEnvelope envelope,
    WorldDataPacket worldData,
    Func<IReadOnlyList<SessionPacketProjection>, OwnerCommitResult>? prepareDelivery = null)
  {
    return _join.HandleWorldDataRequest(envelope, worldData, prepareDelivery);
  }

  /// <inheritdoc cref="JoinCoordinator.HandleSpawnTileData"/>
  public JoinStepResult HandleSpawnTileData(
    InboundPacketEnvelope envelope,
    SpawnTileDataPacket packet,
    Func<SessionBinding, SpawnTileDataPacket, JoinSynchronizationBatch> captureBatch,
    Func<IReadOnlyList<SessionPacketProjection>, OwnerCommitResult>? prepareOutput = null)
  {
    return _join.HandleSpawnTileData(envelope, packet, captureBatch, prepareOutput);
  }

  public OutboundPacketIntent CreateIntent(
    SessionPacketProjection projection,
    Version4WireProfile wireProfile,
    bool enableDebugCommands = false)
  {
    ArgumentNullException.ThrowIfNull(projection);
    ArgumentNullException.ThrowIfNull(wireProfile);
    OutboundPacketIntent intent = wireProfile.CreateOutboundIntent(
      projection.Packet,
      projection.TargetEpochs,
      projection.IgnoreEpoch,
      projection.CommitVersion,
      enableDebugCommands);
    if (intent.MessageId != projection.MessageId)
    {
      throw new InvalidOperationException(
        $"The packet projection id {projection.MessageId} does not match the encoded id {intent.MessageId}.");
    }

    return intent;
  }

  public IReadOnlyList<OutboundPacketIntent> CreateIntents(
    IReadOnlyCollection<SessionPacketProjection> projections,
    Version4WireProfile wireProfile,
    bool enableDebugCommands = false)
  {
    ArgumentNullException.ThrowIfNull(projections);
    ArgumentNullException.ThrowIfNull(wireProfile);
    var intents = new List<OutboundPacketIntent>(projections.Count);
    foreach (SessionPacketProjection projection in projections)
    {
      intents.Add(CreateIntent(projection, wireProfile, enableDebugCommands));
    }

    return intents.AsReadOnly();
  }

  public OutboundPacketIntent? CreateIntent(
    PlayerPacketMutationResult mutation,
    Version4WireProfile wireProfile,
    bool enableDebugCommands = false)
  {
    ArgumentNullException.ThrowIfNull(mutation);
    ArgumentNullException.ThrowIfNull(wireProfile);
    bool isKickRejection = mutation.Result.Status == OwnerCommitStatus.Rejected
      && mutation.CloseAfterResponse
      && mutation.ResponseMessageId == SessionAdmissionCoordinator.KickMessageId
      && mutation.ResponsePacket is KickPacket;
    if ((mutation.Result.Status != OwnerCommitStatus.Applied && !isKickRejection)
      || mutation.ResponseMessageId is not byte messageId
      || mutation.ResponsePacket is null
      || mutation.TargetEpochs.Count == 0
      || mutation.ResponseVersion is not CommitVersion version
      || !version.IsValid)
    {
      return null;
    }

    return CreateIntent(
      new SessionPacketProjection(
        messageId,
        mutation.ResponsePacket,
        mutation.TargetEpochs,
        mutation.IgnoreEpoch,
        version),
      wireProfile,
      enableDebugCommands);
  }
}
