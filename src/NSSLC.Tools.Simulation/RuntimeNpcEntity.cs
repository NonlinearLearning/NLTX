using System.Numerics;
using System.Threading;
using EntityEcs;
using EntityEcs.Components;
using Terraria.Combat;
using Terraria.Content;
using Terraria.Npc;
using Terraria.SpatialSimulation.Components;
using Terraria.Town;
using Terraria.Town.Housing;
using Terraria.Relationships;
using Terraria.WorldStorage;
using NpcRuntimeSlot = Terraria.Npc.NpcSlot;

namespace Terraria.NonAuthoritative.SimulationHost;

internal sealed class RuntimeNpcEntity
{
  private readonly record struct NpcBehaviorSnapshot(
    int Action,
    float State0,
    float State1,
    float State2,
    float State3,
    long LastUpdatedTick);

  private readonly record struct NpcLocalBehaviorSnapshot(
    float State0,
    float State1,
    float State2,
    float State3);

  private readonly record struct NpcHealthSnapshot(int CurrentLife, int MaximumLife);

  private readonly record struct NpcGivenNameSnapshot(string GivenName);

  internal readonly record struct NpcPresentationSnapshot(int TownNpcVariationIndex);

  internal readonly record struct NpcDirectionSnapshot(int Vertical, int Sprite);

  internal readonly record struct NpcMovementTickSnapshot(
    Vector2 OldPosition,
    Vector2 OldVelocity,
    bool CollideX,
    bool CollideY,
    bool Wet,
    bool ShimmerWet,
    bool HoneyWet,
    bool NoGravity,
    bool NoTileCollide);

  internal readonly record struct NpcHousingRelationSnapshot(
    bool IsHomeless,
    bool HomelessDespawn,
    int LookForHomeTimeout,
    TownRoomTilePoint? HomeTile,
    int HousingCategory)
  {
    public bool HasHome => HomeTile.HasValue && !IsHomeless;
  }

  internal readonly record struct NpcImmediateEffectSnapshot(
    int DespawnEncouragementTicks,
    int DustCount,
    bool NetworkUpdateRequested,
    bool JumpRequested,
    float JumpVelocityY,
    bool DoorOpenRequested,
    int DoorTileX,
    int DoorTileY,
    int DoorDirection,
    bool HomeTeleportRequested,
    bool HomeTeleportSucceeded,
    bool HomeTeleportFailed,
    int HomeTeleportCandidateOffset,
    NpcTaskFailureReason HomeTeleportFailureReason,
    Vector2 HomeTeleportPosition,
    bool HousingRevalidationFailed,
    bool HousingRegistrySynchronized,
    Vector2 LastDustPosition,
    Vector2 LastDustVelocity);

  internal readonly record struct NpcTaskSnapshot(
    NpcTaskKind Kind,
    NpcTaskPhase Phase,
    int Cursor,
    NpcTaskFailureReason FailureReason,
    ulong TaskGeneration,
    NpcTaskEndReason EndReason);

  internal readonly record struct NpcParentRelationSnapshot(
    NpcInstanceId ParentInstanceId,
    NpcRuntimeSlot ParentLegacySlot,
    long AttachedAtTick,
    EntityReference ParentReference)
  {
    public bool IsAttached => ParentInstanceId.IsValid;
  }

  private readonly record struct EntityRelationSnapshot(
    EntityReference RelatedEntity,
    EntityRelationKind RelationKind,
    long? AttachedAtTick);

  private static long _nextInstanceId;
  private readonly EntityRuntime _entityRuntime;
  private readonly List<string> _eyeOfCthulhuEffectTrace = new();

  private RuntimeNpcEntity(EntityRuntime entityRuntime)
  {
    _entityRuntime = entityRuntime;
  }

  public required NpcRuntimeSlot Slot { get; init; }

  public required uint SlotGeneration { get; set; }

  public required RuntimeEntityHandle RuntimeHandle { get; init; }

  public required WorldNpcState SavedState { get; init; }

  public required NpcDefinition Definition { get; init; }

  public bool IsNaturallySpawned
  {
    get
    {
      if (!_entityRuntime.TryCapture<NpcNaturalDespawnStateComponent, bool>(
            RuntimeHandle,
            static state => state.IsNaturallySpawned,
            out bool isNaturallySpawned))
      {
        throw new InvalidOperationException("The NPC entity has no natural despawn state.");
      }

      return isNaturallySpawned;
    }
  }

  public NpcInstanceId InstanceId
  {
    get
    {
      if (!_entityRuntime.TryCapture<NpcEntityIdentityComponent, NpcInstanceId>(
            RuntimeHandle,
            static component => component.InstanceId,
            out NpcInstanceId instanceId))
      {
        throw new InvalidOperationException("The NPC identity component is unavailable.");
      }

      return instanceId;
    }
  }

  public int CurrentLife => CaptureHealthSnapshot().CurrentLife;

  public int MaximumLife => CaptureHealthSnapshot().MaximumLife;

  public bool IsActive
  {
    get
    {
      if (!_entityRuntime.TryCapture<NpcLifecycleComponent, bool>(
            RuntimeHandle,
            static component => component.IsActive,
            out bool isActive))
      {
        throw new InvalidOperationException("The NPC lifecycle component is unavailable.");
      }

      return isActive;
    }
  }

  public int BehaviorAction => CaptureBehaviorSnapshot().Behavior.Action;

  public long LastBehaviorUpdatedTick => CaptureBehaviorSnapshot().Behavior.LastUpdatedTick;

