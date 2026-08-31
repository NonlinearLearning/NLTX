using System.Collections.Frozen;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.StatusEffects.Definitions;

public static class LegacyVanityPetBuffRegistry
{
  public const int BuffTypeCount = 389;

  private static readonly FrozenSet<int> _vanityPetBuffTypes =
    new HashSet<int>
    {
      40,
      41,
      42,
      45,
      50,
      51,
      52,
      53,
      54,
      55,
      56,
      61,
      65,
      66,
      81,
      82,
      84,
      85,
      91,
      92,
      127,
      136,
      154,
      191,
      200,
      202,
      217,
      218,
      219,
      258,
      259,
      260,
      261,
      262,
      264,
      266,
      267,
      268,
      274,
      284,
      285,
      286,
      287,
      288,
      289,
      290,
      291,
      292,
      293,
      295,
      296,
      297,
      300,
      301,
      302,
      303,
      304,
      317,
      327,
      328,
      329,
      330,
      331,
      341,
      345,
      349,
      351,
      352,
      354,
      356,
      371,
      372,
      373,
      382
    }.ToFrozenSet();

  public static IReadOnlySet<int> RegisterDefaults()
  {
    return _vanityPetBuffTypes;
  }

  public static bool IsVanityPetBuff(int buffType)
  {
    return _vanityPetBuffTypes.Contains(buffType);
  }
}
