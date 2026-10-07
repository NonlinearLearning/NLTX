namespace Terraria.Player;

/// <summary>
/// 保存玩家死亡宝箱成就的剩余触发资格时间。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>主要源成员：_framesLeftEligibleForDeadmansChestDeathAchievement（第 919 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P06-player-combat-status-component-design.md。</para>
/// <para>依据位置：第 500 行。</para>
/// </remarks>
public sealed class PlayerAchievementEligibilityStateComponent
{
  public int FramesLeftEligibleForDeadmansChestDeathAchievement { get; internal set; }

  internal void ResetForLifecycle()
  {
    FramesLeftEligibleForDeadmansChestDeathAchievement = 0;
  }
}
