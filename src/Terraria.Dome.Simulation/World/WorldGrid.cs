using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using LiquidChangeCommand = Terraria.Dome.Simulation.Commands.LiquidChangeCommand;

namespace Terraria.Dome.Simulation.WorldModel;

public sealed class WorldGrid
{
  public const int SectionHeight = 150;
  public const int SectionWidth = 200;

  private readonly long[,] _sectionVersions;
  private readonly WorldTile[,] _tiles;
  private readonly List<TileChangeCommand> _tileChanges = new();
  private readonly List<TileFrameCommand> _tileFrameChanges = new();

  public WorldGrid(int width, int height, bool initializeLegacyEmptyFrames = false)
  {
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
    if (height % SectionHeight != 0 || width % SectionWidth != 0)
    {
      throw new ArgumentException(
        "World dimensions must be whole Terraria section units.",
        nameof(width));
    }

    Height = height;
    Width = width;
    _tiles = new WorldTile[width, height];
    _sectionVersions = new long[width / SectionWidth, height / SectionHeight];
    if (initializeLegacyEmptyFrames)
    {
      WorldTile emptyTile = new(IsActive: false, Type: 0, FrameX: -1, FrameY: -1);
      for (int x = 0; x < width; x++)
      {
        for (int y = 0; y < height; y++)
        {
          _tiles[x, y] = emptyTile;
        }
      }
    }
  }

  public int Height { get; }
  public int Width { get; }

  public bool Contains(int x, int y)
  {
    return x >= 0 && x < Width && y >= 0 && y < Height;
  }

  public WorldSectionSnapshot CreateSectionSnapshot(WorldSectionCoordinates coordinates)
  {
    ValidateSectionCoordinates(coordinates);
    WorldTile[,] tiles = new WorldTile[SectionWidth, SectionHeight];
    int originX = coordinates.X * SectionWidth;
    int originY = coordinates.Y * SectionHeight;
    for (int y = 0; y < SectionHeight; y++)
    {
      for (int x = 0; x < SectionWidth; x++)
      {
        tiles[x, y] = _tiles[originX + x, originY + y];
      }
    }

    return new WorldSectionSnapshot(
      coordinates,
      SectionWidth,
      SectionHeight,
      _sectionVersions[coordinates.X, coordinates.Y],
      tiles);
  }

  public WorldSectionCoordinates GetSectionCoordinates(int x, int y)
  {
    ValidateTileCoordinates(x, y);
    return new WorldSectionCoordinates(x / SectionWidth, y / SectionHeight);
  }

  public WorldTile GetTile(int x, int y)
  {
    ValidateTileCoordinates(x, y);
    return _tiles[x, y];
  }

  public long GetSectionVersion(WorldSectionCoordinates coordinates)
  {
    ValidateSectionCoordinates(coordinates);
    return _sectionVersions[coordinates.X, coordinates.Y];
  }

  public bool TrySetTile(int x, int y, WorldTile tile)
  {
    if (x < 0 || x >= Width || y < 0 || y >= Height)
    {
      return false;
    }

    if (_tiles[x, y] == tile)
    {
      return true;
    }

    WorldSectionCoordinates coordinates = GetSectionCoordinates(x, y);
    if (!TryAdvanceSectionVersion(coordinates))
    {
      return false;
    }

    _tiles[x, y] = tile;
    return true;
  }

  public bool TrySetLiquid(int x, int y, byte amount, byte type)
  {
    if (x < 0 || x >= Width || y < 0 || y >= Height)
    {
      return false;
    }

    WorldTile current = _tiles[x, y];
    if (current.LiquidAmount == amount && current.LiquidType == type)
    {
      return true;
    }

    WorldSectionCoordinates coordinates = GetSectionCoordinates(x, y);
    if (!TryAdvanceSectionVersion(coordinates))
    {
      return false;
    }

    _tiles[x, y] = current with { LiquidAmount = amount, LiquidType = type };
    return true;
  }

  private bool TryAdvanceSectionVersion(WorldSectionCoordinates coordinates)
  {
    if (_sectionVersions[coordinates.X, coordinates.Y] == long.MaxValue)
    {
      return false;
    }

    _sectionVersions[coordinates.X, coordinates.Y]++;
    return true;
  }

