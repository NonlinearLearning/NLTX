using System;

namespace Terraria.Projectile;

/// <summary>
/// 保存射弹对各 NPC、玩家的命中间隔及再次攻击延迟。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Projectile。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Projectile.cs。</para>
/// <para>
/// 主要源成员：localNPCImmunity（第 158 行）； restrikeDelay（第 192 行）； playerImmune（第 220 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P15-projectile-component-design.md。</para>
/// <para>依据位置：第 17 行。</para>
/// </remarks>
public struct ProjectileHitImmunityStateComponent
{
  public ProjectileHitImmunityStateComponent()
  {
    LocalNpcImmunityTicks = Array.Empty<int>();
    PlayerImmunityTicks = new int[255];
    RestrikeDelayTicks = 0;
  }

  public ProjectileHitImmunityStateComponent(
    int npcCapacity,
    int playerCapacity = 255)
  {
    if (npcCapacity < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(npcCapacity));
    }

    if (playerCapacity < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(playerCapacity));
    }

    LocalNpcImmunityTicks = new int[npcCapacity];
    PlayerImmunityTicks = new int[playerCapacity];
    RestrikeDelayTicks = 0;
  }

  public int[] LocalNpcImmunityTicks;
  public int[] PlayerImmunityTicks;
  public int RestrikeDelayTicks;
}
