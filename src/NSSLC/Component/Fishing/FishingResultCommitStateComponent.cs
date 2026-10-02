using Terraria.Player;

namespace Terraria.Fishing;

// status: proposed
// componentId: FISHING-RESULT-COMMIT-STATE
// designStatus: decision-required
// crossSubsystemOwner: integration-review
public struct FishingResultCommitStateComponent
{
  public FishingResultCommitStateComponent(
    FishingOutcomeKind resultKind,
    TileCoordinate origin,
    IdempotencyKey idempotencyKey)
  {
    ResultKind = resultKind;
    CommitState = FishingCommitState.NotSubmitted;
    CommitAttemptCount = 0;
    ExternalResultId = null;
    Origin = origin;
    IdempotencyKey = idempotencyKey;
  }

  // Must agree with FishingCatchDecisionComponent.OutcomeKind.
  public FishingOutcomeKind ResultKind;

  // Version4 has no unified result commit state.
  public FishingCommitState CommitState;

  public ushort CommitAttemptCount;

  // Item and NPC IDs are discriminated and externally owned.
  public FishingExternalResultId? ExternalResultId;

  // Existing coordinate candidate; source ownership remains under review.
  public TileCoordinate Origin;

  // Version4 has no explicit idempotency key.
  public IdempotencyKey IdempotencyKey;

  public bool IsCommitted =>
    CommitState == FishingCommitState.Committed;
}
