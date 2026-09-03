using Terraria.Dome.Simulation.Items;

namespace Terraria.Dome.Simulation.Player.Components;

public struct PlayerTrashSlotComponent
{
  public ItemStack Item;

  public void Set(ItemStack item)
  {
    Item = item.IsEmpty ? ItemStack.Empty : item;
  }

  public ItemStack Take()
  {
    ItemStack item = Item;
    Item = ItemStack.Empty;
    return item;
  }
}
