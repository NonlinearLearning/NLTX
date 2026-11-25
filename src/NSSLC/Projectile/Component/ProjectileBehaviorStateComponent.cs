using System;

namespace Terraria.Unshared.Projectiles;

/// <summary>Small cross-behavior state visible to the projectile AI adapter.</summary>
public readonly record struct ProjectileBehaviorStateComponent
{
  public ProjectileBehaviorStateComponent(int phase, long revision = 0)
  {
    if (phase < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(phase));
    }

    if (revision < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(revision));
    }

    Phase = phase;
    Revision = revision;
  }

  public int Phase { get; }

  public long Revision { get; }

  public ProjectileBehaviorStateComponent Advance(int phase)
  {
    return new ProjectileBehaviorStateComponent(phase, Revision + 1);
  }
}
