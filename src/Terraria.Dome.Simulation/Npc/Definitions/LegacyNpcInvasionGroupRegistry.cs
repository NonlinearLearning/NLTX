using System;
using System.Collections.Frozen;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.Npc.Definitions;

public static class LegacyNpcInvasionGroupRegistry
{
  public const int NpcTypeCount = 697;

  private static readonly FrozenDictionary<int, int> _invasionGroups =
    new Dictionary<int, int>
    {
      [26] = 1, [27] = 1, [28] = 1, [29] = 1, [111] = 1, [471] = 1, [472] = 1,
      [143] = 2, [144] = 2, [145] = 2,
      [212] = 3, [213] = 3, [214] = 3, [215] = 3, [216] = 3, [252] = 3,
      [491] = 3, [492] = 3, [662] = 3,
      [381] = 4, [382] = 4, [383] = 4, [385] = 4, [386] = 4, [387] = 4,
      [388] = 4, [389] = 4, [390] = 4, [391] = 4, [394] = 4, [395] = 4, [520] = 4
    }.ToFrozenDictionary();

  public static int InvasionGroupTypeCount => _invasionGroups.Count;

  public static IReadOnlyDictionary<int, int> InvasionGroups => _invasionGroups;

  public static int GetInvasionGroup(int npcType)
  {
    ValidateNpcType(npcType);
    return _invasionGroups.GetValueOrDefault(npcType);
  }

  private static void ValidateNpcType(int npcType)
  {
    if (npcType < 0 || npcType >= NpcTypeCount)
    {
      throw new ArgumentOutOfRangeException(nameof(npcType));
    }
  }
}
