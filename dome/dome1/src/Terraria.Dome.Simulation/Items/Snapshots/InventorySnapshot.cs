using System;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.Items.Snapshots;

public sealed class InventorySnapshot
{
  public InventorySnapshot(
    PlayerHandle player,
    IReadOnlyList<ItemInstanceSnapshot> slots,
    int selectedSlot,
    long revision)
  {
    ArgumentNullException.ThrowIfNull(slots);
    if (!player.IsValid || selectedSlot < 0 ||
        selectedSlot >= Terraria.Dome.Simulation.Items.InventoryComponent.HotbarSlotCount ||
        revision < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(selectedSlot));
    }

    Player = player;
    for (int index = 0; index < slots.Count; index++)
    {
      if (slots[index] is null)
      {
        throw new ArgumentException(
          "Inventory snapshots cannot contain null item instances.",
          nameof(slots));
      }
    }

    Slots = Array.AsReadOnly([.. slots]);
    SelectedSlot = selectedSlot;
    Revision = revision;
  }

  public PlayerHandle Player { get; }

  public IReadOnlyList<ItemInstanceSnapshot> Slots { get; }

  public int SelectedSlot { get; }

  public long Revision { get; }
}
