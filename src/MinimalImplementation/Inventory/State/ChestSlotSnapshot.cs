namespace MinimalImplementation.Server.Inventory.State;

public sealed record ChestSlotSnapshot(
  int SlotId,
  ChestSlotGeneration Generation,
  InventoryItemState Item);
