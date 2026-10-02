using System;
using System.Collections.Frozen;
using System.Collections.Generic;

namespace Terraria.WorldGeneration.Components;

public readonly record struct WorldSkyblockGenerationScanSnapshot
{
  public WorldSkyblockGenerationScanSnapshot(
    long generationId,
    ulong scanVersion,
    long worldTileCount,
    int currentActiveTiles,
    IReadOnlySet<ushort> activeTileTypes,
    IReadOnlySet<ushort> wallTypes)
  {
    ArgumentOutOfRangeException.ThrowIfNegative(generationId);
    if (scanVersion == 0)
    {
      throw new ArgumentOutOfRangeException(nameof(scanVersion));
    }

    if (worldTileCount < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(worldTileCount));
    }

    ArgumentOutOfRangeException.ThrowIfNegative(currentActiveTiles);
    ArgumentNullException.ThrowIfNull(activeTileTypes);
    ArgumentNullException.ThrowIfNull(wallTypes);
    HashSet<ushort> activeTileCopy = new(activeTileTypes);
    HashSet<ushort> wallCopy = new(wallTypes);
    GenerationId = generationId;
    ScanVersion = scanVersion;
    WorldTileCount = worldTileCount;
    CurrentActiveTiles = currentActiveTiles;
    ActiveTileTypes = activeTileCopy.ToFrozenSet();
    WallTypes = wallCopy.ToFrozenSet();
  }

  public long GenerationId { get; }

  public ulong ScanVersion { get; }

  public long WorldTileCount { get; }

  public int CurrentActiveTiles { get; }

  public IReadOnlySet<ushort> ActiveTileTypes { get; }

  public IReadOnlySet<ushort> WallTypes { get; }

  public bool HasTile(ushort tileType)
  {
    return ActiveTileTypes.Contains(tileType);
  }

  public bool HasWall(ushort wallType)
  {
    return WallTypes.Contains(wallType);
  }
}
