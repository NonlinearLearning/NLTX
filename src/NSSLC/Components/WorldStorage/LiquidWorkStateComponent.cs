namespace Terraria.WorldStorage;

/// <summary>
/// 保存单个方块坐标上的液体工作调度状态。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Liquid。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Liquid.cs。</para>
/// <para>主要源成员：x（第 44 行）； y（第 46 行）； kill（第 48 行）； delay（第 50 行）。</para>
/// <para>重组说明：Sequence、RetryCount 和入队阶段是液体工作调度新增的状态。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-1/design/。</para>
/// <para>拆分依据文件：2026-09-05-version4-liquid-simulation-component-design.md。</para>
/// <para>依据位置：第 110 行。</para>
/// <para>
/// 用途：液体调度流程用坐标定位工作，并维护等待、跳过、重试、缓冲及处理顺序，液体求解流程读取这些状态。
/// </para>
/// <para>
/// 当前实现：只有可写字段。KillScore 的消费阈值、Delay 的单位和工作退出条件由外部液体流程定义。
/// </para>
/// <para>
/// 原始分级记录（2026-10-08）：C210，L2（通用可选玩法）。分级理由：液体缓冲、排序、工作项、脏区块、预算和流动状态只在启用液体传播时需要。
/// </para>
/// </remarks>
public struct LiquidWorkStateComponent
{
  /// <summary>
  /// 此液体工作对应的世界方块坐标。
  /// </summary>
  public TileCoordinate Coordinate;
  /// <summary>
  /// 此工作是否待处理。
  /// </summary>
  public bool IsPending;
  /// <summary>
  /// 是否要求下一次调度跳过此工作。
  /// </summary>
  public bool SkipOnce;
  /// <summary>
  /// 用于结束或移除工作判定的分数，沿用原版液体工作语义。
  /// </summary>
  public int KillScore;
  /// <summary>
  /// 执行前的等待计数，具体单位由调度流程定义。
  /// </summary>
  public int Delay;
  /// <summary>
  /// 分配给此工作的顺序编号。
  /// </summary>
  public long Sequence;
  /// <summary>
  /// 此工作的重试计数。
  /// </summary>
  public int RetryCount;
  /// <summary>
  /// 此工作是否登记在缓冲状态中。
  /// </summary>
  public bool IsBuffered;
}
