namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct OrePatchEligibilityResult(
  OrePatchEligibilityReason Reason,
  int GroundY)
{
  public bool IsEligible => Reason == OrePatchEligibilityReason.Eligible;

  public static OrePatchEligibilityResult Rejected(OrePatchEligibilityReason reason)
  {
    return new OrePatchEligibilityResult(reason, -1);
  }
}
