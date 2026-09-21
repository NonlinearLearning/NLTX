using System;
using Terraria.Dome.Simulation.StatusEffects.Components;

namespace Terraria.Dome.Simulation.StatusEffects.Systems;

public sealed class BuffDurationSystem
{
  public void Tick(BuffCollectionComponent buffs)
  {
    ArgumentNullException.ThrowIfNull(buffs);
    for (int index = buffs.Count - 1; index >= 0; index--)
    {
      BuffEntry entry = buffs.Entries[index];
      if (entry.RemainingTicks <= 1)
      {
        buffs.RemoveAt(index);
        continue;
      }

      buffs.SetAt(index, entry with { RemainingTicks = entry.RemainingTicks - 1 });
    }
  }
}
