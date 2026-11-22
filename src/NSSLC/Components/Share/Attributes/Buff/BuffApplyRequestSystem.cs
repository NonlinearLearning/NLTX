using System;
using Arch.Core;
using Arch.System;
using ArchEntity = Arch.Core.Entity;
using ArchWorld = Arch.Core.World;
using Terraria.Entity;

namespace Terraria.Attributes;

/// <summary>Consumes cross-domain BuffApplyRequestComponent inputs.</summary>
public sealed class BuffApplyRequestSystem : BaseSystem<ArchWorld, EcsServerTickContext>
{
  private readonly BuffSystem _buffs;
  private readonly QueryDescription _query = new QueryDescription()
      .WithAll<BuffApplyRequestComponent, BuffListComponent>();

  public BuffApplyRequestSystem(ArchWorld world, BuffSystem buffs) : base(world)
  {
    _buffs = buffs ?? throw new ArgumentNullException(nameof(buffs));
  }

  public override void Update(in EcsServerTickContext context)
  {
    ArchWorld world = context.World;
    EcsServerCommandBuffer? commands = context.Commands;
    world.Query(
        in _query,
        (ArchEntity entity,
            ref BuffApplyRequestComponent request,
            ref BuffListComponent activeBuffs) =>
        {
          BuffReceptionCapabilityComponent? capability = world.Has<BuffReceptionCapabilityComponent>(entity)
              ? world.Get<BuffReceptionCapabilityComponent>(entity)
              : null;
          BuffImmunityComponent? immunity = world.Has<BuffImmunityComponent>(entity)
              ? world.Get<BuffImmunityComponent>(entity)
              : null;
          AttributeSetComponent? attributes = world.Has<AttributeSetComponent>(entity)
              ? world.Get<AttributeSetComponent>(entity)
              : null;
          _buffs.TryApply(
              capability,
              immunity,
              activeBuffs,
              attributes,
              in request);

          if (commands is not null)
          {
            commands.Remove<BuffApplyRequestComponent>(entity);
          }
          else
          {
            world.Remove<BuffApplyRequestComponent>(entity);
          }
        });
  }
}
