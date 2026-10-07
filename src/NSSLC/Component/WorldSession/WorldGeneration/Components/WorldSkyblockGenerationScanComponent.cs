using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using Terraria.WorldGeneration.Definitions;

namespace Terraria.WorldGeneration.Components;

/// <summary>
/// 保存空岛规则扫描的方块、墙和活动数量及提交状态。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.WorldGen.Skyblock。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/WorldGen.cs。</para>
/// <para>
/// 主要源成员：noAltars（第 3143 行）； noDungeon（第 3145 行）； noTemple（第 3147 行）； noHellstone（第 3149 行）；
/// noFossils（第 3151 行）； noLifeCrystals（第 3153 行）； noHellforge（第 3155 行）； lowTiles（第 3157 行）；
/// hasTile（第 3159 行）； hasWall（第 3161 行）； currentActiveTiles（第 3163 行）； denyFloatingIslands（第 3165
/// 行）； denyAllGeneration（第 3177 行）； denySomeGeneration（第 3179 行）。
/// </para>
/// <para>重组说明：分列扫描游标、扫描版本、提交前提和显式生命周期是新增状态。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-p18-world-seeds-skyblock-definitions-component-design.md。
/// </para>
/// <para>依据位置：第 132 行。</para>
/// </remarks>
public sealed class WorldSkyblockGenerationScanComponent
{
  private readonly bool[] _hasTile;
  private readonly bool[] _hasWall;
  private int _currentActiveTiles;
  private long _worldTileCount;
  private int _nextColumn;
  private WorldSkyblockGenerationDimensions _columnScanDimensions;
  private bool _isColumnScan;
  private WorldSkyblockGenerationRulesSelection? _lastCommittedRules;

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

  public WorldSkyblockGenerationRulesSelection? LastCommittedRules => _lastCommittedRules;

  public bool PreviousLowTiles => _lastCommittedRules?.LowTiles ?? false;

  internal bool CanCommitScan =>
    !_isColumnScan || _nextColumn == _columnScanDimensions.MaxTilesX;

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
    _isColumnScan = false;
    _nextColumn = 0;
    _columnScanDimensions = default;
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

  internal void BeginColumnScan(
    WorldSkyblockGenerationDimensions dimensions,
    ulong scanVersion)
  {
    if (dimensions.MaxTilesX <= 0)
    {
      throw new ArgumentOutOfRangeException(
        nameof(dimensions),
        "An incremental scan requires at least one world column.");
    }

    BeginScan(scanVersion, dimensions.WorldTileCount);
    _isColumnScan = true;
    _columnScanDimensions = dimensions;
  }

  internal void PrepareColumn(
    int x,
    WorldSkyblockGenerationDimensions dimensions,
    ulong scanVersion)
  {
    if (x == 0)
    {
      if (Lifecycle != WorldSkyblockGenerationScanLifecycle.Ready)
      {
        throw new InvalidOperationException(
          "A new incremental scan cannot begin before the active scan is committed.");
      }

      BeginColumnScan(dimensions, scanVersion);
    }

    if (!_isColumnScan ||
        Lifecycle != WorldSkyblockGenerationScanLifecycle.Scanning ||
        ScanVersion != scanVersion ||
        _columnScanDimensions != dimensions ||
        x != _nextColumn)
    {
      throw new InvalidOperationException(
        "Incremental Skyblock columns must be observed once, in order, for one world scan.");
    }

    _nextColumn++;
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
    _isColumnScan = false;
    _nextColumn = 0;
    _columnScanDimensions = default;
  }

  internal void SetCommittedRules(WorldSkyblockGenerationRulesSelection rules)
  {
    _lastCommittedRules = rules;
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
