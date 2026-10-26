using System;
using System.Collections.Generic;
using Arch.Core;
using Arch.System;
using ArchEntity = Arch.Core.Entity;
using ArchWorld = Arch.Core.World;
using Terraria.Entity;
using Terraria.Relationships;

namespace Terraria.Physics;

/// <summary>Captures geometry-only contacts into the world-scoped Physics buffer.</summary>
public sealed class PhysicsContactCaptureSystem : BaseSystem<ArchWorld, EcsServerTickContext>
{
  private readonly ISpatialWorldQuery _spatialWorld;
  private readonly IEntityRuntimeHandleResolver _handles;
  private readonly EntityCollisionSystem _collision;
  private readonly PhysicsContactBuffer _buffer;
  private readonly QueryDescription _query = new QueryDescription().WithAll<
      LocationComponent,
      ColliderComponent,
      EntityCollisionCapabilityComponent>();

  internal PhysicsContactBuffer Buffer => _buffer;

  public PhysicsContactCaptureSystem(
      ArchWorld world,
      ISpatialWorldQuery spatialWorld,
      IEntityRuntimeHandleResolver handles,
      PhysicsContactBuffer buffer,
      EntityCollisionSystem? collision = null)
      : base(world ?? throw new ArgumentNullException(nameof(world)))
  {
    _spatialWorld = spatialWorld ?? throw new ArgumentNullException(nameof(spatialWorld));
    _handles = handles ?? throw new ArgumentNullException(nameof(handles));
    _buffer = buffer ?? throw new ArgumentNullException(nameof(buffer));
    _collision = collision ?? new EntityCollisionSystem();
  }

  public override void Update(in EcsServerTickContext context)
  {
    SpatialWorldRevision revision = _spatialWorld.Revision;
    _buffer.BeginCapture(context.Tick, revision);
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
      _buffer.Add(in fact);
    }
  }
}
