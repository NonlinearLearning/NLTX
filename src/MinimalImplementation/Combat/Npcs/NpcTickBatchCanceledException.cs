using MinimalImplementation.Combat.Identity;

namespace MinimalImplementation.Combat.Npcs;

/// <summary>
/// A canceled batch that retains its already committed prefix and cancellation token.
/// </summary>
public sealed class NpcTickBatchCanceledException
    : OperationCanceledException {
  internal NpcTickBatchCanceledException(
      CombatEntityIdentity interruptedEntity,
      NpcTickBatchResult partialResult,
      OperationCanceledException innerException)
      : base("NPC tick batch was canceled after partial progress.", innerException,
          innerException.CancellationToken) {
    InterruptedEntity = interruptedEntity;
    PartialResult = partialResult;
  }

  public CombatEntityIdentity InterruptedEntity { get; }

  public NpcTickBatchResult PartialResult { get; }
}
