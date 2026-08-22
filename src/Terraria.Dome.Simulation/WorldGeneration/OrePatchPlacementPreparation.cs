namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct OrePatchPlacementPreparation(
  OrePatchEligibilityResult Eligibility,
  OrePlacementPreparationResult Transaction)
{
  public bool IsPrepared => Eligibility.IsEligible && Transaction.IsPrepared;

  public int GroundY => Eligibility.GroundY;
}
