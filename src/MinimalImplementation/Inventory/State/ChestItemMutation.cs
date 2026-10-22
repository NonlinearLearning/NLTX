namespace MinimalImplementation.Server.Inventory.State;

public sealed record ChestItemMutation(
  int SlotId,
  ChestSlotGeneration ExpectedGeneration,
  InventoryItemState Item);
