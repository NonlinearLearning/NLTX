using System;
using System.Collections.Generic;
using System.Numerics;
using Arch.Core;
using ArchEntity = Arch.Core.Entity;
using ArchWorld = Arch.Core.World;
using Terraria.Ai;
using Terraria.Combat;
using Terraria.Entity;
using Terraria.Inventory;
using Terraria.Movement;
using Terraria.Physics;
using Terraria.Player;
using MovementVelocityComponent = Terraria.Movement.VelocityComponent;

namespace Terraria.Npc;

/// <summary>
/// Projects authoritative ECS values into the detached facts consumed by NPC behaviors.
/// </summary>
public sealed class NpcAiSnapshotProvider : IAiSnapshotProvider<ArchEntity>
{
  private readonly ArchWorld _world;
  private readonly QueryDescription _targetQuery = new QueryDescription().WithAll<
      EntityFactionComponent,
      LifeComponent,
      LocationComponent,
      InventoryComponent>();

  public NpcAiSnapshotProvider(ArchWorld world)
  {
    _world = world ?? throw new ArgumentNullException(nameof(world));
  }

  public AiSnapshot Capture(ArchEntity entity, IReadOnlyList<Type> requiredComponents)
  {
    ArgumentNullException.ThrowIfNull(requiredComponents);
    AiSnapshot snapshot = AiSnapshot.Empty;
    if (requiredComponents.Contains(typeof(NpcAiMovementFact)))
    {
      if (!_world.TryGet(entity, out LocationComponent location) ||
          !_world.TryGet(entity, out MovementVelocityComponent velocity) ||
          !_world.TryGet(entity, out GroundingComponent grounding))
      {
        return snapshot;
      }

      snapshot = snapshot.With(new NpcAiMovementFact(
          location.Coordinates,
          velocity.LinearVelocity,
          grounding.IsGrounded));
    }

    if (requiredComponents.Contains(typeof(NpcAiTargetFact)))
    {
      snapshot = snapshot.With(FindNearestFriendlyTarget(entity));
    }

    return snapshot;
  }

  private NpcAiTargetFact FindNearestFriendlyTarget(ArchEntity source)
  {
    if (!_world.TryGet(source, out LocationComponent sourceLocation))
    {
      return default;
    }

    const float maximumTargetDistance = 3_000f;
    float nearestDistance = maximumTargetDistance * maximumTargetDistance;
    Vector2 nearestPosition = default;
    bool found = false;
    _world.Query(
        in _targetQuery,
        (ArchEntity entity,
            ref EntityFactionComponent faction,
            ref LifeComponent life,
            ref LocationComponent location) =>
        {
          if (entity == source || faction.Value != Faction.Friendly || !life.IsAlive)
          {
            return;
          }

          float distance = Vector2.DistanceSquared(
              sourceLocation.Coordinates,
              location.Coordinates);
          if (distance < nearestDistance)
          {
            nearestDistance = distance;
            nearestPosition = location.Coordinates;
            found = true;
          }
        });
    return new NpcAiTargetFact(nearestPosition, found, found, found ? 1 : 0);
  }
}
