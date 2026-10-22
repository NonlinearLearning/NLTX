using System.Numerics;
using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Inventory.Handlers;
using MinimalImplementation.Server.Inventory.State;
using MinimalImplementation.Server.Session;
using MinimalImplementation.Server.World;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace MinimalImplementation.Server.Host;

public sealed partial class Version4InboundRouter {
  private sealed record PendingClientItemDrop(InventoryItemState Item, Vector2 Position,
    Vector2 Velocity, byte Ownership, ulong WorldEpoch, DateTimeOffset ReceivedAt);

  private readonly Dictionary<SessionEpoch, List<PendingClientItemDrop>> _pendingItemDrops = [];

  private string? HandleWorldItemUpload(InboundPacketEnvelope envelope, SyncItemPacket packet) {
    OwnerCommitResult result = _composition.SessionAdmission.ExecuteAuthenticatedPlayerPacket(
      envelope, 21, [SessionStage.Active], binding => {
        WorldHeaderCapture world = _composition.WorldOwner.CaptureWorldHeader();
        _composition.ClientWorldItems.SetTargets();
        if (packet.ItemIndex == 400) {
          var item = new InventoryItemState(packet.ItemType, packet.Stack, packet.Prefix, false);
          var position = new Vector2(packet.PositionX, packet.PositionY);
          var velocity = new Vector2(packet.VelocityX, packet.VelocityY);
          if (!_composition.ItemValidationPolicy.IsValid(item) || item.IsEmpty ||
              !_composition.Players.TryGetBoundPlayer(envelope.Epoch,
                _composition.SessionAdmission, out var player) || player is null || player.IsDead ||
              Vector2.Distance(position, new Vector2(player.Movement.Position.X,
                player.Movement.Position.Y)) > 128 ||
              Math.Abs(velocity.X) > 128 || Math.Abs(velocity.Y) > 128 ||
              (packet.StateFlags & 3) is not (2 or 3) || packet.Shimmered == true ||
              packet.ShimmerTime is > 0) {
            return OwnerCommitResult.Rejected(ProtocolErrorCode.Forbidden,
              "The new item is not a bounded inventory-drop request from this live player.");
          }
          if (!_pendingItemDrops.TryGetValue(envelope.Epoch, out var pending)) {
            pending = [];
            _pendingItemDrops.Add(envelope.Epoch, pending);
          }
          pending.RemoveAll(drop => drop.WorldEpoch != world.WorldEpoch ||
            envelope.ReceivedAt - drop.ReceivedAt > TimeSpan.FromSeconds(5));
          if (pending.Count >= 32) {
            return OwnerCommitResult.Rejected(ProtocolErrorCode.Forbidden,
              "Too many item creations are waiting for an inventory debit.");
          }
          pending.Add(new PendingClientItemDrop(item, position, velocity,
            (byte)(packet.StateFlags & 3), world.WorldEpoch, envelope.ReceivedAt));
          ServerLog.Info("server.item.drop_pending",
            $"epoch={envelope.Epoch.Value} type={item.Type} stack={item.Stack}");
          return OwnerCommitResult.Applied(world.Version);
        }
        if (!_composition.WorldItemStore.TryCapture(packet.ItemIndex, out var current) ||
            current is null) {
          // The current owner can send one final update after the item has been retired.
          return OwnerCommitResult.Applied(world.Version);
        }
        if (current.Handle.WorldScope != binding.PlayerHandle.WorldScope ||
            !_composition.WorldItemReservationStore.IsReservedBy(current.Handle,
              binding.PlayerHandle.Id, envelope.Epoch, envelope.ReceivedAt)) {
          return OwnerCommitResult.Rejected(ProtocolErrorCode.Forbidden,
            "Only the item's current authenticated owner may update it.");
        }
        if (packet.ItemType != current.Item.Type || packet.Prefix != current.Item.Prefix ||
            packet.Stack <= 0 || packet.Stack > current.Item.Stack ||
            packet.PositionX < 0 || packet.PositionY < 0 ||
            packet.PositionX >= world.Header.Geometry.Width * 16 ||
            packet.PositionY >= world.Header.Geometry.Height * 16) {
          return OwnerCommitResult.Rejected(ProtocolErrorCode.Forbidden,
            "The item update changes its identity, increases its stack or leaves the world.");
        }
        P04InventoryCommitResult committed = _composition.InventoryPacketCommands.HandleWorldItemSync(
          current.Handle, current.Revision, packet, envelope.Epoch);
        return committed.Commit;
      });
    return result.Status == OwnerCommitStatus.Applied ? null : result.Detail;
  }

