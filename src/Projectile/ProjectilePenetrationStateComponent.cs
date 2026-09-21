using System;

namespace Terraria.Projectile;

public struct ProjectilePenetrationStateComponent
{
  public ProjectilePenetrationStateComponent(
    int remainingHits = 1,
    int maximumHits = 1,
    int hitCount = 0,
    bool stopsDealingDamageWhenDepleted = false)
  {
    ValidatePenetrationValue(remainingHits, nameof(remainingHits));
    ValidatePenetrationValue(maximumHits, nameof(maximumHits));
    if (hitCount < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(hitCount));
    }

    RemainingHits = remainingHits;
    MaximumHits = maximumHits;
    HitCount = hitCount;
    StopsDealingDamageWhenDepleted = stopsDealingDamageWhenDepleted;
  }

  public int RemainingHits;
  public int MaximumHits;
  public int HitCount;
  public bool StopsDealingDamageWhenDepleted;

  public bool HasRemainingHits => RemainingHits != 0;

  public bool IsUnlimited => RemainingHits == -1;

  private static void ValidatePenetrationValue(int value, string parameterName)
  {
    if (value < -1)
    {
      throw new ArgumentOutOfRangeException(parameterName);
    }
  }
}
