using System;
using System.Collections.Generic;
using Terraria.WorldGeneration.Adapters;
using Terraria.WorldGeneration.Components;
using Terraria.WorldGeneration.Definitions;
using Terraria.WorldGeneration.Queries;

namespace Terraria.WorldGeneration.Systems;

public static class WorldSkyblockGenerationScanSystem
{
  public static WorldSkyblockGenerationScanSnapshot Scan(
    WorldSkyblockGenerationScanComponent component,
    WorldSkyblockGenerationDimensions dimensions,
    ulong scanVersion,
    IWorldSkyblockGenerationGridReader gridReader)
  {
    ArgumentNullException.ThrowIfNull(component);
    ArgumentNullException.ThrowIfNull(gridReader);
    component.BeginScan(scanVersion, dimensions.WorldTileCount);
    int maxX = dimensions.MaxTilesX - 40;
    int maxY = dimensions.MaxTilesY - 40;
    for (int x = 40; x < maxX; x++)
    {
      for (int y = 40; y < maxY; y++)
      {
        WorldSkyblockGenerationTileObservation observation = gridReader.Read(x, y);
        component.RecordTile(
          observation.TileType,
          observation.WallType,
          observation.IsActive);
      }
    }

    return component.CreateSnapshot();
  }

  public static WorldSkyblockGenerationRulesCommitResult ScanAndCommit(
    WorldSkyblockGenerationScanComponent component,
    WorldSkyblockGenerationDimensions dimensions,
    ulong scanVersion,
    IWorldSkyblockGenerationGridReader gridReader,
    IReadOnlySet<ushort> dungeonTileTypes,
    IReadOnlySet<ushort> dungeonWallTypes,
    bool skyblockWorld,
    ISkyblockGenerationEffectsPort effects)
  {
    WorldSkyblockGenerationScanSnapshot scan = Scan(
      component,
      dimensions,
      scanVersion,
      gridReader);
    return EvaluateAndCommit(
      component,
      scan,
      dungeonTileTypes,
      dungeonWallTypes,
      skyblockWorld,
      effects);
  }

  public static WorldSkyblockGenerationScanSnapshot? AccumulateColumn(
    WorldSkyblockGenerationScanComponent component,
    WorldSkyblockGenerationDimensions dimensions,
    int x,
    ulong scanVersion,
    IWorldSkyblockGenerationGridReader gridReader)
  {
    ArgumentNullException.ThrowIfNull(component);
    ArgumentNullException.ThrowIfNull(gridReader);
    if (x < 0 || x >= dimensions.MaxTilesX)
    {
      throw new ArgumentOutOfRangeException(nameof(x));
    }

    component.PrepareColumn(x, dimensions, scanVersion);
    int maxY = dimensions.MaxTilesY - 40;
    int surfaceBoundary = dimensions.WorldSurfaceY.HasValue
      ? dimensions.WorldSurfaceY.Value + 1
      : 40;
    RecordColumnRows(component, gridReader, x, 40, surfaceBoundary);
    RecordColumnRows(component, gridReader, x, surfaceBoundary, maxY);

    return x == dimensions.MaxTilesX - 1
      ? component.CreateSnapshot()
      : null;
  }

  private static void RecordColumnRows(
    WorldSkyblockGenerationScanComponent component,
    IWorldSkyblockGenerationGridReader gridReader,
    int x,
    int minY,
    int maxY)
  {
    for (int y = minY; y < maxY; y++)
    {
      WorldSkyblockGenerationTileObservation observation = gridReader.Read(x, y);
      component.RecordTile(
        observation.TileType,
        observation.WallType,
        observation.IsActive);
    }
  }

  public static WorldSkyblockGenerationRulesCommitResult? AccumulateColumnAndCommit(
    WorldSkyblockGenerationScanComponent component,
    WorldSkyblockGenerationDimensions dimensions,
    int x,
    ulong scanVersion,
    IWorldSkyblockGenerationGridReader gridReader,
    IReadOnlySet<ushort> dungeonTileTypes,
    IReadOnlySet<ushort> dungeonWallTypes,
    bool skyblockWorld,
    ISkyblockGenerationEffectsPort effects)
  {
    WorldSkyblockGenerationScanSnapshot? scan = AccumulateColumn(
      component,
      dimensions,
      x,
      scanVersion,
      gridReader);
    if (!scan.HasValue)
    {
      return null;
    }

    return EvaluateAndCommit(
      component,
      scan.Value,
      dungeonTileTypes,
      dungeonWallTypes,
      skyblockWorld,
      effects);
  }

  private static WorldSkyblockGenerationRulesCommitResult EvaluateAndCommit(
    WorldSkyblockGenerationScanComponent component,
    WorldSkyblockGenerationScanSnapshot scan,
    IReadOnlySet<ushort> dungeonTileTypes,
    IReadOnlySet<ushort> dungeonWallTypes,
    bool skyblockWorld,
    ISkyblockGenerationEffectsPort effects)
  {
    ArgumentNullException.ThrowIfNull(dungeonTileTypes);
    ArgumentNullException.ThrowIfNull(dungeonWallTypes);
    WorldSkyblockGenerationRulesSelection rules = WorldSkyblockGenerationRulesQuery.Evaluate(
      new WorldSkyblockGenerationRulesInput(
        scan,
        dungeonTileTypes,
        dungeonWallTypes,
        skyblockWorld));
    return WorldSkyblockGenerationRulesCommitSystem.Commit(component, rules, effects);
  }
}
