using System.IO;
using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Identity;
using MinimalImplementation.Server.Inventory.Content;
using MinimalImplementation.Server.Inventory.Rules;
using Terraria.NonAuthoritative.Persistence;

namespace MinimalImplementation.Server.Inventory.State;

/// <summary>Owns chest definitions and item slots; open leases live in ChestLeaseStore.</summary>
public sealed class ChestStore
{
  private readonly Dictionary<ChestId, ChestState> _chests = new();
  private readonly Dictionary<ChestId, long> _nextGeneration = new();
  private readonly int _chestIndexCount;
  private readonly ItemValidationPolicy _itemPolicy;
  private readonly ServerStateCommitGate _commitGate;

  public ChestStore(
    int chestIndexCount,
    ItemValidationPolicy itemPolicy,
    ServerStateCommitGate commitGate)
  {
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(chestIndexCount);
    if (chestIndexCount > ContainerContentFacts.ChestIndexCount)
    {
      throw new ArgumentOutOfRangeException(nameof(chestIndexCount));
    }

    ArgumentNullException.ThrowIfNull(itemPolicy);
    ArgumentNullException.ThrowIfNull(commitGate);
    _chestIndexCount = chestIndexCount;
    _itemPolicy = itemPolicy;
    _commitGate = commitGate;
  }

  internal ServerStateCommitGate CommitGate => _commitGate;

  public OwnerCommitResult Create(
    ChestId id,
    WorldScopeId worldScope,
    WorldTilePoint position,
    ushort kindId,
    string name,
    IReadOnlyList<InventoryItemState> items,
    out ChestSnapshot? snapshot)
  {
    lock (_commitGate.SyncRoot)
    {
      return CreateCore(id, worldScope, position, kindId, name, items, out snapshot);
    }
  }

  private OwnerCommitResult CreateCore(
    ChestId id,
    WorldScopeId worldScope,
    WorldTilePoint position,
    ushort kindId,
    string name,
    IReadOnlyList<InventoryItemState> items,
    out ChestSnapshot? snapshot)
  {
    snapshot = null;
    if (id.Value >= _chestIndexCount || !worldScope.IsValid)
    {
      return OwnerCommitResult.Rejected(ProtocolErrorCode.Forbidden, "The chest identity or world scope is invalid.");
    }

    ArgumentNullException.ThrowIfNull(name);
    if (name.Length > ContainerContentFacts.MaximumChestNameLength)
    {
      return OwnerCommitResult.Rejected(ProtocolErrorCode.InvalidLength, "The chest name exceeds Version4 bounds.");
    }
    ArgumentNullException.ThrowIfNull(items);
    if (items.Count is < 1 or > ContainerContentFacts.AbsoluteMaximumChestSlotCountForVersion4)
    {
      return OwnerCommitResult.Rejected(ProtocolErrorCode.InvalidLength, "The chest slot count is outside Version4 bounds.");
    }

    if (_chests.ContainsKey(id))
    {
      return new OwnerCommitResult(
        OwnerCommitStatus.Conflict,
        _chests[id].Revision,
        ProtocolErrorCode.Conflict,
        "The chest index is already active.");
    }

    foreach (InventoryItemState item in items)
    {
      if (!_itemPolicy.IsValid(item))
      {
        return OwnerCommitResult.Rejected(ProtocolErrorCode.Forbidden, "The chest contains an invalid item.");
      }
    }

    long generationValue = _nextGeneration.GetValueOrDefault(id, 1);
    if (generationValue <= 0 || generationValue == long.MaxValue)
    {
      return new OwnerCommitResult(
        OwnerCommitStatus.Unavailable,
        null,
        ProtocolErrorCode.NotReady,
        "The chest generation space is exhausted.");
    }

    var slots = new ChestSlot[items.Count];
    long nextSlotGeneration = 1;
    for (int index = 0; index < slots.Length; index++)
    {
      slots[index] = new ChestSlot(new ChestSlotGeneration(nextSlotGeneration), items[index]);
      nextSlotGeneration++;
    }

    var state = new ChestState(
      worldScope,
      new ChestGeneration(generationValue),
      position,
      kindId,
      name,
      slots,
      nextSlotGeneration);
    _chests.Add(id, state);
    snapshot = Capture(id, state);
    return OwnerCommitResult.Applied(state.Revision);
  }

