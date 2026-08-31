using Terraria.Dome.Simulation.Components;

namespace Terraria.Dome.Simulation.Projectile.Systems;

public static class ProjectileDirectionSystem
{
  public static void Update(
    ref ProjectileDirectionComponent direction,
    VelocityComponent velocity,
    ProjectileDefinitionComponent definition,
    ProjectileBehaviorComponent behavior)
  {
    if (!AutomaticallyChangesDirection(definition, behavior) || velocity.X == 0.0f)
    {
      return;
    }

    direction.Horizontal = velocity.X < 0.0f ? -1 : 1;
  }

  private static bool AutomaticallyChangesDirection(
    ProjectileDefinitionComponent definition,
    ProjectileBehaviorComponent behavior)
  {
    if (definition.ManualDirectionChange || definition.LegacyAiStyle is 15 or 26 or 65 or 67 or 69 or
        112 or 114 or 123 or 150)
    {
      return false;
    }

    bool isManualAiPhase = ProjectileBehaviorStateProjection.Project(behavior).Ai0 == 1.0f;
    return (definition.LegacyAiStyle is not 3 and not 7 and not 13) || !isManualAiPhase;
  }
}
