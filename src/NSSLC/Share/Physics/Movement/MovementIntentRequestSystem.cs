using System;
using Arch.System;
using ArchWorld = Arch.Core.World;
using ArchEntity = Arch.Core.Entity;
using Terraria.Share.Entities;
using Terraria.Share.Relationships;

namespace Terraria.Share.Movement;

/// <summary>Installs the movement intents other domains requested.</summary>
/// <remarks>
/// Registered in the simulation phase after every producer and immediately before the spatial
/// coordinator, so an intent decided this tick is installed this tick and read by the solver in the
/// same pass — the ordering the producers had when they wrote the component directly.
/// </remarks>
public sealed class MovementIntentRequestSystem : BaseSystem<ArchWorld, EcsServerTickContext>
{
  private readonly IEntityRuntimeResolver _resolver;

  public MovementIntentRequestSystem(ArchWorld ecsWorld, IEntityRuntimeResolver resolver)
      : base(ecsWorld ?? throw new ArgumentNullException(nameof(ecsWorld)))
  {
    _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
  }

  public override void Update(in EcsServerTickContext context)
  {
    // 在查询之外解析队列实体：Require 内部本身要跑一次查询。
    MovementIntentRequestQueueComponent queue = MovementIntentRequestQueue.Require(World);
    IReadOnlyList<MovementIntentRequest> pending = queue.Drain();
    for (int index = 0; index < pending.Count; index++)
    {
      MovementIntentRequest request = pending[index];
      if (!_resolver.TryResolve(request.Target, out ArchEntity entity) ||
          entity == ArchEntity.Null || !World.IsAlive(entity) ||
          !World.Has<MovementIntent>(entity))
      {
        continue;
      }

      World.Get<MovementIntent>(entity) = request.Intent;
    }
  }
}
