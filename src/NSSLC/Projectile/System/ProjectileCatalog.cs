using System;
using System.Collections.Generic;
using Terraria.Share.Entities;

namespace Terraria.Unshared.Projectiles;

/// <summary>Immutable-after-sealing directory of projectile content recipes.</summary>
/// <remarks>
/// 编目只登记配方：装配已经在 <see cref="ProjectileRecipe" /> 构造时完成，
/// <see cref="Register" /> 不调用任何装配函数，因此新增一族不需要动这里。
/// </remarks>
public sealed class ProjectileCatalog
{
  private readonly Dictionary<ProjectileId, ProjectileRecipe> _recipes = [];
  private readonly EntityRegistry _entityDefinitions = new();
  private bool _sealed;

  /// <summary>
  /// 与内容配方一一对应的实体组合定义。注册时一并登记，供生命周期走共享创建路径。
  /// </summary>
  internal EntityRegistry EntityDefinitions => _entityDefinitions;

  /// <summary>Registered recipe count. Used by content loading and verification.</summary>
  public int Count => _recipes.Count;

  /// <summary>Registered recipes in registration order.</summary>
  public IReadOnlyCollection<ProjectileRecipe> Recipes => _recipes.Values;

  /// <summary>True once <see cref="Seal" /> has run; no further registrations are accepted.</summary>
  public bool IsSealed => _sealed;

  public void Register(ProjectileRecipe recipe)
  {
    ArgumentNullException.ThrowIfNull(recipe);
    if (_sealed)
    {
      throw new InvalidOperationException("The projectile definition catalog is sealed.");
    }

    if (!_recipes.TryAdd(recipe.Id, recipe))
    {
      throw new InvalidOperationException(
          $"Projectile definition {recipe.Id} is already registered.");
    }

    // 内容配方与实体组合定义同时登记：两者必须成对存在，避免"有内容但没有生成路径"的中间态。
    _entityDefinitions.Register(recipe.Composition);
  }

  public void Seal()
  {
    _sealed = true;
  }

  public bool TryGet(ProjectileId id, out ProjectileRecipe recipe)
  {
    return _recipes.TryGetValue(id, out recipe!);
  }

  public ProjectileRecipe Get(ProjectileId id)
  {
    if (!TryGet(id, out ProjectileRecipe recipe))
    {
      throw new KeyNotFoundException($"Projectile definition {id} is not registered.");
    }

    return recipe;
  }

  /// <summary>Returns the paired entity composition definition for one content recipe.</summary>
  internal EntityDefinition GetEntityDefinition(ProjectileId id)
  {
    return _entityDefinitions.Get(Get(id).EntityId);
  }
}
