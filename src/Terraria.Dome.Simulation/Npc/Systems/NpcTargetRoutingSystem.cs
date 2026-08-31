using System;
using System.Collections.Generic;
using Arch.Core;
using Terraria.Dome.Simulation.Npc.Definitions;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.Npc.Systems;

public readonly record struct NpcTargetRouteCandidate(
  Entity Entity,
  int TargetIndex,
  bool IsActive);

public readonly record struct PlayerTargetRouteCandidate(
  Entity Entity,
  int TargetIndex,
  bool IsActive,
  bool IsDead,
  bool IsGhost);

public sealed class NpcTargetRoutingSystem
{
  private const int NpcTargetOffset = 300;

  public bool HasPlayerTarget(int encodedTarget, int maximumPlayerCount)
  {
    return maximumPlayerCount > 0 && encodedTarget >= 0 &&
      (long)encodedTarget < maximumPlayerCount;
  }

  public bool IsWithinTargetRange(
    SimulationVector sourcePosition,
    SimulationVector targetPosition,
    float maximumDistance)
  {
    if (!float.IsFinite(sourcePosition.X) || !float.IsFinite(sourcePosition.Y) ||
        !float.IsFinite(targetPosition.X) || !float.IsFinite(targetPosition.Y) ||
        !float.IsFinite(maximumDistance) || maximumDistance < 0.0f)
    {
      return false;
    }

    float deltaX = targetPosition.X - sourcePosition.X;
    float deltaY = targetPosition.Y - sourcePosition.Y;
    return deltaX * deltaX + deltaY * deltaY < maximumDistance * maximumDistance;
  }

  public bool HasValidTarget(
    int encodedTarget,
    bool supportsNpcTargets,
    int maximumPlayerCount,
    int maximumNpcCount,
    IReadOnlyList<PlayerTargetRouteCandidate> playerCandidates,
    IReadOnlyList<NpcTargetRouteCandidate> npcCandidates)
  {
    ArgumentNullException.ThrowIfNull(playerCandidates);
    ArgumentNullException.ThrowIfNull(npcCandidates);
    if (HasPlayerTarget(encodedTarget, maximumPlayerCount))
    {
      for (int index = 0; index < playerCandidates.Count; index++)
      {
        PlayerTargetRouteCandidate candidate = playerCandidates[index];
        if (candidate.TargetIndex == encodedTarget && candidate.Entity != default &&
            candidate.IsActive && !candidate.IsDead && !candidate.IsGhost)
        {
          return true;
        }
      }
    }

    return supportsNpcTargets && TryResolve(
      encodedTarget,
      supportsNpcTargets,
      maximumNpcCount,
      npcCandidates,
      out _);
  }

  public bool HasNpcTarget(int encodedTarget, int maximumNpcCount)
  {
    return maximumNpcCount > 0 && encodedTarget >= NpcTargetOffset &&
      (long)encodedTarget - NpcTargetOffset < maximumNpcCount;
  }

  public bool TryTranslateNpcTargetIndex(
    int encodedTarget,
    int maximumNpcCount,
    out int translatedTargetIndex)
  {
    translatedTargetIndex = 0;
    if (!HasNpcTarget(encodedTarget, maximumNpcCount))
    {
      return false;
    }

    translatedTargetIndex = encodedTarget - NpcTargetOffset;
    return true;
  }

  public bool TryTranslateTargetIndex(
    int encodedTarget,
    int maximumPlayerCount,
    int maximumNpcCount,
    out int translatedTargetIndex,
    out bool isNpcTarget)
  {
    translatedTargetIndex = 0;
    isNpcTarget = false;
    if (TryTranslateNpcTargetIndex(
          encodedTarget,
          maximumNpcCount,
          out translatedTargetIndex))
    {
      isNpcTarget = true;
      return true;
    }

    if (HasPlayerTarget(encodedTarget, maximumPlayerCount))
    {
      translatedTargetIndex = encodedTarget;
      return true;
    }

    return false;
  }

  public bool TryResolve(
    int encodedTarget,
    NpcDefinition definition,
    int maximumNpcCount,
    IReadOnlyList<NpcTargetRouteCandidate> candidates,
    out Entity target)
  {
    return TryResolve(
      encodedTarget,
      definition.SupportsNpcTargets,
      maximumNpcCount,
      candidates,
      out target);
  }

  public bool TryResolve(
    int encodedTarget,
    bool supportsNpcTargets,
    int maximumNpcCount,
    IReadOnlyList<NpcTargetRouteCandidate> candidates,
    out Entity target)
  {
    ArgumentNullException.ThrowIfNull(candidates);
    target = default;
    if (!supportsNpcTargets ||
        !TryTranslateNpcTargetIndex(encodedTarget, maximumNpcCount, out int translatedTargetIndex))
    {
      return false;
    }

    for (int index = 0; index < candidates.Count; index++)
    {
      NpcTargetRouteCandidate candidate = candidates[index];
      if (candidate.TargetIndex == translatedTargetIndex && candidate.Entity != default &&
          candidate.IsActive)
      {
        target = candidate.Entity;
        return true;
      }
    }

    return false;
  }
}
