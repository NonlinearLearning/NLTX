using System;
using System.Collections.Generic;
using Arch.Core;
using Terraria.Dome.Simulation.Npc.Components;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.Npc.Systems;

public readonly record struct NpcTargetCandidate(
  Entity Entity,
  int StablePlayerId,
  SimulationVector Position,
  bool IsActive,
  int Health);

public sealed class NpcTargetSelectionSystem
{
  public NpcTargetComponent SelectTarget(
    SimulationVector npcPosition,
    IReadOnlyList<NpcTargetCandidate> candidates)
  {
    ArgumentNullException.ThrowIfNull(candidates);
    NpcTargetCandidate selected = default;
    float closestDistanceSquared = float.MaxValue;
    bool hasTarget = false;
    for (int index = 0; index < candidates.Count; index++)
    {
      NpcTargetCandidate candidate = candidates[index];
      if (!candidate.IsActive || candidate.Health <= 0)
      {
        continue;
      }

      float distanceSquared = DistanceSquared(npcPosition, candidate.Position);
      if (!hasTarget || distanceSquared < closestDistanceSquared ||
          distanceSquared == closestDistanceSquared &&
          candidate.StablePlayerId < selected.StablePlayerId)
      {
        selected = candidate;
        closestDistanceSquared = distanceSquared;
        hasTarget = true;
      }
    }

    if (!hasTarget)
    {
      return new(default, 0, NpcTargetLockReason.NoValidTarget);
    }

    return new(
      selected.Entity,
      selected.StablePlayerId,
      NpcTargetLockReason.NearestActivePlayer);
  }

  private static float DistanceSquared(SimulationVector left, SimulationVector right)
  {
    float horizontal = right.X - left.X;
    float vertical = right.Y - left.Y;
    return horizontal * horizontal + vertical * vertical;
  }
}
