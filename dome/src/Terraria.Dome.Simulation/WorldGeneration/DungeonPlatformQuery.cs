using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class DungeonPlatformQuery
{
  private const ushort DungeonPlatformTileType = 19;
  private const int FrameHeight = 18;

  public static bool IsPlatformOrShelf(WorldTile tile)
  {
    if (!tile.IsActive || tile.Type != DungeonPlatformTileType)
    {
      return false;
    }

    int frameColumn = tile.FrameY / FrameHeight;
    return frameColumn is 6 or 7 or 8 or >= 9 and <= 12;
  }
}
