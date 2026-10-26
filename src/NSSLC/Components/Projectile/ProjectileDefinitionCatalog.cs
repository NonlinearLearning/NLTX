using System;
using System.Collections.Generic;

namespace Terraria.Projectile;

/// <summary>Immutable-after-sealing directory of projectile content definitions.</summary>
public sealed class ProjectileDefinitionCatalog
{
  private readonly Dictionary<ProjectileDefinitionId, ProjectileDefinition> _definitions = [];
  private bool _sealed;

  public void Register(ProjectileDefinition definition)
  {
    ArgumentNullException.ThrowIfNull(definition);
    if (_sealed)
    {
      throw new InvalidOperationException("The projectile definition catalog is sealed.");
    }

    if (!_definitions.TryAdd(definition.Id, definition))
    {
      throw new InvalidOperationException(
          $"Projectile definition {definition.Id} is already registered.");
    }
  }

  public void Seal()
  {
    _sealed = true;
  }

  public bool TryGet(ProjectileDefinitionId id, out ProjectileDefinition definition)
  {
    return _definitions.TryGetValue(id, out definition!);
  }

  public ProjectileDefinition Get(ProjectileDefinitionId id)
  {
    if (!TryGet(id, out ProjectileDefinition definition))
    {
      throw new KeyNotFoundException($"Projectile definition {id} is not registered.");
    }

    return definition;
  }
}