  public OwnerCommitResult CreateEmpty(
    ChestId id,
    WorldScopeId worldScope,
    WorldTilePoint position,
    ushort kindId,
    string name,
    int slotCount,
    out ChestSnapshot? snapshot)
  {
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(slotCount);
    return Create(id, worldScope, position, kindId, name, CreateEmptyItems(slotCount), out snapshot);
  }

  public bool TryCapture(ChestId id, out ChestSnapshot? snapshot)
  {
    lock (_commitGate.SyncRoot)
    {
      if (!_chests.TryGetValue(id, out ChestState? state))
      {
        snapshot = null;
        return false;
      }

      snapshot = Capture(id, state);
      return true;
    }
  }

  /// <summary>Finds the deterministic authoritative chest at a world coordinate.</summary>
  public bool TryFindByPosition(
    WorldScopeId worldScope,
    WorldTilePoint position,
    out ChestId id,
    out ChestSnapshot? snapshot)
  {
    lock (_commitGate.SyncRoot)
    {
      foreach (KeyValuePair<ChestId, ChestState> entry in _chests.OrderBy(pair => pair.Key.Value))
      {
        if (entry.Value.WorldScope != worldScope || entry.Value.Position != position)
        {
          continue;
        }

        id = entry.Key;
        snapshot = Capture(entry.Key, entry.Value);
        return true;
      }

      id = default;
      snapshot = null;
      return false;
    }
  }

  /// <summary>Captures all active chest snapshots in one world, ordered by chest id.</summary>
  public IReadOnlyList<ChestSnapshot> CaptureWorld(WorldScopeId worldScope)
  {
    if (!worldScope.IsValid)
    {
      return Array.Empty<ChestSnapshot>();
    }

    lock (_commitGate.SyncRoot)
    {
      ChestSnapshot[] snapshots = _chests
        .Where(pair => pair.Value.WorldScope == worldScope)
        .OrderBy(pair => pair.Key.Value)
        .Select(pair => Capture(pair.Key, pair.Value))
        .ToArray();
      return Array.AsReadOnly(snapshots);
    }
  }

  /// <summary>Prepares the persisted chest records without changing the active store.</summary>
  public PreparedChestLoad PrepareRestore(
    WorldScopeId worldScope,
    IReadOnlyList<WorldFileChestRecord> records)
  {
    ArgumentNullException.ThrowIfNull(records);
    lock (_commitGate.SyncRoot)
    {
      if (!worldScope.IsValid)
      {
        throw new ArgumentException("A chest restore requires a valid world scope.",
          nameof(worldScope));
      }

      if (_chests.Count != 0)
      {
        throw new InvalidOperationException("A chest restore requires an empty store.");
      }

      if (records.Count > _chestIndexCount)
      {
        throw new InvalidDataException("The persisted chest count exceeds the Version4 index range.");
      }

      var positions = new HashSet<WorldTilePoint>();
      var restores = new List<PreparedChestRestore>(records.Count);
      for (int index = 0; index < records.Count; index++)
      {
        WorldFileChestRecord record = records[index]
          ?? throw new ArgumentException("A chest record cannot be null.", nameof(records));
        if (record.X < 0 || record.Y < 0)
        {
          throw new InvalidDataException("A persisted chest coordinate must be nonnegative.");
        }

        if (record.Name.Length > ContainerContentFacts.MaximumChestNameLength)
        {
          throw new InvalidDataException("A persisted chest name exceeds Version4 bounds.");
        }

        var position = new WorldTilePoint(record.X, record.Y);
        if (!positions.Add(position))
        {
          throw new InvalidDataException(
            $"The persisted chest section contains a duplicate anchor at {record.X},{record.Y}.");
        }

        IReadOnlyList<InventoryItemState> items = ConvertItems(record.Items);
        var id = new ChestId(checked((ushort)index));
        long generationValue = _nextGeneration.GetValueOrDefault(id, 1);
        if (generationValue <= 0 || generationValue == long.MaxValue)
        {
          throw new InvalidOperationException(
            $"The chest generation space is exhausted for index {id.Value}.");
        }

        var slots = new ChestSlot[items.Count];
        long nextSlotGeneration = 1;
        for (int slotIndex = 0; slotIndex < slots.Length; slotIndex++)
        {
          slots[slotIndex] = new ChestSlot(
            new ChestSlotGeneration(nextSlotGeneration),
            items[slotIndex]);
          nextSlotGeneration++;
        }

        restores.Add(new PreparedChestRestore(
          id,
          new ChestState(
            worldScope,
            new ChestGeneration(generationValue),
            position,
            kindId: 0,
            record.Name,
            slots,
            nextSlotGeneration)));
      }

      return new PreparedChestLoad(this, restores);
    }
  }

