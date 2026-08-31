using System.Collections.Frozen;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.StatusEffects.Definitions;

public static class LegacyMeleeBuffRegistry
{
  public const int BuffTypeCount = 389;

  private static readonly FrozenSet<int> _meleeBuffTypes =
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
    return _meleeBuffTypes;
  }

  public static bool IsMeleeBuff(int buffType)
  {
    return _meleeBuffTypes.Contains(buffType);
  }
}
