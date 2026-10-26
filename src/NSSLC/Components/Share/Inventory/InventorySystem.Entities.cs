using System.Collections.Generic;
using Arch.Core;
using ArchEntity = Arch.Core.Entity;
using Terraria.Combat;
using Terraria.Item;
using Terraria.Relationships;

namespace Terraria.Inventory;

/// <summary>Entity containment and lifecycle operations share the inventory commit boundary.</summary>
public sealed partial class InventorySystem {
  /// <summary>
  /// Stores an entire item entity without copying its stack or other components. The item
  /// and its owner require identities assigned by the host's entity creation boundary.
  /// </summary>
  public InventoryTransferResult TryStoreEntity(
      ArchEntity inventoryEntity,
      ArchEntity itemEntity,
      int? targetSlotIndex = null) {
    InventoryTransferFailure failure = GetInventory(inventoryEntity, out InventoryComponent inventory);
    if (failure != InventoryTransferFailure.None) {
      return InventoryTransferResult.Rejected(failure);
    }

    failure = GetUncontainedItem(itemEntity, out ItemStackComponent item);
    if (failure != InventoryTransferFailure.None) {
      return InventoryTransferResult.Rejected(failure);
    }

    failure = GetDefinition(item.Stack, out ItemDefinition definition);
    if (failure != InventoryTransferFailure.None) {
      return InventoryTransferResult.Rejected(failure);
    }

    if (!TryGetIdentity(inventoryEntity, out EntityUuid inventoryId) ||
        !TryGetIdentity(itemEntity, out EntityUuid itemId)) {
      return InventoryTransferResult.Rejected(InventoryTransferFailure.MissingEntityIdentity);
    }

    if (!TryResolve(inventoryId, out ArchEntity resolvedOwner) ||
        resolvedOwner != inventoryEntity || !TryResolve(itemId, out ArchEntity resolvedItem) ||
        resolvedItem != itemEntity) {
      return InventoryTransferResult.Rejected(InventoryTransferFailure.ContainmentMismatch);
    }

    failure = ValidateContainment(inventoryEntity, itemId);
    if (failure != InventoryTransferFailure.None) {
      return InventoryTransferResult.Rejected(failure);
    }

    InventorySlot[] plan = inventory.CaptureSlots();
    failure = ValidateTargetSlot(plan, targetSlotIndex);
    if (failure != InventoryTransferFailure.None) {
      return InventoryTransferResult.Rejected(failure);
    }

    int position = FindEntitySlot(plan, definition, item.Stack.Quantity, targetSlotIndex);
    if (position < 0) {
      return InventoryTransferResult.Rejected(InventoryTransferFailure.NoCompatibleSlot);
    }

    InventorySlotDefinition slot = plan[position].Definition;
    plan[position] = new InventorySlot(slot, itemEntityId: itemId);
    var relation = new ContainedInInventoryComponent(inventoryId, slot.Index);
    _world.Add(itemEntity, relation);
    inventory.CommitSlots(plan);
    return InventoryTransferResult.Success(item.Stack.Quantity);
  }

  /// <summary>
  /// Detaches an entire item entity. The host then supplies world placement, pickup delay
  /// and spatial activation if the item is to become a world drop.
  /// </summary>
  public InventoryTransferResult TryTakeEntity(
      ArchEntity inventoryEntity,
      int slotIndex,
      out ArchEntity itemEntity) {
    itemEntity = ArchEntity.Null;
    InventoryTransferFailure failure = GetInventory(inventoryEntity, out InventoryComponent inventory);
    if (failure != InventoryTransferFailure.None) {
      return InventoryTransferResult.Rejected(failure);
    }

    if (!inventory.TryGetSlot(slotIndex, out InventorySlot slot)) {
      return InventoryTransferResult.Rejected(InventoryTransferFailure.InvalidSlot);
    }

    failure = GetContainedItem(inventoryEntity, slot, out ArchEntity contained,
        out ItemStackComponent item);
    if (failure != InventoryTransferFailure.None) {
      return InventoryTransferResult.Rejected(failure);
    }

    InventorySlot[] plan = inventory.CaptureSlots();
    plan[FindPosition(plan, slotIndex)] = new InventorySlot(slot.Definition);
    int quantity = item.Stack.Quantity;
    _world.Remove<ContainedInInventoryComponent>(contained);
    inventory.CommitSlots(plan);
    itemEntity = contained;
    return InventoryTransferResult.Success(quantity);
  }

