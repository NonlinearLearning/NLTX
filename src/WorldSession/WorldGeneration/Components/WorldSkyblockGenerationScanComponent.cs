using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using Terraria.WorldGeneration.Definitions;

namespace Terraria.WorldGeneration.Components;

public sealed class WorldSkyblockGenerationScanComponent
{
  private readonly bool[] _hasTile;
  private readonly bool[] _hasWall;
  private int _currentActiveTiles;
  private long _worldTileCount;

  public WorldSkyblockGenerationScanComponent(
    long generationId,
    int tileTypeCount,
    int wallTypeCount)
  {
    ArgumentOutOfRangeException.ThrowIfNegative(generationId);
    if (tileTypeCount <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(tileTypeCount));
    }

    if (wallTypeCount <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(wallTypeCount));
    }

    GenerationId = generationId;
    _hasTile = new bool[tileTypeCount];
    _hasWall = new bool[wallTypeCount];
    Lifecycle = WorldSkyblockGenerationScanLifecycle.Ready;
  }

  public long GenerationId { get; }

  public ulong ScanVersion { get; private set; }

  public WorldSkyblockGenerationScanLifecycle Lifecycle { get; private set; }

  public int TileTypeCount => _hasTile.Length;

  public int WallTypeCount => _hasWall.Length;

  public void BeginScan(
    ulong scanVersion,
    long worldTileCount)
  {
    if (scanVersion == 0)
    {
      throw new ArgumentOutOfRangeException(nameof(scanVersion));
    }

    if (worldTileCount < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(worldTileCount));
    }

    ClearAccumulation();
    ScanVersion = scanVersion;
    _worldTileCount = worldTileCount;
    Lifecycle = WorldSkyblockGenerationScanLifecycle.Scanning;
  }

  public void RecordTile(
    ushort tileType,
    ushort wallType,
    bool isActive)
  {
    if (Lifecycle != WorldSkyblockGenerationScanLifecycle.Scanning)
    {
      throw new InvalidOperationException(
        "Tile observations require an active Skyblock scan.");
    }

    if (tileType >= _hasTile.Length)
    {
      throw new ArgumentOutOfRangeException(nameof(tileType));
    }

    if (wallType >= _hasWall.Length)
    {
      throw new ArgumentOutOfRangeException(nameof(wallType));
    }

    if (isActive)
    {
      _currentActiveTiles++;
      _hasTile[tileType] = true;
    }

    _hasWall[wallType] = true;
  }

  public WorldSkyblockGenerationScanSnapshot CreateSnapshot()
  {
    if (Lifecycle != WorldSkyblockGenerationScanLifecycle.Scanning)
    {
      throw new InvalidOperationException(
        "A Skyblock scan snapshot requires an active scan.");
    }

    return new WorldSkyblockGenerationScanSnapshot(
      GenerationId,
      ScanVersion,
      _worldTileCount,
      _currentActiveTiles,
      CreatePresenceSet(_hasTile),
      CreatePresenceSet(_hasWall));
  }

  public void ResetAfterCommit()
  {
    if (Lifecycle != WorldSkyblockGenerationScanLifecycle.Scanning)
    {
      throw new InvalidOperationException(
        "A Skyblock scan can only be reset after an active scan.");
    }

    ClearAccumulation();
    Lifecycle = WorldSkyblockGenerationScanLifecycle.Ready;
  }

  private void ClearAccumulation()
  {
    Array.Clear(_hasTile);
    Array.Clear(_hasWall);
    _currentActiveTiles = 0;
    _worldTileCount = 0;
  }

  private static FrozenSet<ushort> CreatePresenceSet(bool[] presence)
  {
    List<ushort> values = new();
    for (int index = 0; index < presence.Length; index++)
    {
      if (presence[index])
      {
        values.Add((ushort)index);
      }
    }

    return values.ToFrozenSet();
  }
}
