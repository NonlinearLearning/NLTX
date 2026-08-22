using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class ChestRiggingQuery
{
  private const ushort RiggedChestTileType = 467;
  private const int RiggedFrameBand = 4;

  public static bool IsRigged(WorldTile tile)
  {
    return tile.IsActive && tile.Type == RiggedChestTileType &&
      tile.FrameX / 36 == RiggedFrameBand;
  }
}