  /// <summary>Reads the authoritative stack without exposing a mutable component reference.</summary>
  public bool TryGetStack(ArchEntity inventoryEntity, int slotIndex, out ItemStack stack) {
    stack = default;
    if (GetInventory(inventoryEntity, out InventoryComponent inventory) !=
            InventoryTransferFailure.None ||
        !inventory.TryGetSlot(slotIndex, out InventorySlot slot)) {
      return false;
    }

    if (slot.Content is ItemStack value) {
      stack = value;
      return true;
    }

    if (GetContainedItem(inventoryEntity, slot, out _, out ItemStackComponent item) !=
        InventoryTransferFailure.None) {
      return false;
    }

    stack = item.Stack;
    return true;
  }

  private InventoryTransferResult MoveEntity(
      ArchEntity sourceEntity,
      InventoryComponent source,
      InventorySlot sourceSlot,
      ArchEntity targetEntity,
      InventoryComponent target,
      int quantity,
      int? targetSlotIndex) {
    InventoryTransferFailure failure = GetContainedItem(sourceEntity, sourceSlot,
        out ArchEntity itemEntity, out ItemStackComponent item);
    if (failure != InventoryTransferFailure.None) {
      return InventoryTransferResult.Rejected(failure);
    }

    if (quantity <= 0 || quantity > item.Stack.Quantity) {
      return InventoryTransferResult.Rejected(InventoryTransferFailure.InvalidQuantity);
    }

    if (quantity != item.Stack.Quantity) {
      return InventoryTransferResult.Rejected(InventoryTransferFailure.EntitySplitNotSupported);
    }

    failure = GetDefinition(item.Stack, out ItemDefinition definition);
    if (failure != InventoryTransferFailure.None) {
      return InventoryTransferResult.Rejected(failure);
    }

    if (!TryGetIdentity(targetEntity, out EntityUuid targetId)) {
      return InventoryTransferResult.Rejected(InventoryTransferFailure.MissingEntityIdentity);
    }

    failure = ValidateContainment(targetEntity, sourceSlot.ItemEntityId);
    if (failure != InventoryTransferFailure.None) {
      return InventoryTransferResult.Rejected(failure);
    }

    bool sameInventory = sourceEntity == targetEntity;
    InventorySlot[] sourcePlan = source.CaptureSlots();
    InventorySlot[] targetPlan = sameInventory ? sourcePlan : target.CaptureSlots();
    failure = ValidateTargetSlot(targetPlan, targetSlotIndex);
    if (failure != InventoryTransferFailure.None) {
      return InventoryTransferResult.Rejected(failure);
    }

    sourcePlan[FindPosition(sourcePlan, sourceSlot.Definition.Index)] =
        new InventorySlot(sourceSlot.Definition);
    int position = FindEntitySlot(
        targetPlan,
        definition,
        quantity,
        targetSlotIndex,
        sameInventory ? sourceSlot.Definition.Index : null);
    if (position < 0) {
      return InventoryTransferResult.Rejected(InventoryTransferFailure.NoCompatibleSlot);
    }

    InventorySlotDefinition slot = targetPlan[position].Definition;
    targetPlan[position] = new InventorySlot(slot, itemEntityId: sourceSlot.ItemEntityId);
    var relation = new ContainedInInventoryComponent(targetId, slot.Index);
    ref ContainedInInventoryComponent currentRelation =
        ref _world.Get<ContainedInInventoryComponent>(itemEntity);
    currentRelation = relation;
    source.CommitSlots(sourcePlan);
    if (!sameInventory) {
      target.CommitSlots(targetPlan);
    }

    return InventoryTransferResult.Success(quantity);
  }

