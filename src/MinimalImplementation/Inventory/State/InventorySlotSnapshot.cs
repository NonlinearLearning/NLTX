namespace MinimalImplementation.Server.Inventory.State;

public sealed record InventorySlotSnapshot(
  short SlotId,
  InventorySlotGeneration Generation,
  InventoryItemState Item);