  /// <summary>Commits a previously validated persisted chest set atomically for this store.</summary>
  public ChestLoadCommitResult CommitRestore(PreparedChestLoad prepared)
  {
    ArgumentNullException.ThrowIfNull(prepared);
    lock (_commitGate.SyncRoot)
    {
      if (!ReferenceEquals(prepared.Owner, this) ||
          _chests.Count != 0 ||
          !prepared.TryConsume())
      {
        return ChestLoadCommitResult.Stale;
      }

      var added = new List<ChestId>(prepared.Restores.Count);
      try
      {
        foreach (PreparedChestRestore restore in prepared.Restores)
        {
          _chests.Add(restore.Id, restore.State);
          added.Add(restore.Id);
        }
      }
      catch
      {
        foreach (ChestId id in added)
        {
          _chests.Remove(id);
        }
        throw;
      }

      return new ChestLoadCommitResult(
        ChestLoadCommitStatus.Applied,
        prepared.Restores.Count);
    }
  }

  /// <summary>Removes all chests owned by one world during rollback or world unload.</summary>
  public int ClearForWorldUnload(WorldScopeId worldScope)
  {
    if (!worldScope.IsValid)
    {
      throw new ArgumentException("A chest unload requires a valid world scope.",
        nameof(worldScope));
    }

    lock (_commitGate.SyncRoot)
    {
      ChestId[] ids = _chests
        .Where(pair => pair.Value.WorldScope == worldScope)
        .Select(pair => pair.Key)
        .ToArray();
      foreach (ChestId id in ids)
      {
        _chests.Remove(id);
      }
      return ids.Length;
    }
  }

  public OwnerCommitResult ApplyItems(
    ChestId id,
    WorldScopeId worldScope,
    ChestGeneration generation,
    CommitVersion expectedRevision,
    IReadOnlyCollection<ChestItemMutation> mutations,
    out ChestSnapshot? snapshot)
  {
    lock (_commitGate.SyncRoot)
    {
      snapshot = null;
      ArgumentNullException.ThrowIfNull(mutations);
      ChestBatchCommitResult result = ApplyItemsBatch(
        [new ChestItemMutationBatch(id, worldScope, generation, expectedRevision, mutations)]);
      if (result.IsApplied)
      {
        snapshot = result.Chests[0];
        return OwnerCommitResult.Applied(snapshot.Revision);
      }

      if (_chests.TryGetValue(id, out ChestState? current))
      {
        return new OwnerCommitResult(result.Status, current.Revision, result.ErrorCode, result.Detail);
      }

      return new OwnerCommitResult(result.Status, null, result.ErrorCode, result.Detail);
    }
  }

