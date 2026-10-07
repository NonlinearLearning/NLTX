namespace Terraria.StatusEffects;

/// <summary>
/// 保存主体的状态效果集合。
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
/// </remarks>
public sealed class StatusEffectsComponent
{
  public StatusEffectsComponent(IReadOnlyList<TimedStatusEffect> effects)
  {
    Effects = new List<TimedStatusEffect>(effects);
  }

  public List<TimedStatusEffect> Effects;
}
