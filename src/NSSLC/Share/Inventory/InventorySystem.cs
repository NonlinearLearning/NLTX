using System;
using ArchEntity = Arch.Core.Entity;
using ArchWorld = Arch.Core.World;
using Terraria.Share.Entities;
using Terraria.Share.Items;

namespace Terraria.Share.Inventory;

/// <summary>
/// Coordinates inventory mutations during the host's exclusive, synchronous commit phase.
/// Expected rejections happen before writes. Runtime exceptions propagate to the host.
/// </summary>
public sealed partial class InventorySystem {
  private readonly ArchWorld _world;
  private readonly ItemRegistry _definitions;

  public InventorySystem(ArchWorld world, ItemRegistry definitions) {
    ArgumentNullException.ThrowIfNull(world);
    ArgumentNullException.ThrowIfNull(definitions);
    _world = world;
    _definitions = definitions;
  }

  /// <summary>
  /// Inserts a value stack. The caller retains ownership of any quantity not reported committed.
  /// This method does not mutate the input value or any external source inventory.
  /// </summary>
  public InventoryTransferResult TryInsert(
      ArchEntity inventoryEntity,
      in ItemStack stack,
      int? targetSlotIndex = null) {
    InventoryTransferFailure failure = GetInventory(inventoryEntity, out InventoryComponent inventory);
    if (failure != InventoryTransferFailure.None) {
      return InventoryTransferResult.Rejected(failure);
    }

    failure = GetDefinition(stack, out Item definition);
    if (failure != InventoryTransferFailure.None) {
      return InventoryTransferResult.Rejected(failure);
    }

    InventorySlot[] plan = inventory.CaptureSlots();
    failure = ValidateTargetSlot(plan, targetSlotIndex);
    if (failure != InventoryTransferFailure.None) {
      return InventoryTransferResult.Rejected(failure);
    }

    int inserted = PlanInsert(plan, stack, definition, stack.Quantity, targetSlotIndex);
    if (inserted == 0) {
      return InventoryTransferResult.Rejected(InventoryTransferFailure.NoCompatibleSlot);
    }

    inventory.CommitSlots(plan);
    return InventoryTransferResult.Success(inserted);
  }

  /// <summary>
  /// Overwrites one slot with the supplied value stack, or clears the slot when the stack is
  /// empty. This is the value-oriented entry point for callers that own slot contents without a
  /// backing item entity, such as a protocol adapter projecting a client's inventory.
  /// </summary>
  /// <remarks>
  /// 与 <see cref="TryInsert(ArchEntity, in ItemStack, int?)"/> 的区别在于语义：插入寻找可用槽位并
  /// 允许部分成交，本方法只作用于调用方指定的槽位，且不做跨槽合并。
  /// </remarks>
  public InventoryTransferResult TrySetSlotValue(
      ArchEntity inventoryEntity,
      int slotIndex,
      in ItemStack stack) {
    InventoryTransferFailure failure = GetInventory(inventoryEntity, out InventoryComponent inventory);
    if (failure != InventoryTransferFailure.None) {
      return InventoryTransferResult.Rejected(failure);
    }

    InventorySlot[] plan = inventory.CaptureSlots();
    int position = FindPosition(plan, slotIndex);
    if (position < 0) {
      return InventoryTransferResult.Rejected(InventoryTransferFailure.InvalidSlot);
    }

    InventorySlotDefinition definition = plan[position].Definition;
    if (stack.IsEmpty) {
      plan[position] = new InventorySlot(definition, null);
      inventory.CommitSlots(plan);
      return InventoryTransferResult.Success(0);
    }

    failure = GetDefinition(stack, out Item item);
    if (failure != InventoryTransferFailure.None) {
      return InventoryTransferResult.Rejected(failure);
    }

    if (!definition.Accepts(item)) {
      return InventoryTransferResult.Rejected(InventoryTransferFailure.NoCompatibleSlot);
    }

    if (stack.Quantity > Math.Min(definition.MaxStackSize, item.MaxStack)) {
      return InventoryTransferResult.Rejected(InventoryTransferFailure.InvalidQuantity);
    }

    plan[position] = new InventorySlot(definition, stack);
    inventory.CommitSlots(plan);
    return InventoryTransferResult.Success(stack.Quantity);
  }

