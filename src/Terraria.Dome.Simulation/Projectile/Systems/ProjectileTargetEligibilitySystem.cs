using Terraria.Dome.Simulation.Components;

namespace Terraria.Dome.Simulation.Projectile.Systems;

public sealed class ProjectileTargetEligibilitySystem
{
  public bool CanDamageNpc(ProjectileDefinitionComponent definition)
  {
    return definition.Friendly;
  }
}
