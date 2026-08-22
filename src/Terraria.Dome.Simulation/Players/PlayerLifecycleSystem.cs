using System;
using System.Collections.Generic;
using Arch.Core;
using Terraria.Dome.Simulation.Player.Components;
using ArchWorld = Arch.Core.World;

namespace Terraria.Dome.Simulation.Players;

public sealed class PlayerLifecycleSystem
{
  public void Advance(
    ArchWorld world,
    IReadOnlyDictionary<PlayerHandle, Entity> players,
    Action<PlayerHandle, SimulationVector> enqueueRespawn)
  {
    ArgumentNullException.ThrowIfNull(world);
    ArgumentNullException.ThrowIfNull(players);
    ArgumentNullException.ThrowIfNull(enqueueRespawn);
    foreach (KeyValuePair<PlayerHandle, Entity> entry in players)
    {
      if (!world.IsAlive(entry.Value))
      {
        continue;
      }

      ref PlayerLifecycleComponent lifecycle = ref world.Get<PlayerLifecycleComponent>(entry.Value);
      if (lifecycle.IsActive)
      {
        continue;
      }

      if (lifecycle.RespawnTicks < 0)
      {
        lifecycle.RespawnTicks = 0;
        continue;
      }

      if (lifecycle.RespawnTicks == 0)
      {
        continue;
      }

      lifecycle.RespawnTicks--;
      if (lifecycle.RespawnTicks == 0)
      {
        enqueueRespawn.Invoke(entry.Key, lifecycle.Spawn);
      }
    }
  }
}
