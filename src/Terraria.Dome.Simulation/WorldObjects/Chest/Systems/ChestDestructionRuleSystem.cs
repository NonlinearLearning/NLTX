using Terraria.Dome.Simulation.Items;

namespace Terraria.Dome.Simulation.WorldObjects.Chest.Systems;

public static class ChestDestructionRuleSystem
{
  public static bool CanDestroy(ChestComponent? chest)
  {
    if (chest is null)
    {
      return true;
    }

    for (int slot = 0; slot < ChestComponent.SlotCount; slot++)
    {
      ItemStack item = chest.GetSlot(slot);
      if (!item.IsEmpty && item.ItemType > 0 && item.Quantity > 0)
      {
        return false;
      }
    }

    return true;
  }
}
