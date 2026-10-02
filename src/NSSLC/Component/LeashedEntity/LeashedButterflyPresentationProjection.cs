namespace Terraria.LeashedEntity;

/// <summary>
/// Projects presentation-only butterfly state without mutating gameplay authority.
/// </summary>
public static class LeashedButterflyPresentationProjection
{
  public static LeashedButterflyPresentationSnapshot Project(
    LeashedButterflyVariantComponent variant,
    float opacity,
    bool isFading)
  {
    if (float.IsNaN(opacity) || float.IsInfinity(opacity))
    {
      throw new ArgumentOutOfRangeException(nameof(opacity));
    }

    return new(
      variant.Variant,
      Math.Clamp(opacity, 0f, 1f),
      isFading);
  }
}

