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
  int Health,
  bool IsGhost = false,
  float TargetPriority = float.NaN);

public sealed class NpcTargetSelectionSystem
{
  private const float NoAggroPenalty = 1000.0f;

  public bool TryCalculateTargetPriority(
    SimulationVector sourcePosition,
    SimulationVector targetPosition,
    int targetAggro,
    bool targetHasNoAggro,
    bool npcHasDirection,
    out float priority)
  {
    priority = default;
    if (!float.IsFinite(sourcePosition.X) || !float.IsFinite(sourcePosition.Y) ||
        !float.IsFinite(targetPosition.X) || !float.IsFinite(targetPosition.Y))
    {
      return false;
    }

    float horizontalDistance = MathF.Abs(targetPosition.X - sourcePosition.X);
    float verticalDistance = MathF.Abs(targetPosition.Y - sourcePosition.Y);
    if (!float.IsFinite(horizontalDistance) || !float.IsFinite(verticalDistance))
    {
      return false;
    }

    priority = horizontalDistance + verticalDistance - targetAggro;
    if (targetHasNoAggro && npcHasDirection)
    {
      priority += NoAggroPenalty;
    }

    return float.IsFinite(priority);
  }

  public NpcTargetComponent SelectTarget(
    SimulationVector npcPosition,
    IReadOnlyList<NpcTargetCandidate> candidates)
  {
    ArgumentNullException.ThrowIfNull(candidates);
    if (!float.IsFinite(npcPosition.X) || !float.IsFinite(npcPosition.Y))
    {
      return new(default, 0, NpcTargetLockReason.NoValidTarget);
    }

    NpcTargetCandidate selected = default;
    float closestDistanceSquared = float.MaxValue;
    bool hasTarget = false;
    for (int index = 0; index < candidates.Count; index++)
    {
      NpcTargetCandidate candidate = candidates[index];
      if (candidate.Entity == default || candidate.StablePlayerId <= 0 ||
          !candidate.IsActive || candidate.Health <= 0 || candidate.IsGhost ||
          !float.IsFinite(candidate.Position.X) || !float.IsFinite(candidate.Position.Y))
      {
        continue;
      }

      float distanceSquared = DistanceSquared(npcPosition, candidate.Position);
      if (!float.IsFinite(distanceSquared))
      {
        continue;
      }

      float priority = float.IsFinite(candidate.TargetPriority)
        ? candidate.TargetPriority
        : distanceSquared;
      if (!float.IsFinite(priority))
      {
        continue;
      }

      if (!hasTarget || priority < closestDistanceSquared ||
          priority == closestDistanceSquared &&
          candidate.StablePlayerId < selected.StablePlayerId)
      {
        selected = candidate;
        closestDistanceSquared = priority;
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
