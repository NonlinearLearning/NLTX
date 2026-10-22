namespace Terraria.StatusEffects;

/// <summary>
/// 保存主体当前持有的定时状态效果集合。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>主要源成员：buffType（第 1029 行）； buffTime（第 1031 行）。</para>
/// <para>拆分来源：Terraria.NPC。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/NPC.cs。</para>
/// <para>主要源成员：buffType（第 6067 行）； buffTime（第 6069 行）。</para>
/// <para>重组说明：以 StatusEffect 集合承载效果；原版使用效果编号和时间的平行槽位数组。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/non-authoritative/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-non-authoritative-P07-combat-status-effects-component-design.md。
/// </para>
/// <para>依据位置：第 285 行。</para>
/// <para>
/// 用途：状态系统遍历 TimedStatusEffect 列表，维护效果剩余时间并生成恢复、伤害、能力或控制修正。
/// </para>
/// <para>
/// 当前实现：构造时把输入集合复制到新 List，之后 Effects 可直接增删或替换。去重、叠加、计时和过期删除规则由外部系统决定。
/// </para>
/// <para>
/// 原始分级记录（2026-10-08）：C162，L2（通用可选玩法）。分级理由：通用状态效果集合是可选玩法，不是运行时基础。
/// </para>
/// </remarks>
public sealed class StatusEffectsComponent
{
  public StatusEffectsComponent(IReadOnlyList<TimedStatusEffect> effects)
  {
    Effects = new List<TimedStatusEffect>(effects);
  }

  /// <summary>
  /// 可变的定时状态效果列表；构造时复制集合，元素自身的语义由 TimedStatusEffect 定义。
  /// </summary>
  public List<TimedStatusEffect> Effects;
}
