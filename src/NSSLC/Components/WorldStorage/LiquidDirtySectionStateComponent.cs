namespace Terraria.WorldStorage;

/// <summary>
/// 保存液体分区的变更版本和待同步标记。
/// </summary>
/// <remarks>
/// <para>拆分来源：由 Liquid 的方块液体更新与同步流程重组。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Liquid.cs。</para>
/// <para>重组说明：区块脏标记与 Revision 是液体变化跟踪新增的状态。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-1/design/。</para>
/// <para>拆分依据文件：2026-09-05-version4-liquid-simulation-component-code-draft.md。</para>
/// <para>依据位置：第 273 行。</para>
/// <para>
/// 用途：液体修改流程登记分区发生变化，后续同步或快照流程读取 IsDirty 和 Revision。
/// </para>
/// <para>
/// 当前实现：两个字段可直接写入。版本递增、标脏、清脏和分区发送由外部流程维护。
/// </para>
/// <para>
/// 原始分级记录（2026-10-08）：C207，L2（通用可选玩法）。分级理由：液体缓冲、排序、工作项、脏区块、预算和流动状态只在启用液体传播时需要。
/// </para>
/// </remarks>
public struct LiquidDirtySectionStateComponent
{
  /// <summary>
  /// 液体分区状态版本，由外部提交流程递增。
  /// </summary>
  public long Revision;
  /// <summary>
  /// 此分区是否存在需要后续处理的液体变化。
  /// </summary>
  public bool IsDirty;
}
