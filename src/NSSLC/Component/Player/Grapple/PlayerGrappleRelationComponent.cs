using System;

namespace Terraria.Player.Grapple;

// status: implemented-isolated-core
// crossSubsystemOwner: Projectile identity, lifecycle, network, and persistence
/// <summary>
/// 保存玩家关联的钩爪射弹槽位。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>主要源成员：grappling（第 2136 行）； grapCount（第 2138 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P07-player-mobility-component-design.md。</para>
/// <para>依据位置：第 923 行。</para>
/// </remarks>
public sealed class PlayerGrappleRelationComponent
{
  public const int SlotCount = 20;

  public const int InvalidProjectileSlot = -1;

  public int[] ProjectileSlots { get; } = new int[SlotCount];

  public int Count { get; set; }

  public PlayerGrappleRelationComponent()
  {
    Array.Fill(ProjectileSlots, InvalidProjectileSlot);
  }
}
