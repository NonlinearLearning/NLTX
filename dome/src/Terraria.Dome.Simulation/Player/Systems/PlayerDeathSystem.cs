using System;
using Terraria.Dome.Simulation.Components;
using Terraria.Dome.Simulation.Player.Components;

namespace Terraria.Dome.Simulation.Player.Systems;

public sealed class PlayerDeathSystem
{
  private const int MaxRespawnDelayTicks = 3600;

  public bool TryBeginDeath(
    ref PlayerLifecycleComponent lifecycle,
    ref PlayerDeathDropStateComponent deathDrop,
    HealthComponent health,
    int respawnDelayTicks)
  {
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(respawnDelayTicks);
    ArgumentOutOfRangeException.ThrowIfGreaterThan(respawnDelayTicks, MaxRespawnDelayTicks);
    if (!lifecycle.IsActive || health.Current > 0)
    {
      return false;
    }

    lifecycle.IsActive = false;
    lifecycle.IsDead = true;
    lifecycle.DeadTime = 0;
    lifecycle.RespawnTicks = respawnDelayTicks;
    deathDrop.Begin(0);
    return true;
  }
}