  public MovementStateComponent Movement
  {
    get
    {
      LocationComponent location = CaptureSpatialComponent<LocationComponent>("location");
      VelocityComponent velocity = CaptureSpatialComponent<VelocityComponent>("velocity");
      MovementGroundedStateComponent grounded =
        CaptureSpatialComponent<MovementGroundedStateComponent>("grounded state");
      return new MovementStateComponent(
        new Vector2(location.X, location.Y),
        new Vector2(velocity.X, velocity.Y),
        isGrounded: grounded.IsGrounded);
    }
    set
    {
      if (!_entityRuntime.TryEditComponents<
            LocationComponent,
            VelocityComponent,
            MovementGroundedStateComponent>(
            RuntimeHandle,
            (
              ref LocationComponent location,
              ref VelocityComponent velocity,
              ref MovementGroundedStateComponent grounded) =>
            {
              location.X = value.Position.X;
              location.Y = value.Position.Y;
              velocity.X = value.Velocity.X;
              velocity.Y = value.Velocity.Y;
              grounded = new MovementGroundedStateComponent(value.IsGrounded);
            }))
      {
        throw new InvalidOperationException(
          "The NPC spatial movement output could not be committed.");
      }
    }
  }

  public LocationComponent Location => CaptureSpatialComponent<LocationComponent>("location");

  public VelocityComponent Velocity => CaptureSpatialComponent<VelocityComponent>("velocity");

  public ColliderComponent Collider => CaptureSpatialComponent<ColliderComponent>("collider");

  public static RuntimeNpcEntity Hydrate(
    EntityRuntime entityRuntime,
    RuntimeEntityHandle runtimeHandle,
    NpcRuntimeSlot slot,
    uint slotGeneration,
    WorldNpcState savedState,
    NpcDefinition definition,
    int catalogRevision,
    bool isNaturallySpawned = false)
  {
    ArgumentNullException.ThrowIfNull(entityRuntime);
    // Storage slots can be reused or rebuilt during world hydration.
    // They cannot identify an instance.
    long nextInstanceId = Interlocked.Increment(ref _nextInstanceId);
    if (nextInstanceId <= 0)
    {
      throw new InvalidOperationException("The runtime NPC instance identity space is exhausted.");
    }

    NpcInstanceId instanceId = new((ulong)nextInstanceId);
    NpcTypeId typeId = new(definition.TypeId);
    NpcNetId netId = new(definition.NetId);
    Vector2 position = new(savedState.X, savedState.Y);
    MovementStateComponent initialMovement = new(position: position);
    if (!entityRuntime.TryAttach(
          runtimeHandle,
          new LocationComponent(initialMovement.Position.X, initialMovement.Position.Y)))
    {
      throw new InvalidOperationException("The NPC location component could not be attached.");
    }
    if (!entityRuntime.TryAttach(runtimeHandle, new VelocityComponent(0.0f, 0.0f)))
    {
      throw new InvalidOperationException("The NPC velocity component could not be attached.");
    }
    if (!entityRuntime.TryAttach(
          runtimeHandle,
          new ColliderComponent(
            Math.Max(1, (int)(definition.Movement.Width * definition.Movement.Scale)),
            Math.Max(1, (int)(definition.Movement.Height * definition.Movement.Scale)))))
    {
      throw new InvalidOperationException("The NPC collider component could not be attached.");
    }
    if (!entityRuntime.TryAttach(
          runtimeHandle,
          new MovementGroundedStateComponent(false)))
    {
      throw new InvalidOperationException("The NPC movement grounded state could not be attached.");
    }
    if (!entityRuntime.TryAttach(
          runtimeHandle,
          new NpcHealthComponent(
            definition.Core.DefaultLifeMax,
            definition.Core.DefaultLifeMax)))
    {
      throw new InvalidOperationException("The NPC health component could not be attached.");
    }
    if (!entityRuntime.TryAttach(runtimeHandle, new NpcHitStateComponent()))
    {
      throw new InvalidOperationException("The NPC hit state could not be attached.");
    }
    if (!entityRuntime.TryAttach(runtimeHandle, new NpcStatusFlagsComponent()))
    {
      throw new InvalidOperationException("The NPC status flags could not be attached.");
    }
    if (!entityRuntime.TryAttach(
          runtimeHandle,
          new DefenseComponent(definition.Core.DefaultDefense)))
    {
      throw new InvalidOperationException("The NPC defense component could not be attached.");
    }
    if (!entityRuntime.TryAttach(
          runtimeHandle,
          new NpcLifecycleComponent(
            isActive: true,
            remainingActiveTicks: 0,
            NpcLifecycleStage.Active)))
    {
      throw new InvalidOperationException("The NPC lifecycle component could not be attached.");
    }
    if (!entityRuntime.TryAttach(
          runtimeHandle,
          new NpcNaturalDespawnStateComponent(isNaturallySpawned)))
    {
      throw new InvalidOperationException("The NPC natural despawn state could not be attached.");
    }
    if (!entityRuntime.TryAttach(runtimeHandle, new NpcDirectionComponent(1, 1)))
    {
      throw new InvalidOperationException("The NPC direction component could not be attached.");
    }
    if (!entityRuntime.TryAttach(runtimeHandle, new NpcTargetComponent()))
    {
      throw new InvalidOperationException("The NPC target component could not be attached.");
    }
    if (!entityRuntime.TryAttach(runtimeHandle, new NpcTargetSelectionStateComponent()))
    {
      throw new InvalidOperationException("The NPC target selection state could not be attached.");
    }
    if (!entityRuntime.TryAttach(runtimeHandle, new NpcImmediateEffectStateComponent()))
    {
      throw new InvalidOperationException("The NPC immediate effect state could not be attached.");
    }
    if (!entityRuntime.TryAttach(runtimeHandle, new NpcTaskStateComponent()))
    {
      throw new InvalidOperationException("The NPC task state could not be attached.");
    }
    if (!entityRuntime.TryAttach(
          runtimeHandle,
          new NpcBehaviorStateComponent(
            behaviorKind: definition.Town.IsTownNpc ? 0 : 1,
            legacyAiStyle: definition.Spawn.AiStyle,
            action: 0)))
    {
      throw new InvalidOperationException("The NPC behavior state could not be attached.");
    }
    if (!entityRuntime.TryAttach(
          runtimeHandle,
          new NpcLocalBehaviorStateComponent(
            new float[NpcLocalBehaviorStateComponent.LocalAiSlotCount])))
    {
      throw new InvalidOperationException("The NPC local behavior state could not be attached.");
    }
    if (!entityRuntime.TryAttach(runtimeHandle, new NpcMovementTickStateComponent()))
    {
      throw new InvalidOperationException("The NPC movement tick state could not be attached.");
    }
    if (!entityRuntime.TryAttach(
          runtimeHandle,
          new NpcSpawnAndCritterStateComponent(
            spawnedFromStatue: false,
            canBeReplacedByOtherNpcs: definition.Town.CanBeReplacedByOtherNpcs)))
    {
      throw new InvalidOperationException("The NPC spawn state could not be attached.");
    }
    if (!entityRuntime.TryAttach(
          runtimeHandle,
          new NpcPresentationStateComponent(
            townNpcVariationIndex: savedState.Variation.GetValueOrDefault())))
    {
      throw new InvalidOperationException("The NPC presentation state could not be attached.");
    }
    if (!entityRuntime.TryAttach(runtimeHandle, HydrateHousingRelation(savedState)))
    {
      throw new InvalidOperationException("The NPC housing relation could not be attached.");
    }
    if (!entityRuntime.TryAttach(
          runtimeHandle,
          new NpcEntityIdentityComponent(instanceId, slot)))
    {
      throw new InvalidOperationException("The NPC instance identity could not be attached.");
    }
    if (!entityRuntime.TryAttach(runtimeHandle, new NpcGivenNameComponent(savedState.Name)))
    {
      throw new InvalidOperationException("The NPC given name could not be attached.");
    }
    if (!entityRuntime.TryAttach(runtimeHandle, new NpcLegacySlotComponent(slot)))
    {
      throw new InvalidOperationException("The NPC legacy slot projection could not be attached.");
    }
    if (!entityRuntime.TryAttach(
          runtimeHandle,
          new NpcDefinitionReferenceComponent(typeId, netId, catalogRevision)))
    {
      throw new InvalidOperationException("The NPC definition reference could not be attached.");
    }

    return new RuntimeNpcEntity(entityRuntime)
    {
      Slot = slot,
      SlotGeneration = slotGeneration,
      RuntimeHandle = runtimeHandle,
      SavedState = savedState,
      Definition = definition,
    };
  }

