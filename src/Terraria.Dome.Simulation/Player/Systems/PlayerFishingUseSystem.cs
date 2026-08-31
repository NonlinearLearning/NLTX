using System;
using System.Collections.Generic;
using Arch.Core;
using Terraria.Dome.Simulation.Components;
using Terraria.Dome.Simulation.Items;
using Terraria.Dome.Simulation.Items.Definitions;
using Terraria.Dome.Simulation.Player.Components;
using Terraria.Dome.Simulation.Projectile;
using ArchWorld = Arch.Core.World;

namespace Terraria.Dome.Simulation.Player.Systems;

public sealed class PlayerFishingUseSystem
{
  private readonly QueryDescription _bobberQuery = new QueryDescription()
    .WithAll<ProjectileTagComponent, ProjectileOwnerComponent, ProjectileBobberComponent,
      ProjectileLifetimeComponent>();

  public bool CanUse(
    ArchWorld world,
    IReadOnlyDictionary<PlayerHandle, Entity> players,
    ProjectileStore projectiles,
    PlayerHandle player,
    ItemDefinition definition)
  {
    ArgumentNullException.ThrowIfNull(world);
    ArgumentNullException.ThrowIfNull(players);
    ArgumentNullException.ThrowIfNull(projectiles);
    if (!TryGetOwnedActivePlayer(world, players, player, out _))
    {
      return false;
    }

    if (definition.Gathering is not ItemGatheringDefinition gathering)
    {
      return true;
    }

    bool hasActiveOwnedBobber = false;
    world.Query(
      in _bobberQuery,
      (Entity entity, ref ProjectileOwnerComponent owner,
        ref ProjectileLifetimeComponent lifetime) =>
      {
        if (hasActiveOwnedBobber || owner.Owner != player || lifetime.RemainingTicks <= 0)
        {
          return;
        }

        hasActiveOwnedBobber = projectiles.TryGetValue(entity, out _);
      });

    return PlayerFishingUsePolicy.CanUseFishingPole(
      new PlayerFishingUseInput(gathering.FishingPolePower, hasActiveOwnedBobber));
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
        !world.Get<PlayerLifecycleComponent>(candidate).IsActive)
    {
      return false;
    }

    playerEntity = candidate;
    return true;
  }
}
