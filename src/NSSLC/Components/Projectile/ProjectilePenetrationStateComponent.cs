using System;

namespace Terraria.Projectile;

/// <summary>Tracks accepted target contacts for a projectile instance.</summary>
public readonly record struct ProjectilePenetrationStateComponent
{
  public ProjectilePenetrationStateComponent(int acceptedHitCount = 0)
  {
    if (acceptedHitCount < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(acceptedHitCount));
    }

    AcceptedHitCount = acceptedHitCount;
  }

  public int AcceptedHitCount { get; }

  public ProjectilePenetrationStateComponent RecordAcceptedHit()
  {
    if (AcceptedHitCount == int.MaxValue)
    {
      throw new InvalidOperationException("The projectile accepted hit count is exhausted.");
    }

    return new ProjectilePenetrationStateComponent(AcceptedHitCount + 1);
  }

  public bool HasReachedLimit(float effectiveLimit)
  {
    if (!float.IsFinite(effectiveLimit))
    {
      throw new ArgumentOutOfRangeException(nameof(effectiveLimit));
    }

    // A negative effective limit is the Attributes sentinel for unlimited penetration.
    return effectiveLimit >= 0f && AcceptedHitCount >= MathF.Ceiling(effectiveLimit);
  }
}
