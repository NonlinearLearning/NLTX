using System;
using System.Collections.Generic;

namespace Terraria.WorldGeneration.Components;

/// <summary>
/// 保存世界生成的出生点随机化、陆块和颠倒世界地层数据。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.WorldBuilding.GenVars。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria.WorldBuilding/GenVars.cs。</para>
/// <para>
/// 主要源成员：worldSpawnHasBeenRandomized（第 34 行）； landmassData（第 36 行）； remixSurfaceLayerLow（第 38 行）；
/// remixSurfaceLayerHigh（第 40 行）； remixMushroomLayerLow（第 42 行）； remixMushroomLayerHigh（第 44 行）；
/// boulderPetsPlaced（第 48 行）。
/// </para>
/// <para>重组说明：GenerationId 用于拆分后的版本或实例生命周期管理。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-p17-world-terrain-biomes-genvars-component-design.md。
/// </para>
/// <para>依据位置：第 497 行。</para>
/// </remarks>
public sealed class WorldSpawnAndLandmassComponent
{
  private WorldGenerationLandmassValue[] _landmassData;
  private IReadOnlyList<WorldGenerationLandmassValue> _landmassDataView;

  public WorldSpawnAndLandmassComponent(
    long generationId,
    bool worldSpawnHasBeenRandomized = false,
    IReadOnlyList<WorldGenerationLandmassValue>? landmassData = null,
    int remixSurfaceLayerLow = 0,
    int remixSurfaceLayerHigh = 0,
    int remixMushroomLayerLow = 0,
    int remixMushroomLayerHigh = 0,
    int boulderPetsPlaced = 0)
  {
    if (generationId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(generationId));
    }

    GenerationId = generationId;
    WorldSpawnHasBeenRandomized = worldSpawnHasBeenRandomized;
    RemixSurfaceLayerLow = remixSurfaceLayerLow;
    RemixSurfaceLayerHigh = remixSurfaceLayerHigh;
    RemixMushroomLayerLow = remixMushroomLayerLow;
    RemixMushroomLayerHigh = remixMushroomLayerHigh;
    BoulderPetsPlaced = boulderPetsPlaced;
    _landmassData = CopyLandmassData(landmassData);
    _landmassDataView = Array.AsReadOnly(_landmassData);
  }

  public long GenerationId { get; }

  public bool WorldSpawnHasBeenRandomized { get; private set; }

  public IReadOnlyList<WorldGenerationLandmassValue> LandmassData =>
    _landmassDataView;

  public int RemixSurfaceLayerLow { get; private set; }

  public int RemixSurfaceLayerHigh { get; private set; }

  public int RemixMushroomLayerLow { get; private set; }

  public int RemixMushroomLayerHigh { get; private set; }

  public int BoulderPetsPlaced { get; private set; }

  internal void ReplaceState(
    bool worldSpawnHasBeenRandomized,
    IReadOnlyList<WorldGenerationLandmassValue>? landmassData,
    int remixSurfaceLayerLow,
    int remixSurfaceLayerHigh,
    int remixMushroomLayerLow,
    int remixMushroomLayerHigh,
    int boulderPetsPlaced)
  {
    WorldSpawnHasBeenRandomized = worldSpawnHasBeenRandomized;
    RemixSurfaceLayerLow = remixSurfaceLayerLow;
    RemixSurfaceLayerHigh = remixSurfaceLayerHigh;
    RemixMushroomLayerLow = remixMushroomLayerLow;
    RemixMushroomLayerHigh = remixMushroomLayerHigh;
    BoulderPetsPlaced = boulderPetsPlaced;
    _landmassData = CopyLandmassData(landmassData);
    _landmassDataView = Array.AsReadOnly(_landmassData);
  }

  public WorldSpawnAndLandmassSnapshot CreateSnapshot()
  {
    return new WorldSpawnAndLandmassSnapshot(
      GenerationId,
      WorldSpawnHasBeenRandomized,
      Array.AsReadOnly(CopyLandmassData(_landmassData)),
      RemixSurfaceLayerLow,
      RemixSurfaceLayerHigh,
      RemixMushroomLayerLow,
      RemixMushroomLayerHigh,
      BoulderPetsPlaced);
  }

  private static WorldGenerationLandmassValue[] CopyLandmassData(
    IReadOnlyList<WorldGenerationLandmassValue>? source)
  {
    if (source is null || source.Count == 0)
    {
      return Array.Empty<WorldGenerationLandmassValue>();
    }

    WorldGenerationLandmassValue[] copy = new WorldGenerationLandmassValue[source.Count];
    for (int index = 0; index < source.Count; index++)
    {
      copy[index] = source[index];
    }

    return copy;
  }
}
