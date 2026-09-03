using System;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class TileSolidityOverrideQuery
{
  private static readonly IReadOnlyList<ushort> BoulderTileTypes =
    Array.AsReadOnly(new ushort[] { 138, 484, 664, 711, 712, 713, 714, 715, 716 });
  private static readonly IReadOnlyList<ushort> CrackedBrickTileTypes =
    Array.AsReadOnly(new ushort[] { 481, 482, 483 });

  public static IReadOnlyList<ushort> RegisterBoulderDefaults()
  {
    return BoulderTileTypes;
  }

  public static IReadOnlyList<ushort> RegisterCrackedBrickDefaults()
  {
    return CrackedBrickTileTypes;
  }

  public static TileSolidityOverrideProjection Evaluate(bool solid)
  {
    return new TileSolidityOverrideProjection(
      solid,
      BoulderTileTypes,
      CrackedBrickTileTypes,
      true);
  }
}
