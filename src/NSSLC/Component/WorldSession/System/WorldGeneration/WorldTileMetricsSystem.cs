using System;
using System.Collections.Generic;
using Terraria.WorldGeneration.Adapters;
using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Systems;

public static class WorldTileMetricsSystem
{
  private const int RemixEvilTileId = 474;
  private const int RemixBloodTileId = 195;
  private const int TileScanBoundaryMargin = 40;
  private const int TileScanTickInterval = 30;
  private const int SurfaceTileWeight = 5;
  private const int LowerTileWeight = 1;

  public static void EnsureTileTypeCount(
    WorldTileMetricsComponent component,
    int tileTypeCount)
  {
    ArgumentNullException.ThrowIfNull(component);
    component.EnsureTileTypeCount(tileTypeCount);
  }

  /// <summary>
  /// Exposes the active metric owner's scratch array to its legacy runtime adapter.
  /// </summary>
  public static int[] GetMutableTileCounts(WorldTileMetricsComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);
    return component.TileCounts;
  }

  public static void Reset(WorldTileMetricsComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);
    component.Reset();
  }

  public static WorldTileMetricsSnapshot CreatePendingSnapshot(
    WorldTileMetricsComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);
    return component.CreatePendingSnapshot();
  }

  public static WorldTileMetricsPublicationResult BeginColumn(
    WorldTileMetricsComponent component,
    int columnX)
  {
    ArgumentNullException.ThrowIfNull(component);
    if (columnX != 0)
    {
      return default;
    }

    component.PublishPendingCounts();
    return new WorldTileMetricsPublicationResult(
      true,
      component.CreateSnapshot(),
      true);
  }

  public static void CompleteColumnStart(
    WorldTileMetricsComponent component,
    int columnX)
  {
    ArgumentNullException.ThrowIfNull(component);
    if (columnX == 0)
    {
      component.ClearPendingCounts();
    }
  }

  public static bool AdvanceCadence(
    WorldTileMetricsComponent component,
    int maxTilesX,
    out int columnX)
  {
    ArgumentNullException.ThrowIfNull(component);
    if (maxTilesX <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(maxTilesX));
    }

    component.TileScanTickCount++;
    if (component.TileScanTickCount < TileScanTickInterval)
    {
      columnX = 0;
      return false;
    }

    component.TileScanTickCount = 0;
    if (component.ScheduledTileColumnX < 0)
    {
      component.ScheduledTileColumnX = component.NextTileColumnX;
    }

    columnX = component.ScheduledTileColumnX;
    return true;
  }

  /// <summary>
  /// Commits the scan cursor only after the scheduled column has completed successfully.
  /// </summary>
  public static void CompleteScheduledColumn(
    WorldTileMetricsComponent component,
    int columnX,
    int maxTilesX)
  {
    ArgumentNullException.ThrowIfNull(component);
    if (maxTilesX <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(maxTilesX));
    }

    if (columnX < 0 || columnX >= maxTilesX ||
        component.ScheduledTileColumnX != columnX ||
        component.NextTileColumnX != columnX)
    {
      throw new InvalidOperationException(
        "Only the currently scheduled tile column can advance the scan cursor.");
    }

    component.NextTileColumnX = columnX + 1;
    if (component.NextTileColumnX >= maxTilesX)
    {
      component.NextTileColumnX = 0;
    }

    component.ScheduledTileColumnX = -1;
  }

  /// <summary>
  /// Scans one Version4 column through explicit Tile and Skyblock effect ports.
  /// </summary>
  public static void ScanColumnTiles(
    int columnX,
    double worldSurface,
    int maxTilesY,
    IWorldTileMetricsTileSource tileSource,
    int[] tileCounts,
    Action<ushort> markWallPresent,
    Action incrementActiveTileCount,
    Action<ushort> markTilePresent)
  {
    ArgumentNullException.ThrowIfNull(tileSource);
    ArgumentNullException.ThrowIfNull(tileCounts);
    ArgumentNullException.ThrowIfNull(markWallPresent);
    ArgumentNullException.ThrowIfNull(incrementActiveTileCount);
    ArgumentNullException.ThrowIfNull(markTilePresent);

    int surfaceEndY = (int)(worldSurface + 1.0);
    ushort previousType = 0;
    int runWeight = 0;

    ScanColumnTileRange(
      columnX,
      TileScanBoundaryMargin,
      surfaceEndY,
      SurfaceTileWeight,
      tileSource,
      tileCounts,
      markWallPresent,
      incrementActiveTileCount,
      markTilePresent,
      ref previousType,
      ref runWeight);

    ScanColumnTileRange(
      columnX,
      surfaceEndY,
      maxTilesY - TileScanBoundaryMargin,
      LowerTileWeight,
      tileSource,
      tileCounts,
      markWallPresent,
      incrementActiveTileCount,
      markTilePresent,
      ref previousType,
      ref runWeight);
  }

  public static bool CompleteColumn(
    WorldTileMetricsComponent component,
    int[] tileCounts,
    IReadOnlyList<int> hallowCountCollection,
    IReadOnlyList<int> corruptCountCollection,
    IReadOnlyList<int> crimsonCountCollection,
    bool remixWorld,
    int columnX,
    int maxTilesX)
  {
    AccumulateAlignmentCounts(
      component,
      tileCounts,
      hallowCountCollection,
      corruptCountCollection,
      crimsonCountCollection,
      remixWorld);
    return columnX == maxTilesX - 1;
  }

  public static void AccumulateAlignmentCounts(
    WorldTileMetricsComponent component,
    int[] tileCounts,
    IReadOnlyList<int> hallowCountCollection,
    IReadOnlyList<int> corruptCountCollection,
    IReadOnlyList<int> crimsonCountCollection,
    bool remixWorld,
    bool clearCounts = false)
  {
    ArgumentNullException.ThrowIfNull(component);
    ArgumentNullException.ThrowIfNull(tileCounts);
    ArgumentNullException.ThrowIfNull(hallowCountCollection);
    ArgumentNullException.ThrowIfNull(corruptCountCollection);
    ArgumentNullException.ThrowIfNull(crimsonCountCollection);

    if (clearCounts)
    {
      component.ClearPendingCounts();
    }

    int hallowTotal = 0;
    for (int index = 0; index < hallowCountCollection.Count; index++)
    {
      int count = tileCounts[hallowCountCollection[index]];
      component.AddGoodCount(count);
      hallowTotal += count;
    }

    int corruptionTotal = 0;
    for (int index = 0; index < corruptCountCollection.Count; index++)
    {
      int count = tileCounts[corruptCountCollection[index]];
      component.AddEvilCount(count);
      corruptionTotal += count;
    }

    int crimsonTotal = 0;
    for (int index = 0; index < crimsonCountCollection.Count; index++)
    {
      int count = tileCounts[crimsonCountCollection[index]];
      component.AddBloodCount(count);
      crimsonTotal += count;
    }

    if (remixWorld)
    {
      int count = tileCounts[RemixEvilTileId];
      component.AddEvilCount(count);
      corruptionTotal += count;
      count = tileCounts[RemixBloodTileId];
      component.AddBloodCount(count);
      crimsonTotal += count;
    }

    component.AddSolidCount(
      tileCounts[2] +
      tileCounts[477] +
      tileCounts[1] +
      tileCounts[60] +
      tileCounts[53] +
      tileCounts[161]);
    component.AddSolidCount(hallowTotal);
    component.AddSolidCount(corruptionTotal);
    component.AddSolidCount(crimsonTotal);
    Array.Clear(tileCounts, 0, tileCounts.Length);
  }

  private static void ScanColumnTileRange(
    int columnX,
    int startY,
    int endY,
    int weight,
    IWorldTileMetricsTileSource tileSource,
    int[] tileCounts,
    Action<ushort> markWallPresent,
    Action incrementActiveTileCount,
    Action<ushort> markTilePresent,
    ref ushort previousType,
    ref int runWeight)
  {
    for (int y = startY; y < endY; y++)
    {
      WorldTileMetricsTileSample tile = tileSource.ReadOrCreateTile(columnX, y);
      markWallPresent(tile.Wall);
      if (!tile.IsActive)
      {
        continue;
      }

      incrementActiveTileCount();
      markTilePresent(tile.Type);

      ushort type = tile.Type;
      if (type == 0)
      {
        continue;
      }

      if (type == previousType)
      {
        runWeight += weight;
        continue;
      }

      tileCounts[previousType] += runWeight;
      previousType = type;
      runWeight = weight;
    }

    tileCounts[previousType] += runWeight;
    runWeight = 0;
  }
}
