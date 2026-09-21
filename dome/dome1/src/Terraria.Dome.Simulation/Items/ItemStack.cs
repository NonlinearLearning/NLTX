namespace Terraria.Dome.Simulation.Items;

public readonly record struct ItemStack(ushort ItemType, int Quantity, int Prefix = 0)
{
  public bool IsEmpty => ItemType == 0 || Quantity <= 0;

  public bool IsAir => ItemType == 0 || Quantity <= 0;

  public bool IsActive => ItemType != 0;

  public ushort Type => ItemType;

  public int Stack => Quantity;

  public int PrefixId => Prefix;

  public static ItemStack Empty => default;

  public ItemStack WithQuantity(int quantity)
  {
    return quantity <= 0 ? Empty : new ItemStack(ItemType, quantity, Prefix);
  }
}