  internal NpcMovementTickSnapshot CaptureMovementTickSnapshot()
  {
    if (!_entityRuntime.TryCapture<NpcMovementTickStateComponent, NpcMovementTickSnapshot>(
          RuntimeHandle,
          static component => new NpcMovementTickSnapshot(
            component.OldPosition,
            component.OldVelocity,
            component.CollideX,
            component.CollideY,
            component.Wet,
            component.ShimmerWet,
            component.HoneyWet,
            component.NoGravity,
            component.NoTileCollide),
          out NpcMovementTickSnapshot snapshot))
    {
      throw new InvalidOperationException("The NPC movement tick component is unavailable.");
    }

    return snapshot;
  }

  internal void BeginMovementTick(
    Vector2 position,
    Vector2 velocity,
    bool wet,
    bool shimmerWet,
    bool honeyWet)
  {
    if (!_entityRuntime.TryEdit(
          RuntimeHandle,
          (ref NpcMovementTickStateComponent component) =>
          {
            component.BeginTick(position, velocity, wet, shimmerWet, honeyWet);
          }))
    {
      throw new InvalidOperationException("The NPC movement tick could not begin.");
    }
  }

  internal bool ConsumeJustHit()
  {
    bool justHit = false;
    if (!_entityRuntime.TryEdit(
          RuntimeHandle,
          (ref NpcHitStateComponent component) =>
          {
            justHit = component.Consume();
          }))
    {
      throw new InvalidOperationException("The NPC hit state could not be consumed.");
    }

    return justHit;
  }

  internal void CommitJustHit()
  {
    if (!_entityRuntime.TryEdit(
          RuntimeHandle,
          static (ref NpcHitStateComponent component) => component.CommitHit()))
    {
      throw new InvalidOperationException("The NPC hit state could not be committed.");
    }
  }

  internal void CommitMovementPhysicsFlags(bool noGravity, bool noTileCollide)
  {
    if (!_entityRuntime.TryEdit(
          RuntimeHandle,
          (ref NpcMovementTickStateComponent component) =>
          {
            component.CommitPhysicsFlags(noGravity, noTileCollide);
          }))
    {
      throw new InvalidOperationException("The NPC movement physics flags could not be committed.");
    }
  }

