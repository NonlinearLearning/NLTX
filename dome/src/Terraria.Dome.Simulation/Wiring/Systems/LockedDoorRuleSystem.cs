using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.Wiring.Systems;

public static class LockedDoorRuleSystem
{
  private const ushort DoorTileType = 10;
  private const short FirstLockedDoorFrameY = 594;
  private const short FirstUnlockedDoorFrameX = 54;
  private const short LastLockedDoorFrameY = 646;

  public static bool IsLocked(WorldTile tile)
  {
    return tile.Type == DoorTileType &&
           tile.FrameY >= FirstLockedDoorFrameY &&
           tile.FrameY <= LastLockedDoorFrameY &&
           tile.FrameX < FirstUnlockedDoorFrameX;
  }
}
