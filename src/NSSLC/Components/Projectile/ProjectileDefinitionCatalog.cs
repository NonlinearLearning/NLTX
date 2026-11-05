using System;
using System.Collections.Generic;
using Terraria.Combat;
using Terraria.Entity;

namespace Terraria.Projectile;

/// <summary>Immutable-after-sealing directory of projectile content definitions.</summary>
public sealed class ProjectileDefinitionCatalog
{
  private readonly Dictionary<ProjectileDefinitionId, ProjectileDefinition> _definitions = [];
  private readonly EntityDefinitionRegistry _entityDefinitions = new();
  private bool _sealed;

  /// <summary>
  /// 与内容定义一一对应的实体组合定义。注册时一并构建，供生命周期走共享创建路径。
  /// </summary>
  internal EntityDefinitionRegistry EntityDefinitions => _entityDefinitions;

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

    // 内容定义与实体组合定义同时登记：两者必须成对存在，避免"有内容但没有生成路径"的中间态。
    _entityDefinitions.Register(ProjectileEntityDefinitions.Build(definition));
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

  /// <summary>Returns the paired entity composition definition for one content definition.</summary>
  internal EntityDefinition GetEntityDefinition(ProjectileDefinitionId id)
  {
    return _entityDefinitions.Get(ProjectileEntityDefinitions.IdFor(id));
  }
}
