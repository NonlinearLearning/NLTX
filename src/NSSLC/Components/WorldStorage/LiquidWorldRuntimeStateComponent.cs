namespace Terraria.WorldStorage;

/// <summary>
/// 保存世界级液体容量、更新循环和停滞及紧急处理状态。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Liquid。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Liquid.cs。</para>
/// <para>
/// 主要源成员：maxLiquidBuffer（第 14 行）； maxLiquid（第 16 行）； skipCount（第 18 行）； stuckCount（第 20 行）；
/// stuckAmount（第 22 行）； cycles（第 24 行）； quickFall（第 32 行）； quickSettle（第 34 行）； wetCounter（第 36
/// 行）； panicCounter（第 38 行）； panicMode（第 40 行）； panicY（第 42 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-1/design/。</para>
/// <para>拆分依据文件：2026-09-05-version4-liquid-simulation-component-design.md。</para>
/// <para>依据位置：第 137 行。</para>
/// <para>
/// 用途：液体调度与求解流程共享缓冲容量、活跃工作上限、循环次数和快速处理标记。
/// </para>
/// <para>
/// 当前实现：所有属性可直接修改，默认缓冲容量 50000、活跃上限 25000、循环数 10。容量校验、计数推进和液体算法由外部流程负责。
/// </para>
/// <para>
/// 原始分级记录（2026-10-08）：C211，L2（通用可选玩法）。分级理由：液体缓冲、排序、工作项、脏区块、预算和流动状态只在启用液体传播时需要。
/// </para>
/// </remarks>
public sealed class LiquidWorldRuntimeStateComponent
{
  /// <summary>
  /// 默认液体缓冲容量 50000。
  /// </summary>
  public const int DefaultMaxLiquidBuffer = 50000;
  /// <summary>
  /// 默认活跃液体上限 25000。
  /// </summary>
  public const int DefaultMaxLiquid = 25000;
  /// <summary>
  /// 默认液体更新循环数 10。
  /// </summary>
  public const int DefaultCycles = 10;

  /// <summary>
  /// 配置的液体缓冲容量。
  /// </summary>
  public int MaxLiquidBuffer { get; set; } = DefaultMaxLiquidBuffer;
  /// <summary>
  /// 配置的活跃液体数量上限。
  /// </summary>
  public int MaxLiquid { get; set; } = DefaultMaxLiquid;
  /// <summary>
  /// 当前生效的活跃液体数量上限。
  /// </summary>
  public int CurrentMaxLiquid { get; set; } = DefaultMaxLiquid;
  /// <summary>
  /// 本轮液体更新使用的循环数输入。
  /// </summary>
  public int Cycles { get; set; } = DefaultCycles;
  /// <summary>
  /// 当前液体工作数量。
  /// </summary>
  public int CurrentWorkCount { get; set; }
  /// <summary>
  /// 是否正在清理停滞液体工作。
  /// </summary>
  public bool IsStuckCleanup { get; set; }
  /// <summary>
  /// 是否启用快速下落策略。
  /// </summary>
  public bool QuickFall { get; set; }
  /// <summary>
  /// 是否启用快速稳定策略。
  /// </summary>
  public bool QuickSettle { get; set; }
  /// <summary>
  /// 液体跳过统计计数。
  /// </summary>
  public int SkipCount { get; set; }
  /// <summary>
  /// 液体停滞相关计数。
  /// </summary>
  public int StuckCount { get; set; }
  /// <summary>
  /// 液体停滞数量或程度输入。
  /// </summary>
  public int StuckAmount { get; set; }
  /// <summary>
  /// 原版液体更新 wetCounter 计数。
  /// </summary>
  public int WetCounter { get; set; }
  /// <summary>
  /// 紧急处理计数。
  /// </summary>
  public int PanicCounter { get; set; }
  /// <summary>
  /// 是否进入紧急液体处理模式。
  /// </summary>
  public bool PanicMode { get; set; }
  /// <summary>
  /// 紧急处理使用的纵向位置或游标。
  /// </summary>
  public int PanicY { get; set; }
}
