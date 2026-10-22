namespace Terraria.Combat;

/// <summary>
/// 保存伤害结算使用的减伤比例输入。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>主要源成员：endurance（第 767 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-1/design/。</para>
/// <para>拆分依据文件：2026-09-06-version4-combat-and-status-component-design-report.md。</para>
/// <para>依据位置：第 133 行。</para>
/// <para>
/// 用途：战斗流程读取 Endurance 后应用具体减伤公式，例如把装备和状态效果提供的减伤汇总到同一输入。
/// </para>
/// <para>
/// 当前实现：Endurance 可直接修改；HasReduction 仅判断它是否大于零。比例范围和最终伤害由外部结算流程处理。
/// </para>
/// <para>
/// 原始分级记录（2026-10-08）：C071，L2（通用可选玩法）。分级理由：比例减伤、击退抗性和持续恢复可在初版用固定规则替代。
/// </para>
/// </remarks>
public struct HarmReductionComponent
{


  /// <summary>
  /// 减伤比例输入；当前字段本身没有范围校验。
  /// </summary>
  public float Endurance;

  /// <summary>
  /// 减伤输入大于零时返回 true。
  /// </summary>
  public bool HasReduction => Endurance > 0.0f;
}
