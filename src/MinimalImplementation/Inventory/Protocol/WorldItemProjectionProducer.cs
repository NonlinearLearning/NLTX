using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Inventory.State;
using MinimalImplementation.Server.Protocol;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace MinimalImplementation.Server.Inventory.Protocol;

/// <summary>Encodes committed values and binds all their packets to one transport settlement.</summary>
public sealed class WorldItemProjectionProducer
{
  private readonly WorldItemProjectionJournal _journal;
  private readonly Version4WireProfile _wire;
  private readonly Func<EntityUuid, SessionEpoch, byte?> _resolvePlayerSlot;

  public WorldItemProjectionProducer(
    WorldItemProjectionJournal journal, Version4WireProfile wire,
    Func<EntityUuid, SessionEpoch, byte?> resolvePlayerSlot)
  {
    ArgumentNullException.ThrowIfNull(journal);
    ArgumentNullException.ThrowIfNull(wire);
    ArgumentNullException.ThrowIfNull(resolvePlayerSlot);
    _journal = journal;
    _wire = wire;
    _resolvePlayerSlot = resolvePlayerSlot;
  }

  public WorldItemProjectionProducer(
    WorldItemProjectionJournal journal, Version4WireProfile wire,
    ActiveSessionPlayerSlotResolver playerSlotResolver)
    : this(journal, wire, playerSlotResolver.Resolve)
  {
  }

  /// <summary>Missing session mappings leave the value pending. Never reads fresh item state.</summary>
  public bool TryPrepare(
    Guid batchId, WorldItemProjection value, out IOutboundDeliverySettlement? delivery)
  {
    ArgumentNullException.ThrowIfNull(value);
    delivery = null;
    var packets = new List<object>();
    switch (value.Kind)
    {
      case WorldItemProjectionKind.Shimmer when value.Item is not null:
      case WorldItemProjectionKind.Item when value.Item is not null:
        WorldItemSnapshot item = value.Item;
        if (value.Kind == WorldItemProjectionKind.Shimmer)
        {
          // NetMessage146 truncates the captured Center coordinates before encoding Singles.
          packets.Add(new ShimmerActionsPacket {
            Action = 0,
            Position = new PacketVector2((int)(item.Position.X + 8f), (int)(item.Position.Y + 8f))
          });
        }
        packets.Add(item.Item.IsEmpty
          ? new SyncItemDespawnPacket { ItemIndex = checked((short)item.Handle.SlotId) }
          : CreateItemPacket(item));
        break;
      case WorldItemProjectionKind.OwnershipReleaseRequest when value.Reservation is not null:
        packets.Add(new ReleaseItemOwnershipPacket {
          ItemIndex = checked((short)value.Handle.SlotId)
        });
        break;
      case WorldItemProjectionKind.Ownership:
        byte ownerSlot = byte.MaxValue;
        if (value.Reservation is not null)
        {
          byte? slot = value.Reservation.WirePlayerSlot;
          if (!slot.HasValue || slot.Value == byte.MaxValue)
          {
            return false;
          }

          ownerSlot = slot.Value;
        }

        packets.Add(new ItemOwnerPacket {
          ItemIndex = checked((short)value.Handle.SlotId), ReservedForPlayer = ownerSlot,
          PositionX = value.Position.X, PositionY = value.Position.Y
        });
        break;
      case WorldItemProjectionKind.Inventory when value.Inventory is not null:
        SessionEpoch ownerEpoch = value.InventoryOwnerEpoch ?? value.Target;
        byte? playerSlot = _resolvePlayerSlot.Invoke(value.Inventory.PlayerId, ownerEpoch);
        if (!playerSlot.HasValue)
        {
          return false;
        }

        foreach (InventorySlotSnapshot slot in value.Inventory.Slots)
        {
          // PlayerItemSlotID.CanRelay: private banks 1/2/3 and trash are not relayed.
          if (value.Target != ownerEpoch
            && slot.SlotId >= 99 && slot.SlotId < InventorySlotLayout.VoidStart)
          {
            continue;
          }

          packets.Add(new SyncEquipmentPacket {
            Player = playerSlot.Value, Slot = slot.SlotId,
            ItemType = checked((short)slot.Item.Type), Stack = checked((short)slot.Item.Stack),
            Prefix = slot.Item.Prefix, Favorited = slot.Item.Favorited
          });
        }

        break;
      case WorldItemProjectionKind.Player when value.PlayerProjection is not null:
        packets.Add(value.PlayerProjection.Packet);
        break;
      default:
        return false;
    }

    if (packets.Count == 0)
    {
      return false;
    }

    var intents = packets.Select(packet => _wire.CreateOutboundIntent(
      packet, [value.Target], null, value.Revision)).ToArray();
    return _journal.TryBindDelivery(batchId, value, intents, out delivery);
  }

  /// <summary>Current client carries both optional extensions in packet 21's flag bits.</summary>
  public static SyncItemPacket CreateItemPacket(WorldItemSnapshot item) {
    bool shimmer = item.IsShimmering || item.ShimmerTime > 0;
    bool enemyDelay = item.EnemyPickupDelay > 0;
    return new SyncItemPacket {
      ItemIndex = checked((short)item.Handle.SlotId),
      PositionX = item.Position.X, PositionY = item.Position.Y,
      VelocityX = item.Velocity.X, VelocityY = item.Velocity.Y,
      Stack = checked((short)item.Item.Stack), Prefix = item.Item.Prefix,
      ItemType = checked((short)item.Item.Type),
      StateFlags = (byte)((shimmer ? 4 : 0) | (enemyDelay ? 8 : 0)),
      Shimmered = shimmer ? item.IsShimmering : null,
      ShimmerTime = shimmer ? item.ShimmerTime : null,
      EnemyGrabDelayTime = enemyDelay
        ? (byte)Math.Min(item.EnemyPickupDelay, byte.MaxValue) : null
    };
  }
}
