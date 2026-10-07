using System;
using System.Collections.Generic;
using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Housing;

public sealed class HousingRoomVisitedTilesContext
{
  private readonly HashSet<TilePosition> _visitedTiles = new();
  private readonly List<TilePosition> _visitedTilesInOrder = new();
  private readonly TilePosition _startPosition;

  public HousingRoomVisitedTilesContext(
    TilePosition startPosition,
    int maxRoomTiles,
    int maxRoomSize)
  {
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxRoomTiles);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxRoomSize);

    _startPosition = startPosition;
    MaxRoomTiles = maxRoomTiles;
    MaxRoomSize = maxRoomSize;
    ResetBounds();
  }

  public int MaxRoomTiles { get; }

  public int MaxRoomSize { get; }

  public int NumRoomTiles { get; private set; }

  public int RoomX1 { get; private set; }

  public int RoomX2 { get; private set; }

  public int RoomY1 { get; private set; }

  public int RoomY2 { get; private set; }

  public int VisitedTileCount => _visitedTiles.Count;

  public bool IsAtTileLimit => NumRoomTiles >= MaxRoomTiles;

  public bool HasReachedRoomSizeLimit =>
    RoomX2 - RoomX1 >= MaxRoomSize || RoomY2 - RoomY1 >= MaxRoomSize;

  public bool Contains(TilePosition position)
  {
    return _visitedTiles.Contains(position);
  }

  public IReadOnlyList<TilePosition> CreateSnapshot()
  {
    return Array.AsReadOnly(_visitedTilesInOrder.ToArray());
  }

  public bool WouldExceedRoomSize(TilePosition position)
  {
    if (NumRoomTiles == 0)
    {
      return false;
    }

    long nextX1 = Math.Min(RoomX1, position.X);
    long nextX2 = Math.Max(RoomX2, position.X);
    long nextY1 = Math.Min(RoomY1, position.Y);
    long nextY2 = Math.Max(RoomY2, position.Y);
    return nextX2 - nextX1 >= MaxRoomSize ||
      nextY2 - nextY1 >= MaxRoomSize;
  }

  public bool IsWithinSearchWindow(TilePosition position)
  {
    return Math.Abs((long)position.X - _startPosition.X) <= MaxRoomSize &&
      Math.Abs((long)position.Y - _startPosition.Y) <= MaxRoomSize;
  }

  public bool TryMarkVisited(TilePosition position)
  {
    if (IsAtTileLimit || !IsWithinSearchWindow(position) || !_visitedTiles.Add(position))
    {
      return false;
    }

    _visitedTilesInOrder.Add(position);
    NumRoomTiles++;
    return true;
  }

  public void IncludeInRoomBounds(TilePosition position)
  {
    if (NumRoomTiles == 0)
    {
      RoomX1 = position.X;
      RoomX2 = position.X;
      RoomY1 = position.Y;
      RoomY2 = position.Y;
      return;
    }

    RoomX1 = Math.Min(RoomX1, position.X);
    RoomX2 = Math.Max(RoomX2, position.X);
    RoomY1 = Math.Min(RoomY1, position.Y);
    RoomY2 = Math.Max(RoomY2, position.Y);
  }

  public bool TryAdd(TilePosition position)
  {
    if (IsAtTileLimit || WouldExceedRoomSize(position) || !_visitedTiles.Add(position))
    {
      return false;
    }

    _visitedTilesInOrder.Add(position);
    IncludeInRoomBounds(position);
    NumRoomTiles++;
    return true;
  }

  public void Clear()
  {
    _visitedTiles.Clear();
    _visitedTilesInOrder.Clear();
    NumRoomTiles = 0;
    ResetBounds();
  }

  private void ResetBounds()
  {
    RoomX1 = _startPosition.X;
    RoomX2 = _startPosition.X;
    RoomY1 = _startPosition.Y;
    RoomY2 = _startPosition.Y;
  }
}
