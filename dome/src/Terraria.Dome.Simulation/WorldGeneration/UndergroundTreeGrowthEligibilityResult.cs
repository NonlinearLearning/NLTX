namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct UndergroundTreeGrowthEligibilityResult(
  UndergroundTreeGrowthEligibilityReason Reason,
  int CanopyTopY)
{
  public bool IsEligible => Reason == UndergroundTreeGrowthEligibilityReason.Eligible;

  public static UndergroundTreeGrowthEligibilityResult Rejected(
    UndergroundTreeGrowthEligibilityReason reason)
  {
    return new UndergroundTreeGrowthEligibilityResult(reason, -1);
  }
}
