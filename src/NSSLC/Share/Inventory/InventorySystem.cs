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
/// <remarks>
/// <para>
/// 合并为单一 Item 域后，容器槽只存**指向物品记录的地址**，数量与身份都在记录上。
/// 因此本系统不再持有任何载荷副本：每次读写都经过 <see cref="ItemStore"/>，
/// 拾取因此变成"同一提交内的改址"，而不是两个存储之间的对账。
/// </para>
/// <para>
/// 容器段用 <see cref="ContainerId"/> 作为宿主号。玩家库存的宿主号是玩家的实体 id 偏移，
/// 银行/箱子等宿主按需分配——宿主号只需要在进程内唯一，不进入存档或协议。
/// </para>
/// </remarks>
public sealed partial class InventorySystem {
  private readonly ArchWorld _world;
  private readonly ItemRegistry _definitions;
  private readonly ItemStore _items;

  public InventorySystem(ArchWorld world, ItemRegistry definitions, ItemStore items) {
    ArgumentNullException.ThrowIfNull(world);
    ArgumentNullException.ThrowIfNull(definitions);
    ArgumentNullException.ThrowIfNull(items);
    _world = world;
    _definitions = definitions;
    _items = items;
  }

  /// <summary>Container host number of one entity's inventory.</summary>
  /// <remarks>
  /// 用实体 id 加一作为宿主号：0 保留给"没有宿主"，实体 id 在进程内唯一，因此槽地址不会撞车。
  /// 这个编号只在运行时有效，不写进存档，也不出现在协议里。
  /// </remarks>
  public static int ContainerId(ArchEntity entity) {
    return entity.Id + 1;
  }

