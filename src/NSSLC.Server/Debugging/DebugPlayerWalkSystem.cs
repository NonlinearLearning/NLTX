using System;
using System.Numerics;
using Arch.Core;
using Arch.System;
using ArchWorld = Arch.Core.World;
using Terraria.Entity;
using Terraria.Movement;
using Terraria.Player;
using Terraria.Relationships;

namespace NSSLC.Server.Debugging;

/// <summary>
/// 调试用系统：每 tick 让所有玩家朝 +X 持续移动。
/// </summary>
/// <remarks>
/// 只在通过 <see cref="ServerDebugTools.CreatePlayerWalkRegistration" /> 显式注册时运行，
/// 用于在没有真实客户端的情况下驱动玩家，使 NPC 获得追击目标。
/// <para>
/// 副作用登记：每 tick 向 <see cref="MovementIntentRequestQueueComponent" /> 入队一条玩家移动
/// 请求；不直接写玩家组件，不产生 I/O。
/// </para>
/// </remarks>
public sealed class DebugPlayerWalkSystem : BaseSystem<ArchWorld, EcsServerTickContext>
{
  private readonly IEntityRuntimeHandleResolver _handles;
  private readonly float _horizontalSpeed;
  private readonly QueryDescription _query = new QueryDescription().WithAll<
      LocationComponent,
      PlayerDeathStateComponent>();

  internal DebugPlayerWalkSystem(
      ArchWorld world,
      IEntityRuntimeHandleResolver handles,
      float horizontalSpeed)
      : base(world ?? throw new ArgumentNullException(nameof(world)))
  {
    _handles = handles ?? throw new ArgumentNullException(nameof(handles));
    if (!float.IsFinite(horizontalSpeed) || horizontalSpeed == 0f)
    {
      throw new ArgumentOutOfRangeException(nameof(horizontalSpeed));
    }

    _horizontalSpeed = horizontalSpeed;
  }

  public override void Update(in EcsServerTickContext context)
  {
    long tick = context.Tick;
    MovementIntentRequestQueueComponent queue = MovementIntentRequestQueue.Require(World);
    World.Query(
        in _query,
        (Arch.Core.Entity entity, ref LocationComponent location) =>
        {
          _ = location;
          if (!_handles.TryGetHandle(entity, out EntityRuntimeHandle handle))
          {
            return;
          }

          queue.Enqueue(new MovementIntentRequest(
              handle,
              new MovementIntent(
                  new Vector2(_horizontalSpeed, 0f),
                  tick,
                  source: MovementIntentSource.Input)));
        });
  }
}
