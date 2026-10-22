using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Modules;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace MinimalImplementation.Server.Modules.Banners;

/// <summary>
/// Routes server-authored Banner projections and commits Version4 claim requests.
/// </summary>
public sealed class BannerModuleHandler : IModuleHandler, IModuleEpochRetirable
{
  private readonly BannerState _state;

  public BannerModuleHandler(BannerState state)
  {
    ArgumentNullException.ThrowIfNull(state);
    _state = state;
  }

  public ushort ModuleId => (ushort)Packet82ModuleId.Banners;

  public ModuleDispatchResult Handle(
    InboundPacketEnvelope envelope,
    ModuleDispatchContext context,
    object? decodedPayload)
  {
    ArgumentNullException.ThrowIfNull(envelope);
    ArgumentNullException.ThrowIfNull(context);

    ModuleDispatchResult? validation = Packet82ModuleHandlerValidation.ValidateActiveEnvelope(
      envelope,
      context,
      ModuleId,
      "Banner");
    if (validation is not null)
    {
      return validation;
    }

    if (decodedPayload is not Packet82BannerClaimRequestData request)
    {
      return ModuleDispatchResult.Rejected(
        ProtocolErrorCode.Forbidden,
        "Banner module accepts only the client claim request action on the server.");
    }

    SessionEpoch[] targets = context.ActiveRecipients
      .Select(static recipient => recipient.Epoch)
      .Append(context.Epoch)
      .Distinct()
      .ToArray();
    BannerCommitResult result = _state.ProcessClaimRequest(
      context.Epoch,
      new BannerClaimRequest(request.BannerId, request.Amount),
      targets);
    if (result.Status == BannerCommitStatus.Rejected
        || result.Version is not CommitVersion version)
    {
      return ModuleDispatchResult.Rejected(
        result.ErrorCode ?? ProtocolErrorCode.Conflict,
        "The Banner claim request was rejected by the owner.");
    }

    return ModuleDispatchResult.Handled(
      version,
      result.OutboundIntents,
      replayOutboundEffects: false);
  }

  public BannerSnapshotIntentBatch? CaptureDirtySnapshot(SessionEpoch target)
  {
    return _state.CaptureDirtySnapshot(target);
  }

  public BannerCommitResult ApplyP05Kill(
      P05NpcDeathKey key, short bannerId, int killsRequired,
      IReadOnlyCollection<SessionEpoch> broadcastTargets) {
    return _state.ApplyP05Kill(key, bannerId, killsRequired, broadcastTargets);
  }

  public bool AcknowledgeP05Kill(P05NpcDeathKey key) {
    return _state.AcknowledgeP05Kill(key);
  }

  public ModuleDeliveryResult AcknowledgeSnapshot(
    BannerSnapshotIntentBatch batch,
    IReadOnlyCollection<SendReceipt> receipts)
  {
    return _state.AcknowledgeSnapshot(batch, receipts);
  }

  public void RetireEpoch(SessionEpoch epoch)
  {
    _state.RetireEpoch(epoch);
  }

  public ModuleDeliveryResult AcknowledgeSnapshot(
      BannerSnapshotIntentBatch batch,
      IReadOnlyList<OutboundIntentDelivery> deliveries) {
    return _state.AcknowledgeSnapshot(batch, deliveries);
  }
}
