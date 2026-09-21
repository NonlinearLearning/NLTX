using System.Collections.Frozen;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.StatusEffects.Definitions;

public static class LegacyLightPetBuffRegistry
{
  public const int BuffTypeCount = 389;

  private static readonly FrozenSet<int> _lightPetBuffTypes =
    new HashSet<int>
    {
      19,
      27,
      57,
      101,
      102,
      152,
      155,
      190,
      201,
      294,
      298,
      299
    }.ToFrozenSet();

  public static IReadOnlySet<int> RegisterDefaults()
  {
    return _lightPetBuffTypes;
  }

  public static bool IsLightPetBuff(int buffType)
  {
    return _lightPetBuffTypes.Contains(buffType);
  }
}
