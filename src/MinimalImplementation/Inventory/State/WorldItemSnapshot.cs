using System.Numerics;
using MinimalImplementation.Contracts;

namespace MinimalImplementation.Server.Inventory.State;

public sealed record WorldItemSnapshot(
  WorldItemHandle Handle,
  InventoryItemState Item,
  Vector2 Position,
  Vector2 Velocity,
  CommitVersion Revision,
  int TimeLeft,
  bool IsShimmering,
  int EnemyPickupDelay,
  float ShimmerTime = 0,
  WorldItemRuntime? Runtime = null);
