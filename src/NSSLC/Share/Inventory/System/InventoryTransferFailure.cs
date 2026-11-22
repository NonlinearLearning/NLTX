namespace Terraria.Inventory;

/// <summary>Reason an inventory operation was rejected.</summary>
public enum InventoryTransferFailure : byte {
  None,
  InvalidEntity,
  MissingInventory,
  MissingItemStack,
  MissingItemDefinition,
  InvalidItemStack,
  InvalidQuantity,
  NoCompatibleSlot,
  AlreadyContained,
  SameInventory,
  CommitFailed,
  MissingEntityIdentity,
  InvalidSlot,
  SlotRequiresEntity,
  SlotDoesNotContainEntity,
  ContainmentMismatch,
  ContainmentCycle,
  EntitySplitNotSupported,
  InventoryNotEmpty,
  PickupUnavailable
}
