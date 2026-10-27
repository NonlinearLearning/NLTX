namespace Terraria.Communication;

public readonly record struct SessionEpoch(ulong Value)
{
  public bool IsValid => Value != 0;

  public static SessionEpoch Create(ulong value)
  {
    if (value == 0)
    {
      throw new ArgumentOutOfRangeException(nameof(value), "A session epoch must be non-zero.");
    }

    return new SessionEpoch(value);
  }
}
