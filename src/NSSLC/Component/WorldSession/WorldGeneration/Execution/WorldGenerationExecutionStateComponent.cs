using System;

namespace Terraria.WorldGeneration.Components;

/// <summary>
/// Stores the world-generation lifecycle and diagnostic flags owned by P19.
/// </summary>
/// <remarks>
/// <para>职责：保存世界生成正在执行、连续小块处理和陷阱放置状态。</para>
/// <para>拆分来源：Terraria.WorldGen。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/WorldGen.cs。</para>
/// <para>
/// 主要源成员：generatingWorld（第 4287 行）； SmallConsecutivesFound（第 4302 行）；
/// SmallConsecutivesEliminated（第 4304 行）； placingTraps（第 4326 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-P19-world-generation-execution-component-design.md。
/// </para>
/// <para>依据位置：第 19 行。</para>
/// </remarks>
public sealed class WorldGenerationExecutionStateComponent
{
  public WorldGenerationExecutionStateComponent(
    bool generatingWorld = false,
    int smallConsecutivesFound = 0,
    int smallConsecutivesEliminated = 0,
    bool placingTraps = false)
  {
    ReplaceState(
      generatingWorld,
      smallConsecutivesFound,
      smallConsecutivesEliminated,
      placingTraps);
  }

  public bool GeneratingWorld { get; private set; }

  public int SmallConsecutivesFound { get; private set; }

  public int SmallConsecutivesEliminated { get; private set; }

  public bool PlacingTraps { get; private set; }

  internal void ReplaceState(
    bool generatingWorld,
    int smallConsecutivesFound,
    int smallConsecutivesEliminated,
    bool placingTraps)
  {
    ArgumentOutOfRangeException.ThrowIfNegative(smallConsecutivesFound);
    ArgumentOutOfRangeException.ThrowIfNegative(smallConsecutivesEliminated);

    GeneratingWorld = generatingWorld;
    SmallConsecutivesFound = smallConsecutivesFound;
    SmallConsecutivesEliminated = smallConsecutivesEliminated;
    PlacingTraps = placingTraps;
  }
}
