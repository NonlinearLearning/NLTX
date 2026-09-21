using System.Collections.Frozen;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.StatusEffects.Definitions;

public static class LegacyBuffNoSaveRegistry
{
  public const int BuffTypeCount = 389;

  private static readonly FrozenSet<int> _buffTypes = CreateDefaults();

  public static IReadOnlySet<int> RegisterDefaults()
  {
    return _buffTypes;
  }

  public static bool IsBuffNoSave(int buffType)
  {
    return _buffTypes.Contains(buffType);
  }

  private static FrozenSet<int> CreateDefaults()
  {
    HashSet<int> types = new()
    {
      20, 22, 23, 24, 28, 29, 30, 31, 34, 35, 37, 38, 39, 43, 44, 46, 47,
      48, 58, 59, 60, 62, 63, 64, 67, 68, 69, 70, 72, 80, 87, 88, 89, 93,
      94, 95, 96, 97, 98, 99, 100, 103, 119, 120, 125, 126, 133, 134, 135,
      137, 139, 140, 144, 146, 147, 150, 158, 159, 161, 163, 164, 170, 171,
      172, 182, 187, 188, 194, 195, 196, 197, 198, 199, 205, 213, 214, 215,
      263, 271, 320, 321, 322, 325, 335, 348, 353, 355, 366, 385, 386
    };
    for (int buffType = 173; buffType <= 181; buffType++)
    {
      types.Add(buffType);
    }

    return types.ToFrozenSet();
  }
}