  /// <summary>Validates all listed chest changes before applying any of them.</summary>
  public ChestBatchCommitResult ApplyItemsBatch(
    IReadOnlyCollection<ChestItemMutationBatch> batches)
  {
    ArgumentNullException.ThrowIfNull(batches);
    lock (_commitGate.SyncRoot)
    {
      OwnerCommitResult prepareResult = PrepareItemsBatch(batches, out PreparedItemsBatch? prepared);
      if (prepareResult.Status != OwnerCommitStatus.Applied || prepared is null)
      {
        return ChestBatchCommitResult.From(prepareResult);
      }

      if (!CanCommit(prepared))
      {
        return ChestBatchCommitResult.From(OwnerCommitResult.Rejected(
          ProtocolErrorCode.Conflict,
          "A chest changed before the prepared batch could commit."));
      }

      CommitPrepared(prepared);
      return ChestBatchCommitResult.Applied(prepared.Snapshots);
    }
  }

  internal OwnerCommitResult PrepareItemsBatch(
    IReadOnlyCollection<ChestItemMutationBatch> batches,
    out PreparedItemsBatch? preparedBatch)
  {
    preparedBatch = null;
    ArgumentNullException.ThrowIfNull(batches);
    var seenChests = new HashSet<ChestId>();
    var preparedUpdates = new List<PreparedChestUpdate>(batches.Count);

    foreach (ChestItemMutationBatch batch in batches)
    {
      if (batch is null || !seenChests.Add(batch.ChestId))
      {
        return OwnerCommitResult.Rejected(
          ProtocolErrorCode.Forbidden,
          "The chest batch contains a null or duplicate chest id.");
      }

      if (!_chests.TryGetValue(batch.ChestId, out ChestState? state))
      {
        return Unavailable("A chest in the mutation batch is not available.");
      }

      if (state.WorldScope != batch.WorldScope || state.Generation != batch.Generation)
      {
        return Stale(state.Revision, "A chest world scope or generation changed.");
      }

      if (state.Revision != batch.ExpectedRevision)
      {
        return Conflict(state.Revision, "A chest revision changed.");
      }

      var seenSlots = new HashSet<int>();
      foreach (ChestItemMutation mutation in batch.Mutations)
      {
        if (mutation is null
          || mutation.SlotId < 0
          || mutation.SlotId >= state.Slots.Length
          || !seenSlots.Add(mutation.SlotId))
        {
          return OwnerCommitResult.Rejected(
            ProtocolErrorCode.Forbidden,
            "A chest mutation contains a null, invalid, or duplicate slot id.");
        }

        ChestSlot currentSlot = state.Slots[mutation.SlotId];
        if (currentSlot.Generation != mutation.ExpectedGeneration)
        {
          return Stale(state.Revision, "A chest item slot generation changed.");
        }

        if (!_itemPolicy.IsValid(mutation.Item))
        {
          return OwnerCommitResult.Rejected(
            ProtocolErrorCode.Forbidden,
            "A chest mutation contains an invalid item.");
        }
      }

      if (batch.Mutations.Count > 0
        && (state.NextSlotGeneration > long.MaxValue - batch.Mutations.Count
          || state.Revision.Value == long.MaxValue))
      {
        return Unavailable("A chest revision or slot generation space is exhausted.");
      }

      ChestSlot[] updatedSlots = state.Slots;
      long nextSlotGeneration = state.NextSlotGeneration;
      if (batch.Mutations.Count > 0)
      {
        updatedSlots = state.Slots.ToArray();
        foreach (ChestItemMutation mutation in batch.Mutations)
        {
          updatedSlots[mutation.SlotId] = new ChestSlot(
            new ChestSlotGeneration(nextSlotGeneration),
            mutation.Item);
          nextSlotGeneration++;
        }
      }

      CommitVersion nextRevision = batch.Mutations.Count > 0
        ? state.Revision.Next()
        : state.Revision;
      preparedUpdates.Add(new PreparedChestUpdate(
        batch.ChestId,
        state,
        state.Revision,
        updatedSlots,
        nextSlotGeneration,
        nextRevision,
        batch.Mutations.Count > 0));
    }

    var snapshots = new ChestSnapshot[preparedUpdates.Count];
    for (int index = 0; index < preparedUpdates.Count; index++)
    {
      PreparedChestUpdate update = preparedUpdates[index];
      snapshots[index] = Capture(
        update.Id,
        (ChestState)update.StateToken,
        update.NextRevision,
        (ChestSlot[])update.SlotsToken);
    }

    preparedBatch = new PreparedItemsBatch(
      Array.AsReadOnly(preparedUpdates.ToArray()),
      Array.AsReadOnly(snapshots));
    return new OwnerCommitResult(OwnerCommitStatus.Applied, null, null, null);
  }

