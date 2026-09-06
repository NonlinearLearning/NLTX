namespace Terraria.Fishing;

// status: proposed
// componentId: FISHING-BOBBER-TIMING
// designStatus: candidate
// crossSubsystemOwner: integration-review
public struct BobberTimingComponent
{
  public BobberTimingComponent(int biteThreshold)
  {
    ElapsedTicks = 0;
    BiteThreshold = biteThreshold;
    RetractRequested = false;
    BiteState = FishingBiteState.Waiting;
    LegacyState = null;
  }

  // Fishing-specific elapsed time; this is not Projectile.timeLeft.
  public int ElapsedTicks;

  // The complete reference suggests 660; Version4 did not confirm that value.
  public int BiteThreshold;

  public bool RetractRequested;

  // Must remain consistent with FishingAttemptStateComponent.Phase.
  public FishingBiteState BiteState;

  // Compatibility-only boundary for old ai/localAI values.
  public FishingLegacyBobberState? LegacyState;

  public bool HasReachedBiteThreshold =>
    BiteThreshold >= 0 && ElapsedTicks >= BiteThreshold;
}
