using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Inventory.Handlers;
using MinimalImplementation.Server.Inventory.State;
using MinimalImplementation.Server.Player;
using MinimalImplementation.Server.Session;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace MinimalImplementation.Server.Host;

public sealed partial class Version4InboundRouter {
  private OwnerCommitResult EnsureAdmissionInventory(
      SessionEpoch epoch,
      OutboundPacketIntent response) {
    if (response.MessageId != SessionAdmissionCoordinator.PlayerInfoMessageId) {
      return OwnerCommitResult.Applied(CommitVersion.Initial);
    }

    if (!_composition.SessionAdmission.TryGetBinding(epoch, out SessionBinding? binding)
        || binding is null) {
      return OwnerCommitResult.Rejected(
        ProtocolErrorCode.NotReady,
        "The player binding was not available for inventory initialization.");
    }

    if (_composition.InventoryStore.TryCapture(binding.PlayerHandle.Id, out _)) {
      return OwnerCommitResult.Applied(CommitVersion.Initial);
    }

    return _composition.InventoryStore.CreateEmpty(binding.PlayerHandle.Id, out _);
  }

  private string? ConsumeClientUuid(
      InboundPacketEnvelope envelope,
      Unknown68Packet packet) {
    ArgumentNullException.ThrowIfNull(packet);
    OwnerCommitResult result = _composition.SessionAdmission.ExecuteAuthenticatedPlayerPacket(
      envelope,
      SessionAdmissionCoordinator.Unknown68MessageId,
      [SessionStage.AwaitPlayerData, SessionStage.AwaitSectionRequest,
       SessionStage.Synchronizing, SessionStage.Active],
      _ => OwnerCommitResult.Applied(CommitVersion.Initial));
    return result.Status == OwnerCommitStatus.Applied
      ? null
      : result.Detail ?? "The client UUID packet was rejected.";
  }

  private string? HandleEquipmentUpload(
      InboundPacketEnvelope envelope,
      SyncEquipmentPacket packet) {
    ArgumentNullException.ThrowIfNull(packet);
    OwnerCommitResult result = _composition.SessionAdmission.ExecuteAuthenticatedPlayerPacket(
      envelope,
      SessionAdmissionCoordinator.SyncEquipmentMessageId,
      [SessionStage.AwaitPlayerData, SessionStage.AwaitSectionRequest,
       SessionStage.Synchronizing, SessionStage.Active],
      binding => {
        if (!_composition.InventoryStore.TryCapture(binding.PlayerHandle.Id, out InventorySnapshot? inventory)
            || inventory is null) {
          return OwnerCommitResult.Rejected(
            ProtocolErrorCode.NotReady,
            "The authenticated player's inventory is unavailable.");
        }

        if (packet.Player != binding.PlayerSlot || packet.Slot < 0 ||
            packet.Slot >= inventory.Slots.Count || packet.Stack < 0 || packet.ItemType < 0) {
          return OwnerCommitResult.Rejected(ProtocolErrorCode.Forbidden,
            "The equipment upload does not belong to its authenticated player or slot.");
        }
        OwnerCommitResult? drop = TryCommitPendingItemDrop(envelope, binding, packet, inventory);
        if (drop is not null) {
          if (drop.Status != OwnerCommitStatus.Applied) {
            return drop;
          }
          _composition.InventoryStore.TryCapture(binding.PlayerHandle.Id, out inventory);
        }
        P04InventoryCommitResult committed = _composition.InventoryPacketCommands.HandleEquipment(
          binding.PlayerHandle.Id,
          inventory!.Revision,
          packet);
        return committed.Commit;
      });
    return result.Status == OwnerCommitStatus.Applied
      ? null
      : result.Detail ?? "The equipment packet was rejected.";
  }

  private string? HandleLoadout(
      InboundPacketEnvelope envelope,
      SyncLoadoutPacket packet,
      out IReadOnlyList<OutboundPacketIntent> intents) {
    IReadOnlyList<OutboundPacketIntent> captured = Array.Empty<OutboundPacketIntent>();
    PlayerLoadoutCommit committed = _composition.PlayerStatePacketHandlers.HandleLoadout(
      envelope, packet, _composition.InventoryLoadouts.Prepare, projections => {
        return CaptureSessionOutputIntents(
          SessionAdmissionCoordinator.SyncLoadoutMessageId, projections, out captured);
      });
    intents = committed.Result.Status == OwnerCommitStatus.Applied
      ? captured : Array.Empty<OutboundPacketIntent>();
    return committed.Result.Status == OwnerCommitStatus.Applied
      ? null : committed.Result.Detail ?? "The inventory loadout was rejected.";
  }
}
