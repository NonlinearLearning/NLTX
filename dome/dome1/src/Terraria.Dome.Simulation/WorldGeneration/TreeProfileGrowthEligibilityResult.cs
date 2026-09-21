namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct TreeProfileGrowthEligibilityResult(
  TreeProfileGrowthEligibilityReason Reason,
  int GroundY)
{
  public bool IsEligible => Reason == TreeProfileGrowthEligibilityReason.Eligible;

  public static TreeProfileGrowthEligibilityResult Rejected(
    TreeProfileGrowthEligibilityReason reason)
  {
    return new TreeProfileGrowthEligibilityResult(reason, -1);
  }
}
