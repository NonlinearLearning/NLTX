using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Modules;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace MinimalImplementation.Server.Modules.Bestiary;

/// <summary>
/// Routes server-authored Bestiary facts while rejecting client attempts to write them.
/// </summary>
public sealed class BestiaryModuleHandler : IModuleHandler, IModuleEpochRetirable
{
  private readonly BestiaryState _state;

  public BestiaryModuleHandler(BestiaryState state)
  {
    ArgumentNullException.ThrowIfNull(state);
    _state = state;
  }

  public ushort ModuleId => (ushort)Packet82ModuleId.Bestiary;

  public ModuleDispatchResult Handle(
    InboundPacketEnvelope envelope,
    ModuleDispatchContext context,
    object? decodedPayload)
  {
    ArgumentNullException.ThrowIfNull(envelope);
    ArgumentNullException.ThrowIfNull(context);
    _ = decodedPayload;

    ModuleDispatchResult? validation = Packet82ModuleHandlerValidation.ValidateActiveEnvelope(
      envelope,
      context,
      ModuleId,
      "Bestiary");
    if (validation is not null)
    {
      return validation;
    }

    return ModuleDispatchResult.Rejected(
      ProtocolErrorCode.Forbidden,
      "Bestiary state is server-authored from committed NPC facts.");
  }

  public BestiaryContributionResult ApplyP05Contribution(BestiaryContribution contribution)
  {
    return _state.ApplyP05Contribution(contribution);
  }

  public BestiaryContributionResult ApplyP05Kill(
      P05NpcDeathKey key, short npcNetId, string persistentNpcId,
      IReadOnlyCollection<SessionEpoch> visibleTargets) {
    return _state.ApplyP05Kill(key, npcNetId, persistentNpcId, visibleTargets);
  }

  public bool AcknowledgeP05Kill(P05NpcDeathKey key) {
    return _state.AcknowledgeP05Kill(key);
  }

  public IReadOnlyList<OutboundPacketIntent> CreateSnapshotIntents(
    SessionEpoch target,
    IReadOnlyDictionary<string, short> persistentNpcNetIds)
  {
    return _state.CreateSnapshotIntents(target, persistentNpcNetIds);
  }

  public BestiarySnapshotIntentBatch? CaptureDirtySnapshot(
    SessionEpoch target,
    IReadOnlyDictionary<string, short> persistentNpcNetIds)
  {
    return _state.CaptureDirtySnapshot(target, persistentNpcNetIds);
  }

  public ModuleDeliveryResult AcknowledgeSnapshot(
    BestiarySnapshotIntentBatch batch,
    IReadOnlyCollection<SendReceipt> receipts)
  {
    return _state.AcknowledgeSnapshot(batch, receipts);
  }

  public void RetireEpoch(SessionEpoch epoch)
  {
    _state.RetireEpoch(epoch);
  }

  public ModuleDeliveryResult AcknowledgeSnapshot(
      BestiarySnapshotIntentBatch batch,
      IReadOnlyList<OutboundIntentDelivery> deliveries) {
    return _state.AcknowledgeSnapshot(batch, deliveries);
  }
}