  internal void CommitMovementCollision(bool collideX, bool collideY)
  {
    if (!_entityRuntime.TryEdit(
          RuntimeHandle,
          (ref NpcMovementTickStateComponent component) =>
          {
            component.CommitCollision(collideX, collideY);
          }))
    {
      throw new InvalidOperationException("The NPC movement collision state could not be committed.");
    }
  }

  internal NpcHousingRelationSnapshot CaptureHousingRelationSnapshot()
  {
    if (!_entityRuntime.TryCapture<
          TownHousingRelationStateComponent,
          NpcHousingRelationSnapshot>(
          RuntimeHandle,
          static component => new NpcHousingRelationSnapshot(
            component.IsHomeless,
            component.HomelessDespawn,
            component.LookForHomeTimeout,
            component.HomeTile,
            component.HousingCategory),
          out NpcHousingRelationSnapshot snapshot))
    {
      throw new InvalidOperationException("The NPC housing relation component is unavailable.");
    }

    return snapshot;
  }

  internal void CommitHousingRelation(
    bool isHomeless,
    bool homelessDespawn,
    TownRoomTilePoint? homeTile)
  {
    if (!_entityRuntime.TryEdit(
          RuntimeHandle,
          (ref TownHousingRelationStateComponent component) =>
          {
            component.CommitRelation(
              isHomeless,
              homelessDespawn,
              lookForHomeTimeout: 0,
              homeTile,
              housingCategory: 0);
          }))
    {
      throw new InvalidOperationException("The NPC housing relation could not be committed.");
    }
  }

  internal string CaptureGivenName()
  {
    if (!_entityRuntime.TryCapture<NpcGivenNameComponent, NpcGivenNameSnapshot>(
          RuntimeHandle,
          static component => new NpcGivenNameSnapshot(component.GivenName),
          out NpcGivenNameSnapshot snapshot))
    {
      throw new InvalidOperationException("The NPC given name component is unavailable.");
    }

    return snapshot.GivenName;
  }

  internal NpcPresentationSnapshot CapturePresentationSnapshot()
  {
    if (!_entityRuntime.TryCapture<NpcPresentationStateComponent, NpcPresentationSnapshot>(
          RuntimeHandle,
          static component => new NpcPresentationSnapshot(component.TownNpcVariationIndex),
          out NpcPresentationSnapshot snapshot))
    {
      throw new InvalidOperationException("The NPC presentation component is unavailable.");
    }

    return snapshot;
  }

  internal RuntimeNpcContactSnapshot CaptureContactSnapshot()
  {
    if (!_entityRuntime.TryGetReference(
          RuntimeHandle,
          EntityReferenceScope.Npc,
          out EntityReference reference))
    {
      throw new InvalidOperationException("The NPC root reference is unavailable.");
    }

    LocationComponent location = Location;
    ColliderComponent collider = Collider;
    Vector2 position = new(location.X, location.Y);
    Vector2 colliderPosition = position + new Vector2(collider.OffsetX, collider.OffsetY);
    var hitbox = new NpcTargetGeometrySnapshot(
      colliderPosition,
      Math.Max(1, (int)collider.Width),
      Math.Max(1, (int)collider.Height));
    return new RuntimeNpcContactSnapshot(
      reference,
      IsActive,
      Definition.Capabilities.Hostile,
      Definition.Core.DefaultDamage,
      hitbox);
  }

  private TComponent CaptureSpatialComponent<TComponent>(string componentName)
    where TComponent : struct
  {
    if (!_entityRuntime.TryCapture<TComponent, TComponent>(
          RuntimeHandle,
          static component => component,
          out TComponent component))
    {
      bool hasRuntimeStatus = _entityRuntime.TryGetStatus(
        RuntimeHandle,
        out EntityRuntimeStatus runtimeStatus);
      throw new InvalidOperationException(
        $"The NPC {componentName} component is unavailable " +
        $"(slot {Slot.Value}/{SlotGeneration}, handle {RuntimeHandle}, " +
        $"runtime {_entityRuntime.RuntimeId}, " +
        $"status {(hasRuntimeStatus ? runtimeStatus.ToString() : "missing")}).");
    }

    return component;
  }

  internal NpcAiStateComponent CaptureAiState()
  {
    (NpcBehaviorSnapshot behavior, NpcLocalBehaviorSnapshot localBehavior) =
      CaptureBehaviorSnapshot();
    return new NpcAiStateComponent(
      Definition.Spawn.AiStyle,
      behavior.State0,
      behavior.State1,
      behavior.State2,
      behavior.State3,
      timer: 0,
      localAi0: localBehavior.State0,
      localAi1: localBehavior.State1,
      localAi2: localBehavior.State2,
      localAi3: localBehavior.State3);
  }

  internal int CaptureDefenseValue()
  {
    if (!_entityRuntime.TryCapture<DefenseComponent, int>(
          RuntimeHandle,
          static defense => defense.Value,
          out int defenseValue))
    {
      throw new InvalidOperationException("The NPC defense component is unavailable.");
    }

    return defenseValue;
  }

  internal void CommitDefenseValue(int value)
  {
    if (!_entityRuntime.TryReplace(RuntimeHandle, new DefenseComponent(value)))
    {
      throw new InvalidOperationException("The NPC defense value could not be committed.");
    }
  }

