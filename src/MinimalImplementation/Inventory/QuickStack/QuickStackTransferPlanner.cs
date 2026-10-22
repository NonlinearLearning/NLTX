using MinimalImplementation.Server.Identity;
using MinimalImplementation.Server.Inventory.Content;
using MinimalImplementation.Server.Inventory.Protocol;
using MinimalImplementation.Server.Inventory.Rules;
using MinimalImplementation.Server.Inventory.State;

namespace MinimalImplementation.Server.Inventory.QuickStack;

/// <summary>
/// Calculates a QuickStack transfer from server-owned snapshots. The candidate transfer order is
/// transcribed from the complete earlier source copy; Version4's corresponding implementation is
/// empty, so this planner is not evidence of exact Version4 algorithm parity.
/// </summary>
public sealed class QuickStackTransferPlanner
{
  private readonly ItemValidationPolicy _items;
  private readonly IQuickStackCategoryResolver _categories;

  public QuickStackTransferPlanner(
    ItemValidationPolicy items,
    IQuickStackCategoryResolver categories)
  {
    ArgumentNullException.ThrowIfNull(items);
    ArgumentNullException.ThrowIfNull(categories);
    _items = items;
    _categories = categories;
  }

  public QuickStackPreparation Prepare(
    QuickStackRequestPayload request,
    InventorySnapshot inventory,
    WorldScopeId expectedWorldScope,
    IReadOnlyList<QuickStackDestinationCandidate> destinations)
  {
    ArgumentNullException.ThrowIfNull(request);
    ArgumentNullException.ThrowIfNull(inventory);
    ArgumentNullException.ThrowIfNull(destinations);

    if (!IsValidInventory(inventory))
    {
      return Invalid(QuickStackPreparationStatus.InvalidInventorySnapshot);
    }

    if (!TryCreateSources(request, inventory, out List<WorkingSource>? sources))
    {
      return Invalid(QuickStackPreparationStatus.InvalidRequest);
    }

    if (!expectedWorldScope.IsValid
      || !TryCreateDestinations(
        destinations,
        expectedWorldScope,
        request.SmartStack,
        out List<WorkingDestination>? workingDestinations))
    {
      return Invalid(QuickStackPreparationStatus.InvalidDestinationSnapshot);
    }

    Dictionary<int, List<WorkingDestination>> destinationsByItemType =
      TransferIntoMatchingStacks(sources!, workingDestinations!);
    TransferIntoFreeMatchingSlots(sources!, destinationsByItemType);
    if (request.SmartStack)
    {
      TransferIntoBestCategorySlots(sources!, workingDestinations!);
    }

    return CreatePreparation(sources!, workingDestinations!);
  }

  private bool IsValidInventory(InventorySnapshot inventory)
  {
    if (!inventory.Revision.IsValid || inventory.Slots.Count > short.MaxValue + 1)
    {
      return false;
    }

    for (int index = 0; index < inventory.Slots.Count; index++)
    {
      InventorySlotSnapshot? slot = inventory.Slots[index];
      if (slot is null
        || slot.SlotId != index
        || !slot.Generation.IsValid
        || slot.Item is null
        || !_items.IsValid(slot.Item))
      {
        return false;
      }
    }

    return true;
  }

  private bool TryCreateSources(
    QuickStackRequestPayload request,
    InventorySnapshot inventory,
    out List<WorkingSource>? sources)
  {
    sources = null;
    var selectedSlots = new HashSet<short>();
    var workingSources = new List<WorkingSource>(request.SlotIds.Count);
    foreach (short slotId in request.SlotIds)
    {
      if (slotId < 0
        || slotId >= inventory.Slots.Count
        || !selectedSlots.Add(slotId))
      {
        return false;
      }

      InventorySlotSnapshot slot = inventory.Slots[slotId];
      if (_items.CanQuickStackSource(slot.Item))
      {
        workingSources.Add(new WorkingSource(slot, workingSources.Count));
      }
    }

    sources = workingSources;
    return true;
  }

