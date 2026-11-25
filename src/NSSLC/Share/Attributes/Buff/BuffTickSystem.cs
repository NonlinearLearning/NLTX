using Arch.Core;
using Arch.System;
using ArchEntity = Arch.Core.Entity;
using ArchWorld = Arch.Core.World;
using Terraria.Share.Entities;

namespace Terraria.Share.Attributes;

/// <summary>Advances active shared Buffs for entities with a complete Buff composition.</summary>
public sealed class BuffTickSystem : BaseSystem<ArchWorld, EcsServerTickContext>
{
  private readonly BuffSystem _buffs;
  private readonly QueryDescription _query;

  public BuffTickSystem(ArchWorld world, BuffSystem buffs) : base(world)
  {
    ArgumentNullException.ThrowIfNull(buffs);
    _buffs = buffs;
    _query = new QueryDescription().WithAll<BuffListComponent>();
  }

  public override void Update(in EcsServerTickContext context)
  {
    ArchWorld world = context.World;
    BuffSystem buffs = _buffs;
    world.Query(
        in _query,
        (ArchEntity entity, ref BuffListComponent activeBuffs) =>
        {
          AttributeSetComponent? attributes = null;
          if (world.Has<AttributeSetComponent>(entity))
          {
            attributes = world.Get<AttributeSetComponent>(entity);
          }
          buffs.Advance(activeBuffs, attributes);
        });
  }
}
