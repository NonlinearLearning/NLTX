namespace Terraria.WorldStorage;

/// <summary>
/// 保存分配下一个液体工作序号所需的计数。
/// </summary>
/// <remarks>
/// <para>拆分来源：由 Liquid 的工作队列处理流程重组。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Liquid.cs。</para>
/// <para>重组说明：NextLiquidSequence 是确定工作顺序的新增序号。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-1/design/。</para>
/// <para>拆分依据文件：2026-09-05-version4-liquid-simulation-component-code-draft.md。</para>
/// <para>依据位置：第 307 行。</para>
/// <para>
/// 用途：液体调度流程分配 Sequence 时读取并推进 NextLiquidSequence，为工作记录提供顺序信息。
/// </para>
/// <para>
/// 当前实现：只有一个可写 long 字段。递增、溢出处理和序号作用范围由外部调度流程维护。
/// </para>
/// <para>
/// 原始分级记录（2026-10-08）：C209，L2（通用可选玩法）。分级理由：液体缓冲、排序、工作项、脏区块、预算和流动状态只在启用液体传播时需要。
/// </para>
/// </remarks>
public struct LiquidSequenceStateComponent
{
  /// <summary>
  /// 下一个待分配的液体工作序号。
  /// </summary>
  public long NextLiquidSequence;
}
