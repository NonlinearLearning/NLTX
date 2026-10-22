namespace Terraria.Player;

/// <summary>
/// 保存玩家魔力恢复速率和恢复延迟的加成输入。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>主要源成员：manaRegenBonus（第 674 行）； manaRegenDelayBonus（第 676 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P06-player-combat-status-component-design.md。</para>
/// <para>依据位置：第 332 行。</para>
/// <para>
/// 用途：装备和状态效果先汇总恢复加成，再由魔力恢复流程组合基础速率和等待时间。
/// </para>
/// <para>
/// 当前实现：两个输入默认零，Reset 将其清零。实际魔力值、延迟计时和结算由外部恢复流程处理。
/// </para>
/// <para>
/// 原始分级记录（2026-10-08）：C134，L2（通用可选玩法）。分级理由：魔力恢复、药水延迟和活动计数只在启用法术／魔力时需要。
/// </para>
/// </remarks>
public sealed class PlayerManaRegenModifierComponent
{
  /// <summary>
  /// 魔力恢复速率的额外输入。
  /// </summary>
  public int ManaRegenBonus { get; internal set; }

  /// <summary>
  /// 魔力恢复延迟的修正输入，正负作用由外部公式定义。
  /// </summary>
  public float ManaRegenDelayBonus { get; internal set; }

  /// <summary>
  /// 清零魔力恢复速率加成和恢复延迟修正。
  /// </summary>
  internal void Reset()
  {
    ManaRegenBonus = 0;
    ManaRegenDelayBonus = 0f;
  }
}
