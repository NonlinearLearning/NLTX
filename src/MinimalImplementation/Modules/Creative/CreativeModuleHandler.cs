using MinimalImplementation.Contracts;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace MinimalImplementation.Modules.Creative;

/// <summary>Applies module 5, rejects pending module 6 reports and captures module 9.</summary>
public sealed class CreativeModuleHandler
{
  private readonly CreativePowerStateOwner _powerOwner;
  private readonly CreativeUnlockReportOwner _unlockReportOwner;
  private readonly ICreativeSacrificeFacts _sacrificeFacts;

  public CreativeModuleHandler(
    CreativePowerStateOwner powerOwner,
    CreativeUnlockReportOwner unlockReportOwner,
    ICreativeSacrificeFacts sacrificeFacts)
  {
    ArgumentNullException.ThrowIfNull(powerOwner);
    ArgumentNullException.ThrowIfNull(unlockReportOwner);
    ArgumentNullException.ThrowIfNull(sacrificeFacts);
    _powerOwner = powerOwner;
    _unlockReportOwner = unlockReportOwner;
    _sacrificeFacts = sacrificeFacts;
  }

  public CreativeModuleHandlingResult HandleCreativePower(
    byte authenticatedPlayerId,
    bool countsAsGameplayHost,
    CreativePowerWireMessage message)
  {
    ArgumentNullException.ThrowIfNull(message);
    CreativePowerApplyResult result = _powerOwner.Apply(
      authenticatedPlayerId,
      countsAsGameplayHost,
      message);
    return new CreativeModuleHandlingResult(result.Commit, result.Effect, null);
  }

  public CreativeModuleHandlingResult HandleUnlockReport(
    byte authenticatedPlayerId,
    CreativeUnlockReportWireMessage message)
  {
    ArgumentNullException.ThrowIfNull(message);
    OwnerCommitResult commit = _unlockReportOwner.Record(
      authenticatedPlayerId,
      message,
      _sacrificeFacts,
      out CreativeUnlockReport? report);
    return new CreativeModuleHandlingResult(commit, null, report);
  }

  public OwnerCommitResult RejectClientPermissionUpdate(Packet82PermissionData message)
  {
    ArgumentNullException.ThrowIfNull(message);
    return OwnerCommitResult.Rejected(
      ProtocolErrorCode.Forbidden,
      "Creative power permission module 9 is a server-to-client snapshot only.");
  }

  public IReadOnlyList<Packet82PermissionData> CreatePermissionSnapshot()
  {
    return _powerOwner.CapturePermissionPacketData();
  }

  public CreativePermissionSnapshotBatch? CaptureDirtyPermissionSnapshot(SessionEpoch target)
  {
    return _powerOwner.CaptureDirtyPermissionSnapshot(target);
  }

  public CreativePermissionDeliveryResult AcknowledgePermissionSnapshot(
    CreativePermissionSnapshotBatch batch,
    IReadOnlyCollection<SendReceipt> receipts)
  {
    return _powerOwner.AcknowledgePermissionSnapshot(batch, receipts);
  }

  public CreativePowerEffectDeliveryBatch CreateEffectDeliveryBatch(
    CreativePowerEffect effect,
    IReadOnlyCollection<OutboundPacketIntent> intents)
  {
    return _powerOwner.CreateEffectDeliveryBatch(effect, intents);
  }

  public CreativePowerEffectDeliveryResult AcknowledgeEffectDelivery(
    CreativePowerEffectDeliveryBatch batch,
    IReadOnlyCollection<SendReceipt> receipts)
  {
    return _powerOwner.AcknowledgeEffectDelivery(batch, receipts);
  }

  public CreativePowerEffectDeliverySettlement CreateEffectDeliverySettlement(
    CreativePowerEffectDeliveryBatch batch)
  {
    ArgumentNullException.ThrowIfNull(batch);
    return new CreativePowerEffectDeliverySettlement(this, batch);
  }

  public void RetireEpoch(SessionEpoch epoch)
  {
    _powerOwner.RetireEpoch(epoch);
  }

  internal CreativePowerEffectDeliveryResult SettleEffectDelivery(
      CreativePowerEffectDeliveryBatch batch,
      IReadOnlyList<OutboundIntentDelivery> deliveries) {
    return _powerOwner.SettleEffectDelivery(batch, deliveries);
  }

  internal bool IsEffectDeliverySettled(CreativePowerEffectDeliveryBatch batch) {
    return _powerOwner.IsEffectDeliverySettled(batch);
  }

  internal CreativePowerEffectDeliveryBatch? CapturePendingEffectDelivery(
      CreativePowerEffectDeliveryBatch batch) {
    return _powerOwner.CapturePendingEffectDelivery(batch);
  }
}

public sealed record CreativeModuleHandlingResult(
  OwnerCommitResult Commit,
  CreativePowerEffect? PowerEffect,
  CreativeUnlockReport? UnlockReport);
