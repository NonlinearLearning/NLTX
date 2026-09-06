namespace Terraria.Items;

public struct ItemStackComponent
{
  public ItemStackComponent(
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

  public bool IsEmpty => Quantity <= 0;

  public bool IsFull =>
    !IsUnlimited &&
    MaximumQuantity > 0 &&
    Quantity >= MaximumQuantity;

  public bool IsValid =>
    Quantity >= 0 &&
    (IsUnlimited || (MaximumQuantity >= 0 && Quantity <= MaximumQuantity));
}