  /// <summary>Removes a value stack from one slot.</summary>
  public InventoryTransferResult TryRemove(
      ArchEntity inventoryEntity,
      int slotIndex,
      int quantity,
      out ItemStack removed) {
    removed = default;
    InventoryTransferFailure failure = GetInventory(inventoryEntity, out InventoryComponent inventory);
    if (failure != InventoryTransferFailure.None) {
      return InventoryTransferResult.Rejected(failure);
    }

    if (!inventory.TryGetSlot(slotIndex, out InventorySlot slot)) {
      return InventoryTransferResult.Rejected(InventoryTransferFailure.InvalidSlot);
    }

    if (slot.Content is not ItemStack stack || quantity <= 0 || quantity > stack.Quantity) {
      return InventoryTransferResult.Rejected(InventoryTransferFailure.InvalidQuantity);
    }

    InventorySlot[] plan = inventory.CaptureSlots();
    PlanRemove(plan, slotIndex, stack, quantity);
    ItemStack taken = stack.WithQuantity(quantity);
    inventory.CommitSlots(plan);
    removed = taken;
    return InventoryTransferResult.Success(quantity);
  }

  /// <summary>
  /// Moves the full requested quantity, or rejects without mutation. An explicit target slot
  /// permits movement within the same inventory. Retained entities cannot be split or merged.
  /// </summary>
  public InventoryTransferResult TryMove(
      ArchEntity sourceEntity,
      int sourceSlotIndex,
      ArchEntity targetEntity,
      int quantity,
      int? targetSlotIndex = null) {
    InventoryTransferFailure failure = GetInventory(sourceEntity, out InventoryComponent source);
    if (failure != InventoryTransferFailure.None) {
      return InventoryTransferResult.Rejected(failure);
    }

    failure = GetInventory(targetEntity, out InventoryComponent target);
    if (failure != InventoryTransferFailure.None) {
      return InventoryTransferResult.Rejected(failure);
    }

    bool sameInventory = sourceEntity == targetEntity;
    if (sameInventory && (!targetSlotIndex.HasValue || targetSlotIndex.Value == sourceSlotIndex)) {
      return InventoryTransferResult.Rejected(InventoryTransferFailure.SameInventory);
    }

    if (!sameInventory && ReferenceEquals(source, target)) {
      throw new InvalidOperationException("Entities must not share a mutable inventory component.");
    }

    if (!source.TryGetSlot(sourceSlotIndex, out InventorySlot sourceSlot)) {
      return InventoryTransferResult.Rejected(InventoryTransferFailure.InvalidSlot);
    }

    if (sourceSlot.Content is not ItemStack stack || quantity <= 0 || quantity > stack.Quantity) {
      return InventoryTransferResult.Rejected(InventoryTransferFailure.InvalidQuantity);
    }

    failure = GetDefinition(stack, out Item definition);
    if (failure != InventoryTransferFailure.None) {
      return InventoryTransferResult.Rejected(failure);
    }

    InventorySlot[] sourcePlan = source.CaptureSlots();
    InventorySlot[] targetPlan = sameInventory ? sourcePlan : target.CaptureSlots();
    failure = ValidateTargetSlot(targetPlan, targetSlotIndex);
    if (failure != InventoryTransferFailure.None) {
      return InventoryTransferResult.Rejected(failure);
    }

    PlanRemove(sourcePlan, sourceSlotIndex, stack, quantity);
    if (PlanInsert(
            targetPlan,
            stack,
            definition,
            quantity,
            targetSlotIndex,
            sameInventory ? sourceSlotIndex : null) != quantity) {
      return InventoryTransferResult.Rejected(InventoryTransferFailure.NoCompatibleSlot);
    }

    source.CommitSlots(sourcePlan);
    if (!sameInventory) {
      target.CommitSlots(targetPlan);
    }

    return InventoryTransferResult.Success(quantity);
  }

