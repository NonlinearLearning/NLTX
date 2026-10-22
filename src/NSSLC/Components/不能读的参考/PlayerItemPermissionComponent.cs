namespace Terraria.Player;

/// <summary>
/// 保存玩家持物许可和自动刷漆、自动致动能力。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>
/// 主要源成员：IsAllowedToHoldItems（第 1849 行）； autoPaint（第 1891 行）； autoActuator（第 1893 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P06-player-combat-status-component-design.md。</para>
/// <para>依据位置：第 1051 行。</para>
/// <para>
/// 用途：装备与效果流程汇总持物和工具能力，物品使用及方块交互流程读取结果。
/// </para>
/// <para>
/// 当前实现：默认允许持物，两个自动能力默认关闭。ResetEffects 恢复这些基础值，实际使用物品和操作方块由外部流程执行。
/// </para>
/// <para>
/// 原始分级记录（2026-10-08）：C130，L2（通用可选玩法）。分级理由：该类型当前无生产消费者。禁止拾取、持物权限、自然保护和自动涂漆是默认允许规则之外的可选策
/// 略。
/// </para>
/// </remarks>
public sealed class PlayerItemPermissionComponent
{
  /// <summary>
  /// 当前是否允许玩家持有或使用手中物品，基础值 true。
  /// </summary>
  public bool IsAllowedToHoldItems { get; internal set; } = true;

  /// <summary>
  /// 自动刷漆能力是否生效。
  /// </summary>
  public bool AutoPaint { get; internal set; }

  /// <summary>
  /// 自动致动器能力是否生效。
  /// </summary>
  public bool AutoActuator { get; internal set; }

  /// <summary>
  /// 清除本轮临时效果输入并恢复基础值，供外部效果流程重新汇总。
  /// </summary>
  internal void ResetEffects()
  {
    IsAllowedToHoldItems = true;
    AutoPaint = false;
    AutoActuator = false;
  }
}
