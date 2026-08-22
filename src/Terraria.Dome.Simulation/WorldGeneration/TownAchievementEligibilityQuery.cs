using System;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class TownAchievementEligibilityQuery
{
  private static readonly int[] RealEstateNpcTypes =
  [
    38, 17, 107, 19, 22, 124, 228, 178, 18, 229, 209, 54, 108, 160, 20, 369, 207, 227,
    208, 441, 353, 550, 588, 633, 663
  ];

  private static readonly int[] TownSlimeNpcTypes = [670, 678, 679, 680, 681, 682, 683, 684];

  public static TownAchievementEligibilityResult Evaluate(
    IReadOnlyCollection<int> activeNpcTypes)
  {
    ArgumentNullException.ThrowIfNull(activeNpcTypes);
    int realEstateMissingCount = CountMissing(activeNpcTypes, RealEstateNpcTypes);
    int townSlimesMissingCount = CountMissing(activeNpcTypes, TownSlimeNpcTypes);
    return new TownAchievementEligibilityResult(
      realEstateMissingCount == 0,
      townSlimesMissingCount == 0,
      realEstateMissingCount,
      townSlimesMissingCount,
      true);
  }

  private static int CountMissing(
    IReadOnlyCollection<int> activeNpcTypes,
    IReadOnlyList<int> requiredTypes)
  {
    HashSet<int> active = new(activeNpcTypes);
    int missingCount = 0;
    foreach (int requiredType in requiredTypes)
    {
      if (!active.Contains(requiredType))
      {
        missingCount++;
      }
    }

    return missingCount;
  }
}
