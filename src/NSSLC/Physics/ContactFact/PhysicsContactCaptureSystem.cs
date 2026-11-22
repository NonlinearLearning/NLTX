using System;
using System.Collections.Generic;
using Arch.Core;
using Arch.System;
using ArchEntity = Arch.Core.Entity;
using ArchWorld = Arch.Core.World;
using Terraria.Entity;
using Terraria.Relationships;

namespace Terraria.Physics;

/// <summary>Captures geometry-only contacts into the world-scoped Physics contact queue component.</summary>
public sealed class PhysicsContactCaptureSystem : BaseSystem<ArchWorld, EcsServerTickContext>
{
  private readonly ISpatialWorldQuery _spatialWorld;
  private readonly IEntityRuntimeHandleResolver _handles;
  private readonly EntityCollisionSystem _collision;
  private readonly QueryDescription _query = new QueryDescription().WithAll<
      LocationComponent,
      ColliderComponent,
      EntityCollisionCapabilityComponent>();
  private readonly QueryDescription _queueQuery =
      new QueryDescription().WithAll<PhysicsContactQueueComponent>();

  public PhysicsContactCaptureSystem(
      ArchWorld world,
      ISpatialWorldQuery spatialWorld,
      IEntityRuntimeHandleResolver handles)
      : base(world ?? throw new ArgumentNullException(nameof(world)))
  {
    _spatialWorld = spatialWorld ?? throw new ArgumentNullException(nameof(spatialWorld));
    _handles = handles ?? throw new ArgumentNullException(nameof(handles));
    _collision = new EntityCollisionSystem();
  }

  public override void Update(in EcsServerTickContext context)
  {
    PhysicsContactQueueComponent queue = RequireQueue();
    SpatialWorldRevision revision = _spatialWorld.Revision;
    queue.BeginCapture(context.Tick, revision);
    List<SpatialBody<ArchEntity>> bodies = [];
    World.Query(
        in _query,
        (ArchEntity entity,
            ref LocationComponent location,
            ref ColliderComponent collider,
            ref EntityCollisionCapabilityComponent capability) =>
        {
          if (!_handles.TryGetHandle(entity, out _))
          {
            return;
          }

          SpatialEntitySnapshot<ArchEntity> snapshot = new(
              entity,
              location,
              World.TryGet(entity, out Terraria.Movement.VelocityComponent velocity)
                  ? velocity
                  : Terraria.Movement.VelocityComponent.Stationary,
              collider);
          bodies.Add(new SpatialBody<ArchEntity>(snapshot, capability));
        });

    IReadOnlyList<SpatialContactPair<ArchEntity>> pairs = _collision.FindContacts(bodies);
    for (int index = 0; index < pairs.Count; index++)
    {
      SpatialContactPair<ArchEntity> pair = pairs[index];
      if (!_handles.TryGetHandle(pair.First, out EntityRuntimeHandle first) ||
          !_handles.TryGetHandle(pair.Second, out EntityRuntimeHandle second))
      {
        continue;
      }

      PhysicsContactFact fact = new(
          first,
          second,
          pair.Contact,
          context.Tick,
          revision,
          substep: 0);
      queue.Add(in fact);
    }
  }

  private PhysicsContactQueueComponent RequireQueue()
  {
    ArchEntity queueEntity = default;
    bool found = false;
    World.Query(
        in _queueQuery,
        (ArchEntity entity) =>
        {
          queueEntity = entity;
          found = true;
        });

    if (!found)
    {
      throw new InvalidOperationException(
          "A PhysicsContactQueueComponent entity must exist before contact capture runs.");
    }

    return World.Get<PhysicsContactQueueComponent>(queueEntity);
  }
}
