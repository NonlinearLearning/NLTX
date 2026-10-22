using System.Numerics;

namespace MinimalImplementation.Combat.Projectiles;

/// <summary>
/// Required world and center-tile facts for ordinary type-1 Arrow wind physics.
/// Capture all fields from the motion snapshot; a missing tile means a real null tile,
/// not missing coverage. Type 1 has no WindPhysicsImmunity override in the source.
/// </summary>
public sealed record ProjectileArrowWindFacts(
    bool WindPhysicsEnabled,
    double WorldSurfaceTiles,
    float WindSpeedCurrent,
    float WindPhysicsStrength,
    bool CenterTileExists,
    ushort CenterTileWall) {
  private const float _maximumHorizontalSpeed = 16f;
  private const float _windSpeedThresholdMultiplier = 180f;

  internal bool TryApply(float centerY, Vector2 velocity, out Vector2 result) {
    result = velocity;
    double surfaceY = WorldSurfaceTiles * 16d;
    float acceleration = WindSpeedCurrent * WindPhysicsStrength;
    float speedThreshold = Math.Abs(acceleration) * _windSpeedThresholdMultiplier;
    if (!float.IsFinite(centerY) || !float.IsFinite(velocity.X) ||
        !float.IsFinite(velocity.Y) || !double.IsFinite(surfaceY) ||
        !float.IsFinite(WindSpeedCurrent) || !float.IsFinite(WindPhysicsStrength) ||
        !float.IsFinite(acceleration) || !float.IsFinite(speedThreshold)) {
      return false;
    }

    if (WindPhysicsEnabled && (double)centerY < surfaceY && CenterTileExists &&
        CenterTileWall == 0 &&
        ((velocity.X > 0f && WindSpeedCurrent < 0f) ||
        (velocity.X < 0f && WindSpeedCurrent > 0f) ||
        Math.Abs(velocity.X) < speedThreshold) &&
        Math.Abs(velocity.X) < _maximumHorizontalSpeed) {
      result.X += acceleration;
      // Source Update discards MathHelper.Clamp's return value, so do not clamp X here.
    }
    return float.IsFinite(result.X);
  }
}