  public void CommitLiquidChanges(IReadOnlyList<LiquidChangeCommand> changes)
  {
    ArgumentNullException.ThrowIfNull(changes);
    List<LiquidChangeCommand> ordered = [.. changes];
    ordered.Sort(LiquidChangeCommandComparer.Instance);
    Dictionary<WorldSectionCoordinates, int> versionDeltas = new();
    for (int index = 0; index < ordered.Count; index++)
    {
      LiquidChangeCommand change = ordered[index];
      if (change.Sequence < 0 || change.Sequence >= long.MaxValue || !Contains(change.X, change.Y))
      {
        throw new InvalidOperationException("A queued liquid change was outside the world.");
      }

      WorldTile current = _tiles[change.X, change.Y];
      if (current.LiquidAmount == change.Amount && current.LiquidType == change.Type)
      {
        continue;
      }

      WorldSectionCoordinates coordinates = GetSectionCoordinates(change.X, change.Y);
      int delta = versionDeltas.TryGetValue(coordinates, out int existing) ? existing + 1 : 1;
      if (_sectionVersions[coordinates.X, coordinates.Y] > long.MaxValue - delta)
      {
        throw new InvalidOperationException("A liquid change would exhaust its section version.");
      }

      versionDeltas[coordinates] = delta;
    }

    for (int index = 0; index < ordered.Count; index++)
    {
      LiquidChangeCommand change = ordered[index];
      if (!TrySetLiquid(change.X, change.Y, change.Amount, change.Type))
      {
        throw new InvalidOperationException("A queued liquid change was outside the world.");
      }
    }
  }

  public void EnqueueTileChange(TileChangeCommand command)
  {
    if (command.Sequence < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(command));
    }

    _tileChanges.Add(command);
  }

  public void EnqueueTileFrameChange(TileFrameCommand command)
  {
    if (command.Sequence < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(command));
    }

    _tileFrameChanges.Add(command);
  }

  public void CommitTileChanges()
  {
    TileChangeCommitSystem commitSystem = new();
    if (!commitSystem.TryCommit(this, _tileChanges, out TileChangeCommitResult result))
    {
      _tileChanges.Clear();
      throw new InvalidOperationException(result.FailureReason);
    }

    _tileChanges.Clear();
    if (!commitSystem.TryCommit(this, _tileFrameChanges, out TileFrameCommitResult frameResult))
    {
      _tileFrameChanges.Clear();
      throw new InvalidOperationException(frameResult.FailureReason);
    }

    _tileFrameChanges.Clear();
  }

  public WorldGridSnapshot CreateSnapshot(WorldMetadata metadata)
  {
    ArgumentNullException.ThrowIfNull(metadata);
    if (metadata.Width != Width || metadata.Height != Height)
    {
      throw new ArgumentException(
        "World metadata dimensions must match the grid.",
        nameof(metadata));
    }

    return new WorldGridSnapshot(
      metadata,
      (WorldTile[,])_tiles.Clone(),
      (long[,])_sectionVersions.Clone());
  }

  public static WorldGrid FromSnapshot(WorldGridSnapshot snapshot)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    WorldGrid grid = new(snapshot.Metadata.Width, snapshot.Metadata.Height);
    WorldTile[,] tiles = snapshot.CopyTiles();
    long[,] sectionVersions = snapshot.CopySectionVersions();
    Array.Copy(tiles, grid._tiles, tiles.Length);
    Array.Copy(sectionVersions, grid._sectionVersions, sectionVersions.Length);
    return grid;
  }

  private sealed class LiquidChangeCommandComparer : IComparer<LiquidChangeCommand>
  {
    public static readonly LiquidChangeCommandComparer Instance = new();

    public int Compare(LiquidChangeCommand first, LiquidChangeCommand second)
    {
      int sequence = first.Sequence.CompareTo(second.Sequence);
      if (sequence != 0)
      {
        return sequence;
      }

      int x = first.X.CompareTo(second.X);
      if (x != 0)
      {
        return x;
      }

      int y = first.Y.CompareTo(second.Y);
      if (y != 0)
      {
        return y;
      }

      int type = first.Type.CompareTo(second.Type);
      return type != 0 ? type : first.Amount.CompareTo(second.Amount);
    }
  }

  private void ValidateSectionCoordinates(WorldSectionCoordinates coordinates)
  {
    if (coordinates.X < 0 || coordinates.X >= _sectionVersions.GetLength(0) ||
        coordinates.Y < 0 || coordinates.Y >= _sectionVersions.GetLength(1))
    {
      throw new ArgumentOutOfRangeException(nameof(coordinates));
    }
  }

  private void ValidateTileCoordinates(int x, int y)
  {
    if (x < 0 || x >= Width || y < 0 || y >= Height)
    {
      throw new ArgumentOutOfRangeException(nameof(x));
    }
  }
}
