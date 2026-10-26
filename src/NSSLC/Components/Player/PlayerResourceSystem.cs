using Arch.Core;
using Arch.System;
using ArchWorld = Arch.Core.World;
using Terraria.Entity;

namespace Terraria.Player;

/// <summary>Advances player-owned resource regeneration from explicit tick time.</summary>
public sealed class PlayerResourceSystem : BaseSystem<ArchWorld, EcsServerTickContext>
{
  private readonly QueryDescription _query = new QueryDescription()
      .WithAll<PlayerResourceStateComponent>();

  public PlayerResourceSystem(ArchWorld world) : base(world)
  {
  }

  public override void Update(in EcsServerTickContext context)
  {
    float deltaTime = context.DeltaTime;
    World.Query(
        in _query,
        (ref PlayerResourceStateComponent resources) => resources.Advance(deltaTime));
  }
}
