using System.Collections.Generic;

namespace Terraria.Dome.Simulation.Combat.Components;

public sealed class HitImmunityComponent
{
  public Dictionary<int, int> NpcTicks { get; } = new();

  public bool IsImmune(int targetId)
  {
    return NpcTicks.TryGetValue(targetId, out int ticks) && ticks > 0;
  }

  public void Set(int targetId, int ticks)
  {
    NpcTicks[targetId] = ticks;
  }
}
