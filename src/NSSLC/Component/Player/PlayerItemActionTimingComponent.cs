namespace Terraria.Player;

// Stores the paired Player item-action timers and the tool-use marker.
// Item definitions, reuse/input policy, Wiring and Combat effects remain external.
/// <summary>
/// 保存玩家物品动画、使用时间和工具计时。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>
/// 主要源成员：itemAnimation（第 2393 行）； itemAnimationMax（第 2395 行）； itemTime（第 2397 行）； itemTimeMax（第
/// 2399 行）； toolTime（第 2401 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-P04-player-lifecycle-interaction-component-design.md。
/// </para>
/// <para>依据位置：第 623 行。</para>
/// </remarks>
public sealed class PlayerItemActionTimingComponent
{
  public int AnimationRemaining { get; set; }

  public int AnimationDuration { get; set; }

  public int UseRemaining { get; set; }

  public int UseDuration { get; set; }

  public int ToolUseMarker { get; set; }
}
