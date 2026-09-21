using System;
using System.Collections.Frozen;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.Npc.Definitions;

public static class LegacyNpcCheckActiveKeepAliveRegistry
{
  public const int NpcTypeCount = 697;

  private static readonly FrozenSet<int> _staticKeepAliveNpcTypes =
    new HashSet<int>
    {
      7,
      10,
      13,
      35,
      36,
      39,
      87,
      127,
      128,
      129,
      130,
      131,
      392,
      393,
      394,
      491,
      492
    }.ToFrozenSet();

  public static int StaticKeepAliveNpcTypeCount => _staticKeepAliveNpcTypes.Count;

  public static IReadOnlySet<int> StaticKeepAliveNpcTypes => _staticKeepAliveNpcTypes;

  public static bool IsStaticKeepAliveType(int npcType)
  {
    ValidateNpcType(npcType);
    return _staticKeepAliveNpcTypes.Contains(npcType);
  }

  private static void ValidateNpcType(int npcType)
  {
    if (npcType < 0 || npcType >= NpcTypeCount)
    {
      throw new ArgumentOutOfRangeException(nameof(npcType));
    }
  }
}
