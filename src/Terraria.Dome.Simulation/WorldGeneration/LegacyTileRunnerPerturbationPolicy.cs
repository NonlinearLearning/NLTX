namespace Terraria.Dome.Simulation.WorldGeneration;

public sealed record LegacyTileRunnerPerturbation(
  double CenterX,
  double CenterY,
  bool AppliedDrunkJitter);

public static class LegacyTileRunnerPerturbationPolicy
{
  public static LegacyTileRunnerPerturbation ApplyDrunkJitter(
    double centerX,
    double centerY,
    bool drunkWorld,
    int gateRoll,
    int offsetXRoll,
    int offsetYRoll)
  {
    bool apply = drunkWorld && gateRoll == 0;
    if (!apply)
    {
      return new LegacyTileRunnerPerturbation(centerX, centerY, false);
    }

    return new LegacyTileRunnerPerturbation(
      centerX + offsetXRoll * 0.05,
      centerY + offsetYRoll * 0.05,
      true);
  }
}
