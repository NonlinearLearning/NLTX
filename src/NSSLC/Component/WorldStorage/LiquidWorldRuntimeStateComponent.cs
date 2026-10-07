namespace Terraria.WorldStorage;

/// <summary>
/// 保存世界液体模拟的容量、循环预算和应急状态。
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
/// </remarks>
public sealed class LiquidWorldRuntimeStateComponent
{
  public const int DefaultMaxLiquidBuffer = 50000;
  public const int DefaultMaxLiquid = 25000;
  public const int DefaultCycles = 10;

  public int MaxLiquidBuffer { get; set; } = DefaultMaxLiquidBuffer;
  public int MaxLiquid { get; set; } = DefaultMaxLiquid;
  public int CurrentMaxLiquid { get; set; } = DefaultMaxLiquid;
  public int Cycles { get; set; } = DefaultCycles;
  public int CurrentWorkCount { get; set; }
  public bool IsStuckCleanup { get; set; }
  public bool QuickFall { get; set; }
  public bool QuickSettle { get; set; }
  public int SkipCount { get; set; }
  public int StuckCount { get; set; }
  public int StuckAmount { get; set; }
  public int WetCounter { get; set; }
  public int PanicCounter { get; set; }
  public bool PanicMode { get; set; }
  public int PanicY { get; set; }
}
