using System.Collections.Frozen;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class OrdinaryTreeGroundQuery
{
  private static readonly IReadOnlySet<ushort> SuitableGroundTileTypes = new HashSet<ushort>
  {
    2, 23, 60, 70, 109, 147, 199, 477, 492, 633, 661, 662
  }.ToFrozenSet();

  public static IReadOnlySet<ushort> RegisterDefaults()
  {
    return SuitableGroundTileTypes;
  }

  public static bool IsSuitable(ushort tileType)
  {
    return SuitableGroundTileTypes.Contains(tileType);
  }
}
