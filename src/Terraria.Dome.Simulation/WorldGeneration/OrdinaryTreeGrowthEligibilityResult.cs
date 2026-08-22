namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct OrdinaryTreeGrowthEligibilityResult(
  OrdinaryTreeGrowthEligibilityReason Reason,
  int GroundY)
{
  public bool IsEligible => Reason == OrdinaryTreeGrowthEligibilityReason.Eligible;

  public static OrdinaryTreeGrowthEligibilityResult Rejected(
    OrdinaryTreeGrowthEligibilityReason reason)
  {
    return new OrdinaryTreeGrowthEligibilityResult(reason, -1);
  }
}
