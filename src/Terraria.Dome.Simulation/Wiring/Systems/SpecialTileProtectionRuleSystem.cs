using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.Wiring.Systems;

public static class SpecialTileProtectionRuleSystem
{
  public static bool ShouldProtectAbove(ushort candidateTileType, WorldTile aboveTile)
  {
    if (!aboveTile.IsActive || candidateTileType == aboveTile.Type)
    {
      return false;
    }

    return aboveTile.Type switch
    {
      323 => aboveTile.FrameX == 66 || aboveTile.FrameX == 220,
      21 or 26 or 72 or 77 or 88 or 467 or 488 => true,
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
