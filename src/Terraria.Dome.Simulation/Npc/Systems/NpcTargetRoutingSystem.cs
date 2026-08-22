using System;
using System.Collections.Generic;
using Arch.Core;

namespace Terraria.Dome.Simulation.Npc.Systems;

public readonly record struct NpcTargetRouteCandidate(
  Entity Entity,
  int TargetIndex,
  bool IsActive);

public sealed class NpcTargetRoutingSystem
{
  private const int NpcTargetOffset = 300;

  public bool TryResolve(
    int encodedTarget,
    bool supportsNpcTargets,
    int maximumNpcCount,
    IReadOnlyList<NpcTargetRouteCandidate> candidates,
    out Entity target)
  {
    ArgumentNullException.ThrowIfNull(candidates);
    target = default;
    if (!supportsNpcTargets || maximumNpcCount <= 0 ||
        encodedTarget < NpcTargetOffset || encodedTarget >= NpcTargetOffset + maximumNpcCount)
    {
      return false;
    }

    int translatedTargetIndex = encodedTarget - NpcTargetOffset;
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
