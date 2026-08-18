using System.Collections.Generic;
using Terraria.Dome.Simulation.Combat.Components;

namespace Terraria.Dome.Simulation.Combat.Systems;

public sealed class HitImmunitySystem
{
  public bool IsImmune(HitImmunityComponent immunity, int targetId)
  {
    return immunity.IsImmune(targetId);
  }

  public void Apply(HitImmunityComponent immunity, int targetId, int ticks)
  {
    immunity.Set(targetId, ticks);
  }

  public void Tick(HitImmunityComponent immunity)
  {
    List<int> expired = new();
    foreach (KeyValuePair<int, int> entry in immunity.NpcTicks)
    {
      if (entry.Value <= 1)
      {
        expired.Add(entry.Key);
      }
      else
      {
        immunity.NpcTicks[entry.Key] = entry.Value - 1;
      }
    }

    for (int index = 0; index < expired.Count; index++)
    {
      immunity.NpcTicks.Remove(expired[index]);
    }
  }
}
