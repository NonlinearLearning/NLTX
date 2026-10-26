using System;

namespace Terraria.Projectile;

/// <summary>Runtime count for the optional bounce capability.</summary>
public readonly record struct ProjectileBounceStateComponent
{
  public ProjectileBounceStateComponent(int bounceCount = 0)
  {
    if (bounceCount < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(bounceCount));
    }

    BounceCount = bounceCount;
  }

  public int BounceCount { get; }

  public bool CanBounce(ProjectileBounceCapabilityComponent capability)
  {
    return capability.Enabled && BounceCount < capability.MaximumBounces;
  }

  public ProjectileBounceStateComponent AfterBounce()
  {
    return new ProjectileBounceStateComponent(BounceCount + 1);
  }
}
