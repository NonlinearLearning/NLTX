using System;
using Arch.Core;
using Arch.System;
using ArchWorld = Arch.Core.World;
using Terraria.Entity;

namespace Terraria.World;

/// <summary>
/// Advances one authoritative <see cref="WorldRuntime" /> from the ECS tick context.
/// </summary>
/// <remarks>
/// Register this system before interaction commit when the host wants world time and revision to
/// advance once per server tick. The host still owns the clock and calls <see cref="EcsServer"/>.
/// </remarks>
public sealed class WorldTickSystem : BaseSystem<ArchWorld, EcsServerTickContext>
{
  private readonly WorldRuntime _world;

  public WorldTickSystem(ArchWorld ecsWorld, WorldRuntime world)
      : base(ecsWorld ?? throw new ArgumentNullException(nameof(ecsWorld)))
  {
    _world = world ?? throw new ArgumentNullException(nameof(world));
  }

  public WorldRuntime WorldRuntime => _world;

  public WorldTickResult LastResult { get; private set; } = WorldTickResult.NotRun;

  public override void Update(in EcsServerTickContext context)
  {
    LastResult = _world.AdvanceTick(context.Tick, context.DeltaTime);
    if (!LastResult.Applied)
    {
      throw new InvalidOperationException(LastResult.Failure);
    }
  }
}
