namespace MinimalImplementation.Server.Inventory.State;

public sealed record InventoryMutation(
  short SlotId,
  InventorySlotGeneration ExpectedGeneration,
  InventoryItemState Item);
