using MinimalImplementation.Server.Identity;
using MinimalImplementation.Server.Inventory.Content;
using MinimalImplementation.Server.Inventory.Protocol;
using MinimalImplementation.Server.Inventory.State;

namespace MinimalImplementation.Server.Inventory.Crafting;

/// <summary>
/// Plans chest material consumption against immutable snapshots. The caller must revalidate every
/// generation and revision and commit all returned chest writes atomically on the server loop.
/// </summary>
public sealed class CraftingMaterialPlanner
{
  private readonly ICraftingMaterialCatalog _catalog;

  public CraftingMaterialPlanner(ICraftingMaterialCatalog catalog)
  {
    ArgumentNullException.ThrowIfNull(catalog);
    _catalog = catalog;
  }

  public CraftingMaterialPreparation Prepare(
    CraftingModule11Request request,
    IReadOnlyList<ChestSnapshot> accessibleChests)
  {
    ArgumentNullException.ThrowIfNull(request);
    ArgumentNullException.ThrowIfNull(accessibleChests);

    var snapshotsById = new Dictionary<ChestId, WorkingChest>(accessibleChests.Count);
    WorldScopeId? selectedWorldScope = null;

    for (int chestIndex = 0; chestIndex < accessibleChests.Count; chestIndex++)
    {
      ChestSnapshot? chest = accessibleChests[chestIndex];
      if (chest is null
        || chest.Id.Value >= ContainerContentFacts.ChestIndexCount
        || chest.Slots.Count is < 1 or > ContainerContentFacts.AbsoluteMaximumChestSlotCountForVersion4
        || !chest.Generation.IsValid
        || !chest.Revision.IsValid
        || snapshotsById.ContainsKey(chest.Id))
      {
        return InvalidChestSnapshot();
      }

      if (selectedWorldScope is { } worldScope && worldScope != chest.WorldScope)
      {
        return InvalidChestSnapshot();
      }

      selectedWorldScope ??= chest.WorldScope;

      var slots = new InventoryItemState[chest.Slots.Count];
      var slotGenerations = new ChestSlotGeneration[chest.Slots.Count];
      for (int slotIndex = 0; slotIndex < chest.Slots.Count; slotIndex++)
      {
        ChestSlotSnapshot? slot = chest.Slots[slotIndex];
        if (slot is null
          || slot.SlotId != slotIndex
          || !slot.Generation.IsValid
          || slot.Item is null
          || slot.Item.Stack < 0)
        {
          return InvalidChestSnapshot();
        }

        slots[slotIndex] = slot.Item;
        slotGenerations[slotIndex] = slot.Generation;
      }

      snapshotsById.Add(chest.Id, new WorkingChest(chest, slots, slotGenerations));
    }

    var workingChests = new List<WorkingChest>(snapshotsById.Count);
    var seenRequestedChestIds = new HashSet<int>();
    foreach (int requestedChestId in request.ChestIds)
    {
      if (requestedChestId < 0
        || requestedChestId >= ContainerContentFacts.ChestIndexCount
        || !seenRequestedChestIds.Add(requestedChestId))
      {
        continue;
      }

      var chestId = new ChestId((ushort)requestedChestId);
      if (snapshotsById.TryGetValue(chestId, out WorkingChest? chest))
      {
        workingChests.Add(chest);
      }
    }

    if (workingChests.Count != snapshotsById.Count)
    {
      return InvalidChestSnapshot();
    }

    foreach (CraftingMaterialRequirement? requirement in request.Materials)
    {
      if (requirement is null
        || requirement.Stack <= 0
        || !_catalog.IsSupportedRequirement(requirement.ItemIdOrRecipeGroup))
      {
        return new CraftingMaterialPreparation(
          CraftingMaterialPreparationStatus.UnsupportedMaterial,
          Array.Empty<ChestItemMutationBatch>(),
          requirement?.ItemIdOrRecipeGroup);
      }
    }

    foreach (CraftingMaterialRequirement requirement in request.Materials)
    {
      int remaining = requirement.Stack;
      foreach (WorkingChest chest in workingChests)
      {
        for (int slotIndex = 0; slotIndex < chest.Items.Length && remaining > 0; slotIndex++)
        {
          InventoryItemState item = chest.Items[slotIndex];
          if (item.IsEmpty || !_catalog.Matches(requirement.ItemIdOrRecipeGroup, item.Type))
          {
            continue;
          }

          int consumed = Math.Min(item.Stack, remaining);
          chest.Items[slotIndex] = item.WithStack(item.Stack - consumed);
          remaining -= consumed;
        }

        if (remaining == 0)
        {
          break;
        }
      }

      if (remaining > 0)
      {
        return new CraftingMaterialPreparation(
          CraftingMaterialPreparationStatus.InsufficientMaterials,
          Array.Empty<ChestItemMutationBatch>(),
          requirement.ItemIdOrRecipeGroup);
      }
    }

    var plans = new List<ChestItemMutationBatch>(workingChests.Count);
    foreach (WorkingChest chest in workingChests)
    {
      var mutations = new List<ChestItemMutation>();
      for (int slotIndex = 0; slotIndex < chest.Items.Length; slotIndex++)
      {
        if (chest.Items[slotIndex] != chest.Snapshot.Slots[slotIndex].Item)
        {
          mutations.Add(new ChestItemMutation(
            slotIndex,
            chest.SlotGenerations[slotIndex],
            chest.Items[slotIndex]));
        }
      }

      if (mutations.Count > 0)
      {
        plans.Add(new ChestItemMutationBatch(
          chest.Snapshot.Id,
          chest.Snapshot.WorldScope,
          chest.Snapshot.Generation,
          chest.Snapshot.Revision,
          mutations));
      }
    }

    return new CraftingMaterialPreparation(
      CraftingMaterialPreparationStatus.Prepared,
      plans);
  }

  private static CraftingMaterialPreparation InvalidChestSnapshot()
  {
    return new CraftingMaterialPreparation(
      CraftingMaterialPreparationStatus.InvalidChestSnapshot,
      Array.Empty<ChestItemMutationBatch>());
  }

  private sealed class WorkingChest(
    ChestSnapshot snapshot,
    InventoryItemState[] items,
    ChestSlotGeneration[] slotGenerations)
  {
    public ChestSnapshot Snapshot { get; } = snapshot;
    public InventoryItemState[] Items { get; } = items;
    public ChestSlotGeneration[] SlotGenerations { get; } = slotGenerations;
  }
}
