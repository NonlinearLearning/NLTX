using System;
using System.Numerics;

namespace Terraria.Unshared.Projectiles;

/// <summary>
/// AI output consumed by projectile simulation; it is not a direct component write.
/// </summary>
public readonly record struct ProjectileAiIntent
{
  public ProjectileAiIntent(
      ProjectileAiIntentKind kind,
      Vector2 desiredVelocity,
      int targetRevision = 0)
  {
    if (!Enum.IsDefined(kind))
    {
      throw new ArgumentOutOfRangeException(nameof(kind));
    }

    if (!float.IsFinite(desiredVelocity.X) || !float.IsFinite(desiredVelocity.Y))
    {
      throw new ArgumentOutOfRangeException(nameof(desiredVelocity));
    }

    if (targetRevision < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(targetRevision));
    }

    Kind = kind;
    DesiredVelocity = desiredVelocity;
    TargetRevision = targetRevision;
  }

  public ProjectileAiIntentKind Kind { get; }

  public Vector2 DesiredVelocity { get; }

  public int TargetRevision { get; }

  public static ProjectileAiIntent Maintain(Vector2 velocity)
  {
    return new ProjectileAiIntent(ProjectileAiIntentKind.MaintainVelocity, velocity);
  }
}

public enum ProjectileAiIntentKind : byte
{
  MaintainVelocity = 0,
  AdjustVelocity = 1,
  RequestTermination = 2
}