  private bool TryCreateDestinations(
    IReadOnlyList<QuickStackDestinationCandidate> destinations,
    WorldScopeId expectedWorldScope,
    bool includeCategoryScores,
    out List<WorkingDestination>? workingDestinations)
  {
    workingDestinations = null;
    var ids = new HashSet<ushort>();
    var candidates = new List<WorkingDestination>(destinations.Count);

    foreach (QuickStackDestinationCandidate? candidate in destinations)
    {
      if (candidate?.Chest is not { } chest
        || chest.Id.Value >= ContainerContentFacts.ChestIndexCount
        || !chest.WorldScope.IsValid
        || chest.WorldScope != expectedWorldScope
        || !chest.Generation.IsValid
        || !chest.Revision.IsValid
        || chest.Slots.Count is < 1
          or > ContainerContentFacts.AbsoluteMaximumChestSlotCountForVersion4
        || !ids.Add(chest.Id.Value))
      {
        return false;
      }

      var slots = new InventoryItemState[chest.Slots.Count];
      var generations = new ChestSlotGeneration[chest.Slots.Count];
      var categoryScores = new Dictionary<int, int>();
      var freeSlots = new List<int>();
      for (int index = 0; index < chest.Slots.Count; index++)
      {
        ChestSlotSnapshot? slot = chest.Slots[index];
        if (slot is null
          || slot.SlotId != index
          || !slot.Generation.IsValid
          || slot.Item is null
          || !_items.IsValid(slot.Item))
        {
          return false;
        }

        slots[index] = slot.Item;
        generations[index] = slot.Generation;
        if (slot.Item.IsEmpty)
        {
          freeSlots.Add(index);
        }
        else if (includeCategoryScores
          && _categories.TryGetCategory(slot.Item.Type, out int category))
        {
          categoryScores[category] = categoryScores.GetValueOrDefault(category) + 1;
        }
      }

      // The earlier candidate skips a chest that has no occupied slots.
      if (freeSlots.Count != chest.Slots.Count)
      {
        candidates.Add(new WorkingDestination(
          candidate,
          slots,
          generations,
          freeSlots.ToArray(),
          categoryScores));
      }
    }

    workingDestinations = candidates;
    return true;
  }

  private Dictionary<int, List<WorkingDestination>> TransferIntoMatchingStacks(
    IReadOnlyList<WorkingSource> sources,
    IReadOnlyList<WorkingDestination> destinations)
  {
    var destinationsByItemType = new Dictionary<int, List<WorkingDestination>>();
    var indexedDestinationsByItemType = new Dictionary<int, HashSet<WorkingDestination>>();

    foreach (WorkingDestination destination in destinations)
    {
      for (int destinationSlot = 0; destinationSlot < destination.Items.Length; destinationSlot++)
      {
        InventoryItemState destinationItem = destination.Items[destinationSlot];
        if (destinationItem.IsEmpty)
        {
          continue;
        }

        for (int sourceIndex = 0; sourceIndex < sources.Count; sourceIndex++)
        {
          WorkingSource source = sources[sourceIndex];
          InventoryItemState sourceItem = source.Item;
          if (sourceItem.IsEmpty || sourceItem.Type != destinationItem.Type)
          {
            continue;
          }

          if (destination.Candidate.IsLockedOrInUse)
          {
            source.TransferBlocked = true;
            destination.TransferBlocked = true;
            continue;
          }

          if (_items.CanStack(destinationItem, sourceItem)
            && ItemContentFacts.TryGetMaximumStack(destinationItem.Type, out int maximumStack)
            && destinationItem.Stack < maximumStack)
          {
            int moved = Math.Min(maximumStack - destinationItem.Stack, sourceItem.Stack);
            destination.Items[destinationSlot] = destinationItem.WithStack(
              destinationItem.Stack + moved);
            source.Item = sourceItem.WithStack(sourceItem.Stack - moved);
            destinationItem = destination.Items[destinationSlot];
            sourceItem = source.Item;
            if (sourceItem.IsEmpty)
            {
              continue;
            }
          }

          AddMatchingDestination(
            destinationItem.Type,
            destination,
            destinationsByItemType,
            indexedDestinationsByItemType);
        }
      }
    }

    return destinationsByItemType;
  }

  private static void AddMatchingDestination(
    int itemType,
    WorkingDestination destination,
    Dictionary<int, List<WorkingDestination>> destinationsByItemType,
    Dictionary<int, HashSet<WorkingDestination>> indexedDestinationsByItemType)
  {
    if (!destinationsByItemType.TryGetValue(itemType, out List<WorkingDestination>? matches))
    {
      matches = new List<WorkingDestination>();
      destinationsByItemType.Add(itemType, matches);
      indexedDestinationsByItemType.Add(itemType, new HashSet<WorkingDestination>());
    }

    if (indexedDestinationsByItemType[itemType].Add(destination))
    {
      matches.Add(destination);
    }
  }

  private void TransferIntoFreeMatchingSlots(
    IReadOnlyList<WorkingSource> sources,
    IReadOnlyDictionary<int, List<WorkingDestination>> destinationsByItemType)
  {
    foreach (WorkingSource source in sources)
    {
      if (source.Item.IsEmpty
        || !destinationsByItemType.TryGetValue(
          source.Item.Type,
          out List<WorkingDestination>? matches))
      {
        continue;
      }

      WorkingDestination? destination = matches.FirstOrDefault(candidate => candidate.HasFreeSlot);
      if (destination is null)
      {
        continue;
      }

      Consolidate(sources, source);
      InsertIntoFreeSlot(source, destination);
    }
  }

