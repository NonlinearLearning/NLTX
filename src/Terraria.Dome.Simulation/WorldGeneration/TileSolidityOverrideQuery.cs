using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class TileSolidityOverrideQuery
{
  private static readonly ushort[] BoulderTileTypes = [138, 484, 664, 711, 712, 713, 714, 715, 716];
  private static readonly ushort[] CrackedBrickTileTypes = [481, 482, 483];

  public static TileSolidityOverrideProjection Evaluate(bool solid)
  {
    return new TileSolidityOverrideProjection(
      solid,
      BoulderTileTypes,
      CrackedBrickTileTypes,
      true);
  }
}