  private InventoryTransferFailure GetContainedItem(
      ArchEntity ownerEntity,
      InventorySlot slot,
      out ArchEntity itemEntity,
      out ItemStackComponent item) {
    itemEntity = ArchEntity.Null;
    item = null!;
    if (!slot.ContainsEntity) {
      return InventoryTransferFailure.SlotDoesNotContainEntity;
    }

    if (!TryGetIdentity(ownerEntity, out EntityUuid ownerId) ||
        !TryResolve(slot.ItemEntityId, out itemEntity) ||
        !_world.Has<ContainedInInventoryComponent>(itemEntity)) {
      return InventoryTransferFailure.ContainmentMismatch;
    }

    ContainedInInventoryComponent relation = _world.Get<ContainedInInventoryComponent>(itemEntity);
    if (relation.InventoryId != ownerId || relation.SlotIndex != slot.Definition.Index) {
      return InventoryTransferFailure.ContainmentMismatch;
    }

    InventoryTransferFailure failure = GetItem(itemEntity, out item);
    if (failure != InventoryTransferFailure.None) {
      return failure;
    }

    return item.Stack.IsEmpty
        ? InventoryTransferFailure.InvalidItemStack
        : InventoryTransferFailure.None;
  }

  private InventoryTransferFailure ValidateContainment(ArchEntity targetEntity, EntityUuid itemId) {
    HashSet<EntityUuid> visited = new();
    ArchEntity current = targetEntity;
    while (true) {
      if (!TryGetIdentity(current, out EntityUuid currentId)) {
        return InventoryTransferFailure.MissingEntityIdentity;
      }

      if (currentId == itemId || !visited.Add(currentId)) {
        return InventoryTransferFailure.ContainmentCycle;
      }

      if (!_world.Has<ContainedInInventoryComponent>(current)) {
        return InventoryTransferFailure.None;
      }

      ContainedInInventoryComponent relation = _world.Get<ContainedInInventoryComponent>(current);
      if (!TryResolve(relation.InventoryId, out ArchEntity parent) ||
          GetInventory(parent, out InventoryComponent inventory) != InventoryTransferFailure.None ||
          !inventory.TryGetSlot(relation.SlotIndex, out InventorySlot slot) ||
          slot.ItemEntityId != currentId) {
        return InventoryTransferFailure.ContainmentMismatch;
      }

      current = parent;
    }
  }

  private bool TryGetIdentity(ArchEntity entity, out EntityUuid id) {
    id = default;
    if (!IsAlive(entity) || !_world.Has<EntityIdentityComponent>(entity)) {
      return false;
    }

    id = _world.Get<EntityIdentityComponent>(entity).Id;
    return id.IsValid;
  }

  private bool TryResolve(EntityUuid id, out ArchEntity entity) {
    entity = ArchEntity.Null;
    if (!id.IsValid) {
      return false;
    }

    ArchEntity match = ArchEntity.Null;
    int matches = 0;
    var query = new QueryDescription().WithAll<EntityIdentityComponent>();
    _world.Query(in query, (ArchEntity candidate, ref EntityIdentityComponent identity) => {
      if (identity.Id == id) {
        match = candidate;
        matches++;
      }
    });
    entity = match;
    return matches == 1 && IsAlive(match);
  }

  private static int FindEntitySlot(
      InventorySlot[] slots,
      ItemDefinition definition,
      int quantity,
      int? targetSlotIndex,
      int? excludedSlotIndex = null) {
    for (int position = 0; position < slots.Length; position++) {
      InventorySlot slot = slots[position];
      if (slot.IsEmpty && slot.Definition.Accepts(definition) &&
          quantity <= slot.Definition.MaxStackSize &&
          (!excludedSlotIndex.HasValue || slot.Definition.Index != excludedSlotIndex.Value) &&
          (!targetSlotIndex.HasValue || slot.Definition.Index == targetSlotIndex.Value)) {
        return position;
      }
    }

    return -1;
  }
}
