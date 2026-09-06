using System.Collections.Generic;
using Terraria.WorldInteraction.Tiles;

namespace Terraria.Physics;

public sealed class DoorTraversalComponent
{
  public List<TileCoordinate> TrackedDoorAnchors = new();
  public int LastTraversalDirection = 1;
  public long? LastTraversalAtTick;
  public int VelocityOnlyOpenTicksRemaining;
  public bool HasVelocityOnlyOpeningWindow => VelocityOnlyOpenTicksRemaining > 0;
  public bool IsTrackingDoor => TrackedDoorAnchors.Count != 0;
}
