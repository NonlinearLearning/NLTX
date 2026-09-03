using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class HousingRoomMinimumSizePolicy
{
  public const int Version4MinimumRoomTileCount = 60;

  public static HousingRoomMinimumSizeDecision Evaluate(
    int roomTileCount,
    int minimumRoomTileCount = Version4MinimumRoomTileCount)
  {
    if (roomTileCount < 0 || minimumRoomTileCount <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(roomTileCount));
    }

    bool isTooSmall = roomTileCount < minimumRoomTileCount;
    return new HousingRoomMinimumSizeDecision(
      roomTileCount,
      minimumRoomTileCount,
      !isTooSmall,
      isTooSmall);
  }
}
