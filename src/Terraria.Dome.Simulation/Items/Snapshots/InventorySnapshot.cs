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
    if (selectedSlot < 0 ||
        selectedSlot >= Terraria.Dome.Simulation.Items.InventoryComponent.HotbarSlotCount ||
        revision < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(selectedSlot));
    }

    Player = player;
    Slots = Array.AsReadOnly([.. slots]);
    SelectedSlot = selectedSlot;
    Revision = revision;
  }

  public PlayerHandle Player { get; }

  public IReadOnlyList<ItemInstanceSnapshot> Slots { get; }

  public int SelectedSlot { get; }

  public long Revision { get; }
}
