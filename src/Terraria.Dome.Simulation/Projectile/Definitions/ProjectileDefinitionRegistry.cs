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
