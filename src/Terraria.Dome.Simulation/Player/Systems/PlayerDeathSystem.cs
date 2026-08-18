using System;
using Terraria.Dome.Simulation.Components;
using Terraria.Dome.Simulation.Player.Components;

namespace Terraria.Dome.Simulation.Player.Systems;

public sealed class PlayerDeathSystem
{
  public bool TryBeginDeath(
    ref PlayerLifecycleComponent lifecycle,
    HealthComponent health,
    int respawnDelayTicks)
  {
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(respawnDelayTicks);
    if (!lifecycle.IsActive || health.Current > 0)
    {
      return false;
    }

    lifecycle.IsActive = false;
    lifecycle.RespawnTicks = respawnDelayTicks;
    return true;
  }
}