  internal void ApplySmallBlueSlimeDefaults()
  {
    if (Definition.TypeId != 1 || Definition.NetId != 1)
    {
      throw new InvalidOperationException(
        "Small Blue Slime defaults can only be applied to the Blue Slime definition.");
    }

    int width = Math.Max(1, (int)(Definition.Movement.Width * 0.9f));
    int height = Math.Max(1, (int)(Definition.Movement.Height * 0.9f));
    if (!_entityRuntime.TryReplace(RuntimeHandle, new NpcHealthComponent(30, 30)) ||
        !_entityRuntime.TryReplace(RuntimeHandle, new DefenseComponent(4)) ||
        !_entityRuntime.TryReplace(RuntimeHandle, new ColliderComponent(width, height)))
    {
      throw new InvalidOperationException(
        "Small Blue Slime defaults could not be committed to the runtime entity.");
    }
  }

  internal bool TryEditCombatComponents(
    EntityComponentPairEditor<NpcHealthComponent, NpcLifecycleComponent> editor)
  {
    return _entityRuntime.TryEditPair(RuntimeHandle, editor);
  }

  internal NpcDirectionSnapshot CaptureDirection()
  {
    if (!_entityRuntime.TryCapture<NpcDirectionComponent, NpcDirectionSnapshot>(
          RuntimeHandle,
          static direction => new NpcDirectionSnapshot(direction.Vertical, direction.Sprite),
          out NpcDirectionSnapshot direction))
    {
      throw new InvalidOperationException("The NPC direction component is unavailable.");
    }

    return direction;
  }

  internal bool CaptureConfused()
  {
    if (!_entityRuntime.TryCapture<NpcStatusFlagsComponent, bool>(
          RuntimeHandle,
          static flags => flags.Confused,
          out bool confused))
    {
      throw new InvalidOperationException("The NPC status flags component is unavailable.");
    }

    return confused;
  }

  internal NpcImmediateEffectSnapshot CaptureImmediateEffects()
  {
    if (!_entityRuntime.TryCapture<
          NpcImmediateEffectStateComponent,
          NpcImmediateEffectSnapshot>(
          RuntimeHandle,
          static component => new NpcImmediateEffectSnapshot(
            component.DespawnEncouragementTicks,
            component.DustCount,
            component.NetworkUpdateRequested,
            component.JumpRequested,
            component.JumpVelocityY,
            component.DoorOpenRequested,
            component.DoorTileX,
            component.DoorTileY,
            component.DoorDirection,
            component.HomeTeleportRequested,
            component.HomeTeleportSucceeded,
            component.HomeTeleportFailed,
            component.HomeTeleportCandidateOffset,
            component.HomeTeleportFailureReason,
            component.HomeTeleportPosition,
            component.HousingRevalidationFailed,
            component.HousingRegistrySynchronized,
            component.LastDustPosition,
            component.LastDustVelocity),
          out NpcImmediateEffectSnapshot snapshot))
    {
      throw new InvalidOperationException("The NPC immediate effect state component is unavailable.");
    }

    return snapshot;
  }

  internal NpcTaskSnapshot CaptureTaskSnapshot()
  {
    if (!_entityRuntime.TryCapture<NpcTaskStateComponent, NpcTaskSnapshot>(
          RuntimeHandle,
          static component => new NpcTaskSnapshot(
            component.Kind,
            component.Phase,
            component.Cursor,
            component.FailureReason,
            component.TaskGeneration,
            component.LastEndReason),
          out NpcTaskSnapshot snapshot))
    {
      throw new InvalidOperationException("The NPC task state component is unavailable.");
    }

    return snapshot;
  }

  internal bool TryAttachParentRelation(
    RuntimeNpcEntity parent,
    long attachedAtTick)
  {
    ArgumentNullException.ThrowIfNull(parent);
    if (!_entityRuntime.Has<NpcParentRelationComponent>(RuntimeHandle) &&
        !_entityRuntime.Has<EntityRelationState>(RuntimeHandle))
    {
      if (!_entityRuntime.TryAttach(
            RuntimeHandle,
            new NpcParentRelationComponent(
              parent.InstanceId,
              parent.Slot,
              attachedAtTick)))
      {
        return false;
      }

      if (!_entityRuntime.TryGetReference(
            parent.RuntimeHandle,
            EntityReferenceScope.Npc,
            out EntityReference parentReference) ||
          !_entityRuntime.TryAttach(
            RuntimeHandle,
            new EntityRelationState(
              parentReference,
              EntityRelationKind.Parent,
              expectedRevision: 0,
              attachedAtTick)))
      {
        _entityRuntime.TryDetach<NpcParentRelationComponent>(RuntimeHandle);
        return false;
      }

      return true;
    }

    return false;
  }

  internal bool TryCaptureParentRelation(
    out NpcParentRelationSnapshot snapshot)
  {
    if (!_entityRuntime.TryCapture<
          NpcParentRelationComponent,
          NpcParentRelationSnapshot>(
          RuntimeHandle,
          static relation => new NpcParentRelationSnapshot(
            relation.ParentInstanceId,
            relation.ParentLegacySlot,
            relation.AttachedAtTick,
            EntityReference.None),
          out snapshot))
    {
      snapshot = default;
      return false;
    }

    if (_entityRuntime.TryCapture<
          EntityRelationState,
          EntityRelationSnapshot>(
          RuntimeHandle,
          static relation => new EntityRelationSnapshot(
            relation.RelatedEntity,
            relation.RelationKind,
            relation.AttachedAtTick),
          out EntityRelationSnapshot entityRelation) &&
        entityRelation.RelationKind == EntityRelationKind.Parent &&
        entityRelation.RelatedEntity.Scope == EntityReferenceScope.Npc &&
        entityRelation.AttachedAtTick == snapshot.AttachedAtTick)
    {
      snapshot = snapshot with { ParentReference = entityRelation.RelatedEntity };
    }

    return true;
  }

