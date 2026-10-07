namespace Terraria.WorldStorage;

/// <summary>
/// 保存单个液体工作项的坐标、延迟、跳过和重试状态。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Liquid。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Liquid.cs。</para>
/// <para>主要源成员：x（第 44 行）； y（第 46 行）； kill（第 48 行）； delay（第 50 行）。</para>
/// <para>重组说明：Sequence、RetryCount 和入队阶段是液体工作调度新增的状态。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-1/design/。</para>
/// <para>拆分依据文件：2026-09-05-version4-liquid-simulation-component-design.md。</para>
/// <para>依据位置：第 110 行。</para>
/// </remarks>
public struct LiquidWorkStateComponent
{
  public TileCoordinate Coordinate;
  public bool IsPending;
  public bool SkipOnce;
  public int KillScore;
  public int Delay;
  public long Sequence;
  public int RetryCount;
  public bool IsBuffered;
}
