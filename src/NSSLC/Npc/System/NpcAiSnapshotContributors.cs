using System;
using System.Collections.Generic;
using System.Numerics;
using Arch.Core;
using ArchEntity = Arch.Core.Entity;
using ArchWorld = Arch.Core.World;
using Terraria.Share.Ai;
using Terraria.Share.Attributes;
using Terraria.Share.Combat;
using Terraria.Share.Entities;
using Terraria.Share.Inventory;
using Terraria.Share.Movement;
using Terraria.Share.Physics;
using Terraria.Share.Player;
using Terraria.Share.Relationships;
using MovementVelocityComponent = Terraria.Share.Movement.VelocityComponent;

namespace Terraria.Unshared.Npc;

/// <summary>
/// Projects one source domain's authoritative values into the NPC AI fact set.
/// </summary>
/// <remarks>
/// A contributor owns its query and its fact types. A missing source component leaves the snapshot
/// unchanged instead of aborting the capture: the binding still refuses to run because
/// <see cref="AiRequirements.FindMissing"/> reports the absent fact.
/// </remarks>
public interface INpcAiSnapshotContributor
{
  /// <summary>Fact types this contributor can project.</summary>
  IReadOnlyList<Type> FactTypes { get; }

  /// <summary>Adds this contributor's facts and returns the resulting snapshot.</summary>
  AiSnapshot Contribute(ArchEntity entity, AiSnapshot snapshot);
}

/// <summary>Builds the default NPC fact projection set.</summary>
public static class NpcAiSnapshotContributors
{
  /// <summary>
  /// Creates one contributor per source domain: movement, target selection, life and world.
  /// </summary>
  public static IReadOnlyList<INpcAiSnapshotContributor> CreateDefault(
      ArchWorld world,
      Func<NpcAiWorldFact>? worldFacts = null)
  {
    ArgumentNullException.ThrowIfNull(world);
    return
    [
      new NpcAiMovementSnapshotContributor(world),
      new NpcAiTargetSnapshotContributor(world),
      new NpcAiLifeSnapshotContributor(world),
      new NpcAiWorldSnapshotContributor(worldFacts)
    ];
  }
}

/// <summary>Projects position, velocity and grounding from the movement domain.</summary>
internal sealed class NpcAiMovementSnapshotContributor : INpcAiSnapshotContributor
{
  private static readonly Type[] Types = [typeof(NpcAiMovementFact)];

  private readonly ArchWorld _world;

  public NpcAiMovementSnapshotContributor(ArchWorld world)
  {
    _world = world ?? throw new ArgumentNullException(nameof(world));
  }

  public IReadOnlyList<Type> FactTypes => Types;

  public AiSnapshot Contribute(ArchEntity entity, AiSnapshot snapshot)
  {
    if (!_world.TryGet(entity, out LocationComponent location) ||
        !_world.TryGet(entity, out MovementVelocityComponent velocity) ||
        !_world.TryGet(entity, out GroundingComponent grounding))
    {
      return snapshot;
    }

    return snapshot.With(new NpcAiMovementFact(
        location.Coordinates,
        velocity.LinearVelocity,
        grounding.IsGrounded));
  }
}

/// <summary>Selects the nearest visible friendly target from the entity relationships domain.</summary>
internal sealed class NpcAiTargetSnapshotContributor : INpcAiSnapshotContributor
{
  private static readonly Type[] Types = [typeof(NpcAiTargetFact)];

  private readonly ArchWorld _world;
  private readonly QueryDescription _targetQuery = new QueryDescription().WithAll<
      EntityFactionComponent,
      LifeComponent,
      LocationComponent,
      InventoryComponent>();

  public NpcAiTargetSnapshotContributor(ArchWorld world)
  {
    _world = world ?? throw new ArgumentNullException(nameof(world));
  }

  public IReadOnlyList<Type> FactTypes => Types;

  public AiSnapshot Contribute(ArchEntity entity, AiSnapshot snapshot)
  {
    return snapshot.With(FindNearestFriendlyTarget(entity));
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
    bool nearestVisible = false;
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
            // 隐身是 InvisibilityComponent 的行为结果，不是属性标量。投影只把可见性
            // 事实交给 AI；目标仍然可供行为判断距离，但不可被追击或攻击。
            nearestVisible = !(_world.Has<InvisibilityComponent>(entity) &&
                _world.Get<InvisibilityComponent>(entity).IsActive);
          }
        });
    return new NpcAiTargetFact(nearestPosition, nearestVisible, found, found ? 1 : 0);
  }
}

/// <summary>Projects the life domain into the AI fact set.</summary>
internal sealed class NpcAiLifeSnapshotContributor : INpcAiSnapshotContributor
{
  private static readonly Type[] Types = [typeof(NpcAiLifeFact)];

  private readonly ArchWorld _world;

  public NpcAiLifeSnapshotContributor(ArchWorld world)
  {
    _world = world ?? throw new ArgumentNullException(nameof(world));
  }

  public IReadOnlyList<Type> FactTypes => Types;

  public AiSnapshot Contribute(ArchEntity entity, AiSnapshot snapshot)
  {
    NpcAiLifeFact lifeFact = _world.TryGet(entity, out LifeComponent life)
        ? new NpcAiLifeFact(life.Current, life.Maximum)
        : default;
    return snapshot.With(lifeFact);
  }
}

/// <summary>Projects world-level facts supplied by the host.</summary>
internal sealed class NpcAiWorldSnapshotContributor : INpcAiSnapshotContributor
{
  private static readonly Type[] Types = [typeof(NpcAiWorldFact)];

  private readonly Func<NpcAiWorldFact>? _worldFacts;

  public NpcAiWorldSnapshotContributor(Func<NpcAiWorldFact>? worldFacts)
  {
    _worldFacts = worldFacts;
  }

  public IReadOnlyList<Type> FactTypes => Types;

  public AiSnapshot Contribute(ArchEntity entity, AiSnapshot snapshot)
  {
    return snapshot.With(_worldFacts?.Invoke() ?? default);
  }
}
