using System;
using System.Collections.Generic;
using Terraria.Dome.Protocol.V1456.Packets;
using Terraria.Dome.Simulation.Items;
using Terraria.Dome.Simulation.Items.Components;
using Terraria.Dome.Simulation.Items.Definitions;
using Terraria.Dome.Simulation.Items.Snapshots;

namespace Terraria.Dome.Server.Replication;

public sealed class EquipmentReplicationAssembler
{
  private const int ArmorSlotBase = 59;
  private const int VanityArmorSlotOffset = 10;

  public IReadOnlyList<byte[]> CollectFrames(
    byte playerSlot,
    EquipmentSnapshot equipment,
    InventorySnapshot inventory)
  {
    ArgumentNullException.ThrowIfNull(equipment);
    ArgumentNullException.ThrowIfNull(inventory);
    if (equipment.Player != inventory.Player)
    {
      throw new ArgumentException(
        "Equipment and inventory snapshots must belong to the same player.");
    }

    List<byte[]> frames = new(equipment.Slots.Count);
    for (int index = 0; index < equipment.Slots.Count; index++)
    {
      ItemEquipmentStateComponent state = equipment.Slots[index];
      if (state.SourceSlot < 0 || state.SourceSlot >= inventory.Slots.Count ||
          !TryGetArmorSlotOffset(state.Slot, out int offset))
      {
        throw new ArgumentOutOfRangeException(nameof(equipment));
      }

      ItemInstanceSnapshot instance = inventory.Slots[state.SourceSlot];
      if (instance.Stack.IsEmpty || instance.State.PrefixId > byte.MaxValue)
      {
        throw new InvalidOperationException(
          "An equipment source item cannot be represented by the V1456 equipment packet.");
      }

      int slotId = ArmorSlotBase + offset;
      if (state.IsVanity)
      {
        slotId += VanityArmorSlotOffset;
      }

      frames.Add(TerrariaPacketCodec.Encode(new PlayerEquipmentPacket(
        playerSlot,
        slotId,
        instance.Stack.Quantity,
        (byte)instance.State.PrefixId,
        instance.Stack.ItemType,
        instance.State.IsFavorited,
        IsNewAndShiny: false)));
    }

    return frames;
  }

  public IReadOnlyList<EquipmentSnapshot> Project(
    IReadOnlyList<EquipmentSnapshot> snapshots)
  {
    ArgumentNullException.ThrowIfNull(snapshots);
    return Array.AsReadOnly([.. snapshots]);
  }

  private static bool TryGetArmorSlotOffset(ItemEquipmentSlot slot, out int offset)
  {
    switch (slot)
    {
      case ItemEquipmentSlot.Head:
        offset = 0;
        return true;
      case ItemEquipmentSlot.Body:
        offset = 1;
        return true;
      case ItemEquipmentSlot.Legs:
        offset = 2;
        return true;
      case ItemEquipmentSlot.Accessory:
        offset = 3;
        return true;
      case ItemEquipmentSlot.Wings:
        offset = 4;
        return true;
      case ItemEquipmentSlot.Shield:
        offset = 5;
        return true;
      case ItemEquipmentSlot.Vanity:
        offset = 6;
        return true;
      default:
        offset = 0;
        return false;
    }
  }
}
