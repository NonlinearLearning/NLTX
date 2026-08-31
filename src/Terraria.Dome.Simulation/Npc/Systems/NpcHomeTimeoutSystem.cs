using System;
using Terraria.Dome.Simulation.Npc.Components;

namespace Terraria.Dome.Simulation.Npc.Systems;

public sealed class NpcHomeTimeoutSystem
{
  public void Tick(ref NpcHomeComponent home)
  {
    if (home.LookForHomeTimeout < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(home));
    }

    if (home.LookForHomeTimeout > 0)
    {
      home.LookForHomeTimeout--;
    }
  }
}
