namespace Terraria.Input.LockOn;

public readonly record struct LockOnPolicy
{
  public const int DefaultHoldLifetimeTicks = 40;
  public const float DefaultRangePixels = 2000f;

  public LockOnPolicy(
    float rangePixels = DefaultRangePixels,
    int holdLifetimeTicks = DefaultHoldLifetimeTicks)
  {
    if (rangePixels < 0f)
    {
      throw new ArgumentOutOfRangeException(nameof(rangePixels));
    }
    if (holdLifetimeTicks < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(holdLifetimeTicks));
    }

    RangePixels = rangePixels;
    HoldLifetimeTicks = holdLifetimeTicks;
  }

  public static LockOnPolicy Default => new(
    DefaultRangePixels,
    DefaultHoldLifetimeTicks);

  public int HoldLifetimeTicks { get; }

  public float RangePixels { get; }
}
