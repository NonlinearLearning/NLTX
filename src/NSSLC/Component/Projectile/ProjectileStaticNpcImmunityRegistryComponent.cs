using System;

namespace Terraria.Projectile;

/// <summary>
/// World-level absolute expiry ticks for per-projectile-type NPC immunity.
/// </summary>
/// <remarks>
/// <para>职责：保存世界级的按射弹类型与 NPC 索引区分的命中免疫到期表。</para>
/// <para>拆分来源：Terraria.Projectile。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Projectile.cs。</para>
/// <para>主要源成员：perIDStaticNPCImmunity（第 92 行）。</para>
/// <para>重组说明：保存绝对到期 tick；原静态表改由世界级组件持有。</para>
/// <para>拆分依据目录：docs/system-decomposition/reports/。</para>
/// <para>
/// 拆分依据文件：2026-09-30-system-decomposition-authoritative-P15-projectile-execution.md。
/// </para>
/// <para>依据位置：第 55 行。</para>
/// </remarks>
public sealed class ProjectileStaticNpcImmunityRegistryComponent
{
  private readonly uint[][] _immunityExpiryByProjectileType;

  public ProjectileStaticNpcImmunityRegistryComponent(
    int projectileTypeCapacity,
    int npcCapacity)
  {
    if (projectileTypeCapacity <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(projectileTypeCapacity));
    }

    if (npcCapacity < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(npcCapacity));
    }

    _immunityExpiryByProjectileType = new uint[projectileTypeCapacity][];
    for (int projectileType = 0;
      projectileType < _immunityExpiryByProjectileType.Length;
      projectileType++)
    {
      _immunityExpiryByProjectileType[projectileType] = new uint[npcCapacity];
    }
  }

  internal int NpcCapacity => _immunityExpiryByProjectileType[0].Length;

  internal uint[][] ImmunityExpiryByProjectileType =>
    _immunityExpiryByProjectileType;
}
