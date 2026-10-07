using System;

namespace Terraria.Projectile;

/// <summary>
/// 保存射弹内容类型、行为键和定义目录版本。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Projectile。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Projectile.cs。</para>
/// <para>主要源成员：type（第 118 行）； aiStyle（第 136 行）。</para>
/// <para>重组说明：BehaviorKey 与 CatalogRevision 是定义目录拆分后的引用表达。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P15-projectile-component-design.md。</para>
/// <para>依据位置：第 16 行。</para>
/// </remarks>
public readonly record struct ProjectileDefinitionReferenceComponent
{
  public ProjectileDefinitionReferenceComponent(
    int projectileType,
    int catalogRevision = 0)
    : this(projectileType, behaviorKey: 0, catalogRevision)
  {
  }

  public ProjectileDefinitionReferenceComponent(
    int projectileType,
    int behaviorKey,
    int catalogRevision)
  {
    if (projectileType < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(projectileType));
    }

    if (catalogRevision < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(catalogRevision));
    }

    ProjectileType = projectileType;
    BehaviorKey = behaviorKey;
    CatalogRevision = catalogRevision;
  }

  public int ProjectileType { get; }

  public int BehaviorKey { get; }

  public int CatalogRevision { get; }

  public bool HasProjectileType => ProjectileType > 0;
}
