using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.Movement.Components;

public sealed class DoorTraversalComponent
{
  private readonly List<WorldTileCoordinate> _trackedDoorAnchors = new();

  public IReadOnlyList<WorldTileCoordinate> TrackedDoorAnchors =>
    _trackedDoorAnchors.AsReadOnly();
  public int VelocityOnlyOpenTicksRemaining { get; private set; }
  public int LastTraversalDirection { get; private set; } = 1;
  public long? LastTraversalAtTick { get; private set; }
  public bool HasVelocityOnlyOpeningWindow => VelocityOnlyOpenTicksRemaining > 0;
  public bool IsTrackingDoor => _trackedDoorAnchors.Count != 0;

  public void TrackDoor(WorldTileCoordinate anchor)
  {
    if (!_trackedDoorAnchors.Contains(anchor))
    {
      _trackedDoorAnchors.Add(anchor);
    }
  }

  public void UntrackDoor(WorldTileCoordinate anchor)
  {
    _ = _trackedDoorAnchors.Remove(anchor);
  }

  public void ClearTrackedDoors()
  {
    _trackedDoorAnchors.Clear();
  }

  public void OpenVelocityOnlyWindow(int ticks)
  {
    if (ticks < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(ticks));
    }

    VelocityOnlyOpenTicksRemaining = ticks;
  }

  public void AdvanceTick()
  {
    if (VelocityOnlyOpenTicksRemaining > 0)
    {
      VelocityOnlyOpenTicksRemaining--;
    }
  }

  public void RecordTraversal(int direction, long tick)
  {
    if (tick < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(tick));
    }

    LastTraversalDirection = direction < 0 ? -1 : 1;
    LastTraversalAtTick = tick;
  }
}
