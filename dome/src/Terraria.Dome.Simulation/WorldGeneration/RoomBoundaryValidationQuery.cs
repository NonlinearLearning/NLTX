using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class RoomBoundaryValidationQuery
{
  public static RoomBoundaryValidationResult Evaluate(
    int x,
    int y,
    int worldWidth,
    int worldHeight,
    int roomTileCount,
    int roomMinX,
    int roomMaxX,
    int roomMinY,
    int roomMaxY,
    int maxRoomTiles,
    int maxRoomSize,
    bool stopOnFail,
    bool roomTilesContainsPoint)
  {
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(worldWidth);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(worldHeight);
    ArgumentOutOfRangeException.ThrowIfNegative(roomTileCount);
    bool edge = x < 10 || y < 10 || x >= worldWidth - 10 || y >= worldHeight - 10;
    bool outOfBounds = !roomTilesContainsPoint;
    bool tileLimit = stopOnFail && roomTileCount >= maxRoomTiles;
    bool sizeLimit = stopOnFail &&
      (roomMaxX - roomMinX >= maxRoomSize || roomMaxY - roomMinY >= maxRoomSize);
    bool stop = edge || outOfBounds || tileLimit || sizeLimit;
    return new RoomBoundaryValidationResult(
      !stop,
      stop,
      edge,
      outOfBounds,
      tileLimit,
      sizeLimit,
      roomTileCount,
      roomMaxX - roomMinX,
      roomMaxY - roomMinY,
      true);
  }
}
