using System.Collections.Immutable;

namespace Terraria.WorldProgression.Components;

public sealed class TransitionRecoveryStateComponent
{
  public RecoveryPhase RecoveryPhase { get; set; }
  public TransitionPhase? LastSafePhase { get; set; }
  public ImmutableArray<int> PendingBatchIndexes { get; set; } = ImmutableArray<int>.Empty;
  public TransitionFailureCode? FailureCode { get; set; }
  public int RetryCount { get; set; }
  public bool ResultUnknown { get; set; }
  public long RecoveryRevision { get; set; }
}
