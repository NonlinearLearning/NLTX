using System.Collections.Frozen;
using System.Collections.Generic;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.Wiring.Systems;

public static class SpecialTileProtectionRuleSystem
{
  private static readonly IReadOnlySet<ushort> AlwaysProtectedTileTypes = new HashSet<ushort>
  {
    21, 26, 72, 77, 88, 467, 488
  }.ToFrozenSet();

  public static IReadOnlySet<ushort> RegisterAlwaysProtectedDefaults()
  {
    return AlwaysProtectedTileTypes;
  }

  public static bool ShouldProtectAbove(ushort candidateTileType, WorldTile aboveTile)
  {
    if (!aboveTile.IsActive || candidateTileType == aboveTile.Type)
    {
      return false;
    }

    return aboveTile.Type switch
    {
      323 => aboveTile.FrameX == 66 || aboveTile.FrameX == 220,
      _ when AlwaysProtectedTileTypes.Contains(aboveTile.Type) => true,
      80 => IsProtectedType80Frame(aboveTile.FrameX),
      _ => false
    };
  }

  private static bool IsProtectedType80Frame(short frameX)
  {
    int frameColumn = frameX / 18;
    return frameColumn <= 1 || frameColumn is 4 or 5;
  }
}
