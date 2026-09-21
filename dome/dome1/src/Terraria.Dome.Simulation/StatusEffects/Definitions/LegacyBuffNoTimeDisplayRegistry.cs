using System.Collections.Frozen;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.StatusEffects.Definitions;

public static class LegacyBuffNoTimeDisplayRegistry
{
  public const int BuffTypeCount = 389;

  private static readonly FrozenSet<int> _buffTypes = CreateDefaults();

  public static IReadOnlySet<int> RegisterDefaults()
  {
    return _buffTypes;
  }

  public static bool IsBuffNoTimeDisplay(int buffType)
  {
    return _buffTypes.Contains(buffType);
  }

  private static FrozenSet<int> CreateDefaults()
  {
    HashSet<int> types = new()
    {
      19, 27, 28, 29, 34, 37, 38, 40, 41, 42, 43, 45, 49, 60, 62, 64, 68,
      81, 82, 83, 93, 95, 96, 97, 98, 99, 100, 101, 102, 125, 126, 133, 134,
      135, 136, 139, 140, 150, 159, 161, 163, 165, 170, 171, 172, 182, 186,
      187, 188, 199, 200, 201, 202, 213, 214, 216, 217, 219, 258, 259, 260,
      261, 262, 263, 264, 266, 267, 268, 271, 274, 317, 322, 325, 327, 328,
      329, 330, 331, 334, 335, 341, 345, 348, 349, 351, 352, 353, 354, 355,
      356, 366, 371, 372, 373, 382, 385, 386
    };
    for (int buffType = 284; buffType <= 304; buffType++)
    {
      types.Add(buffType);
    }

    return types.ToFrozenSet();
  }
}
