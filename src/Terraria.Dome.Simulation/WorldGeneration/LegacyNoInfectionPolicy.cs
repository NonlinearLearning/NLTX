using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct LegacyNoInfectionConversion(bool Tiles, bool Walls)
{
  public bool HasWork => Tiles || Walls;
}

public static class LegacyNoInfectionPolicy
{
  public static LegacyNoInfectionConversion GetConversion(WorldTile tile, bool worldIsInfected)
  {
    bool tiles = tile.Type != 70;
    bool walls = tile.WallType != 80;
    if (worldIsInfected)
    {
      tiles &= tile.Type is not (203 or 25);
      walls &= tile.WallType is not (83 or 3);
    }

    return new LegacyNoInfectionConversion(tiles, walls);
  }
}
