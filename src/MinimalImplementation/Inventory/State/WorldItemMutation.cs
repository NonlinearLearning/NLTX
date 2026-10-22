using System.Numerics;

namespace MinimalImplementation.Server.Inventory.State;

public sealed record WorldItemMutation(
  InventoryItemState Item,
  Vector2 Position,
  Vector2 Velocity,
  int TimeLeft,
  bool IsShimmering,
  int EnemyPickupDelay,
  float ShimmerTime = 0,
  WorldItemRuntime? Runtime = null)
{
  public static WorldItemMutation FromSnapshot(WorldItemSnapshot item) => new(
    item.Item, item.Position, item.Velocity, item.TimeLeft, item.IsShimmering,
    item.EnemyPickupDelay, item.ShimmerTime, item.Runtime);
}