  internal bool HasParentRelation
  {
    get
    {
      if (_entityRuntime.Has<NpcParentRelationComponent>(RuntimeHandle))
      {
        return true;
      }

      return _entityRuntime.TryCapture<
          EntityRelationState,
          EntityRelationSnapshot>(
          RuntimeHandle,
          static relation => new EntityRelationSnapshot(
            relation.RelatedEntity,
            relation.RelationKind,
            relation.AttachedAtTick),
          out EntityRelationSnapshot relationSnapshot) &&
        relationSnapshot.RelationKind == EntityRelationKind.Parent;
    }
  }

  internal bool TryDetachParentRelation()
  {
    bool detachedParent = !_entityRuntime.Has<NpcParentRelationComponent>(RuntimeHandle) ||
      _entityRuntime.TryDetach<NpcParentRelationComponent>(RuntimeHandle);
    bool detachedGeneric = !_entityRuntime.Has<EntityRelationState>(RuntimeHandle) ||
      _entityRuntime.TryDetach<EntityRelationState>(RuntimeHandle);
    return detachedParent && detachedGeneric;
  }

  internal void EnterTask(NpcTaskKind kind)
  {
    CommitTaskResult(
      state => NpcTaskLifecycleSystem.Enter(state, kind));
  }

  internal bool TryCaptureTaskReference(out NpcTaskReference reference)
  {
    return _entityRuntime.TryCapture<NpcTaskStateComponent, NpcTaskReference>(
      RuntimeHandle,
      component => new NpcTaskReference(RuntimeHandle, component.TaskGeneration),
      out reference);
  }

  internal bool TryTerminateTask(
    NpcTaskReference reference,
    NpcTaskEndReason reason,
    out NpcTaskTerminationResult result)
  {
    NpcTaskTerminationResult termination = default;
    bool edited = _entityRuntime.TryEdit(
      RuntimeHandle,
      (ref NpcTaskStateComponent state) =>
      {
        termination = NpcTaskLifecycleSystem.Terminate(state, RuntimeHandle, reference, reason);
      });
    result = termination;
    return edited && termination.Accepted;
  }

  internal bool TryAdvanceTask(
    NpcTaskReference reference,
    NpcTaskKind kind,
    out NpcTaskReferenceOperationResult result)
  {
    return TryCommitTaskOperation(
      state => NpcTaskLifecycleSystem.Advance(state, RuntimeHandle, reference, kind),
      out result);
  }

  internal bool TryInterruptTask(
    NpcTaskReference reference,
    NpcTaskKind kind,
    NpcTaskFailureReason reason,
    out NpcTaskReferenceOperationResult result)
  {
    return TryCommitTaskOperation(
      state => NpcTaskLifecycleSystem.Interrupt(state, RuntimeHandle, reference, kind, reason),
      out result);
  }

  internal bool TryCompleteTask(
    NpcTaskReference reference,
    NpcTaskKind kind,
    out NpcTaskReferenceOperationResult result)
  {
    return TryCommitTaskOperation(
      state => NpcTaskLifecycleSystem.Complete(state, RuntimeHandle, reference, kind),
      out result);
  }

  internal bool TryFailTask(
    NpcTaskReference reference,
    NpcTaskKind kind,
    NpcTaskFailureReason reason,
    out NpcTaskReferenceOperationResult result)
  {
    return TryCommitTaskOperation(
      state => NpcTaskLifecycleSystem.Fail(state, RuntimeHandle, reference, kind, reason),
      out result);
  }

  private bool TryCommitTaskOperation(
    Func<NpcTaskStateComponent, NpcTaskReferenceOperationResult> transition,
    out NpcTaskReferenceOperationResult result)
  {
    NpcTaskReferenceOperationResult operation = default;
    bool edited = _entityRuntime.TryEdit(
      RuntimeHandle,
      (ref NpcTaskStateComponent state) => operation = transition(state));
    result = operation;
    return edited && operation.Accepted;
  }

  private void CommitTaskResult(
    Func<NpcTaskStateComponent, NpcTaskLifecycleResult> transition)
  {
    if (!_entityRuntime.TryEdit(
          RuntimeHandle,
          (ref NpcTaskStateComponent state) => transition(state)))
    {
      throw new InvalidOperationException("The NPC task state could not be committed.");
    }
  }

  internal void CommitDirection(int direction, int directionY)
  {
    if (!_entityRuntime.TryEdit(
          RuntimeHandle,
          (ref NpcDirectionComponent component) =>
          {
            component.Sprite = direction;
            component.Vertical = directionY;
          }))
    {
      throw new InvalidOperationException("The NPC direction component could not be committed.");
    }
  }

  internal void BeginImmediateEffectsTick()
  {
    if (!_entityRuntime.TryEdit(
          RuntimeHandle,
          static (ref NpcImmediateEffectStateComponent component) => component.BeginTick()))
    {
      throw new InvalidOperationException("The NPC immediate effect state could not be reset.");
    }
  }

  internal void BeginEyeOfCthulhuEffectTraceTick()
  {
    _eyeOfCthulhuEffectTrace.Clear();
  }