  private void TransferIntoBestCategorySlots(
    IReadOnlyList<WorkingSource> sources,
    IReadOnlyList<WorkingDestination> destinations)
  {
    foreach (WorkingSource source in sources)
    {
      if (source.Item.IsEmpty
        || source.TransferBlocked
        || !_categories.TryGetCategory(source.Item.Type, out int category))
      {
        continue;
      }

      WorkingDestination? bestDestination = null;
      int bestScore = 0;
      foreach (WorkingDestination destination in destinations)
      {
        if (!destination.HasFreeSlot
          || !destination.CategoryScores.TryGetValue(category, out int score)
          || score <= bestScore)
        {
          continue;
        }

        bestDestination = destination;
        bestScore = score;
      }

      if (bestDestination is null)
      {
        continue;
      }

      if (bestDestination.Candidate.IsLockedOrInUse)
      {
        source.TransferBlocked = true;
        bestDestination.TransferBlocked = true;
        continue;
      }

      Consolidate(sources, source);
      InsertIntoFreeSlot(source, bestDestination);
    }
  }

  private void Consolidate(IReadOnlyList<WorkingSource> sources, WorkingSource source)
  {
    if (!ItemContentFacts.TryGetMaximumStack(source.Item.Type, out int maximumStack))
    {
      return;
    }

    for (int sourceIndex = source.Index + 1;
      sourceIndex < sources.Count && source.Item.Stack < maximumStack;
      sourceIndex++)
    {
      WorkingSource other = sources[sourceIndex];
      if (other.Item.IsEmpty || !_items.CanStack(source.Item, other.Item))
      {
        continue;
      }

      int moved = Math.Min(maximumStack - source.Item.Stack, other.Item.Stack);
      source.Item = source.Item.WithStack(source.Item.Stack + moved);
      other.Item = other.Item.WithStack(other.Item.Stack - moved);
    }
  }

  private static void InsertIntoFreeSlot(WorkingSource source, WorkingDestination destination)
  {
    int slotIndex = destination.ConsumeFreeSlot();
    InventoryItemState item = source.Item;
    destination.Items[slotIndex] = item;
    source.Item = InventoryItemState.Empty;
  }

  private static QuickStackPreparation CreatePreparation(
    IReadOnlyList<WorkingSource> sources,
    IReadOnlyList<WorkingDestination> destinations)
  {
    var inventoryMutations = new List<InventoryMutation>();
    foreach (WorkingSource source in sources)
    {
      if (source.Item != source.Snapshot.Item)
      {
        inventoryMutations.Add(new InventoryMutation(
          source.Snapshot.SlotId,
          source.Snapshot.Generation,
          source.Item));
      }
    }

    var chestBatches = new List<ChestItemMutationBatch>();
    var blockedChestIds = new List<ushort>();
    foreach (WorkingDestination destination in destinations)
    {
      var mutations = new List<ChestItemMutation>();
      for (int slotIndex = 0; slotIndex < destination.Items.Length; slotIndex++)
      {
        if (destination.Items[slotIndex] != destination.Candidate.Chest.Slots[slotIndex].Item)
        {
          mutations.Add(new ChestItemMutation(
            slotIndex,
            destination.SlotGenerations[slotIndex],
            destination.Items[slotIndex]));
        }
      }

      if (mutations.Count > 0)
      {
        ChestSnapshot chest = destination.Candidate.Chest;
        chestBatches.Add(new ChestItemMutationBatch(
          chest.Id,
          chest.WorldScope,
          chest.Generation,
          chest.Revision,
          mutations));
      }

      if (destination.TransferBlocked)
      {
        blockedChestIds.Add(destination.Candidate.Chest.Id.Value);
      }
    }

    return new QuickStackPreparation(
      QuickStackPreparationStatus.Prepared,
      inventoryMutations,
      chestBatches,
      blockedChestIds);
  }

  private static QuickStackPreparation Invalid(QuickStackPreparationStatus status)
  {
    return new QuickStackPreparation(
      status,
      Array.Empty<InventoryMutation>(),
      Array.Empty<ChestItemMutationBatch>(),
      Array.Empty<ushort>());
  }

  private sealed class WorkingSource(InventorySlotSnapshot snapshot, int index)
  {
    public InventorySlotSnapshot Snapshot { get; } = snapshot;
    public InventoryItemState Item { get; set; } = snapshot.Item;
    public bool TransferBlocked { get; set; }
    public int Index { get; } = index;
  }

  private sealed class WorkingDestination(
    QuickStackDestinationCandidate candidate,
    InventoryItemState[] items,
    ChestSlotGeneration[] slotGenerations,
    int[] freeSlots,
    Dictionary<int, int> categoryScores)
  {
    private int _nextFreeSlot;

    public QuickStackDestinationCandidate Candidate { get; } = candidate;
    public InventoryItemState[] Items { get; } = items;
    public ChestSlotGeneration[] SlotGenerations { get; } = slotGenerations;
    public Dictionary<int, int> CategoryScores { get; } = categoryScores;
    public int[] FreeSlots { get; } = freeSlots;
    public bool TransferBlocked { get; set; }
    public bool HasFreeSlot => _nextFreeSlot < FreeSlots.Length;

    public int ConsumeFreeSlot()
    {
      return FreeSlots[_nextFreeSlot++];
    }
  }
}