  internal bool CanCommit(PreparedItemsBatch preparedBatch)
  {
    ArgumentNullException.ThrowIfNull(preparedBatch);
    foreach (PreparedChestUpdate update in preparedBatch.Updates)
    {
      if (!_chests.TryGetValue(update.Id, out ChestState? current)
        || !ReferenceEquals(current, update.StateToken)
        || current.Revision != update.ExpectedRevision)
      {
        return false;
      }
    }

    return true;
  }

  internal void CommitPrepared(PreparedItemsBatch preparedBatch)
  {
    ArgumentNullException.ThrowIfNull(preparedBatch);
    foreach (PreparedChestUpdate update in preparedBatch.Updates)
    {
      if (!update.HasChanges)
      {
        continue;
      }

      ChestState state = (ChestState)update.StateToken;
      state.Slots = (ChestSlot[])update.SlotsToken;
      state.NextSlotGeneration = update.NextSlotGeneration;
      state.Revision = update.NextRevision;
    }
  }

  public OwnerCommitResult SetName(
    ChestId id,
    WorldScopeId worldScope,
    ChestGeneration generation,
    CommitVersion expectedRevision,
    string name,
    out ChestSnapshot? snapshot)
  {
    lock (_commitGate.SyncRoot)
    {
      return SetNameCore(id, worldScope, generation, expectedRevision, name, out snapshot);
    }
  }

  private OwnerCommitResult SetNameCore(
    ChestId id,
    WorldScopeId worldScope,
    ChestGeneration generation,
    CommitVersion expectedRevision,
    string name,
    out ChestSnapshot? snapshot)
  {
    snapshot = null;
    if (!_chests.TryGetValue(id, out ChestState? state))
    {
      return Unavailable("The chest is not available.");
    }

    if (state.WorldScope != worldScope || state.Generation != generation)
    {
      return Stale(state.Revision, "The chest world scope or generation changed.");
    }

    if (state.Revision != expectedRevision)
    {
      return Conflict(state.Revision, "The chest revision changed.");
    }

    ArgumentNullException.ThrowIfNull(name);
    if (name.Length > ContainerContentFacts.MaximumChestNameLength)
    {
      return OwnerCommitResult.Rejected(ProtocolErrorCode.InvalidLength, "The chest name exceeds Version4 bounds.");
    }
    if (state.Revision.Value == long.MaxValue)
    {
      return Unavailable("The chest revision space is exhausted.");
    }

    state.Name = name;
    state.Revision = state.Revision.Next();
    snapshot = Capture(id, state);
    return OwnerCommitResult.Applied(state.Revision);
  }

  public OwnerCommitResult Resize(
    ChestId id,
    WorldScopeId worldScope,
    ChestGeneration generation,
    CommitVersion expectedRevision,
    int newSize,
    Func<int, bool> isAllowedSize,
    out ChestSnapshot? snapshot)
  {
    snapshot = null;
    ArgumentNullException.ThrowIfNull(isAllowedSize);
    if (newSize is < 1 or > ContainerContentFacts.AbsoluteMaximumChestSlotCountForVersion4
      || !isAllowedSize(newSize))
    {
      return OwnerCommitResult.Rejected(ProtocolErrorCode.Forbidden, "The requested chest size is not allowed.");
    }

    lock (_commitGate.SyncRoot)
    {
      return ResizeCore(id, worldScope, generation, expectedRevision, newSize, out snapshot);
    }
  }

