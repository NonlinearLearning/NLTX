using System;
using Terraria.Dome.Simulation.Npc.Components;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.Npc.Systems;

public readonly record struct NpcSegmentFollowResult(SimulationVector Velocity, int Facing);

public sealed class NpcSegmentFollowSystem
{
  public NpcSegmentFollowResult Evaluate(
    NpcSegmentComponent segment,
    NpcChaseState followState,
    SimulationVector segmentPosition,
    SimulationVector parentPosition)
  {
    if (!segment.Root.IsValid || segment.IsRoot || !segment.Parent.IsValid)
    {
      throw new ArgumentException(
        "Segment follow requires a non-root segment with a valid parent.",
        nameof(segment));
    }

    if (!IsFinite(segmentPosition) || !IsFinite(parentPosition) ||
        !float.IsFinite(followState.Speed) || followState.Speed < 0.0f ||
        !float.IsFinite(followState.StoppingDistance) || followState.StoppingDistance < 0.0f)
    {
      throw new ArgumentOutOfRangeException(nameof(followState));
    }

    float deltaX = parentPosition.X - segmentPosition.X;
    float deltaY = parentPosition.Y - segmentPosition.Y;
    float distance = MathF.Sqrt(deltaX * deltaX + deltaY * deltaY);
    if (!float.IsFinite(distance) || distance <= followState.StoppingDistance ||
        followState.Speed == 0.0f)
    {
      return new NpcSegmentFollowResult(default, 0);
    }

    float inverseDistance = 1.0f / distance;
    SimulationVector velocity = new(
      deltaX * inverseDistance * followState.Speed,
      deltaY * inverseDistance * followState.Speed);
    int facing = MathF.Sign(deltaX);
    return new NpcSegmentFollowResult(velocity, facing);
  }

  private static bool IsFinite(SimulationVector value)
  {
    return float.IsFinite(value.X) && float.IsFinite(value.Y);
  }
}
