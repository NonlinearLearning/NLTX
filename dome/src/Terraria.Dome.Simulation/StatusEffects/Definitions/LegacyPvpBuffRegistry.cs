using System.Collections.Frozen;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.StatusEffects.Definitions;

public static class LegacyPvpBuffRegistry
{
  public const int BuffTypeCount = 389;

  private static readonly FrozenSet<int> _pvpBuffTypes =
    new HashSet<int>
    {
      20,
      24,
      30,
      31,
      36,
      39,
      44,
      69,
      70,
      103,
      119,
      120,
      137,
      320,
      323,
      324
    }.ToFrozenSet();

  public static IReadOnlySet<int> RegisterDefaults()
  {
    return _pvpBuffTypes;
  }

  public static bool IsPvpBuff(int buffType)
  {
    return _pvpBuffTypes.Contains(buffType);
  }
}
