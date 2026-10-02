namespace Terraria.Content.Items;

public readonly record struct ItemNetworkId
{
  public ItemNetworkId(int value)
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
