namespace Terraria.WorldStorage;

/// <summary>
/// 保存液体工作项的下一排序序号。
/// </summary>
/// <remarks>
/// <para>拆分来源：由 Liquid 的工作队列处理流程重组。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Liquid.cs。</para>
/// <para>重组说明：NextLiquidSequence 是确定工作顺序的新增序号。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-1/design/。</para>
/// <para>拆分依据文件：2026-09-05-version4-liquid-simulation-component-code-draft.md。</para>
/// <para>依据位置：第 307 行。</para>
/// </remarks>
public struct LiquidSequenceStateComponent
{
  public long NextLiquidSequence;
}
