using Terraria.Dome.Simulation.Components;

namespace Terraria.Dome.Simulation.Projectile.Systems;

public static class ProjectileDamageEligibilityPolicy
{
  public const int LocalAiGatedProjectileType = 1091;

  public static bool CanDealDamage(
    ProjectileDefinitionComponent definition,
    ProjectileBehaviorComponent behavior)
  {
    if (definition.ProjectileType != LocalAiGatedProjectileType)
    {
      return true;
    }

    return float.IsFinite(behavior.State.LocalAi0) && behavior.State.LocalAi0 > 0.0f;
  }
}
