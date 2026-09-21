using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class HousingHomeSpotQuery
{
  public static bool IsEligible(WorldTile tile)
  {
    return !tile.IsActive || tile.Type != 379;
  }
}
