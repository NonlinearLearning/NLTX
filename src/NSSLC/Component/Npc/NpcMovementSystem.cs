using System.Numerics;
using Terraria.SpatialSimulation.Components;

namespace Terraria.Npc;

public sealed class NpcMovementSystem
{
  private const int NpcTypeWithExtraVerticalKnockback = 185;

  public NpcKnockbackResult CalculateKnockback(in NpcKnockbackRequest request)
  {
    Vector2 velocity = request.CurrentVelocity;
    if (request.Knockback <= 0.0f || request.KnockbackResistance <= 0.0f)
    {
      return new NpcKnockbackResult(
        IsEligible: false,
        VelocityBefore: velocity,
        VelocityAfter: velocity);
    }

    float knockback = ResolveKnockbackMagnitude(request);
    int damageThreshold = unchecked(
      request.ResolvedDamage * (request.ExpertMode ? 15 : 10));
    if (damageThreshold > request.LifeMaximum)
    {
      velocity = ApplyHighDamageKnockback(request, velocity, knockback);
    }
    else
    {
      float verticalScale = request.NoGravity ? 0.5f : 0.75f;
      velocity.Y = -knockback * verticalScale * request.KnockbackResistance;
      velocity.X = knockback * request.HitDirection * request.KnockbackResistance;
    }

    return new NpcKnockbackResult(
      IsEligible: true,
      VelocityBefore: request.CurrentVelocity,
      VelocityAfter: velocity);
  }

  public NpcKnockbackResult ApplyKnockback(
    NpcTypeId npcType,
    NpcKnockbackInput input,
    int resolvedDamage,
    int lifeMaximum,
    bool critical,
    ref MovementStateComponent movementState)
  {
    var request = new NpcKnockbackRequest(
      npcType,
      movementState.Velocity,
      input.Knockback,
      input.HitDirection,
      input.KnockbackResistance,
      input.OnFire2,
      critical,
      resolvedDamage,
      lifeMaximum,
      input.ExpertMode,
      input.NoGravity);
    NpcKnockbackResult result = CalculateKnockback(in request);
    if (result.IsEligible)
    {
      movementState.Velocity = result.VelocityAfter;
    }

    return result;
  }

  private static float ResolveKnockbackMagnitude(NpcKnockbackRequest request)
  {
    float knockback = request.Knockback * request.KnockbackResistance;
    if (request.OnFire2)
    {
      knockback *= 1.1f;
    }

    if (knockback > 8.0f)
    {
      knockback = 8.0f + (knockback - 8.0f) * 0.9f;
    }

    if (knockback > 10.0f)
    {
      knockback = 10.0f + (knockback - 10.0f) * 0.8f;
    }

    if (knockback > 12.0f)
    {
      knockback = 12.0f + (knockback - 12.0f) * 0.7f;
    }

    if (knockback > 14.0f)
    {
      knockback = 14.0f + (knockback - 14.0f) * 0.6f;
    }

    if (knockback > 16.0f)
    {
      knockback = 16.0f;
    }

    return request.Critical ? knockback * 1.4f : knockback;
  }

  private static Vector2 ApplyHighDamageKnockback(
    NpcKnockbackRequest request,
    Vector2 velocity,
    float knockback)
  {
    if (request.HitDirection < 0 && velocity.X > -knockback)
    {
      if (velocity.X > 0.0f)
      {
        velocity.X -= knockback;
      }

      velocity.X -= knockback;
      if (velocity.X < -knockback)
      {
        velocity.X = -knockback;
      }
    }
    else if (request.HitDirection > 0 && velocity.X < knockback)
    {
      if (velocity.X < 0.0f)
      {
        velocity.X += knockback;
      }

      velocity.X += knockback;
      if (velocity.X > knockback)
      {
        velocity.X = knockback;
      }
    }

    if (request.NpcType.Value == NpcTypeWithExtraVerticalKnockback)
    {
      knockback *= 1.5f;
    }

    float verticalImpulse = request.NoGravity
      ? knockback * -0.5f
      : knockback * -0.75f;
    if (velocity.Y > verticalImpulse)
    {
      velocity.Y += verticalImpulse;
      if (velocity.Y < verticalImpulse)
      {
        velocity.Y = verticalImpulse;
      }
    }

    return velocity;
  }
}
