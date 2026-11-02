using Arch.Core;
using Arch.System;
using ArchWorld = Arch.Core.World;
using ArchEntity = Arch.Core.Entity;
using Terraria.Combat;
using Terraria.Entity;
using Terraria.Relationships;

namespace Terraria.Player;

/// <summary>
/// Commits player death and same-instance respawn state without owning spatial placement.
/// </summary>
public sealed class PlayerDeathRespawnSystem : BaseSystem<ArchWorld, EcsServerTickContext>
{
  private readonly IEntityRuntimeHandleResolver _handles;
  private readonly QueryDescription _query = new QueryDescription()
      .WithAll<LifeComponent, PlayerDeathStateComponent, PlayerRespawnStateComponent>();

  public PlayerDeathRespawnSystem(ArchWorld world, IEntityRuntimeHandleResolver handles)
      : base(world)
  {
    _handles = handles ?? throw new ArgumentNullException(nameof(handles));
  }

  public override void Update(in EcsServerTickContext context)
  {
    long tick = context.Tick;
    // 在查询之外解析队列实体：Require 内部本身要跑一次查询。
    LifeRestoreRequestQueueComponent restores = LifeRestoreRequestQueue.Require(World);
    World.Query(
        in _query,
        (ArchEntity entity,
            ref LifeComponent life,
            ref PlayerDeathStateComponent death,
            ref PlayerRespawnStateComponent respawn) =>
        {
          if (!life.IsAlive && !death.IsDead)
          {
            death.MarkDead(tick);
            respawn.Schedule(tick);
            return;
          }

          if (life.IsAlive && death.IsDead)
          {
            return;
          }

          if (!death.IsDead || !respawn.IsScheduled || !respawn.Advance())
          {
            return;
          }

          // LifeComponent 归 Combat 域所有：这里只表达"恢复到满血"的意图。
          if (_handles.TryGetHandle(entity, out EntityRuntimeHandle handle))
          {
            restores.Enqueue(handle);
          }

          death.Clear();
          respawn.Cancel();
        });
  }
}
