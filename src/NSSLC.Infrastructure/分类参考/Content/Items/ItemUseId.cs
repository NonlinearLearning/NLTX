namespace Terraria.Content.Items;

public readonly record struct ItemUseId
{
  public ItemUseId(long value)
  {
    if (value <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(value));
    }

    Value = value;
  }

  public bool IsValid => Value > 0;

  public long Value { get; }
}
