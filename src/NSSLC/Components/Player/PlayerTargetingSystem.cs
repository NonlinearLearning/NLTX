using System.Collections.Generic;
using Arch.Core;
using Arch.System;
using ArchEntity = Arch.Core.Entity;
using ArchWorld = Arch.Core.World;
using Terraria.Combat;
using Terraria.Entity;
using Terraria.Relationships;

namespace Terraria.Player;

/// <summary>Removes target relations whose identity is no longer present in the current world.</summary>
public sealed class PlayerTargetingSystem : BaseSystem<ArchWorld, EcsServerTickContext>
{
  private readonly QueryDescription _identityQuery = new QueryDescription()
      .WithAll<EntityIdentityComponent>();
  private readonly QueryDescription _targetQuery = new QueryDescription()
      .WithAll<PlayerTargetingStateComponent>();

  public PlayerTargetingSystem(ArchWorld world) : base(world)
  {
  }

  public override void Update(in EcsServerTickContext context)
  {
    HashSet<EntityUuid> identities = [];
    World.Query(
        in _identityQuery,
        (ArchEntity _, ref EntityIdentityComponent identity) => identities.Add(identity.Id));

    World.Query(
        in _targetQuery,
        (ref PlayerTargetingStateComponent targeting) =>
        {
          if (targeting.HasTarget && !identities.Contains(targeting.TargetId))
          {
            targeting.Clear();
          }
        });
  }
}
