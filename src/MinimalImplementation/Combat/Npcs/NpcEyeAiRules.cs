using System.Numerics;

namespace MinimalImplementation.Combat.Npcs;

/// <summary>AI_002_FloatingEye's ordinary type-2 branch at its default scale of one.</summary>
internal static class NpcEyeAiRules {
  internal static Vector2 Bounce(Vector2 velocity, NpcMotionState motion, bool right) {
    if (motion.CollideX) {
      velocity.X = motion.OldVelocity.X * -0.5f;
      if (!right && velocity.X > 0f && velocity.X < 2f) {
        velocity.X = 2f;
      }
      if (right && velocity.X < 0f && velocity.X > -2f) {
        velocity.X = -2f;
      }
    }
    if (motion.CollideY) {
      velocity.Y = motion.OldVelocity.Y * -0.5f;
      if (velocity.Y > 0f && velocity.Y < 1f) {
        velocity.Y = 1f;
      }
      if (velocity.Y < 0f && velocity.Y > -1f) {
        velocity.Y = -1f;
      }
    }
    return velocity;
  }

  internal static Vector2 Steer(Vector2 velocity, bool right, bool down, bool wet) {
    velocity.X = Accelerate(velocity.X, right, 4f, 0.1f, 0.1f, 0.05f);
    velocity.Y = Accelerate(velocity.Y, down, 1.5f, 0.04f, 0.05f, 0.03f);
    if (wet) {
      if (velocity.Y > 0f) {
        velocity.Y *= 0.95f;
      }
      velocity.Y = Math.Max(-4f, velocity.Y - 0.5f);
    }
    return velocity;
  }

  private static float Accelerate(
      float velocity, bool positive, float maximum, float acceleration,
      float reverseAcceleration, float oppositeAdjustment) {
    float signed = positive ? velocity : -velocity;
    if (signed < maximum) {
      signed += acceleration;
      if (signed < -maximum) {
        signed += reverseAcceleration;
      } else if (signed < 0f) {
        signed -= oppositeAdjustment;
      }
      signed = Math.Min(signed, maximum);
    }
    return positive ? signed : -signed;
  }
}
