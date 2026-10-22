using MinimalImplementation.Contracts;

namespace MinimalImplementation.Server.Inventory.State;

public sealed record InventoryTransactionResult(
  OwnerCommitResult Commit,
  InventorySnapshot? Inventory,
  IReadOnlyList<ChestSnapshot> Chests);
