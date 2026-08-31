using Terraria.Dome.Simulation.Components;
using Terraria.Dome.Simulation.Npc.Components;

namespace Terraria.Dome.Simulation.Projectile.Systems;

public static class ProjectileReflectionEligibilityPolicy
{
  public static bool CanBeReflected(
    ProjectileDefinitionComponent definition,
    bool isActive)
  {
    if (!isActive || !definition.Friendly || definition.Hostile || definition.DefaultDamage <= 0)
    {
      return false;
    }

    return definition.ProjectileType is 728 or 955 ||
      definition.BehaviorId is 1 or 2 or 8 or 21 or 24 or 28 or 29 or 131;
  }

  public static bool CanBeReflectedByNpc(
    ProjectileDefinitionComponent definition,
    int runtimeDamage,
    NpcBehaviorStateComponent npcBehavior,
    bool isActive)
  {
    if (!isActive || !npcBehavior.ReflectsProjectiles || !definition.Friendly ||
        definition.Hostile || runtimeDamage <= 0)
    {
      return false;
    }

    return definition.ProjectileType is 728 or 955 ||
      definition.LegacyAiStyle is 1 or 2 or 8 or 21 or 24 or 28 or 29 or 131;
  }
}
