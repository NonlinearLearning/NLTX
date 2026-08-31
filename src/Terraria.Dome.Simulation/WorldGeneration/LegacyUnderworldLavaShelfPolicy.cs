using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyUnderworldLavaShelfPolicy
{
  public const int FirstShelfOffset = 145;
  public const int SecondShelfOffset = 144;

  public static bool ShouldFill(WorldTile tile)
  {
    return !tile.IsActive;
  }

  public static int GetShelfY(int height, int offset)
  {
    return height - offset;
  }
}
