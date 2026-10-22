using MinimalImplementation.Contracts;

namespace MinimalImplementation.Server.Inventory.State;

/// <summary>
/// A null mutation retires the exact generation. ShimmerEffect captures the state at
/// GetShimmered, before subsequent movement, for its ordered 146/145 projection.
/// </summary>
public sealed record WorldItemChange(
  WorldItemHandle Handle,
  CommitVersion ExpectedRevision,
  WorldItemMutation? Mutation,
  WorldItemMutation? ShimmerEffect = null,
  bool Broadcast = true);