  private OwnerCommitResult? TryCommitPendingItemDrop(InboundPacketEnvelope envelope,
      SessionBinding binding, SyncEquipmentPacket packet, InventorySnapshot inventory) {
    if (!_pendingItemDrops.TryGetValue(envelope.Epoch, out var pending) ||
        packet.Slot < 0 || packet.Slot >= InventorySlotLayout.ArmorStart) {
      return null;
    }
    WorldHeaderCapture world = _composition.WorldOwner.CaptureWorldHeader();
    pending.RemoveAll(drop => drop.WorldEpoch != world.WorldEpoch ||
      envelope.ReceivedAt - drop.ReceivedAt > TimeSpan.FromSeconds(5));
    InventorySlotSnapshot source = inventory.Slots[packet.Slot];
    int remaining = packet.ItemType == source.Item.Type && packet.Prefix == source.Item.Prefix
      ? packet.Stack : 0;
    int removed = source.Item.Stack - remaining;
    PendingClientItemDrop? drop = pending.FirstOrDefault(candidate =>
      candidate.Item.Type == source.Item.Type && candidate.Item.Prefix == source.Item.Prefix &&
      candidate.Item.Stack == removed);
    if (drop is null) {
      return null;
    }
    if (!_composition.Players.TryGetBoundPlayer(envelope.Epoch,
        _composition.SessionAdmission, out var player) || player is null || player.IsDead) {
      return OwnerCommitResult.Rejected(ProtocolErrorCode.StaleEpoch,
        "The inventory debit has no current active player.");
    }
    IReadOnlyList<int> slots = _composition.WorldItemStore.CaptureFreeSlots(1);
    if (slots.Count == 0) {
      return OwnerCommitResult.Rejected(ProtocolErrorCode.NotReady,
        "All world-item slots are occupied; the inventory debit is not committed.");
    }
    var spawn = new WorldItemSpawn(binding.PlayerHandle.WorldScope, slots[0],
      new WorldItemMutation(drop.Item, drop.Position, drop.Velocity, 0, false, 0,
        Runtime: new WorldItemRuntime(OwnTime: 100,
          OwnIgnore: drop.Ownership == 3 ? -1 : binding.PlayerSlot,
          NoGrabDelay: drop.Ownership == 3 ? 100 : 0)));
    var preparation = new InventoryItemDropPreparation(_composition.InventoryStore,
      binding, player, world, inventory, source, spawn);
    _composition.ClientWorldItems.SetTargets();
    OwnerCommitResult result = _composition.WorldItemDrops.Prepare(binding.PlayerHandle.WorldScope,
      world, envelope.ReceivedAt, preparation, [], out var transaction,
      echoInventoryToOwner: false);
    if (transaction is not null) {
      result = transaction.Commit().Result;
    }
    if (result.Status == OwnerCommitStatus.Applied) {
      pending.Remove(drop);
      ServerLog.Info("server.item.dropped",
        $"epoch={envelope.Epoch.Value} source={source.SlotId} slot={slots[0]} " +
        $"type={drop.Item.Type} stack={drop.Item.Stack}");
    }
    return result;
  }

  private string? HandleWorldItemRelease(InboundPacketEnvelope envelope,
      ReleaseItemOwnershipPacket packet) {
    OwnerCommitResult result = _composition.SessionAdmission.ExecuteAuthenticatedPlayerPacket(
      envelope, 39, [SessionStage.Active], binding => {
        _composition.ClientWorldItems.SetTargets();
        if (_composition.WorldItemStore.TryCapture(packet.ItemIndex, out var item) &&
            item is not null && item.Handle.WorldScope == binding.PlayerHandle.WorldScope) {
          _composition.WorldItemReservationStore.Release(item.Handle,
            binding.PlayerHandle.Id, envelope.Epoch);
        }
        // Reselection runs after item motion; a force request remains server-owned this frame.
        return OwnerCommitResult.Applied(CommitVersion.Initial);
      });
    return result.Status == OwnerCommitStatus.Applied ? null : result.Detail;
  }

  private string? HandleWorldItemRetirement(InboundPacketEnvelope envelope,
      SyncItemDespawnPacket packet) {
    OwnerCommitResult result = _composition.SessionAdmission.ExecuteAuthenticatedPlayerPacket(
      envelope, 151, [SessionStage.Active], binding => {
        _composition.ClientWorldItems.SetTargets();
        if (!_composition.WorldItemStore.TryCapture(packet.ItemIndex, out var item) || item is null) {
          return OwnerCommitResult.Applied(CommitVersion.Initial);
        }
        if (item.Handle.WorldScope != binding.PlayerHandle.WorldScope ||
            !_composition.WorldItemReservationStore.IsReservedBy(item.Handle,
              binding.PlayerHandle.Id, envelope.Epoch, envelope.ReceivedAt)) {
          return OwnerCommitResult.Rejected(ProtocolErrorCode.Forbidden,
            "Only the current owner may retire an item through the vanilla protocol.");
        }
        // Vanilla clients own pickup/destruction decisions and subsequently upload inventory
        // via packet 5. Packet 151 is not proof of a pickup and must not credit inventory twice.
        OwnerCommitResult committed = _composition.WorldItemStore.Despawn(item.Handle, item.Revision);
        if (committed.Status == OwnerCommitStatus.Applied) {
          ServerLog.Info("server.item.retired",
            $"epoch={envelope.Epoch.Value} slot={packet.ItemIndex} type={item.Item.Type}");
        }
        return committed;
      });
    return result.Status == OwnerCommitStatus.Applied ? null : result.Detail;
  }
}
