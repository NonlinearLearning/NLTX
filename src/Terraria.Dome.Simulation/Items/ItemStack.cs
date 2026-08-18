namespace Terraria.Dome.Simulation.Items;

public readonly record struct ItemStack(ushort ItemType, int Quantity)
{
  public bool IsEmpty => ItemType == 0 || Quantity <= 0;

  public static ItemStack Empty => default;

  public ItemStack WithQuantity(int quantity)
  {
    return quantity <= 0 ? Empty : new ItemStack(ItemType, quantity);
  }
}
