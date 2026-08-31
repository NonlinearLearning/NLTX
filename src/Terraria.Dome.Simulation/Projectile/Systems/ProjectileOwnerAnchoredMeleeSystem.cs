using System;
using Terraria.Dome.Simulation.Components;
using Terraria.Dome.Simulation.Projectile.Behaviors;

namespace Terraria.Dome.Simulation.Projectile.Systems;

public sealed class ProjectileOwnerAnchoredMeleeSystem
{
  public bool TryAdvance(
    ProjectileBehaviorComponent behavior,
    ref TransformComponent projectileTransform,
    VelocityComponent projectileVelocity,
    TransformComponent ownerTransform)
  {
    if (behavior.BehaviorId != LegacyAiStyle190ProjectileBehavior.Id)
    {
      return true;
    }

    if (!float.IsFinite(projectileVelocity.X) || !float.IsFinite(projectileVelocity.Y) ||
        !float.IsFinite(ownerTransform.X) || !float.IsFinite(ownerTransform.Y) ||
        behavior.State.Secondary <= 0.0f)
    {
      return false;
    }

    projectileTransform.X = ownerTransform.X - projectileVelocity.X;
    projectileTransform.Y = ownerTransform.Y - projectileVelocity.Y;
    return behavior.State.Phase < behavior.State.Secondary;
  }

  public bool TryAdvance(
    ProjectileBehaviorComponent behavior,
    ref TransformComponent projectileTransform,
    ref VelocityComponent projectileVelocity,
    TransformComponent ownerTransform,
    VelocityComponent ownerVelocity)
  {
    if (behavior.BehaviorId != LegacyType607ProjectileBehavior.Id)
    {
      return true;
    }

    if (!float.IsFinite(behavior.State.Secondary) ||
        !float.IsFinite(projectileVelocity.X) || !float.IsFinite(projectileVelocity.Y) ||
        !float.IsFinite(ownerTransform.X) || !float.IsFinite(ownerTransform.Y) ||
        !float.IsFinite(ownerVelocity.X) || !float.IsFinite(ownerVelocity.Y))
    {
      return false;
    }

    float nextVelocityX = MathF.Cos(behavior.State.Secondary) * 100.0f - ownerVelocity.X;
    float nextVelocityY = MathF.Sin(behavior.State.Secondary) * 100.0f - ownerVelocity.Y;
    if (!float.IsFinite(nextVelocityX) || !float.IsFinite(nextVelocityY))
    {
      return false;
    }

    projectileVelocity.X = nextVelocityX;
    projectileVelocity.Y = nextVelocityY;
    projectileTransform.X = ownerTransform.X - nextVelocityX;
    projectileTransform.Y = ownerTransform.Y - nextVelocityY;
    return true;
  }
}
