namespace Terraria.Content.Items;

public readonly record struct InventorySlotIndex
{
  public InventorySlotIndex(int value)
  {
    if (value < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(value));
    }

    Value = value;
  }

  public bool IsValid => Value >= 0;

  public int Value { get; }
}
