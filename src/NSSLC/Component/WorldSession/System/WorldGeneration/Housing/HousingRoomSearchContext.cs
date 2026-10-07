using System.Collections.Generic;
using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Housing;

public sealed class HousingRoomSearchContext
{
  private readonly Stack<TilePosition> _roomCheckStack = new();

  public HousingRoomSearchContext(
    TilePosition startPosition,
    int maxRoomTiles,
    int maxRoomSize,
    int tileTypeCount)
  {
    VisitedTiles = new HousingRoomVisitedTilesContext(
      startPosition,
      maxRoomTiles,
      maxRoomSize);
    TileClassification = new HousingRoomTileClassificationContext(tileTypeCount);
    DiagnosticFlags = new HousingRoomDiagnosticFlags();
  }

  public HousingRoomVisitedTilesContext VisitedTiles { get; }

  public HousingRoomTileClassificationContext TileClassification { get; }

  public HousingRoomDiagnosticFlags DiagnosticFlags { get; }

  public TilePosition? LastFoundHouse { get; private set; }

  public bool IsTryingAlternateHousingSpot { get; private set; }

  public int? SharedRoomX { get; private set; }

  public int RoomCheckStackCount => _roomCheckStack.Count;

  public bool IsRoomCheckStackEmpty => _roomCheckStack.Count == 0;

  public void SetLastFoundHouse(TilePosition? position)
  {
    LastFoundHouse = position;
  }

  public void SetAlternateHousingSpot(bool isTryingAlternateHousingSpot)
  {
    IsTryingAlternateHousingSpot = isTryingAlternateHousingSpot;
  }

  public void SetSharedRoomX(int? sharedRoomX)
  {
    SharedRoomX = sharedRoomX;
  }

  public void PushRoomCheck(TilePosition position)
  {
    _roomCheckStack.Push(position);
  }

  public bool TryPopRoomCheck(out TilePosition position)
  {
    return _roomCheckStack.TryPop(out position);
  }

  public void Clear()
  {
    VisitedTiles.Clear();
    TileClassification.Clear();
    DiagnosticFlags.Clear();
    _roomCheckStack.Clear();
    LastFoundHouse = null;
    IsTryingAlternateHousingSpot = false;
    SharedRoomX = null;
  }
}
