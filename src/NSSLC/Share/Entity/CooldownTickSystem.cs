using Arch.Core;
using Arch.System;
using ArchWorld = Arch.Core.World;

namespace Terraria.Share.Entities;

/// <summary>Advances cooldown state owned by entities carrying CooldownComponent.</summary>
public sealed class CooldownTickSystem : BaseSystem<ArchWorld, EcsServerTickContext>
{
  private readonly QueryDescription _query = new QueryDescription().WithAll<CooldownComponent>();

  public CooldownTickSystem(ArchWorld world) : base(world)
  {
  }

  public override void Update(in EcsServerTickContext context)
  {
    context.World.Query(in _query, static (ref CooldownComponent cooldowns) =>
    {
      cooldowns.AdvanceOneTick();
    });
  }
}
