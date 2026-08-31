using System.Collections.Frozen;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.Npc.Definitions;

public static class LegacySlimeRainNpcRegistry
{
  public const int NpcTypeCount = 697;

  private const int BlueSlimeNpcType = 1;

  private static readonly FrozenSet<int> _slimeRainNpcTypes =
    new HashSet<int> { BlueSlimeNpcType }.ToFrozenSet();

  public static IReadOnlySet<int> RegisterDefaults()
  {
    return _slimeRainNpcTypes;
  }

  public static bool IsSlimeRainNpc(int npcType)
  {
    return _slimeRainNpcTypes.Contains(npcType);
  }
}
