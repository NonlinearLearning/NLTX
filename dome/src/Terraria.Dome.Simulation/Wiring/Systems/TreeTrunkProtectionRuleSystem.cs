using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.Wiring.Systems;

public static class TreeTrunkProtectionRuleSystem
{
  public static bool ShouldProtectAbove(ushort candidateTileType, WorldTile aboveTile)
  {
    if (!LegacyTreeTrunkRuleSystem.IsTreeTrunk(aboveTile.Type) ||
        candidateTileType == aboveTile.Type)
    {
      return false;
    }

    bool isFirstFrameException = aboveTile.FrameX == 66 &&
      aboveTile.FrameY >= 0 && aboveTile.FrameY <= 44;
    bool isSecondFrameException = aboveTile.FrameX == 88 &&
      aboveTile.FrameY >= 66 && aboveTile.FrameY <= 110;
    return !isFirstFrameException && !isSecondFrameException && aboveTile.FrameY < 198;
  }
}
