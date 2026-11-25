using System;
using Arch.System;
using ArchWorld = Arch.Core.World;
using ArchEntity = Arch.Core.Entity;
using Terraria.Share.Entities;
using Terraria.Share.Relationships;

namespace Terraria.Share.Combat;

/// <summary>Applies the life restores other domains asked for.</summary>
/// <remarks>
/// Registered in the commit phase directly after the player death/respawn system, so a respawn that
/// becomes due in this tick is restored to full life in the same tick it always was.
/// </remarks>
public sealed class LifeRestoreRequestSystem : BaseSystem<ArchWorld, EcsServerTickContext>
{
  private readonly IEntityRuntimeResolver _resolver;

  public LifeRestoreRequestSystem(ArchWorld ecsWorld, IEntityRuntimeResolver resolver)
      : base(ecsWorld ?? throw new ArgumentNullException(nameof(ecsWorld)))
  {
    _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
  }

  public override void Update(in EcsServerTickContext context)
  {
    // 在查询之外解析队列实体：Require 内部本身要跑一次查询。
    LifeRestoreRequestQueueComponent queue = LifeRestoreRequestQueue.Require(World);
    IReadOnlyList<LifeRestoreRequest> pending = queue.Drain();
    for (int index = 0; index < pending.Count; index++)
    {
      LifeRestoreRequest request = pending[index];
      EntityRuntimeHandle handle = request.Target;
      if (!_resolver.TryResolve(handle, out ArchEntity entity) ||
          entity == ArchEntity.Null || !World.IsAlive(entity) ||
          !World.Has<LifeComponent>(entity))
      {
        continue;
      }

      ref LifeComponent life = ref World.Get<LifeComponent>(entity);
      int nextCurrent = request.Amount == 0
          ? life.Maximum
          : Math.Min(life.Maximum, life.Current + request.Amount);
      life = life.WithCurrent(nextCurrent);
    }
  }
}
