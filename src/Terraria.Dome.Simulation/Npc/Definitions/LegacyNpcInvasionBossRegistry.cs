using System;
using System.Collections.Frozen;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.Npc.Definitions;

public static class LegacyNpcInvasionBossRegistry
{
  public const int NpcTypeCount = 697;

  private static readonly FrozenSet<int> _invasionBossNpcTypes =
    new HashSet<int>
    {
      315,
      325,
      327,
      328,
      344,
      345,
      346
    }.ToFrozenSet();

  public static int InvasionBossNpcTypeCount => _invasionBossNpcTypes.Count;

  public static IReadOnlySet<int> InvasionBossNpcTypes => _invasionBossNpcTypes;

  public static bool IsInvasionBoss(int npcType)
  {
    ValidateNpcType(npcType);
    return _invasionBossNpcTypes.Contains(npcType);
  }

  private static void ValidateNpcType(int npcType)
  {
    if (npcType < 0 || npcType >= NpcTypeCount)
    {
      throw new ArgumentOutOfRangeException(nameof(npcType));
    }
  }
}
