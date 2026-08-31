using System;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyDualDungeonPassPolicy
{
  private static readonly string[] DisabledPassIdValues =
  [
    "IceBiome",
    "DesertBiome",
    "Jungle",
    "JungleShrines",
    "ChestsInJungleShrines",
    "Beehives",
    "BeeLarvaInBeehives",
    "LihzahrdTemple",
    "LihzahrdTemplePart2",
    "LihzahrdAltar",
    "CorruptionAndCrimson",
    "Shimmer"
  ];

  private static readonly IReadOnlySet<string> DisabledPassIdSet = new HashSet<string>(
    DisabledPassIdValues,
    StringComparer.Ordinal);

  public static IReadOnlyList<string> DisabledPassIds => DisabledPassIdValues;

  public static bool ShouldDisable(string passId, bool dualDungeonsEnabled)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(passId);
    return dualDungeonsEnabled && DisabledPassIdSet.Contains(passId);
  }
}
