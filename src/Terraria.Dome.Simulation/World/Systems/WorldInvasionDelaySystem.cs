using System;

namespace Terraria.Dome.Simulation.WorldModel.Systems;

public sealed class WorldInvasionDelaySystem
{
  public int AdvanceAtDayStart(int invasionDelayTicks)
  {
    ArgumentOutOfRangeException.ThrowIfNegative(invasionDelayTicks);
    return invasionDelayTicks > 0 ? invasionDelayTicks - 1 : 0;
  }
}
