namespace Terraria.WorldProgression.Components;

/// <summary>
/// 保存世界感染传播许可。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.WorldGen。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/WorldGen.cs。</para>
/// <para>主要源成员：AllowedToSpreadInfections（第 4185 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-P16-world-lifecycle-housing-metrics-component-design.md。
/// </para>
/// <para>依据位置：第 17 行。</para>
/// </remarks>
public sealed class WorldInfectionPolicyStateComponent
{
  public bool IsInfectionSpreadAllowed { get; set; } = true;
}
