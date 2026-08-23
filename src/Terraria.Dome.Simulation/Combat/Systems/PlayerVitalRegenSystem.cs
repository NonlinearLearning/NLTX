using System;
using System.Collections.Generic;
using Arch.Core;
using World = Arch.Core.World;
using Terraria.Dome.Simulation.Combat.Components;
using Terraria.Dome.Simulation.Components;

namespace Terraria.Dome.Simulation.Combat.Systems;

public sealed class PlayerVitalRegenSystem
{
  public void Apply(Arch.Core.World world, IReadOnlyCollection<Entity> players)
  {
    foreach (Entity entity in players)
    {
      ref HealthComponent health = ref world.Get<HealthComponent>(entity);
      ref HealthRegenerationComponent healthRegeneration =
        ref world.Get<HealthRegenerationComponent>(entity);
      if (health.Current >= health.Maximum)
      {
        healthRegeneration.RegenerationAccumulator = 0;
      }
      else if (healthRegeneration.DelayTicks > 0)
      {
        healthRegeneration.DelayTicks--;
      }
      else
      {
        healthRegeneration.RegenerationAccumulator = checked(
          healthRegeneration.RegenerationAccumulator +
          HealthRegenerationComponent.DefaultRegenUnitsPerTick +
          healthRegeneration.EquipmentRegenUnitsPerTick);
        while (healthRegeneration.RegenerationAccumulator >=
               HealthRegenerationComponent.RegenUnitsPerHealthPoint)
        {
          health.Current = Math.Min(health.Current + 1, health.Maximum);
          healthRegeneration.RegenerationAccumulator -=
            HealthRegenerationComponent.RegenUnitsPerHealthPoint;
          if (health.Current >= health.Maximum)
          {
            healthRegeneration.RegenerationAccumulator = 0;
            break;
          }
        }
      }

      ref ManaComponent mana = ref world.Get<ManaComponent>(entity);
      if (mana.Current >= mana.Maximum)
      {
        mana.RegenerationAccumulator = 0;
        continue;
      }

      if (mana.RegenerationDelayTicks > 0)
      {
        mana.RegenerationDelayTicks--;
        continue;
      }

      mana.RegenerationAccumulator = mana.RegenerationAccumulator >= 2
        ? 2
        : mana.RegenerationAccumulator + 1;
      if (mana.RegenerationAccumulator >= 2)
      {
        mana.Current++;
        mana.RegenerationAccumulator = 0;
      }
    }
  }
}