  /// <summary>Reads the stack a slot refers to, or <c>null</c> when the slot is empty.</summary>
  public ItemStack? ReadSlot(ArchEntity inventoryEntity, InventorySlot slot) {
    if (slot.Content is not ItemContainerAddress address) {
      return null;
    }

    return _items.TryGetContainedStack(address, out ItemStack stack) ? stack : null;
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

    int container = ContainerId(inventoryEntity);
    int inserted = PlanInsert(plan, container, stack, definition, stack.Quantity, targetSlotIndex);
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
    ItemContainerAddress address = new(ContainerId(inventoryEntity), slotIndex);
    if (stack.IsEmpty) {
      _items.TryClearContainer(address, out _);
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

    // 先清掉旧记录再写新值：同一槽位只允许一条记录，地址唯一是硬约束。
    _items.TryClearContainer(address, out _);
    if (!_items.TryStoreIntoContainer(address, stack, out _)) {
      return InventoryTransferResult.Rejected(InventoryTransferFailure.CommitFailed);
    }

    plan[position] = new InventorySlot(definition, address);
    inventory.CommitSlots(plan);
    return InventoryTransferResult.Success(stack.Quantity);
  }

  /// <summary>
  /// Sets the slot this inventory holds, or clears the relation when <paramref name="held"/> is
  /// <c>null</c>.
  /// </summary>
  /// <remarks>
  /// 与 <see cref="TrySetSlotValue"/> 一样是槽位语义：只接受本库存确实存在的槽号，不做跨槽合并，
  /// 也不搬运载荷。手持关系因此始终指向"某一格现有内容"，不会产生第二份物品副本。
  /// </remarks>
  public InventoryTransferResult TrySetHeldSlot(ArchEntity inventoryEntity, HeldSlot? held) {
    InventoryTransferFailure failure = GetInventory(inventoryEntity, out InventoryComponent inventory);
    if (failure != InventoryTransferFailure.None) {
      return InventoryTransferResult.Rejected(failure);
    }

    if (held is HeldSlot selected && !inventory.TryGetSlot(selected.Index, out _)) {
      return InventoryTransferResult.Rejected(InventoryTransferFailure.InvalidSlot);
    }

    inventory.CommitHeld(held);
    return InventoryTransferResult.Success(0);
  }

  /// <summary>
  /// Reads the held stack of one inventory. This is the server-side form of Version4
  /// <c>Player.HeldItem</c>; it returns the slot's own value and never a copy of the payload.
  /// </summary>
  public bool TryGetHeldStack(ArchEntity inventoryEntity, out ItemStack stack) {
    stack = default;
    if (GetInventory(inventoryEntity, out InventoryComponent inventory) !=
        InventoryTransferFailure.None) {
      return false;
    }

    if (inventory.TryGetHeld() is not InventorySlot held) {
      return false;
    }

    if (ReadSlot(inventoryEntity, held) is not ItemStack heldStack) {
      return false;
    }

    stack = heldStack;
    return true;
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

    if (slot.Content is not ItemContainerAddress address ||
        ReadSlot(inventoryEntity, slot) is not ItemStack stack ||
        quantity <= 0 || quantity > stack.Quantity) {
      return InventoryTransferResult.Rejected(InventoryTransferFailure.InvalidQuantity);
    }

    if (!_items.TryShrinkContainerStack(address, quantity, out _, out _)) {
      return InventoryTransferResult.Rejected(InventoryTransferFailure.CommitFailed);
    }

    InventorySlot[] plan = inventory.CaptureSlots();
    int position = FindPosition(plan, slotIndex);
    if (quantity == stack.Quantity) {
      plan[position] = new InventorySlot(plan[position].Definition, null);
      inventory.CommitSlots(plan);
    }

    removed = stack.WithQuantity(quantity);
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

    if (sourceSlot.Content is not ItemContainerAddress sourceAddress ||
        ReadSlot(sourceEntity, sourceSlot) is not ItemStack stack ||
        quantity <= 0 || quantity > stack.Quantity) {
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

    int targetContainer = ContainerId(targetEntity);
    int moved = PlanInsert(
        targetPlan,
        targetContainer,
        stack,
        definition,
        quantity,
        targetSlotIndex,
        sameInventory ? sourceSlotIndex : null);
    if (moved != quantity) {
      return InventoryTransferResult.Rejected(InventoryTransferFailure.NoCompatibleSlot);
    }

    _items.TryShrinkContainerStack(sourceAddress, quantity, out _, out _);
    if (quantity == stack.Quantity) {
      int sourcePosition = FindPosition(sourcePlan, sourceSlotIndex);
      sourcePlan[sourcePosition] = new InventorySlot(sourcePlan[sourcePosition].Definition, null);
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

  /// <summary>
  /// Plans an insertion across the slot table and writes the corresponding dormant records.
  /// </summary>
  /// <remarks>
  /// 两遍扫描与原实现一致：先填同类未满的堆，再占用空槽。区别在于这里写入的是物品记录，
  /// 槽位表只保存地址——因此"计划"与"记录"必须同时成立，任何一步失败都不提交槽位表。
  /// </remarks>
  private int PlanInsert(
      InventorySlot[] plan,
      int container,
      ItemStack stack,
      Item definition,
      int quantity,
      int? targetSlotIndex,
      int? excludedSlotIndex = null) {
    int remaining = quantity;
    for (int pass = 0; pass < 2 && remaining > 0; pass++) {
      for (int position = 0; position < plan.Length && remaining > 0; position++) {
        InventorySlot slot = plan[position];
        if ((targetSlotIndex.HasValue && slot.Definition.Index != targetSlotIndex.Value) ||
            (excludedSlotIndex.HasValue && slot.Definition.Index == excludedSlotIndex.Value) ||
            !slot.Definition.Accepts(definition) ||
            (pass == 0 && slot.IsEmpty) || (pass == 1 && !slot.IsEmpty)) {
          continue;
        }

        ItemContainerAddress address = new(container, slot.Definition.Index);
        ItemStack? existing = _items.TryGetContainedStack(address, out ItemStack contained)
            ? contained
            : null;
        if (existing is ItemStack present && !present.CanStackWith(stack)) {
          continue;
        }

        int currentQuantity = existing?.Quantity ?? 0;
        int capacity = Math.Min(slot.Definition.MaxStackSize, definition.MaxStack) - currentQuantity;
        if (capacity <= 0) {
          continue;
        }

        int added = Math.Min(capacity, remaining);
        if (existing.HasValue) {
          if (!_items.TryGrowContainerStack(address, added, out _)) {
            continue;
          }
        } else if (!_items.TryStoreIntoContainer(address, stack.WithQuantity(added), out _)) {
          continue;
        }

        plan[position] = new InventorySlot(slot.Definition, address);
        remaining -= added;
      }
    }

    return quantity - remaining;
  }
}
