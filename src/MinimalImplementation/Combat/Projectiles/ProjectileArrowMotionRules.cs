using System.Numerics;

namespace MinimalImplementation.Combat.Projectiles;

/// <summary>Type-1 HandleMovement arithmetic; world effects and Kill are separate.</summary>
internal static class ProjectileArrowMotionRules {
  internal readonly record struct Result(Vector2 Position, Vector2 Velocity, bool TileHit);

  private const int _maximumSegments = 300;
  private const float _restoreVelocityTolerance = 0.0001f;

  internal static bool TryCalculate(
      ProjectileView source, Vector2 desiredVelocity, ProjectileLiquidState liquidState,
      IProjectileMotionSnapshot snapshot,
      out Result result) {
    result = default;
    Vector2 position = source.Position;
    var center = new Vector2(
        position.X + source.Width / 2f, position.Y + source.Height / 2f);
    Vector2 origin = center - new Vector2(source.Width, source.Height) * new Vector2(0.5f);
    Vector2 velocity = desiredVelocity;
    float remaining = liquidState.Wet ? 0f : velocity.Length();
    if (!IsFinite(position) || !IsFinite(origin) || !IsFinite(velocity) ||
        !float.IsFinite(remaining)) {
      return false;
    }

    bool tileHit = false;
    Vector2 wetVelocity = Vector2.Zero;
    int stepSize = Math.Clamp(Math.Min(source.Width, source.Height), 3, 16);
    if (liquidState.Wet) {
      velocity = snapshot.TileCollision(origin, velocity, source.Width, source.Height);
      if (!IsFinite(velocity)) {
        return false;
      }
      float factor = liquidState.ShimmerWet ? 0.375f : liquidState.HoneyWet ? 0.25f : 0.5f;
      wetVelocity = velocity * factor;
      if (velocity.X != desiredVelocity.X) {
        wetVelocity.X = velocity.X;
      }
      if (velocity.Y != desiredVelocity.Y) {
        wetVelocity.Y = velocity.Y;
      }
      // Source keeps this displacement even if SlopeCollision changes full velocity.
      if (!ResolveSlope()) {
        return false;
      }
    } else if (remaining > stepSize) {
      Vector2 probe = snapshot.TileCollision(origin, velocity, source.Width, source.Height);
      if (!IsFinite(probe)) {
        return false;
      }
      Vector2 direction = Vector2.Normalize(velocity);
      if (probe.Y == 0f) {
        // The source does not normalize again after suppressing the Y component.
        direction.Y = 0f;
      }
      Vector2 sum = Vector2.Zero;
      for (int segment = 0; segment < _maximumSegments && remaining > 0f; segment++) {
        float length = Math.Min(remaining, stepSize);
        remaining -= length;
        Vector2 resolved = snapshot.TileCollision(
            origin, direction * length, source.Width, source.Height);
        if (!IsFinite(resolved)) {
          return false;
        }
        origin += resolved;
        velocity = resolved;
        if (!ResolveSlope()) {
          return false;
        }
        // Source SwitchTiles effects at this segment boundary are not executed here.
        sum += velocity;
        if (!IsFinite(sum)) {
          return false;
        }
      }
      velocity = sum;
      if (Math.Abs(velocity.X - desiredVelocity.X) < _restoreVelocityTolerance) {
        velocity.X = desiredVelocity.X;
      }
      if (Math.Abs(velocity.Y - desiredVelocity.Y) < _restoreVelocityTolerance) {
        velocity.Y = desiredVelocity.Y;
      }
      if (!ResolveSlope()) {
        return false;
      }
    } else {
      velocity = snapshot.TileCollision(origin, velocity, source.Width, source.Height);
      if (!IsFinite(velocity) || !ResolveSlope()) {
        return false;
      }
    }

    tileHit |= desiredVelocity != velocity;
    // Wet survivors use reduced displacement without reducing persistent velocity.
    // Kill entry adds full velocity; the inactive object's later movement is separate.
    position += liquidState.Wet && !tileHit ? wetVelocity : velocity;
    if (!IsFinite(position)) {
      return false;
    }
    result = new Result(position, velocity, tileHit);
    return true;

    // Only local calculation state changes here. The explicit snapshot supplies all reads.
    bool ResolveSlope() {
      if (!IsFinite(origin) || !IsFinite(velocity)) {
        return false;
      }
      Vector4 slope = snapshot.SlopeCollision(origin, velocity, source.Width, source.Height);
      if (!float.IsFinite(slope.X) || !float.IsFinite(slope.Y) ||
          !float.IsFinite(slope.Z) || !float.IsFinite(slope.W)) {
        return false;
      }
      Vector2 offset = position - origin;
      var slopePosition = new Vector2(slope.X, slope.Y);
      var slopeVelocity = new Vector2(slope.Z, slope.W);
      tileHit |= origin != slopePosition || velocity != slopeVelocity;
      origin = slopePosition;
      position = origin + offset;
      velocity = slopeVelocity;
      return IsFinite(position);
    }
  }

  private static bool IsFinite(Vector2 value) {
    return float.IsFinite(value.X) && float.IsFinite(value.Y);
  }
}