  /// <summary>Removes the capability only after all stored values and entities are taken out.</summary>
  public InventoryTransferResult TryRemoveInventoryComponent(
      ArchEntity entity,
      EcsServerCommandBuffer? commands = null) {
    InventoryTransferFailure failure = GetInventory(entity, out InventoryComponent inventory);
    if (failure != InventoryTransferFailure.None) {
      return InventoryTransferResult.Rejected(failure);
    }

    if (!inventory.IsEmpty) {
      return InventoryTransferResult.Rejected(InventoryTransferFailure.InventoryNotEmpty);
    }

    commands?.Remove<InventoryComponent>(entity);
    if (commands is null) {
      _world.Remove<InventoryComponent>(entity);
    }
    return InventoryTransferResult.Success(0);
  }

  private InventoryTransferFailure GetInventory(
      ArchEntity entity,
      out InventoryComponent inventory) {
    inventory = null!;
    if (!IsAlive(entity)) {
      return InventoryTransferFailure.InvalidEntity;
    }

    if (!_world.Has<InventoryComponent>(entity)) {
      return InventoryTransferFailure.MissingInventory;
    }

    inventory = _world.Get<InventoryComponent>(entity);
    return inventory is null
        ? InventoryTransferFailure.MissingInventory
        : InventoryTransferFailure.None;
  }

  private InventoryTransferFailure GetDefinition(
      in ItemStack stack,
      out Item definition) {
    definition = null!;
    if (stack.IsEmpty) {
      return InventoryTransferFailure.InvalidItemStack;
    }

    if (!_definitions.TryGet(stack.ItemId, out definition)) {
      return InventoryTransferFailure.MissingItemDefinition;
    }

    return definition.AcceptsQuantity(stack.Quantity)
        ? InventoryTransferFailure.None
        : InventoryTransferFailure.InvalidItemStack;
  }

  private bool IsAlive(ArchEntity entity) {
    return entity != ArchEntity.Null && entity.WorldId == _world.Id && _world.IsAlive(entity);
  }

  private static InventoryTransferFailure ValidateTargetSlot(
      InventorySlot[] slots,
      int? index) {
    return index.HasValue && FindPosition(slots, index.Value) < 0
        ? InventoryTransferFailure.InvalidSlot
        : InventoryTransferFailure.None;
  }

  private static int FindPosition(InventorySlot[] slots, int index) {
    for (int position = 0; position < slots.Length; position++) {
      if (slots[position].Definition.Index == index) {
        return position;
      }
    }

    return -1;
  }

  private static void PlanRemove(
      InventorySlot[] plan,
      int slotIndex,
      ItemStack stack,
      int quantity) {
    int position = FindPosition(plan, slotIndex);
    ItemStack? remaining = quantity == stack.Quantity
        ? null
        : stack.WithQuantity(stack.Quantity - quantity);
    plan[position] = new InventorySlot(plan[position].Definition, remaining);
  }

  private static int PlanInsert(
      InventorySlot[] plan,
      ItemStack stack,
      Item definition,
      int quantity,
      int? targetSlotIndex,
      int? excludedSlotIndex = null) {
    int remaining = quantity;
    // Fill compatible existing stacks before consuming empty slots.
    for (int pass = 0; pass < 2 && remaining > 0; pass++) {
      for (int position = 0; position < plan.Length && remaining > 0; position++) {
        InventorySlot slot = plan[position];
        if ((targetSlotIndex.HasValue && slot.Definition.Index != targetSlotIndex.Value) ||
            (excludedSlotIndex.HasValue && slot.Definition.Index == excludedSlotIndex.Value) ||
            !slot.Definition.Accepts(definition) ||
            (pass == 0 && slot.IsEmpty) || (pass == 1 && !slot.IsEmpty)) {
          continue;
        }

        if (slot.Content is ItemStack existing && !existing.CanStackWith(stack)) {
          continue;
        }

        int currentQuantity = slot.Content?.Quantity ?? 0;
        int capacity = Math.Min(slot.Definition.MaxStackSize, definition.MaxStack) - currentQuantity;
        if (capacity <= 0) {
          continue;
        }

        int added = Math.Min(capacity, remaining);
        ItemStack next = stack.WithQuantity(currentQuantity + added);
        plan[position] = new InventorySlot(slot.Definition, next);
        remaining -= added;
      }
    }

    return quantity - remaining;
  }
}
