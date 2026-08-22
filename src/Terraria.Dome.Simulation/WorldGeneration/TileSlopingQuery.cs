namespace Terraria.Dome.Simulation.WorldGeneration;

public static class TileSlopingQuery
{
  public static bool ForbidsSloping(ushort tileType)
  {
    return tileType is 21 or 26 or 77 or 88 or 235 or 237 or 441 or 467 or 468 or 470 or
      475 or 488 or 597;
  }
}