  internal void RecordEyeOfCthulhuEffect(string effect)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(effect);
    _eyeOfCthulhuEffectTrace.Add(effect);
  }

  internal IReadOnlyList<string> CaptureEyeOfCthulhuEffectTrace()
  {
    return Array.AsReadOnly(_eyeOfCthulhuEffectTrace.ToArray());
  }

  internal void CommitDespawnEncouragement(int ticks)
  {
    if (!_entityRuntime.TryEdit(
          RuntimeHandle,
          (ref NpcImmediateEffectStateComponent component) =>
          {
            component.CommitDespawnEncouragement(ticks);
          }))
    {
      throw new InvalidOperationException("The NPC despawn effect could not be committed.");
    }
  }

  internal void CommitDustEffect(Vector2 position, Vector2 velocity)
  {
    if (!_entityRuntime.TryEdit(
          RuntimeHandle,
          (ref NpcImmediateEffectStateComponent component) =>
          {
            component.CommitDust(position, velocity);
          }))
    {
      throw new InvalidOperationException("The NPC dust effect could not be committed.");
    }
  }

  internal void CommitNetworkUpdateIntent()
  {
    if (!_entityRuntime.TryEdit(
          RuntimeHandle,
          static (ref NpcImmediateEffectStateComponent component) =>
          {
            component.CommitNetworkUpdate();
          }))
    {
      throw new InvalidOperationException("The NPC network update intent could not be committed.");
    }
  }

  internal void CommitJumpEffect(float velocityY)
  {
    if (!_entityRuntime.TryEdit(
          RuntimeHandle,
          (ref NpcImmediateEffectStateComponent component) =>
          {
            component.CommitJump(velocityY);
          }))
    {
      throw new InvalidOperationException("The NPC jump effect could not be committed.");
    }
  }

  internal void CommitDoorOpenEffect(int tileX, int tileY, int direction)
  {
    if (!_entityRuntime.TryEdit(
          RuntimeHandle,
          (ref NpcImmediateEffectStateComponent component) =>
          {
            component.CommitDoorOpen(tileX, tileY, direction);
          }))
    {
      throw new InvalidOperationException("The NPC door effect could not be committed.");
    }
  }

  internal void CommitHomeTeleportEffect(Vector2 position, int candidateOffset)
  {
    if (!_entityRuntime.TryEdit(
          RuntimeHandle,
          (ref NpcImmediateEffectStateComponent component) =>
          {
            component.CommitHomeTeleport(position, candidateOffset);
          }))
    {
      throw new InvalidOperationException(
        "The NPC home teleport effect could not be committed.");
    }
  }

  internal void CommitHomeTeleportFailureEffect(NpcTaskFailureReason reason)
  {
    if (!_entityRuntime.TryEdit(
          RuntimeHandle,
          (ref NpcImmediateEffectStateComponent component) =>
          {
            component.CommitHomeTeleportFailure(reason);
          }))
    {
      throw new InvalidOperationException(
        "The NPC home teleport failure effect could not be committed.");
    }
  }

  internal void CommitHousingRevalidationFailureEffect()
  {
    if (!_entityRuntime.TryEdit(
          RuntimeHandle,
          static (ref NpcImmediateEffectStateComponent component) =>
          {
            component.CommitHousingRevalidationFailure();
          }))
    {
      throw new InvalidOperationException(
        "The NPC housing revalidation failure effect could not be committed.");
    }
  }

  internal void CommitHousingRegistrySynchronizationEffect()
  {
    if (!_entityRuntime.TryEdit(
          RuntimeHandle,
          static (ref NpcImmediateEffectStateComponent component) =>
          {
            component.CommitHousingRegistrySynchronization();
          }))
    {
      throw new InvalidOperationException(
        "The NPC housing registry synchronization effect could not be committed.");
    }
  }

  internal int CaptureTargetSlot()
  {
    if (!_entityRuntime.TryCapture<NpcTargetComponent, int>(
          RuntimeHandle,
          static target => target.LegacyTargetIndex,
          out int targetSlot))
    {
      throw new InvalidOperationException("The NPC target component is unavailable.");
    }

    return targetSlot;
  }

  internal (int CurrentTarget, int PreviousTarget) CaptureTargetIndices()
  {
    if (!_entityRuntime.TryCapture<
          NpcTargetComponent,
          (int CurrentTarget, int PreviousTarget)>(
          RuntimeHandle,
          static target => (target.LegacyTargetIndex, target.PreviousLegacyTargetIndex),
          out (int CurrentTarget, int PreviousTarget) targetIndices))
    {
      throw new InvalidOperationException("The NPC target component is unavailable.");
    }

    return targetIndices;
  }

  internal void CommitTargetSelection(int targetSlot)
  {
    NpcTargetComponent target = targetSlot >= 0
      ? new NpcTargetComponent(NpcTargetKind.Player, legacyTargetIndex: targetSlot)
      : new NpcTargetComponent();
    if (!_entityRuntime.TryReplace(RuntimeHandle, target))
    {
      throw new InvalidOperationException("The NPC target component could not be committed.");
    }
  }

  internal void CommitTargetSelection(
    in NpcTargetSelectionResult result,
    bool requestNetworkUpdate)
  {
    if (!result.ShouldCommit)
    {
      return;
    }

    NpcTargetSelectionResult committedResult = result;
    NpcTargetComponent target = result.HasTarget
      ? new NpcTargetComponent(
          result.TargetKind,
          legacyTargetIndex: result.LegacyTargetIndex,
          previousLegacyTargetIndex: CaptureTargetSlot())
      : new NpcTargetComponent();
    if (!_entityRuntime.TryReplace(RuntimeHandle, target))
    {
      throw new InvalidOperationException("The NPC target component could not be committed.");
    }

    if (!_entityRuntime.TryEdit(
          RuntimeHandle,
          (ref NpcTargetSelectionStateComponent state) => state.Commit(in committedResult)))
    {
      throw new InvalidOperationException("The NPC target selection state could not be committed.");
    }

    CommitDirection(result.Direction, result.DirectionY);
    if (requestNetworkUpdate && result.NetUpdateRequested)
    {
      CommitNetworkUpdateIntent();
    }
  }

  internal void CommitAiState(in NpcAiStateComponent state, int action)
  {
    if (!_entityRuntime.Has<NpcLocalBehaviorStateComponent>(RuntimeHandle) ||
        !_entityRuntime.Has<NpcBehaviorStateComponent>(RuntimeHandle))
    {
      throw new InvalidOperationException("The NPC behavior component set is incomplete.");
    }

    NpcAiStateComponent committedState = state;
    bool localBehaviorCommitted = _entityRuntime.TryEdit<NpcLocalBehaviorStateComponent>(
      RuntimeHandle,
      (ref NpcLocalBehaviorStateComponent component) =>
      {
        component.LocalAiSlots[0] = committedState.LocalAi0;
        component.LocalAiSlots[1] = committedState.LocalAi1;
        component.LocalAiSlots[2] = committedState.LocalAi2;
        component.LocalAiSlots[3] = committedState.LocalAi3;
      });
    if (!localBehaviorCommitted)
    {
      throw new InvalidOperationException("The NPC local AI state could not be committed.");
    }

    bool behaviorCommitted = _entityRuntime.TryEdit<NpcBehaviorStateComponent>(
      RuntimeHandle,
      (ref NpcBehaviorStateComponent component) =>
      {
        component.Action = action;
        component.CommitAiStateSlots(committedState);
      });
    if (!behaviorCommitted)
    {
      throw new InvalidOperationException("The NPC AI state could not be committed.");
    }
  }

  internal void MarkBehaviorUpdated(long tickNumber)
  {
    if (!_entityRuntime.TryEdit<NpcBehaviorStateComponent>(
          RuntimeHandle,
          (ref NpcBehaviorStateComponent component) =>
          {
            component.LastUpdatedTick = tickNumber;
          }))
    {
      throw new InvalidOperationException("The NPC behavior update tick could not be committed.");
    }
  }

  internal bool TryResetOutsidePlayerRangeTicks()
  {
    return _entityRuntime.TryEdit(
      RuntimeHandle,
      static (ref NpcNaturalDespawnStateComponent state) =>
      {
        state.TicksOutsidePlayerRange = 0;
      });
  }

  internal bool TryAdvanceOutsidePlayerRangeTicks(int graceTicks, out bool shouldDespawn)
  {
    bool result = false;
    if (graceTicks <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(graceTicks));
    }

    bool edited = _entityRuntime.TryEdit(
      RuntimeHandle,
      (ref NpcNaturalDespawnStateComponent state) =>
      {
        if (state.TicksOutsidePlayerRange < graceTicks)
        {
          state.TicksOutsidePlayerRange++;
        }

        result = state.TicksOutsidePlayerRange >= graceTicks;
      });
    shouldDespawn = result;
    return edited;
  }

  private static TownHousingRelationStateComponent HydrateHousingRelation(
    WorldNpcState savedState)
  {
    TownRoomTilePoint? homeTile = null;
    if (!savedState.Homeless &&
        savedState.Home.X >= 0 &&
        savedState.Home.Y >= 0)
    {
      homeTile = new TownRoomTilePoint(savedState.Home.X, savedState.Home.Y);
    }

    var housingRelation = new TownHousingRelationStateComponent();
    // The finite simulation definitions use these legacy defaults and expose no housing profile.
    housingRelation.CommitRelation(
      isHomeless: savedState.Homeless,
      homelessDespawn: savedState.HomelessDespawn,
      lookForHomeTimeout: 0,
      homeTile: homeTile,
      housingCategory: 0);
    return housingRelation;
  }

  private (NpcBehaviorSnapshot Behavior, NpcLocalBehaviorSnapshot LocalBehavior)
    CaptureBehaviorSnapshot()
  {
    if (!_entityRuntime.TryCapture<NpcBehaviorStateComponent, NpcBehaviorSnapshot>(
          RuntimeHandle,
          static component => new NpcBehaviorSnapshot(
            component.Action,
            component.AuthoritativeAiSlots[0],
            component.AuthoritativeAiSlots[1],
            component.AuthoritativeAiSlots[2],
            component.AuthoritativeAiSlots[3],
            component.LastUpdatedTick),
          out NpcBehaviorSnapshot behavior) ||
        !_entityRuntime.TryCapture<NpcLocalBehaviorStateComponent, NpcLocalBehaviorSnapshot>(
          RuntimeHandle,
          static component => new NpcLocalBehaviorSnapshot(
            component.LocalAiSlots[0],
            component.LocalAiSlots[1],
            component.LocalAiSlots[2],
            component.LocalAiSlots[3]),
          out NpcLocalBehaviorSnapshot localBehavior))
    {
      throw new InvalidOperationException("The NPC behavior component set is incomplete.");
    }

    return (behavior, localBehavior);
  }

  private NpcHealthSnapshot CaptureHealthSnapshot()
  {
    if (!_entityRuntime.TryCapture<NpcHealthComponent, NpcHealthSnapshot>(
          RuntimeHandle,
          static component => new NpcHealthSnapshot(
            component.CurrentLife,
            component.MaximumLife),
          out NpcHealthSnapshot health))
    {
      throw new InvalidOperationException("The NPC health component is unavailable.");
    }

    return health;
  }
}
