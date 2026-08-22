using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Components;

namespace Terraria.Dome.Simulation.Projectile.Definitions;

public sealed class ProjectileDefinitionRegistry
{
  private readonly IReadOnlyDictionary<int, ProjectileDefinition> _definitions;

  public ProjectileDefinitionRegistry(IEnumerable<ProjectileDefinition> definitions)
  {
    ArgumentNullException.ThrowIfNull(definitions);
    Dictionary<int, ProjectileDefinition> indexed = new();
    foreach (ProjectileDefinition definition in definitions)
    {
      if (definition.ProjectileType <= 0 || definition.BehaviorId <= 0 ||
          definition.Damage < 0 || definition.LifetimeTicks <= 0 ||
          !float.IsFinite(definition.Collider.Width) || definition.Collider.Width <= 0.0f ||
          !float.IsFinite(definition.Collider.Height) || definition.Collider.Height <= 0.0f ||
          definition.MaximumPenetration == 0 || definition.MaximumPenetration < -1)
      {
        throw new ArgumentOutOfRangeException(nameof(definitions));
      }

      if (!indexed.TryAdd(definition.ProjectileType, definition))
      {
        throw new ArgumentException("Projectile definitions must have unique types.", nameof(definitions));
      }
    }

    _definitions = indexed;
  }

  public static ProjectileDefinitionRegistry CreateDefault()
  {
    return new ProjectileDefinitionRegistry([
      new ProjectileDefinition(1, 1, 10, 30, new ColliderComponent(0.5f, 0.5f), true, false, 1),
      new ProjectileDefinition(2, 2, 10, 30, new ColliderComponent(0.5f, 0.5f), true, false, 1)]);
  }

  public IReadOnlyDictionary<int, ProjectileDefinition> Definitions => _definitions;

  public bool TryGet(int projectileType, out ProjectileDefinition definition)
  {
    return _definitions.TryGetValue(projectileType, out definition);
  }
}
