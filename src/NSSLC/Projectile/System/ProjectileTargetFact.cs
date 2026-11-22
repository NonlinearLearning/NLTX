using System;
using System.Numerics;

namespace Terraria.Projectile;

/// <summary>Detached target observation supplied to projectile AI.</summary>
public readonly record struct ProjectileTargetFact
{
  public ProjectileTargetFact(
      Vector2 position,
      int revision,
      bool isVisible,
      bool isAlive)
  {
    if (!float.IsFinite(position.X) || !float.IsFinite(position.Y))
    {
      throw new ArgumentOutOfRangeException(nameof(position));
    }

    if (revision < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(revision));
    }

    Position = position;
    Revision = revision;
    IsVisible = isVisible;
    IsAlive = isAlive;
  }

  public Vector2 Position { get; }

  public int Revision { get; }

  public bool IsVisible { get; }

  public bool IsAlive { get; }

  public bool IsUsable => IsVisible && IsAlive;
}
