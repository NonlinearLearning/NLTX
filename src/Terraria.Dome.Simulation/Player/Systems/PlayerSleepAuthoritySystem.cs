using System;
using System.Collections.Generic;
using Arch.Core;
using Terraria.Dome.Simulation.Components;
using Terraria.Dome.Simulation.Player.Components;
using ArchWorld = Arch.Core.World;

namespace Terraria.Dome.Simulation.Player.Systems;

public sealed class PlayerSleepAuthoritySystem
{
  public bool TryGetState(
    ArchWorld world,
    IReadOnlyDictionary<PlayerHandle, Entity> players,
    PlayerHandle player,
    out PlayerSleepComponent state)
  {
    ArgumentNullException.ThrowIfNull(world);
    ArgumentNullException.ThrowIfNull(players);
    state = default;
    if (!TryGetOwnedActivePlayer(world, players, player, out Entity playerEntity))
    {
      return false;
    }

    state = world.Get<PlayerSleepComponent>(playerEntity);
    return true;
  }

  public bool TrySetSleeping(
    ArchWorld world,
    IReadOnlyDictionary<PlayerHandle, Entity> players,
    PlayerHandle player,
    bool isSleeping)
  {
    ArgumentNullException.ThrowIfNull(world);
    ArgumentNullException.ThrowIfNull(players);
    if (!TryGetOwnedActivePlayer(world, players, player, out Entity playerEntity))
    {
      return false;
    }

    ref PlayerSleepComponent state = ref world.Get<PlayerSleepComponent>(playerEntity);
    if (isSleeping)
    {
      state.StartSleeping();
    }
    else
    {
      state.StopSleeping(PlayerSleepWakeReason.External);
    }

    return true;
  }

  private static bool TryGetOwnedActivePlayer(
    ArchWorld world,
    IReadOnlyDictionary<PlayerHandle, Entity> players,
    PlayerHandle player,
    out Entity playerEntity)
  {
    playerEntity = default;
    if (!player.IsValid || !players.TryGetValue(player, out Entity candidate) ||
        !world.IsAlive(candidate) || !world.Has<PlayerTagComponent>(candidate) ||
        !world.Has<PlayerIdentityComponent>(candidate) ||
        world.Get<PlayerIdentityComponent>(candidate).Player != player ||
        !world.Has<PlayerLifecycleComponent>(candidate) ||
        !world.Get<PlayerLifecycleComponent>(candidate).IsActive ||
        !world.Has<PlayerSleepComponent>(candidate))
    {
      return false;
    }

    playerEntity = candidate;
    return true;
  }
}
