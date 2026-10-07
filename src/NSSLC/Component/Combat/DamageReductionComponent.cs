namespace Terraria.Combat;

/// <summary>
/// 保存伤害结算使用的减伤比例。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>主要源成员：endurance（第 767 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-1/design/。</para>
/// <para>拆分依据文件：2026-09-06-version4-combat-and-status-component-design-report.md。</para>
/// <para>依据位置：第 133 行。</para>
/// </remarks>
public struct HarmReductionComponent
{


  public float Endurance;

  public bool HasReduction => Endurance > 0.0f;
}
