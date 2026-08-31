using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Items;
using Terraria.Dome.Simulation.Items.Components;
using Terraria.Dome.Simulation.Items.Definitions;

namespace Terraria.Dome.Simulation.Items.Snapshots;

public sealed class EquipmentSnapshot
{
  public EquipmentSnapshot(
    PlayerHandle player,
    IReadOnlyList<ItemEquipmentStateComponent> slots,
    long revision)
  {
    ArgumentNullException.ThrowIfNull(slots);
    if (!player.IsValid || revision < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(player));
    }

    Player = player;
    for (int index = 0; index < slots.Count; index++)
    {
      ItemEquipmentStateComponent state = slots[index];
      if (!Enum.IsDefined(state.Slot) || state.Slot == ItemEquipmentSlot.None ||
          state.SourceSlot < 0 || state.SourceSlot >= InventoryComponent.SlotCount)
      {
        throw new ArgumentOutOfRangeException(nameof(slots));
      }
    }

    Slots = Array.AsReadOnly([.. slots]);
    Revision = revision;
  }

  public PlayerHandle Player { get; }

  public IReadOnlyList<ItemEquipmentStateComponent> Slots { get; }

  public long Revision { get; }
}
