using System;
using Terraria.Dome.Simulation.Combat.Components;
using Terraria.Dome.Simulation.StatusEffects.Components;

namespace Terraria.Dome.Simulation.StatusEffects.Systems;

public sealed class BuffEffectSystem
{
  public void Apply(BuffCollectionComponent buffs, ref ManaComponent mana)
  {
    ArgumentNullException.ThrowIfNull(buffs);
    for (int index = 0; index < buffs.Count; index++)
    {
      BuffEntry entry = buffs.Entries[index];
      if (entry.Type == 1 && mana.Current < mana.Maximum)
      {
        mana.Current++;
      }
    }
  }
}
