namespace Terraria.Player;

/// <summary>
/// 保存玩家魔力恢复规则计算出的最大恢复延迟输入。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>主要源成员：maxRegenDelay（第 986 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P06-player-combat-status-component-design.md。</para>
/// <para>依据位置：第 523 行。</para>
/// <para>
/// 用途：原版 maxRegenDelay 根据当前魔力与魔力上限等输入计算，供后续魔力恢复等待规则读取。
/// </para>
/// <para>
/// 当前实现：只保存 MaxRegenDelay，生命周期重置时归零；最大延迟的计算和当前延迟的递减由外部恢复流程负责。
/// </para>
/// <para>
/// 原始分级记录（2026-10-08）：C139，L2（通用可选玩法）。分级理由：槽位、免疫、元素状态和恢复延迟只在启用状态效果时需要。
/// </para>
/// </remarks>
public sealed class PlayerRegenDelayStateComponent
{
  /// <summary>
  /// 魔力恢复的最大延迟输入，沿用原版 maxRegenDelay 语义。
  /// </summary>
  public float MaxRegenDelay { get; internal set; }

  /// <summary>
  /// 将最大魔力恢复延迟输入归零。
  /// </summary>
  internal void ResetForLifecycle()
  {
    MaxRegenDelay = 0f;
  }
}
