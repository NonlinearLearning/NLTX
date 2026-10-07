namespace Terraria.WorldStorage;

/// <summary>
/// 保存液体变化导致的区块脏标记和版本。
/// </summary>
/// <remarks>
/// <para>拆分来源：由 Liquid 的方块液体更新与同步流程重组。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Liquid.cs。</para>
/// <para>重组说明：区块脏标记与 Revision 是液体变化跟踪新增的状态。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-1/design/。</para>
/// <para>拆分依据文件：2026-09-05-version4-liquid-simulation-component-code-draft.md。</para>
/// <para>依据位置：第 273 行。</para>
/// </remarks>
public struct LiquidDirtySectionStateComponent
{
  public long Revision;
  public bool IsDirty;
}
