namespace Terraria.Content.Items;

public readonly record struct ProjectileContentId
{
  public ProjectileContentId(int value)
  {
    if (value < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(value));
    }

    Value = value;
  }

  public static ProjectileContentId None => new(0);

  public bool HasValue => Value > 0;

  public int Value { get; }
}
