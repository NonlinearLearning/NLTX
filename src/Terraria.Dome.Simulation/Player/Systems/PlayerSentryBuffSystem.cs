using System;
using Terraria.Dome.Simulation.StatusEffects.Components;
using Terraria.Dome.Simulation.StatusEffects.Definitions;

namespace Terraria.Dome.Simulation.Player.Systems;

public sealed class PlayerSentryBuffSystem
{
  public int CalculateCapacityBonus(BuffCollectionComponent buffs)
  {
    ArgumentNullException.ThrowIfNull(buffs);

    int totalCapacityBonus = 0;
    foreach (BuffEntry entry in buffs.Entries)
    {
      if (entry.RemainingTicks <= 0 ||
          !LegacySentryBuffRegistry.TryGet(entry.Type, out int capacityBonus))
      {
        continue;
      }

      totalCapacityBonus = checked(totalCapacityBonus + capacityBonus);
    }

    return totalCapacityBonus;
  }
}
