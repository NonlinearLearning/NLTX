namespace Terraria.Dome.Simulation.WorldGeneration;

public static class HousingWallSafetyPolicy
{
  public static HousingWallSafetyDecision Evaluate(
    bool hasHorizontalHousingBoundary,
    bool hasVerticalHousingBoundary,
    bool currentTileHasWall)
  {
    if (hasHorizontalHousingBoundary && hasVerticalHousingBoundary)
    {
      return new HousingWallSafetyDecision(HousingWallSafetyRejectionReason.None, true);
    }

    HousingWallSafetyRejectionReason reason = currentTileHasWall
      ? HousingWallSafetyRejectionReason.TooManyUnsafeWalls
      : HousingWallSafetyRejectionReason.HoleInWallIsTooBig;
    return new HousingWallSafetyDecision(reason, false);
  }
}
