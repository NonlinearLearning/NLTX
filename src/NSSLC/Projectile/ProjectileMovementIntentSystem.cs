using Arch.Core;
using Arch.System;
using ArchEntity = Arch.Core.Entity;
using ArchWorld = Arch.Core.World;
using Terraria.Entity;
using Terraria.Movement;
using Terraria.Physics;

namespace Terraria.Projectile;

/// <summary>Refreshes the per-tick movement intent for every live projectile.</summary>
/// <remarks>
/// <c>SpatialSimulationSystem</c> 的第一道校验是 <c>intent.Tick != frame.Tick</c> 就直接拒绝
/// （"The movement intent belongs to another simulation tick."）。NPC 侧由
/// <c>NpcAiIntentAdapter</c> 每帧写入新意图满足它，玩家侧由输入写入；投射物没有任何
/// 意图生产者，因此必须自己每帧续一次，否则只有生成当帧能模拟，之后永久被拒。
///
/// 这里写 <see cref="MovementIntent.Stop"/> 是刻意的：投射物不追求任何目标速度，
/// 配合生成时设定的 <c>acceleration = 0</c> 与 <see cref="MovementMode.Flight"/>，
/// 速度原样保留，表现为匀速直线运动。
/// </remarks>
public sealed class ProjectileMovementIntentSystem : BaseSystem<ArchWorld, EcsServerTickContext>
{
  private readonly QueryDescription _query = new QueryDescription().WithAll<
      ProjectileDispositionStateComponent,
      GroundingComponent>();

  public ProjectileMovementIntentSystem(ArchWorld world)
      : base(world)
  {
  }

  public override void Update(in EcsServerTickContext context)
  {
    long tick = context.Tick;
    World.Query(
        in _query,
        (ArchEntity entity,
            ref ProjectileDispositionStateComponent disposition,
            ref MovementIntent intent) =>
        {
          if (!disposition.IsActive)
          {
            return;
          }

          intent = MovementIntent.Stop(tick);
        });
  }
}