  private OwnerCommitResult ResizeCore(
    ChestId id,
    WorldScopeId worldScope,
    ChestGeneration generation,
    CommitVersion expectedRevision,
    int newSize,
    out ChestSnapshot? snapshot)
  {
    snapshot = null;
    if (!_chests.TryGetValue(id, out ChestState? state))
    {
      return Unavailable("The chest is not available.");
    }

    if (state.WorldScope != worldScope || state.Generation != generation)
    {
      return Stale(state.Revision, "The chest world scope or generation changed.");
    }

    if (state.Revision != expectedRevision)
    {
      return Conflict(state.Revision, "The chest revision changed.");
    }

    if (newSize < state.Slots.Length)
    {
      for (int index = newSize; index < state.Slots.Length; index++)
      {
        if (!state.Slots[index].Item.IsEmpty)
        {
          return OwnerCommitResult.Rejected(ProtocolErrorCode.Conflict, "Shrinking would discard chest contents.");
        }
      }
    }

    if (newSize == state.Slots.Length)
    {
      snapshot = Capture(id, state);
      return OwnerCommitResult.Applied(state.Revision);
    }

    int newSlots = Math.Max(0, newSize - state.Slots.Length);
    if (state.Revision.Value == long.MaxValue || state.NextSlotGeneration > long.MaxValue - newSlots)
    {
      return Unavailable("The chest revision or slot generation space is exhausted.");
    }

    var resizedSlots = new ChestSlot[newSize];
    int copiedCount = Math.Min(newSize, state.Slots.Length);
    Array.Copy(state.Slots, resizedSlots, copiedCount);
    for (int index = copiedCount; index < resizedSlots.Length; index++)
    {
      resizedSlots[index] = new ChestSlot(
        new ChestSlotGeneration(state.NextSlotGeneration),
        InventoryItemState.Empty);
      state.NextSlotGeneration++;
    }

    state.Slots = resizedSlots;
    state.Revision = state.Revision.Next();
    snapshot = Capture(id, state);
    return OwnerCommitResult.Applied(state.Revision);
  }

  public OwnerCommitResult Destroy(
    ChestId id,
    WorldScopeId worldScope,
    ChestGeneration generation,
    CommitVersion expectedRevision)
  {
    lock (_commitGate.SyncRoot)
    {
      return DestroyCore(id, worldScope, generation, expectedRevision);
    }
  }

  private OwnerCommitResult DestroyCore(
    ChestId id,
    WorldScopeId worldScope,
    ChestGeneration generation,
    CommitVersion expectedRevision)
  {
    if (!_chests.TryGetValue(id, out ChestState? state))
    {
      return Unavailable("The chest is not available.");
    }

    if (state.WorldScope != worldScope || state.Generation != generation)
    {
      return Stale(state.Revision, "The chest world scope or generation changed.");
    }

    if (state.Revision != expectedRevision)
    {
      return Conflict(state.Revision, "The chest revision changed.");
    }

    if (state.Slots.Any(slot => !slot.Item.IsEmpty))
    {
      return OwnerCommitResult.Rejected(
        ProtocolErrorCode.Conflict,
        "The chest must be emptied or its contents transferred before destruction.");
    }

    if (state.Generation.Value == long.MaxValue)
    {
      return Unavailable("The chest generation space is exhausted.");
    }

    _nextGeneration[id] = state.Generation.Value + 1;
    _chests.Remove(id);
    return OwnerCommitResult.Applied(state.Revision);
  }

  private static ChestSnapshot Capture(ChestId id, ChestState state)
  {
    return Capture(id, state, state.Revision, state.Slots);
  }

  private static ChestSnapshot Capture(
    ChestId id,
    ChestState state,
    CommitVersion revision,
    IReadOnlyList<ChestSlot> sourceSlots)
  {
    var slots = new ChestSlotSnapshot[sourceSlots.Count];
    for (int index = 0; index < sourceSlots.Count; index++)
    {
      ChestSlot slot = sourceSlots[index];
      slots[index] = new ChestSlotSnapshot(index, slot.Generation, slot.Item);
    }

    return new ChestSnapshot(
      id,
      state.WorldScope,
      state.Generation,
      state.Position,
      state.KindId,
      state.Name,
      revision,
      slots);
  }

