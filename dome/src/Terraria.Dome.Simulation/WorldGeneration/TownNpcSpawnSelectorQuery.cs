using System;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class TownNpcSpawnSelectorQuery
{
  public static int Select(
    IReadOnlyList<int> occupantTypes,
    IReadOnlyList<TownNpcSpawnCandidate> candidates)
  {
    ArgumentNullException.ThrowIfNull(occupantTypes);
    ArgumentNullException.ThrowIfNull(candidates);
    Dictionary<int, TownNpcSpawnCandidate> byType = new(candidates.Count);
    foreach (TownNpcSpawnCandidate candidate in candidates)
    {
      _ = byType.TryAdd(candidate.Type, candidate);
    }

    foreach (int occupantType in occupantTypes)
    {
      if (byType.TryGetValue(occupantType, out TownNpcSpawnCandidate? candidate) &&
          IsEligible(candidate))
      {
        return candidate.Type;
      }
    }

    int prioritizedFallback = -1;
    foreach (TownNpcSpawnCandidate candidate in candidates)
    {
      if (!candidate.CanSpawn || !candidate.SpecialConditionsAllowed)
      {
        continue;
      }

      if (candidate.AlreadyPresent)
      {
        continue;
      }

      if (candidate.HasRoom || candidate.IsTownPet)
      {
        return candidate.Type;
      }

      if (candidate.IsPrioritized)
      {
        prioritizedFallback = candidate.Type;
      }
    }

    return prioritizedFallback;
  }

  private static bool IsEligible(TownNpcSpawnCandidate candidate)
  {
    return candidate.CanSpawn && !candidate.AlreadyPresent &&
      candidate.SpecialConditionsAllowed;
  }
}
