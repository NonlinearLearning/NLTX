using MinimalImplementation.Contracts;

namespace MinimalImplementation.Server.Inventory.State;

public sealed record WorldItemInventoryChange(
  EntityUuid PlayerId,
  SessionEpoch Epoch,
  CommitVersion ExpectedRevision,
  IReadOnlyCollection<InventoryMutation> Mutations,
  bool EchoToOwner = true);
