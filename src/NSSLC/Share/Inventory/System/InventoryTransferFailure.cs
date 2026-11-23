namespace Terraria.Inventory;

/// <summary>Reason an inventory operation was rejected.</summary>
/// <remarks>
/// 物品实体路径在 P1-A（[执行计划](../../../../docs/plans/2026-11-23-item-payload-dual-host-execution-plan.md)）
/// 移除后，只与该路径相关的失败值一并删除：`AlreadyContained`、`MissingItemStack`、
/// `SlotRequiresEntity`、`SlotDoesNotContainEntity`、`ContainmentMismatch`、`ContainmentCycle`、
/// `EntitySplitNotSupported`、`MissingEntityIdentity`、`PickupUnavailable`。
/// 容器现在只承载值，可拒绝的原因只剩能力、定义、数量、槽位与容量。
/// </remarks>
public enum InventoryTransferFailure : byte {
  None,
  InvalidEntity,
  MissingInventory,
  MissingItemDefinition,
  InvalidItemStack,
  InvalidQuantity,
  NoCompatibleSlot,
  SameInventory,
  CommitFailed,
  InvalidSlot,
  InventoryNotEmpty
}
