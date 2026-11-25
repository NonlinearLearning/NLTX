using System;
using System.Collections.Generic;
using Terraria.Share.Entities;

namespace Terraria.Unshared.Projectiles;

/// <summary>Pairs one content id with the sections that compose its entity, plus the folded result.</summary>
/// <remarks>
/// 配方只回答「谁参与装配」，装配结果由 <see cref="ProjectileComposition" /> 折叠得出。
/// 组合在构造时就算完：编目只登记配方，不调用装配函数——否则"登记了内容但没有生成路径"
/// 这个中间态就没有任何东西挡得住。
/// <para>
/// 顺序不承载语义：两遍折叠（先 <see cref="IProjectileSection.Collect" /> 再
/// <see cref="IProjectileSection.Contribute" />）已经消除了"谁先谁后决定结果"的可能。
/// </para>
/// </remarks>
public sealed class ProjectileRecipe
{
  private readonly IProjectileSection[] _sections;

  public ProjectileRecipe(ProjectileId id, IEnumerable<IProjectileSection> sections)
  {
    if (!id.IsValid)
    {
      throw new ArgumentException("A projectile recipe id is required.", nameof(id));
    }

    ArgumentNullException.ThrowIfNull(sections);
    Id = id;
    EntityId = new EntityId($"projectile/{id}");
    _sections = [.. sections];
    if (_sections.Length == 0)
    {
      throw new ArgumentException(
          "A projectile recipe needs at least one section.", nameof(sections));
    }

    Composition = ProjectileComposition.Create(this);
  }

  public ProjectileId Id { get; }

  /// <summary>内容定义 id 到实体组合定义 id 的唯一映射，注册与查表共用。</summary>
  public EntityId EntityId { get; }

  /// <summary>Sections in declaration order; order carries no meaning.</summary>
  public IReadOnlyList<IProjectileSection> Sections => _sections;

  /// <summary>构造时折叠出的实体组合定义；只读，可被反复生成使用。</summary>
  public EntityDefinition Composition { get; }

  /// <summary>查找本族 section；内容族没参与时返回 false。</summary>
  public bool TryGetSection<TSection>(out TSection section)
      where TSection : class, IProjectileSection
  {
    foreach (IProjectileSection candidate in _sections)
    {
      if (candidate is TSection match)
      {
        section = match;
        return true;
      }
    }

    section = null!;
    return false;
  }

  /// <summary>按当前 section 列表重新折叠一次；影子对照与测试用，生产路径读 <see cref="Composition" />。</summary>
  public EntityDefinition CreateComposition()
  {
    return ProjectileComposition.Create(this);
  }
}
