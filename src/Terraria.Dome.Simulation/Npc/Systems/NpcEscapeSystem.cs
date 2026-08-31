using System;
using Terraria.Dome.Simulation.Npc.Commands;
using Terraria.Dome.Simulation.Npc.Components;
using Terraria.Dome.Simulation.Npc.Definitions;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.Npc.Systems;

public readonly record struct NpcEscapeResult(
  SimulationVector Velocity,
  int Facing,
  DespawnNpcCommand? DespawnCommand);

public sealed class NpcEscapeSystem
{
  public NpcEscapeResult Evaluate(
    NpcHandle npc,
    SimulationVector npcPosition,
    SimulationVector targetPosition,
    bool hasTarget,
    int timeLeft,
    float maximumDistance,
    float escapeSpeed,
    NpcFaction faction)
  {
    if (faction != NpcFaction.Hostile || !npc.IsValid || !IsFinite(npcPosition) ||
        !IsFinite(targetPosition) ||
        timeLeft < 0 || !float.IsFinite(maximumDistance) || maximumDistance <= 0.0f ||
        !float.IsFinite(escapeSpeed) || escapeSpeed < 0.0f)
    {
      throw new ArgumentOutOfRangeException(nameof(npc));
    }

    if (timeLeft == 0)
    {
      return new(default, 0, new DespawnNpcCommand(npc, NpcDespawnReason.TimedOut));
    }

    if (hasTarget)
    {
      float deltaX = npcPosition.X - targetPosition.X;
      float deltaY = npcPosition.Y - targetPosition.Y;
      double distanceSquared = (double)deltaX * deltaX + (double)deltaY * deltaY;
      double maximumDistanceSquared = (double)maximumDistance * maximumDistance;
      if (distanceSquared >= maximumDistanceSquared)
      {
        return new(default, 0, new DespawnNpcCommand(npc, NpcDespawnReason.OutOfRange));
      }

      double distance = Math.Sqrt(distanceSquared);
      if (!double.IsFinite(distance))
      {
        throw new ArgumentOutOfRangeException(nameof(targetPosition));
      }

      if (distance > 0.0f && escapeSpeed > 0.0f)
      {
        float scale = (float)(escapeSpeed / distance);
        SimulationVector velocity = new(deltaX * scale, deltaY * scale);
        int facing = deltaX > 0.0f ? 1 : deltaX < 0.0f ? -1 : 0;
        return new(velocity, facing, null);
      }
    }

    return new(default, 0, null);
  }

  private static bool IsFinite(SimulationVector value)
  {
    return float.IsFinite(value.X) && float.IsFinite(value.Y);
  }
}
