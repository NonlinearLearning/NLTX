using System;
using System.Collections.Frozen;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.Npc.Definitions;

public static class LegacyNpcTownRegistry
{
  public const int NpcTypeCount = 697;

  private static readonly FrozenSet<int> _staticTownNpcTypes =
    new HashSet<int>
    {
      17,
      18,
      19,
      20,
      22,
      37,
      38,
      54,
      107,
      108,
      124,
      142,
      160,
      178,
      207,
      208,
      209,
      227,
      228,
      229,
      353,
      368,
      369,
      441,
      550,
      588,
      633,
      637,
      638,
      656,
      663,
      670,
      678,
      679,
      680,
      681,
      682,
      683,
      684
    }.ToFrozenSet();

  public static int StaticTownNpcTypeCount => _staticTownNpcTypes.Count;

  public static IReadOnlySet<int> StaticTownNpcTypes => _staticTownNpcTypes;

  public static bool IsTownNpc(int npcType)
  {
    ValidateNpcType(npcType);
    return _staticTownNpcTypes.Contains(npcType);
  }

  private static void ValidateNpcType(int npcType)
  {
    if (npcType < 0 || npcType >= NpcTypeCount)
    {
      throw new ArgumentOutOfRangeException(nameof(npcType));
    }
  }
}
