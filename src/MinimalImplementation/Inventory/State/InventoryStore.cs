using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Inventory.Rules;

namespace MinimalImplementation.Server.Inventory.State;

/// <summary>Owns the writable player item slots; snapshots never expose its backing array.</summary>
public sealed class InventoryStore
{
  private readonly Dictionary<EntityUuid, InventoryState> _inventories = new();
  private readonly Dictionary<EntityUuid, long> _nextGenerationByPlayer = new();
  private readonly ItemValidationPolicy _itemPolicy;
  private readonly ServerStateCommitGate _commitGate;
  private readonly int _slotCount;

  public InventoryStore(
    int slotCount,
    ItemValidationPolicy itemPolicy,
    ServerStateCommitGate commitGate)
  {
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(slotCount);
    if (slotCount > short.MaxValue + 1)
    {
      throw new ArgumentOutOfRangeException(nameof(slotCount));
    }

    ArgumentNullException.ThrowIfNull(itemPolicy);
    ArgumentNullException.ThrowIfNull(commitGate);
    _slotCount = slotCount;
    _itemPolicy = itemPolicy;
    _commitGate = commitGate;
  }

  internal ServerStateCommitGate CommitGate => _commitGate;

  public OwnerCommitResult Create(
    EntityUuid playerId,
    IReadOnlyList<InventoryItemState> initialItems,
    out InventorySnapshot? snapshot)
  {
    lock (_commitGate.SyncRoot)
    {
      snapshot = null;
      if (!playerId.IsValid)
      {
        return OwnerCommitResult.Rejected(ProtocolErrorCode.Forbidden, "The player identity is invalid.");
      }

      ArgumentNullException.ThrowIfNull(initialItems);
      if (initialItems.Count != _slotCount)
      {
        return OwnerCommitResult.Rejected(ProtocolErrorCode.InvalidLength, "The initial inventory has the wrong slot count.");
      }

      if (_inventories.ContainsKey(playerId))
      {
        return new OwnerCommitResult(
          OwnerCommitStatus.Conflict,
          _inventories[playerId].Revision,
          ProtocolErrorCode.Conflict,
          "The player inventory already exists.");
      }

      for (int index = 0; index < initialItems.Count; index++)
      {
        InventoryItemState item = initialItems[index];
        if (!_itemPolicy.IsValid(item))
        {
          return OwnerCommitResult.Rejected(ProtocolErrorCode.Forbidden, $"Inventory item at slot {index} is invalid.");
        }
      }

      long generationValue = _nextGenerationByPlayer.GetValueOrDefault(playerId, 1);
      if (generationValue <= 0 || generationValue == long.MaxValue)
      {
        return Unavailable("The inventory slot generation space is exhausted.");
      }

      var slots = new InventorySlot[_slotCount];
      var generation = new InventorySlotGeneration(generationValue);
      for (int index = 0; index < slots.Length; index++)
      {
        slots[index] = new InventorySlot(generation, initialItems[index]);
      }

      var state = new InventoryState(
        slots,
        CommitVersion.Initial,
        generationValue + 1);
      _inventories.Add(playerId, state);
      snapshot = Capture(playerId, state);
      return OwnerCommitResult.Applied(state.Revision);
    }
  }

  public OwnerCommitResult CreateEmpty(EntityUuid playerId, out InventorySnapshot? snapshot)
  {
    return Create(playerId, CreateEmptySlots(), out snapshot);
  }

  public bool TryCapture(EntityUuid playerId, out InventorySnapshot? snapshot)
  {
    lock (_commitGate.SyncRoot)
    {
      if (!_inventories.TryGetValue(playerId, out InventoryState? state))
      {
        snapshot = null;
        return false;
      }

      snapshot = Capture(playerId, state);
      return true;
    }
  }

  public OwnerCommitResult Apply(
    EntityUuid playerId,
    CommitVersion expectedRevision,
    IReadOnlyCollection<InventoryMutation> mutations,
    out InventorySnapshot? snapshot)
  {
    lock (_commitGate.SyncRoot)
    {
      snapshot = null;
      OwnerCommitResult preparedResult = PrepareApply(playerId, expectedRevision, mutations, out PreparedMutation? prepared);
      if (preparedResult.Status != OwnerCommitStatus.Applied || prepared is null)
      {
        return preparedResult;
      }

      return CommitPrepared(prepared, out snapshot);
    }
  }

