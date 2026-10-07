namespace Terraria.Player;

/// <summary>
/// 保存玩家物品使用动画、冷却、重用和持续使用状态。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>
/// 主要源成员：reuseDelay（第 991 行）； pendingItemReuse（第 1007 行）； heldProj（第 1035 行）； delayUseItem（第 1292
/// 行）； channel（第 1318 行）； itemAnimation（第 2393 行）； itemAnimationMax（第 2395 行）； itemTime（第 2397
/// 行）； itemTimeMax（第 2399 行）； toolTime（第 2401 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-P04-player-lifecycle-interaction-component-design.md。
/// </para>
/// <para>依据位置：第 192 行。</para>
/// </remarks>
public sealed class PlayerUseComponent
{
  public int AnimationRemainingTicks { get; set; }

  public int AnimationDurationTicks { get; set; }

  public int UseRemainingTicks { get; set; }

  public int UseDurationTicks { get; set; }

  public int ToolTime { get; set; }

  public int ReuseDelayRemainingTicks { get; set; }

  public bool HasPendingReuse { get; set; }

  public bool IsChanneling { get; set; }

  public bool IsUseDelayed { get; set; }

  // Compatibility projection only; it is not a Projectile entity identity.
  public LegacyProjectileSlot? HeldProjectile { get; set; }

  public bool LastUseAttemptSucceeded { get; set; }
}
