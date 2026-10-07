namespace Terraria.Projectile;

public enum ProjectileCollidingGeometryResult
{
  /// <summary>The candidate filters rejected the target before geometry ran.</summary>
  NotEvaluated,

  /// <summary>A mapped source geometry branch intersects the target.</summary>
  Collision,

  /// <summary>A mapped source geometry branch does not intersect the target.</summary>
  NoCollision,

  /// <summary>Required source geometry or higher-priority dispatch is not mapped.</summary>
  Unsupported,
}
