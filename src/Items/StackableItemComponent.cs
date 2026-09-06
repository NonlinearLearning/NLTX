namespace Terraria.Items;

public struct StackableItemComponent
{
  public StackableItemComponent(
    int quantity,
    int maximumQuantity,
    ulong stackKey = 0,
    bool isUnlimited = false)
  {
    Quantity = quantity;
    MaximumQuantity = maximumQuantity;
    StackKey = stackKey;
    IsUnlimited = isUnlimited;
  }

  public int Quantity;
  public int MaximumQuantity;
  public ulong StackKey;
  public bool IsUnlimited;

  public int RemainingCapacity => Math.Max(0, MaximumQuantity - Quantity);
  public bool IsEmpty => Quantity <= 0;
  public bool IsFull => Quantity >= MaximumQuantity;
}
