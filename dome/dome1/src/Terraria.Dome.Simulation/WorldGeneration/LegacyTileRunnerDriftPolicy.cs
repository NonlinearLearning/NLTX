namespace Terraria.Dome.Simulation.WorldGeneration;

public sealed record LegacyTileRunnerDrift(
  double CenterX,
  double CenterY,
  double DirectionX,
  double DirectionY,
  double RemainingSteps,
  bool AppliedExtraDrift);

public static class LegacyTileRunnerDriftPolicy
{
  public static LegacyTileRunnerDrift ApplyExtraDrift(
    double centerX,
    double centerY,
    double directionX,
    double directionY,
    double strength,
    double remainingSteps,
    bool drunkWorld,
    int drunkGateRoll,
    int directionXRoll,
    int directionYRoll)
  {
    bool apply = strength > 50.0 && (!drunkWorld || drunkGateRoll != 0);
    if (!apply)
    {
      return new LegacyTileRunnerDrift(
        centerX,
        centerY,
        directionX,
        directionY,
        remainingSteps,
        false);
    }

    return new LegacyTileRunnerDrift(
      centerX + directionX,
      centerY + directionY,
      directionX + directionXRoll * 0.05,
      directionY + directionYRoll * 0.05,
      remainingSteps - 1.0,
      true);
  }
}
