using System;
using System.Collections.Generic;
using Arch.Core;
using ArchWorld = Arch.Core.World;
using Terraria.Dome.Simulation.Components;
using Terraria.Dome.Simulation.Player.Components;

namespace Terraria.Dome.Simulation.Player.Systems;

public sealed class PlayerSentryAuthoritySystem
{
  public bool TryGetMaximumTurrets(
    ArchWorld world,
    IReadOnlyDictionary<PlayerHandle, Entity> players,
    PlayerHandle player,
    out int maximumTurrets)
  {
    ArgumentNullException.ThrowIfNull(world);
    ArgumentNullException.ThrowIfNull(players);
    maximumTurrets = 0;
    if (!TryGetState(world, players, player, out PlayerSentryStateComponent state))
    {
      return false;
    }

    maximumTurrets = state.MaximumTurrets;
    return true;
  }

  public bool TrySetMaximumTurrets(
    ArchWorld world,
    IReadOnlyDictionary<PlayerHandle, Entity> players,
    PlayerHandle player,
    int maximumTurrets)
  {
    ArgumentNullException.ThrowIfNull(world);
    ArgumentNullException.ThrowIfNull(players);
    ArgumentOutOfRangeException.ThrowIfNegative(maximumTurrets);
    if (!players.TryGetValue(player, out Entity playerEntity) ||
        !IsOwnedPlayerEntity(world, playerEntity, player))
    {
      return false;
    }

    ref PlayerSentryStateComponent state = ref world.Get<PlayerSentryStateComponent>(playerEntity);
    state.SetMaximumTurrets(maximumTurrets);
    return true;
  }

  public bool TrySetEquipmentCapacityBonus(
    ArchWorld world,
    IReadOnlyDictionary<PlayerHandle, Entity> players,
    PlayerHandle player,
    int capacityBonus)
  {
    ArgumentNullException.ThrowIfNull(world);
    ArgumentNullException.ThrowIfNull(players);
    ArgumentOutOfRangeException.ThrowIfNegative(capacityBonus);
    if (!players.TryGetValue(player, out Entity playerEntity) ||
        !IsOwnedPlayerEntity(world, playerEntity, player))
    {
      return false;
    }

    ref PlayerSentryStateComponent state = ref world.Get<PlayerSentryStateComponent>(playerEntity);
    state.SetEquipmentCapacityBonus(capacityBonus);
    return true;
  }

  public bool TrySetBuffCapacityBonus(
    ArchWorld world,
    IReadOnlyDictionary<PlayerHandle, Entity> players,
    PlayerHandle player,
    int capacityBonus)
  {
    ArgumentNullException.ThrowIfNull(world);
    ArgumentNullException.ThrowIfNull(players);
    ArgumentOutOfRangeException.ThrowIfNegative(capacityBonus);
    if (!players.TryGetValue(player, out Entity playerEntity) ||
        !IsOwnedPlayerEntity(world, playerEntity, player))
    {
      return false;
    }

    ref PlayerSentryStateComponent state = ref world.Get<PlayerSentryStateComponent>(playerEntity);
    state.SetBuffCapacityBonus(capacityBonus);
    return true;
  }

  public bool TrySetArmorSetCapacityBonus(
    ArchWorld world,
    IReadOnlyDictionary<PlayerHandle, Entity> players,
    PlayerHandle player,
    int capacityBonus)
  {
    ArgumentNullException.ThrowIfNull(world);
    ArgumentNullException.ThrowIfNull(players);
    ArgumentOutOfRangeException.ThrowIfNegative(capacityBonus);
    if (!players.TryGetValue(player, out Entity playerEntity) ||
        !IsOwnedPlayerEntity(world, playerEntity, player))
    {
      return false;
    }

    ref PlayerSentryStateComponent state = ref world.Get<PlayerSentryStateComponent>(playerEntity);
    state.SetArmorSetCapacityBonus(capacityBonus);
    return true;
  }

  private static bool TryGetState(
    ArchWorld world,
    IReadOnlyDictionary<PlayerHandle, Entity> players,
    PlayerHandle player,
    out PlayerSentryStateComponent state)
  {
    state = default;
    if (!players.TryGetValue(player, out Entity playerEntity) ||
        !IsOwnedPlayerEntity(world, playerEntity, player))
    {
      return false;
    }

    state = world.Get<PlayerSentryStateComponent>(playerEntity);
    return state.MaximumTurrets >= 0;
  }

  private static bool IsOwnedPlayerEntity(
    ArchWorld world,
    Entity playerEntity,
    PlayerHandle player)
  {
    return player.IsValid &&
      world.IsAlive(playerEntity) &&
      world.Has<PlayerTagComponent>(playerEntity) &&
      world.Has<PlayerIdentityComponent>(playerEntity) &&
      world.Get<PlayerIdentityComponent>(playerEntity).Player == player &&
      world.Has<PlayerLifecycleComponent>(playerEntity) &&
      world.Get<PlayerLifecycleComponent>(playerEntity).IsActive &&
      world.Has<PlayerSentryStateComponent>(playerEntity);
  }
}
