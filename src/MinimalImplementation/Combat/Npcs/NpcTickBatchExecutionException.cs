using MinimalImplementation.Combat.Identity;

namespace MinimalImplementation.Combat.Npcs;

/// <summary>
/// An unexpected failure after a batch capture, with its already committed prefix.
/// </summary>
public sealed class NpcTickBatchExecutionException
    : Exception {
  internal NpcTickBatchExecutionException(
      CombatEntityIdentity interruptedEntity,
      NpcTickBatchResult partialResult,
      Exception innerException)
      : base("NPC tick batch execution was interrupted after partial progress.", innerException) {
    InterruptedEntity = interruptedEntity;
    PartialResult = partialResult;
  }

  public CombatEntityIdentity InterruptedEntity { get; }

  public NpcTickBatchResult PartialResult { get; }
}
