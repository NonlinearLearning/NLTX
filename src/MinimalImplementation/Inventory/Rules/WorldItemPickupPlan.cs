using MinimalImplementation.Server.Inventory.State;

namespace MinimalImplementation.Server.Inventory.Rules;

public sealed record WorldItemPickupPlan(
  InventoryItemState Remainder,
  IReadOnlyList<InventoryMutation> Mutations);
