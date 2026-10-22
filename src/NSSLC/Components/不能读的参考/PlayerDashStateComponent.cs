namespace Terraria.Player.Mobility;

// status: implemented-isolated-core
// crossSubsystemOwner: integration-review for input, equipment, Spatial, effects, network, and persistence
/// <summary>
/// 保存玩家冲刺种类、当前冲刺状态和恢复计时。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>
/// 主要源成员：dashType（第 678 行）； dashTime（第 682 行）； timeSinceLastDashStarted（第 684 行）； dashDelay（第 686
/// 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P07-player-mobility-component-design.md。</para>
/// <para>依据位置：第 413 行。</para>
/// <para>
/// 用途：输入与移动流程通过这些状态协调冲刺触发、持续和再次触发间隔，例如装备提供冲刺种类后由移动流程执行。
/// </para>
/// <para>
/// 当前实现：只有可写属性。DashType 和 ActiveDash 的编号含义、计数正负方向及 tick 推进规则由外部冲刺流程约定。
/// </para>
/// <para>
/// 原始分级记录（2026-10-08）：C104，L2（通用可选玩法）。分级理由：冲刺与恢复计时是移动扩展，走跑跳闭环可以不启用。
/// </para>
/// </remarks>
public sealed class PlayerDashStateComponent
{
  /// <summary>
  /// 当前具备的冲刺种类编号。
  /// </summary>
  public int DashType { get; set; }

  /// <summary>
  /// 当前冲刺的活动状态或种类值，由冲刺流程解释。
  /// </summary>
  public int ActiveDash { get; set; }

  /// <summary>
  /// 冲刺相关计时，沿用原版 dashTime 的状态语义。
  /// </summary>
  public int DashTime { get; set; }

  /// <summary>
  /// 距离最近一次开始冲刺的计数，由外部流程推进。
  /// </summary>
  public int TimeSinceLastDashStarted { get; set; }

  /// <summary>
  /// 再次冲刺所需的恢复或延迟计时。
  /// </summary>
  public int DashDelay { get; set; }
}
