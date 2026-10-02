using System.Collections.Generic;

namespace Terraria.Items;

public sealed class CraftingStateComponent
{
  private readonly Dictionary<PersistentContainerId, long> _expectedSourceRevisions;

  public CraftingStateComponent(
    TransactionId? transactionId = null,
    RecipeDefinitionRef? recipeRef = null,
    int requestedQuantity = 0,
    IReadOnlyDictionary<PersistentContainerId, long>? expectedSourceRevisions = null,
    long? acceptedAtTick = null,
    long craftSequence = 0,
    CraftingPhase phase = CraftingPhase.Idle)
  {
    TransactionId = transactionId;
    RecipeRef = recipeRef;
    RequestedQuantity = requestedQuantity;
    _expectedSourceRevisions = expectedSourceRevisions is null
      ? []
      : new Dictionary<PersistentContainerId, long>(expectedSourceRevisions);
    AcceptedAtTick = acceptedAtTick;
    CraftSequence = craftSequence;
    Phase = phase;
  }

  public TransactionId? TransactionId;
  public RecipeDefinitionRef? RecipeRef;
  public int RequestedQuantity;
  public long? AcceptedAtTick;
  public long CraftSequence;
  public CraftingPhase Phase;

  public IReadOnlyDictionary<PersistentContainerId, long> ExpectedSourceRevisions =>
    _expectedSourceRevisions;

  public bool HasTransaction => TransactionId.HasValue;

  public bool HasActiveCraft =>
    TransactionId.HasValue &&
    RecipeRef.HasValue &&
    RequestedQuantity > 0 &&
    Phase is CraftingPhase.Accepted or CraftingPhase.Consuming or CraftingPhase.Producing;
}
