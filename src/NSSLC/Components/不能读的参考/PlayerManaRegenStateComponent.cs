namespace Terraria.Player;

/// <summary>
/// 保存玩家魔力恢复速率、累积量、等待计时和恢复增益标记。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>
/// 主要源成员：manaRegen（第 1375 行）； manaRegenCount（第 1377 行）； manaRegenDelay（第 1379 行）； manaRegenBuff（第
/// 1381 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P06-player-combat-status-component-design.md。</para>
/// <para>依据位置：第 564 行。</para>
/// <para>
/// 用途：魔力恢复流程先处理等待计时，再累计恢复量并换算为实际魔力变化。
/// </para>
/// <para>
/// 状态变化：ResetEffects 清零本轮速率并关闭恢复增益，保留累积量和延迟；ResetForLifecycle 清空全部状态。具体恢复阈值和计时规则由外部流程决定
/// 。
/// </para>
/// <para>
/// 原始分级记录（2026-10-08）：C135，L2（通用可选玩法）。分级理由：魔力恢复、药水延迟和活动计数只在启用法术／魔力时需要。
/// </para>
/// </remarks>
public sealed class PlayerManaRegenStateComponent
{
  /// <summary>
  /// 当前魔力恢复速率输入。
  /// </summary>
  public int ManaRegen { get; internal set; }

  /// <summary>
  /// 尚未换算为实际魔力变化的累积量。
  /// </summary>
  public int ManaRegenCount { get; internal set; }

  /// <summary>
  /// 魔力恢复开始前的等待计时。
  /// </summary>
  public float ManaRegenDelay { get; internal set; }

  /// <summary>
  /// 魔力恢复增益是否生效。
  /// </summary>
  public bool ManaRegenBuff { get; internal set; }

  /// <summary>
  /// 清零本轮恢复速率和增益标记，保留累积量与延迟。
  /// </summary>
  internal void ResetEffects()
  {
    ManaRegen = 0;
    ManaRegenBuff = false;
  }

  /// <summary>
  /// 清零恢复速率、累积量、等待计时和增益标记。
  /// </summary>
  internal void ResetForLifecycle()
  {
    ManaRegen = 0;
    ManaRegenCount = 0;
    ManaRegenDelay = 0f;
    ManaRegenBuff = false;
  }
}
