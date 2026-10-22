namespace Terraria.Player;

/// <summary>
/// 保存玩家生命恢复速率、未结算累积量和自然恢复进度。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>主要源成员：lifeRegen（第 1369 行）； lifeRegenCount（第 1371 行）； lifeRegenTime（第 1373 行）。</para>
/// <para>重组说明：LifeRegenTime 是可被效果加速的自然恢复进度。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P06-player-combat-status-component-design.md。</para>
/// <para>依据位置：第 563 行。</para>
/// <para>
/// 用途：生命恢复流程根据 LifeRegen 累积 LifeRegenCount，再按规则换算生命变化；LifeRegenTime 表达可被效果调整的恢复进度。
/// </para>
/// <para>
/// 状态变化：ResetEffects 只清零本轮速率，保留累积量和恢复进度；ResetForLifecycle 清零全部状态。实际生命值修改由外部结算流程负责。
/// </para>
/// <para>
/// 原始分级记录（2026-10-08）：C131，L2（通用可选玩法）。分级理由：槽位、免疫、元素状态和恢复延迟只在启用状态效果时需要。
/// </para>
/// </remarks>
public sealed class PlayerLifeRegenStateComponent
{
  /// <summary>
  /// 当前生命恢复或持续损失的速率输入。
  /// </summary>
  public int LifeRegen { get; internal set; }

  /// <summary>
  /// 尚未结算为实际生命变化的累积量。
  /// </summary>
  public int LifeRegenCount { get; internal set; }

  /// <summary>
  /// 自然恢复进度，沿用原版 lifeRegenTime 语义，不能直接视为真实秒数。
  /// </summary>
  public float LifeRegenTime { get; internal set; }

  /// <summary>
  /// 只清零本轮生命恢复速率，保留跨帧累积量与恢复进度。
  /// </summary>
  internal void ResetEffects()
  {
    LifeRegen = 0;
  }

  /// <summary>
  /// 清零速率、累积量和恢复进度。
  /// </summary>
  internal void ResetForLifecycle()
  {
    LifeRegen = 0;
    LifeRegenCount = 0;
    LifeRegenTime = 0f;
  }
}
