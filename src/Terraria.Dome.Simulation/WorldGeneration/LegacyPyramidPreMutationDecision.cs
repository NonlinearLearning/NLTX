namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct LegacyPyramidPreMutationDecision(
  LegacyPyramidPreMutationRejectionReason RejectionReason)
{
  public bool IsRejected => RejectionReason != LegacyPyramidPreMutationRejectionReason.None;

  public bool IsEligible => !IsRejected;
}
