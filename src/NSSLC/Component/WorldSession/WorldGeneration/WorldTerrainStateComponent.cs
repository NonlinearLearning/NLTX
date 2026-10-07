using System;

namespace Terraria.WorldGeneration.Components;

/// <summary>
/// 保存世界地形边界、群落范围和特殊地表规则。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.WorldGen。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/WorldGen.cs。</para>
/// <para>主要源成员：beachDistance（第 4109 行）。</para>
/// <para>重组说明：GenerationId、TerrainRevision 用于拆分后的版本或实例生命周期管理。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/non-authoritative/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-non-authoritative-P03-world-generation-dungeons-component-design.md。
/// </para>
/// <para>依据位置：第 209 行。</para>
/// </remarks>
public sealed class WorldTerrainStateComponent
{
  public WorldTerrainStateComponent(long generationId)
  {
    if (generationId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(generationId));
    }

    GenerationId = generationId;
  }

  public long GenerationId { get; }

  public ulong TerrainRevision { get; init; }

  public int? UnderworldLayerY { get; init; }

  public int? OceanLevelY { get; init; }

  public int? BeachDistance { get; init; }

  public int? BeachSandDepth { get; init; }

  public double? SurfaceOffset { get; init; }

  public TilePosition? Jungle { get; init; }

  public TilePosition? Snow { get; init; }

  public TilePosition? Desert { get; init; }

  public bool? SurfaceIsDesert { get; init; }

  public bool? SurfaceIsMushrooms { get; init; }

  public bool? SurfaceIsInSpace { get; init; }

  public bool? IsOceanAtSpawn { get; init; }

  public bool? IsBeachAtSpawn { get; init; }

  public bool? IsNoSurface { get; init; }

  public bool? IsRemix { get; init; }

  public bool? IsErrorWorld { get; init; }
}
