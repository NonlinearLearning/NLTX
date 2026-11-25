using System;
using System.Collections.Generic;
using Arch.Core;
using Arch.System;
using ArchEntity = Arch.Core.Entity;
using ArchWorld = Arch.Core.World;
using Terraria.Share.Combat;
using Terraria.Share.Entities;
using Terraria.Share.Relationships;

namespace Terraria.Unshared.Player;

/// <summary>Consumes every Player-owned Combat result without storing a LastResult component.</summary>
public sealed class PlayerCombatResultSystem : BaseSystem<ArchWorld, EcsServerTickContext>
{
  private readonly IEntityRuntimeHandleResolver _handles;
  private readonly QueryDescription _query = new QueryDescription().WithAll<
      PlayerDeathStateComponent>();

  public PlayerCombatResultSystem(
      ArchWorld world,
      IEntityRuntimeHandleResolver handles)
      : base(world ?? throw new ArgumentNullException(nameof(world)))
  {
    _handles = handles ?? throw new ArgumentNullException(nameof(handles));
  }

  public override void Update(in EcsServerTickContext context)
  {
    CombatDamageResultQueueComponent resultQueue = CombatDamageResultQueue.Require(World);
    World.Query(
        in _query,
        (ArchEntity entity) =>
        {
          _ = CombatResultDrain.Drain(resultQueue, _handles, entity);
        });
  }
}
