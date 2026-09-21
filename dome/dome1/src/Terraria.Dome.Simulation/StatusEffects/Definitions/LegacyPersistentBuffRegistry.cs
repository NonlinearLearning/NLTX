using System.Collections.Frozen;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.StatusEffects.Definitions;

public static class LegacyPersistentBuffRegistry
{
  public const int BuffTypeCount = 389;

  private static readonly FrozenSet<int> _persistentBuffTypes =
    new HashSet<int>
    {
      71,
      73,
      74,
      75,
      76,
      77,
      78,
      79
    }.ToFrozenSet();

  public static IReadOnlySet<int> RegisterDefaults()
  {
    return _persistentBuffTypes;
  }

  public static bool IsPersistentBuff(int buffType)
  {
    return _persistentBuffTypes.Contains(buffType);
  }
}
