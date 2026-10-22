using System.Numerics;

namespace MinimalImplementation.Combat.Npcs;

/// <summary>Ordinary StrikeNPC math; type-specific transformations are not dispatched here.</summary>
internal static class NpcDamageRules {
  internal static int CalculateDamage(int damage, int defense, bool critical) {
    double amount = Math.Max(1d, damage - defense * 0.5d);
    if (critical) {
      amount *= 2d;
    }
    return (int)Math.Min(int.MaxValue, amount);
  }

  internal static Vector2 Knockback(
      Vector2 velocity, float resist, int maximumLife, int damage, NpcDamageRequest hit) {
    if (hit.Knockback <= 0f || resist <= 0f) {
      return velocity;
    }
    float power = hit.Knockback * resist;
    if (hit.ShadowFlame) {
      power *= 1.1f;
    }
    if (power > 8f) {
      power = 8f + (power - 8f) * 0.9f;
    }
    if (power > 10f) {
      power = 10f + (power - 10f) * 0.8f;
    }
    if (power > 12f) {
      power = 12f + (power - 12f) * 0.7f;
    }
    if (power > 14f) {
      power = 14f + (power - 14f) * 0.6f;
    }
    power = Math.Min(power, 16f);
    if (hit.Critical) {
      power *= 1.4f;
    }
    if ((long)damage * (hit.ExpertMode ? 15 : 10) > maximumLife) {
      if (hit.HitDirection < 0 && velocity.X > -power) {
        if (velocity.X > 0f) {
          velocity.X -= power;
        }
        velocity.X = Math.Max(-power, velocity.X - power);
      } else if (hit.HitDirection > 0 && velocity.X < power) {
        if (velocity.X < 0f) {
          velocity.X += power;
        }
        velocity.X = Math.Min(power, velocity.X + power);
      }
      float vertical = -power * (hit.NoGravity ? 0.5f : 0.75f);
      if (velocity.Y > vertical) {
        velocity.Y = Math.Max(vertical, velocity.Y + vertical);
      }
    } else {
      velocity.Y = -power * (hit.NoGravity ? 0.5f : 0.75f) * resist;
      velocity.X = power * hit.HitDirection * resist;
    }
    return velocity;
  }
}
