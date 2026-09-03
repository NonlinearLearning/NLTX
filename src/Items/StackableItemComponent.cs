namespace Terraria.Items;

public struct StackableItemComponent
{
  public StackableItemComponent(int quantity, int maximumQuantity)
  {
    Quantity = quantity;
    MaximumQuantity = maximumQuantity;
  }

  public int Quantity;
  public int MaximumQuantity;
}
