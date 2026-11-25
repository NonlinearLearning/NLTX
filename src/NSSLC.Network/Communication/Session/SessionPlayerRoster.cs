using System;
using System.Collections.Generic;
using System.Numerics;
using Arch.Core;
using ArchEntity = Arch.Core.Entity;
using ArchWorld = Arch.Core.World;
using Terraria.Share.Combat;
using Terraria.Share.Entities;
using Terraria.Share.Movement;
using Terraria.Share.Physics;
using Terraria.Share.Relationships;
using Terraria.Unshared.Session;

namespace Terraria.Network;

/// <summary>Read-only roster of the players currently bound to an active session.</summary>
/// <remarks>
/// The session component is the authority for "this epoch is a player in this world"; the roster
/// projects it onto the spatial components other domains need. It never writes: callers copy the
/// values they use, so no domain gains access to a player entity.
/// </remarks>
public static class SessionPlayerRoster
{
  private static readonly QueryDescription Query =
      new QueryDescription().WithAll<CommunicationSessionComponent>();

  public static void Collect(
      ArchWorld world,
      IEntityRuntimeResolver resolver,
      List<SessionPlayer> destination)
  {
    ArgumentNullException.ThrowIfNull(world);
    ArgumentNullException.ThrowIfNull(resolver);
    ArgumentNullException.ThrowIfNull(destination);

    destination.Clear();
    world.Query(
        in Query,
        (ref CommunicationSessionComponent session) =>
        {
          if (session.Stage != CommunicationSessionStage.Active ||
              session.Player is not EntityRuntimeHandle handle ||
              !handle.IsValid ||
              !resolver.TryResolve(handle, out ArchEntity player) ||
              player == ArchEntity.Null ||
              !world.IsAlive(player) ||
              !world.Has<LocationComponent>(player) ||
              !world.Has<ColliderComponent>(player))
          {
            return;
          }

          bool alive = !world.Has<LifeComponent>(player) || world.Get<LifeComponent>(player).IsAlive;
          destination.Add(new SessionPlayer(
              session.Epoch,
              handle,
              world.Get<LocationComponent>(player).Coordinates,
              world.Get<ColliderComponent>(player).Size,
              alive));
        });
  }

  /// <summary>Copy of the roster, for callers that iterate while other systems mutate.</summary>
  public static SessionPlayer[] Snapshot(ArchWorld world, IEntityRuntimeResolver resolver)
  {
    List<SessionPlayer> buffer = [];
    Collect(world, resolver, buffer);
    return [.. buffer];
  }
}
