using System;
using Terraria.Dome.Simulation.Inventory.Components;
using Terraria.Dome.Simulation.Items;

namespace Terraria.Dome.Simulation.Inventory.Systems;

public sealed class ItemSelectionSystem
{
  public void Apply(InventoryComponent inventory, ref SelectedItemComponent selection, int slot)
  {
    ArgumentNullException.ThrowIfNull(inventory);
    if (selection.Revision < 0 || selection.Revision == int.MaxValue)
    {
      return;
    }

    _ = inventory.GetSlot(slot);
    inventory.SetSelectedSlot(slot);
    selection.SelectedSlot = slot;
    selection.Revision++;
  }
}