  internal OwnerCommitResult PrepareApply(
    EntityUuid playerId,
    CommitVersion expectedRevision,
    IReadOnlyCollection<InventoryMutation> mutations,
    out PreparedMutation? prepared)
  {
    prepared = null;
    ArgumentNullException.ThrowIfNull(mutations);
    if (!playerId.IsValid)
    {
      return OwnerCommitResult.Rejected(ProtocolErrorCode.Forbidden, "The player identity is invalid.");
    }

    if (!_inventories.TryGetValue(playerId, out InventoryState? state))
    {
      return Unavailable("The player inventory is not available.");
    }

    if (state.Revision != expectedRevision)
    {
      return Conflict(state.Revision, "The player inventory revision changed.");
    }

    if (mutations.Count == 0)
    {
      prepared = new PreparedMutation(
        playerId,
        state,
        expectedRevision,
        state.Slots,
        state.NextSlotGeneration,
        false,
        state.Revision,
        Capture(playerId, state.Revision, state.Slots));
      return OwnerCommitResult.Applied(state.Revision);
    }

    var seen = new HashSet<short>();
    foreach (InventoryMutation mutation in mutations)
    {
      if (mutation.SlotId < 0
        || mutation.SlotId >= state.Slots.Length
        || !seen.Add(mutation.SlotId))
      {
        return OwnerCommitResult.Rejected(ProtocolErrorCode.Forbidden, "The inventory mutation contains an invalid slot id.");
      }

      InventorySlot current = state.Slots[mutation.SlotId];
      if (current.Generation != mutation.ExpectedGeneration)
      {
        return Stale(state.Revision, "The inventory slot generation changed.");
      }

      if (!_itemPolicy.IsValid(mutation.Item))
      {
        return OwnerCommitResult.Rejected(ProtocolErrorCode.Forbidden, "The inventory mutation contains an invalid item.");
      }
    }

    if (state.NextSlotGeneration > long.MaxValue - mutations.Count)
    {
      return Unavailable("The inventory slot generation space is exhausted.");
    }

    if (state.Revision.Value == long.MaxValue)
    {
      return Unavailable("The inventory revision space is exhausted.");
    }

    var updatedSlots = state.Slots.ToArray();
    long nextSlotGeneration = state.NextSlotGeneration;
    foreach (InventoryMutation mutation in mutations)
    {
      var generation = new InventorySlotGeneration(nextSlotGeneration);
      nextSlotGeneration++;
      updatedSlots[mutation.SlotId] = new InventorySlot(generation, mutation.Item);
    }

    CommitVersion nextRevision = state.Revision.Next();
    prepared = new PreparedMutation(
      playerId,
      state,
      expectedRevision,
      updatedSlots,
      nextSlotGeneration,
      true,
      nextRevision,
      Capture(playerId, nextRevision, updatedSlots));
    return OwnerCommitResult.Applied(nextRevision);
  }

  internal bool CanCommit(PreparedMutation prepared)
  {
    ArgumentNullException.ThrowIfNull(prepared);
    return _inventories.TryGetValue(prepared.PlayerId, out InventoryState? current)
      && ReferenceEquals(current, prepared.StateToken)
      && current.Revision == prepared.ExpectedRevision;
  }

  internal OwnerCommitResult CommitPrepared(
    PreparedMutation prepared,
    out InventorySnapshot? snapshot)
  {
    ArgumentNullException.ThrowIfNull(prepared);
    snapshot = null;
    if (!CanCommit(prepared))
    {
      return _inventories.TryGetValue(prepared.PlayerId, out InventoryState? current)
        ? Conflict(current.Revision, "The player inventory changed after the transaction was prepared.")
        : Unavailable("The player inventory is not available.");
    }

    InventoryState state = (InventoryState)prepared.StateToken;
    if (prepared.HasChanges)
    {
      state.Slots = (InventorySlot[])prepared.SlotsToken;
      state.NextSlotGeneration = prepared.NextSlotGeneration;
      state.Revision = prepared.NextRevision;
    }

    snapshot = prepared.Snapshot;
    return OwnerCommitResult.Applied(state.Revision);
  }

