using MinimalImplementation.Contracts;

namespace MinimalImplementation.Server.Inventory.State;

public sealed record WorldItemTransactionResult(
  OwnerCommitResult Result,
  InventorySnapshot? Inventory,
  IReadOnlyList<WorldItemSnapshot> WorldItems);
