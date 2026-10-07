using System;

namespace Terraria.Projectile;

/// <summary>
/// 保存射弹使用局部、静态或拥有者命中冷却的策略。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Projectile。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Projectile.cs。</para>
/// <para>
/// 主要源成员：usesLocalNPCImmunity（第 160 行）； usesIDStaticNPCImmunity（第 162 行）；
/// appliesImmunityTimeOnSingleHits（第 164 行）； usesOwnerMeleeHitCD（第 218 行）； localNPCHitCooldown（第
/// 256 行）； idStaticNPCHitCooldown（第 258 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P15-projectile-component-design.md。</para>
/// <para>依据位置：第 17 行。</para>
/// </remarks>
public struct ProjectileHitImmunityPolicyComponent
{
  public ProjectileHitImmunityPolicyComponent(
    bool usesLocalNpcImmunity = false,
    bool usesStaticNpcImmunity = false,
    int localNpcCooldownTicks = -2,
    int staticNpcCooldownTicks = -1,
    bool appliesOnSingleHit = false,
    bool usesOwnerMeleeCooldown = false,
    bool copiesOwnerCooldownOnSpawn = false)
  {
    if (localNpcCooldownTicks < -2)
    {
      throw new ArgumentOutOfRangeException(nameof(localNpcCooldownTicks));
    }

    if (staticNpcCooldownTicks < -1)
    {
      throw new ArgumentOutOfRangeException(nameof(staticNpcCooldownTicks));
    }

    UsesLocalNpcImmunity = usesLocalNpcImmunity;
    UsesStaticNpcImmunity = usesStaticNpcImmunity;
    LocalNpcCooldownTicks = localNpcCooldownTicks;
    StaticNpcCooldownTicks = staticNpcCooldownTicks;
    AppliesOnSingleHit = appliesOnSingleHit;
    UsesOwnerMeleeCooldown = usesOwnerMeleeCooldown;
    CopiesOwnerCooldownOnSpawn = copiesOwnerCooldownOnSpawn;
  }

  public bool UsesLocalNpcImmunity;
  public bool UsesStaticNpcImmunity;
  public int LocalNpcCooldownTicks;
  public int StaticNpcCooldownTicks;
  public bool AppliesOnSingleHit;
  public bool UsesOwnerMeleeCooldown;
  public bool CopiesOwnerCooldownOnSpawn;

  public bool WritesLocalNpcImmunity =>
    UsesLocalNpcImmunity && LocalNpcCooldownTicks != -2;

  public bool UsesStaticNpcImmunityRegistry =>
    UsesStaticNpcImmunity;
}
