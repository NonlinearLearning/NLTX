namespace Terraria.Player;

/// <summary>
/// 保存玩家脱离黏附状态所需的挣脱进度计数。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>主要源成员：stickyBreak（第 1399 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P06-player-combat-status-component-design.md。</para>
/// <para>依据位置：第 656 行。</para>
/// <para>
/// 用途：移动与输入流程维护原版 stickyBreak 计数，决定何时解除黏附或束缚相关状态。
/// </para>
/// <para>
/// 当前实现：只有一个计数属性，ResetForLifecycle 将其归零；增长方式、阈值和实际解除效果由外部流程定义。
/// </para>
/// <para>
/// 原始分级记录（2026-10-08）：C145，L2（通用可选玩法）。分级理由：区域、游泳、呼吸、熔岩耐受、风力及表面状态属于额外环境玩法。
/// </para>
/// </remarks>
public sealed class PlayerStickyBreakStateComponent
{
  /// <summary>
  /// 黏附状态的挣脱计数，沿用原版 stickyBreak 语义。
  /// </summary>
  public int StickyBreak { get; internal set; }

  /// <summary>
  /// 将黏附挣脱计数归零。
  /// </summary>
  internal void ResetForLifecycle()
  {
    StickyBreak = 0;
  }
}
