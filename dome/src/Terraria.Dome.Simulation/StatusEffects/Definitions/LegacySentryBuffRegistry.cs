using System.Collections.Frozen;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.StatusEffects.Definitions;

public static class LegacySentryBuffRegistry
{
  public const ushort WarTableBuffType = 348;

  private static readonly FrozenDictionary<ushort, int> _capacityBonuses =
    new Dictionary<ushort, int>
    {
      [WarTableBuffType] = 1
    }.ToFrozenDictionary();

  public static IReadOnlyDictionary<ushort, int> CapacityBonuses => _capacityBonuses;

  public static bool TryGet(ushort buffType, out int capacityBonus)
  {
    return _capacityBonuses.TryGetValue(buffType, out capacityBonus);
  }
}