  public OwnerCommitResult Remove(EntityUuid playerId, CommitVersion expectedRevision)
  {
    lock (_commitGate.SyncRoot)
    {
      if (!_inventories.TryGetValue(playerId, out InventoryState? state))
      {
        return Unavailable("The player inventory is not available.");
      }

      if (state.Revision != expectedRevision)
      {
        return Conflict(state.Revision, "The player inventory revision changed.");
      }

      if (state.Slots.Any(slot => !slot.Item.IsEmpty))
      {
        return OwnerCommitResult.Rejected(
          ProtocolErrorCode.Conflict,
          "The inventory must be durably transferred or emptied before removal.");
      }

      _nextGenerationByPlayer[playerId] = state.NextSlotGeneration;
      _inventories.Remove(playerId);
      return OwnerCommitResult.Applied(state.Revision);
    }
  }

  private static InventorySnapshot Capture(EntityUuid playerId, InventoryState state)
  {
    return Capture(playerId, state.Revision, state.Slots);
  }

  private static InventorySnapshot Capture(
    EntityUuid playerId,
    CommitVersion revision,
    IReadOnlyList<InventorySlot> sourceSlots)
  {
    var slots = new InventorySlotSnapshot[sourceSlots.Count];
    for (int index = 0; index < sourceSlots.Count; index++)
    {
      InventorySlot slot = sourceSlots[index];
      slots[index] = new InventorySlotSnapshot((short)index, slot.Generation, slot.Item);
    }

    return new InventorySnapshot(playerId, revision, slots);
  }

  private static InventoryItemState[] CreateEmptySlots(int slotCount)
  {
    var items = new InventoryItemState[slotCount];
    Array.Fill(items, InventoryItemState.Empty);
    return items;
  }

  private InventoryItemState[] CreateEmptySlots()
  {
    return CreateEmptySlots(_slotCount);
  }

  private static OwnerCommitResult Stale(CommitVersion revision, string detail)
  {
    return new OwnerCommitResult(OwnerCommitStatus.Stale, revision, ProtocolErrorCode.StaleEntity, detail);
  }

  private static OwnerCommitResult Conflict(CommitVersion revision, string detail)
  {
    return new OwnerCommitResult(OwnerCommitStatus.Conflict, revision, ProtocolErrorCode.Conflict, detail);
  }

  private static OwnerCommitResult Unavailable(string detail)
  {
    return new OwnerCommitResult(OwnerCommitStatus.Unavailable, null, ProtocolErrorCode.NotReady, detail);
  }

  private sealed class InventoryState(
    InventorySlot[] slots,
    CommitVersion revision,
    long nextSlotGeneration)
  {
    public InventorySlot[] Slots { get; set; } = slots;
    public CommitVersion Revision { get; set; } = revision;
    public long NextSlotGeneration { get; set; } = nextSlotGeneration;
  }

  internal sealed class PreparedMutation(
    EntityUuid playerId,
    object stateToken,
    CommitVersion expectedRevision,
    object slotsToken,
    long nextSlotGeneration,
    bool hasChanges,
    CommitVersion nextRevision,
    InventorySnapshot snapshot)
  {
    internal EntityUuid PlayerId { get; } = playerId;
    internal object StateToken { get; } = stateToken;
    internal CommitVersion ExpectedRevision { get; } = expectedRevision;
    internal object SlotsToken { get; } = slotsToken;
    internal long NextSlotGeneration { get; } = nextSlotGeneration;
    internal bool HasChanges { get; } = hasChanges;
    internal CommitVersion NextRevision { get; } = nextRevision;
    internal InventorySnapshot Snapshot { get; } = snapshot;
  }

  private sealed record InventorySlot(InventorySlotGeneration Generation, InventoryItemState Item);
}
