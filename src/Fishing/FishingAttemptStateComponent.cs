namespace Terraria.Fishing;

// status: proposed
// componentId: FISHING-ATTEMPT-STATE
// designStatus: decision-required
// crossSubsystemOwner: integration-review
public struct FishingAttemptStateComponent
{
  public FishingAttemptStateComponent(
    FishingAttemptId attemptId,
    PlayerEntityId owner,
    ProjectileEntityId? bobber)
  {
    AttemptId = attemptId;
    Owner = owner;
    Bobber = bobber;
    Phase = FishingAttemptPhase.Waiting;
    Revision = 0;
    TerminalReason = null;
  }

  // Version4 has no explicit attempt identity.
  public FishingAttemptId AttemptId;

  // Do not store Projectile.owner's network slot directly.
  public PlayerEntityId Owner;

  // This is a relation, not a copy of Projectile state or a network ID.
  public ProjectileEntityId? Bobber;

  // Map from the current Dome FishingBobberPhase only after BD-COMP-01 is resolved.
  public FishingAttemptPhase Phase;

  // Candidate convergence/version field; Version4 has no explicit attempt revision.
  public uint Revision;

  // Null while the attempt remains non-terminal.
  public FishingTerminalReason? TerminalReason;

  public bool IsTerminal => TerminalReason.HasValue;
}