  private static InventoryItemState[] CreateEmptyItems(int slotCount)
  {
    if (slotCount > ContainerContentFacts.AbsoluteMaximumChestSlotCountForVersion4)
    {
      throw new ArgumentOutOfRangeException(nameof(slotCount));
    }

    var items = new InventoryItemState[slotCount];
    Array.Fill(items, InventoryItemState.Empty);
    return items;
  }

  private InventoryItemState[] ConvertItems(IReadOnlyList<WorldFileChestItem> persistedItems)
  {
    ArgumentNullException.ThrowIfNull(persistedItems);
    if (persistedItems.Count == 0)
    {
      return CreateEmptyItems(ContainerContentFacts.DefaultChestSlotCount);
    }

    if (persistedItems.Count > ContainerContentFacts.AbsoluteMaximumChestSlotCountForVersion4)
    {
      throw new InvalidDataException("The persisted chest slot count exceeds Version4 bounds.");
    }

    var items = new InventoryItemState[persistedItems.Count];
    for (int index = 0; index < persistedItems.Count; index++)
    {
      WorldFileChestItem item = persistedItems[index]
        ?? throw new ArgumentException("A persisted chest item cannot be null.",
          nameof(persistedItems));
      InventoryItemState converted = new(item.Type, item.Stack, item.Prefix, Favorited: false);
      if (!_itemPolicy.IsValid(converted))
      {
        throw new InvalidDataException(
          $"The persisted chest item at slot {index} is outside Version4 item facts.");
      }
      items[index] = converted;
    }

    return items;
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

  internal sealed class ChestState(
    WorldScopeId worldScope,
    ChestGeneration generation,
    WorldTilePoint position,
    ushort kindId,
    string name,
    ChestSlot[] slots,
    long nextSlotGeneration)
  {
    public WorldScopeId WorldScope { get; } = worldScope;
    public ChestGeneration Generation { get; } = generation;
    public WorldTilePoint Position { get; } = position;
    public ushort KindId { get; } = kindId;
    public string Name { get; set; } = name;
    public ChestSlot[] Slots { get; set; } = slots;
    public CommitVersion Revision { get; set; } = CommitVersion.Initial;
    public long NextSlotGeneration { get; set; } = nextSlotGeneration;
  }

  internal sealed record ChestSlot(ChestSlotGeneration Generation, InventoryItemState Item);

  internal sealed record PreparedItemsBatch(
      IReadOnlyList<PreparedChestUpdate> Updates,
      IReadOnlyList<ChestSnapshot> Snapshots);

  internal sealed record PreparedChestUpdate(
    ChestId Id,
    object StateToken,
    CommitVersion ExpectedRevision,
    object SlotsToken,
    long NextSlotGeneration,
    CommitVersion NextRevision,
    bool HasChanges);

  internal sealed record PreparedChestRestore(ChestId Id, ChestState State);

  public sealed class PreparedChestLoad
  {
    internal PreparedChestLoad(ChestStore owner, IReadOnlyList<PreparedChestRestore> restores)
    {
      Owner = owner;
      Restores = restores;
    }

    internal ChestStore Owner { get; }

    internal IReadOnlyList<PreparedChestRestore> Restores { get; }

    internal bool TryConsume()
    {
      if (IsConsumed)
      {
        return false;
      }

      IsConsumed = true;
      return true;
    }

    private bool IsConsumed { get; set; }
  }

  public enum ChestLoadCommitStatus
  {
    Applied,
    Stale
  }

  public sealed record ChestLoadCommitResult(ChestLoadCommitStatus Status, int Count)
  {
    public static ChestLoadCommitResult Stale { get; } =
      new(ChestLoadCommitStatus.Stale, 0);
  }
}
