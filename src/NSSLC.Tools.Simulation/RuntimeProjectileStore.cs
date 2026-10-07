using System.Collections.Immutable;
using System.Collections.Generic;
using System.Numerics;
using EntityEcs;
using EntityEcs.Components;
using NSSLC.WorldGeneration;
using Terraria.Content;
using Terraria.Npc;
using Terraria.NonAuthoritative.Persistence;
using Terraria.NonAuthoritative.Simulation;
using Terraria.Projectile;
using Terraria.Relationships;
using Terraria.SpatialSimulation;
using Terraria.WorldGeneration.Adapters;
using LegacyVector2 = NSSLC.WorldGeneration.Geometry.Vector2;

namespace Terraria.NonAuthoritative.SimulationHost;

internal sealed class RuntimeProjectileStore
{
  private readonly LoadedWorldSession _session;
  private readonly ContentCatalog _catalog;
  private readonly RuntimeNpcStore _npcs;
  private readonly RuntimePlayerStore _players;
  private readonly RuntimeWorldItemStore _worldItems;
  private readonly ProjectileStaticNpcImmunityRegistryComponent _staticNpcImmunityRegistry;
  private readonly ProjectileLifecycleSystem _lifecycle;
  private readonly EntityRuntime _runtime;
  private readonly ProjectileSimulationTickPhase _simulationPhase;
  private readonly ProjectileDefinitionHydrationContext _hydrationContext;

  public RuntimeProjectileStore(
    LoadedWorldSession session,
    ContentCatalog catalog,
    RuntimeNpcStore npcs,
    RuntimePlayerStore players,
    RuntimeWorldItemStore worldItems,
    ProjectileStaticNpcImmunityRegistryComponent staticNpcImmunityRegistry)
  {
    _session = session ?? throw new ArgumentNullException(nameof(session));
    _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
    _npcs = npcs ?? throw new ArgumentNullException(nameof(npcs));
    _players = players ?? throw new ArgumentNullException(nameof(players));
    _worldItems = worldItems ?? throw new ArgumentNullException(nameof(worldItems));
    _staticNpcImmunityRegistry = staticNpcImmunityRegistry ??
      throw new ArgumentNullException(nameof(staticNpcImmunityRegistry));
    _runtime = session.Storage.ProjectileRuntime;
    _lifecycle = new ProjectileLifecycleSystem(session.Storage);
    var coordinator = new ProjectileTickCoordinator(_lifecycle);
    _simulationPhase = new ProjectileSimulationTickPhase(coordinator, CreateTickAdapter);
    _hydrationContext = new ProjectileDefinitionHydrationContext(
      checked((int)catalog.CatalogRevision),
      npcCapacity: RuntimeNpcStore.MaximumNpcCapacity,
      playerCapacity: byte.MaxValue);
  }

  public int ActiveCount => _session.Storage.Projectiles.ActiveCount;

  public int AcceptedNpcHitCount { get; private set; }

  public int TileCollisionCount { get; private set; }

  public ProjectileTickResult LastTickResult => _simulationPhase.LastTickResult;

  public IWorldSimulationTickPhase SimulationTickPhase => _simulationPhase;

  public bool TrySpawnArrow(
    int projectileType,
    int ownerSlot,
    Vector2 center,
    Vector2 velocity,
    int damage,
    float knockback)
  {
    if (!_players.TryGetEntityReference(ownerSlot, out EntityReference ownerReference))
    {
      return false;
    }

    var command = new ProjectileSpawnCommand(
      projectileType,
      new ProjectileOwnerReference(ownerReference, ownerSlot),
      center,
      velocity,
      damage,
      originalDamage: damage,
      knockback);
    return _lifecycle.TrySpawn(command, _catalog.Projectiles, _hydrationContext, out _);
  }

  private IProjectileTickAdapter CreateTickAdapter(WorldSimulationTickContext context)
  {
    ArgumentNullException.ThrowIfNull(context);
    if (!ReferenceEquals(context.Session, _session))
    {
      throw new InvalidOperationException(
        "The projectile adapter received a tick context for another world session.");
    }

    if (!context.WorldSnapshot.IsCommitted || !context.WorldSnapshot.Descriptor.HasValue)
    {
      throw new InvalidOperationException(
        "The projectile adapter requires committed world dimensions.");
    }

    var descriptor = context.WorldSnapshot.Descriptor.Value;
    return new RuntimeProjectileTickAdapter(
      _runtime,
      _lifecycle,
      _npcs,
      _players,
      _worldItems,
      _staticNpcImmunityRegistry,
      context.TickNumber,
      descriptor.SizeX,
      descriptor.SizeY,
      () => AcceptedNpcHitCount++,
      () => TileCollisionCount++);
  }

  private sealed class RuntimeProjectileTickAdapter : IProjectileTickAdapter
  {
    private const float WaterMovementScale = 0.5f;
    private const float HoneyMovementScale = 0.25f;
    private const float ShimmerMovementScale = 0.375f;

    private readonly EntityRuntime _runtime;
    private readonly ProjectileLifecycleSystem _lifecycle;
    private readonly RuntimeNpcStore _npcs;
    private readonly RuntimePlayerStore _players;
    private readonly RuntimeWorldItemStore _worldItems;
    private readonly ProjectileStaticNpcImmunityRegistryComponent _staticNpcImmunityRegistry;
    private readonly long _tickNumber;
    private readonly int _maxTilesX;
    private readonly int _maxTilesY;
    private readonly Action _onAcceptedNpcHit;
    private readonly Action _onTileCollision;

    public RuntimeProjectileTickAdapter(
      EntityRuntime runtime,
      ProjectileLifecycleSystem lifecycle,
      RuntimeNpcStore npcs,
      RuntimePlayerStore players,
      RuntimeWorldItemStore worldItems,
      ProjectileStaticNpcImmunityRegistryComponent staticNpcImmunityRegistry,
      long tickNumber,
      int maxTilesX,
      int maxTilesY,
      Action onAcceptedNpcHit,
      Action onTileCollision)
    {
      _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
      _lifecycle = lifecycle;
      _npcs = npcs;
      _players = players;
      _worldItems = worldItems;
      _staticNpcImmunityRegistry = staticNpcImmunityRegistry;
      _tickNumber = tickNumber;
      _maxTilesX = maxTilesX;
      _maxTilesY = maxTilesY;
      _onAcceptedNpcHit = onAcceptedNpcHit ?? throw new ArgumentNullException(nameof(onAcceptedNpcHit));
      _onTileCollision = onTileCollision ?? throw new ArgumentNullException(nameof(onTileCollision));
    }

    public void PreUpdateAllProjectiles()
    {
    }

    public bool TryGetOwnerMovementDelta(
      ProjectileTickContext context,
      EntityReference ownerReference,
      out Vector2 movementDelta)
    {
      return _players.TryGetOwnerMovementDelta(
        ownerReference,
        _tickNumber,
        out movementDelta);
    }

    public void UpdateProjectile(ProjectileTickContext context)
    {
      UpdateProjectileState(context);
    }

    public ProjectileTickSubstepResult UpdateProjectileStep(ProjectileTickContext context)
    {
      UpdateProjectileState(context);
      return ProjectileTickSubstepResult.Completed;
    }

    public void PostUpdateAllProjectiles()
    {
    }

    private void UpdateProjectileState(ProjectileTickContext context)
    {
      RuntimeEntityHandle runtimeHandle = context.RuntimeHandle;
      ProjectileDefinitionComponent definition = ReadComponent<ProjectileDefinitionComponent>(
        runtimeHandle);
      if (definition.ProjectileType != 1 || definition.BehaviorKey != 1)
      {
        throw new NotSupportedException(
          $"Projectile type {definition.ProjectileType} with AI style " +
          $"{definition.BehaviorKey} is outside the simulation support manifest.");
      }

      ProjectileBehaviorStateComponent aiBehavior =
        ReadComponent<ProjectileBehaviorStateComponent>(runtimeHandle);
      LocationComponent aiLocation = ReadComponent<LocationComponent>(runtimeHandle);
      VelocityComponent aiVelocity = ReadComponent<VelocityComponent>(runtimeHandle);
      ProjectileTrajectoryStateComponent trajectory =
        ReadComponent<ProjectileTrajectoryStateComponent>(runtimeHandle);
      ProjectileMotionAndAiSystem.AdvanceOrdinaryArrowAi(
        in definition,
        ref aiBehavior,
        ref aiLocation,
        ref aiVelocity,
        ref trajectory);
      if (!_runtime.TryEditComponents<
        ProjectileBehaviorStateComponent,
        LocationComponent,
        VelocityComponent>(
        runtimeHandle,
        (ref ProjectileBehaviorStateComponent behavior,
          ref LocationComponent location,
          ref VelocityComponent velocity) =>
        {
          behavior = aiBehavior;
          location = aiLocation;
          velocity = aiVelocity;
        }) ||
        !_runtime.TryEdit<ProjectileTrajectoryStateComponent>(
          runtimeHandle,
          (ref ProjectileTrajectoryStateComponent current) => current = trajectory))
      {
        throw new InvalidOperationException(
          "Ordinary-arrow AI could not edit its required Projectile components.");
      }

      ProjectileKinematicsStateComponent kinematics = CaptureKinematics(runtimeHandle);
      ProjectileCollisionPolicyComponent collision =
        ReadComponent<ProjectileCollisionPolicyComponent>(runtimeHandle);
      ColliderComponent collider = ReadComponent<ColliderComponent>(runtimeHandle);
      int width = (int)collider.Width;
      int height = (int)collider.Height;
      LegacyVector2 projectilePosition = new(
        kinematics.Position.X,
        kinematics.Position.Y);
      bool wet = false;
      bool lavaWet = false;
      bool honeyWet = false;
      bool shimmerWet = false;
      if (!collision.IgnoreWater)
      {
        if (!TryCaptureWetCollisionTiles(
          projectilePosition,
          width,
          height,
          out ImmutableArray<SpatialTileSnapshot> wetCollisionTiles) ||
          !ProjectileWetCollisionQuery.TryEvaluate(
            new Vector2(projectilePosition.X, projectilePosition.Y),
            width,
            height,
            wetCollisionTiles,
            _maxTilesX,
            _maxTilesY,
            out wet,
            out lavaWet,
            out honeyWet,
            out shimmerWet))
        {
          throw new InvalidOperationException(
            "The projectile liquid collision query could not capture complete tile facts.");
        }

        // This headless host has no audiovisual owner for the returned splash request.
        ProjectileWetStateComponent wetState =
          ReadComponent<ProjectileWetStateComponent>(runtimeHandle);
        _ = ProjectileWetStateSystem.Advance(
          ref wetState,
          definition.ProjectileType,
          wet,
          lavaWet,
          honeyWet,
          shimmerWet);
        if (!_runtime.TryEdit<ProjectileWetStateComponent>(
          runtimeHandle,
          (ref ProjectileWetStateComponent current) => current = wetState))
        {
          throw new InvalidOperationException(
            "Projectile liquid state could not be committed.");
        }

        wet = wetState.Wet;
        honeyWet = wetState.HoneyWet;
        shimmerWet = wetState.ShimmerWet;
      }

      (
        LegacyVector2 resolvedPosition,
        LegacyVector2 resolvedVelocity,
        LegacyVector2 wetVelocity,
        bool collidedWithTile) =
        ResolveArrowTileMovement(
          projectilePosition,
          new LegacyVector2(kinematics.Velocity.X, kinematics.Velocity.Y),
          width,
          height,
          wet,
          honeyWet,
          shimmerWet);

      LegacyVector2 committedPosition = resolvedPosition;
      if (collidedWithTile)
      {
        // Type 1's default collision response advances once before Kill.
        committedPosition += resolvedVelocity;
      }

      committedPosition += wet ? wetVelocity : resolvedVelocity;
      Vector2 position = new(committedPosition.X, committedPosition.Y);
      Vector2 velocity = new(resolvedVelocity.X, resolvedVelocity.Y);
      if (!_runtime.TryEditComponents<
        LocationComponent,
        VelocityComponent,
        DirectionComponent>(
        runtimeHandle,
        (ref LocationComponent location,
          ref VelocityComponent currentVelocity,
          ref DirectionComponent direction) =>
          ProjectileMotionAndAiSystem.CommitOrdinaryArrowStep(
            in definition,
            ref location,
            ref currentVelocity,
            ref direction,
            position,
            velocity)))
      {
        throw new InvalidOperationException(
          "Ordinary-arrow movement could not edit its required Projectile components.");
      }
      if (collidedWithTile)
      {
        _onTileCollision();
        if (!_lifecycle.TryTerminate(context.Handle, ProjectileEndReason.DestroyedByCollision))
        {
          throw new InvalidOperationException("A tile-hit projectile could not be terminated.");
        }

        return;
      }

      ProjectileSourceMetadataComponent source =
        ReadComponent<ProjectileSourceMetadataComponent>(runtimeHandle);
      ProjectileDispositionStateComponent disposition =
        ReadComponent<ProjectileDispositionStateComponent>(runtimeHandle);
      ProjectileDamagePayloadComponent damage =
        ReadComponent<ProjectileDamagePayloadComponent>(runtimeHandle);
      ProjectileUpdateCadenceComponent cadence =
        ReadComponent<ProjectileUpdateCadenceComponent>(runtimeHandle);
      ProjectileIdentityComponent identity =
        ReadComponent<ProjectileIdentityComponent>(runtimeHandle);
      if (ProjectileMotionAndAiSystem.ApplyMagicQuiverExtraUpdate(
        in source,
        in disposition,
        in damage,
        ref cadence,
        _players.HasMagicQuiver(identity.OwnerReference)))
      {
        if (!_runtime.TryEdit<ProjectileUpdateCadenceComponent>(
          runtimeHandle,
          (ref ProjectileUpdateCadenceComponent currentCadence) =>
            currentCadence = cadence))
        {
          throw new InvalidOperationException(
            "Magic Quiver could not commit Projectile update cadence.");
        }
      }
      TryHitNpc(_tickNumber, context);
    }

    private ProjectileKinematicsStateComponent CaptureKinematics(
      RuntimeEntityHandle runtimeHandle)
    {
      LocationComponent location = ReadComponent<LocationComponent>(runtimeHandle);
      VelocityComponent velocity = ReadComponent<VelocityComponent>(runtimeHandle);
      return new ProjectileKinematicsStateComponent(
        new Vector2(location.X, location.Y),
        new Vector2(velocity.X, velocity.Y));
    }

    private TComponent ReadComponent<TComponent>(RuntimeEntityHandle runtimeHandle)
      where TComponent : struct
    {
      TComponent value = default;
      if (!_runtime.TryInspect(
        runtimeHandle,
        (in TComponent component) => value = component))
      {
        throw new InvalidOperationException(
          $"The projectile root does not expose {typeof(TComponent).Name}.");
      }

      return value;
    }

    private bool TryCaptureWetCollisionTiles(
      LegacyVector2 position,
      int width,
      int height,
      out ImmutableArray<SpatialTileSnapshot> tiles)
    {
      tiles = ImmutableArray<SpatialTileSnapshot>.Empty;
      if (_maxTilesX <= 0 ||
          _maxTilesY <= ProjectileWetCollisionQuery.WorldSurfaceBufferTiles ||
          width <= 0 ||
          height <= 0 ||
          !float.IsFinite(position.X) ||
          !float.IsFinite(position.Y) ||
          !float.IsFinite(position.X + width) ||
          !float.IsFinite(position.Y + height))
      {
        return false;
      }

      int firstTileX = Math.Clamp(
        (int)(position.X / SpatialTileSnapshot.TileSize) - 1,
        0,
        _maxTilesX - 1);
      int endTileX = Math.Clamp(
        (int)((position.X + width) / SpatialTileSnapshot.TileSize) + 2,
        0,
        _maxTilesX - 1);
      int firstTileY = Math.Clamp(
        (int)(position.Y / SpatialTileSnapshot.TileSize) - 1,
        0,
        _maxTilesY - ProjectileWetCollisionQuery.WorldSurfaceBufferTiles);
      int endTileY = Math.Clamp(
        (int)((position.Y + height) / SpatialTileSnapshot.TileSize) + 2,
        0,
        _maxTilesY - ProjectileWetCollisionQuery.WorldSurfaceBufferTiles);

      if (firstTileX >= endTileX || firstTileY >= endTileY)
      {
        return true;
      }

      int captureTopTile = Math.Max(0, firstTileY - 1);
      return LegacySpatialTileSnapshotAdapter.TryCapture(
        firstTileX,
        captureTopTile,
        endTileX - firstTileX,
        endTileY - captureTopTile,
        out tiles);
    }

    private static (
      LegacyVector2 Position,
      LegacyVector2 Velocity,
      LegacyVector2 WetVelocity,
      bool CollidedWithTile)
      ResolveArrowTileMovement(
        LegacyVector2 projectilePosition,
        LegacyVector2 velocity,
        int width,
        int height,
        bool wet,
        bool honeyWet,
        bool shimmerWet)
    {
      LegacyVector2 lastVelocity = velocity;
      LegacyVector2 probePosition = projectilePosition;
      LegacyVector2 wetVelocity = velocity;
      bool collidedWithTile = false;
      int stepSize = Math.Clamp(Math.Min(width, height), 3, 16);

      if (wet)
      {
        velocity = Collision.TileCollision(
          probePosition,
          velocity,
          width,
          height,
          fallThrough: true,
          fall2: true,
          gravDir: 1,
          ignoreDoors: false);
        float wetVelocityScale = shimmerWet
          ? ShimmerMovementScale
          : honeyWet ? HoneyMovementScale : WaterMovementScale;
        wetVelocity = velocity * wetVelocityScale;
        if (velocity.X != lastVelocity.X)
        {
          wetVelocity.X = velocity.X;
        }

        if (velocity.Y != lastVelocity.Y)
        {
          wetVelocity.Y = velocity.Y;
        }
      }
      else if (velocity.Length() > stepSize)
      {
        LegacyVector2 fullMovement = Collision.TileCollision(
          probePosition,
          velocity,
          width,
          height,
          fallThrough: true,
          fall2: true,
          gravDir: 1,
          ignoreDoors: false);
        float remainingDistance = velocity.Length();
        LegacyVector2 direction = velocity.SafeNormalize(LegacyVector2.Zero);
        if (fullMovement.Y == 0f)
        {
          direction.Y = 0f;
        }

        LegacyVector2 totalVelocity = LegacyVector2.Zero;
        for (int step = 0; remainingDistance > 0f && step < 300; step++)
        {
          float distance = Math.Min(remainingDistance, stepSize);
          remainingDistance -= distance;
          LegacyVector2 stepVelocity = Collision.TileCollision(
            probePosition,
            direction * distance,
            width,
            height,
            fallThrough: true,
            fall2: true,
            gravDir: 1,
            ignoreDoors: false,
            ignoreAetheriumPlatforms: false,
            hoik: true);
          probePosition += stepVelocity;
          velocity = stepVelocity;

          var slopeResult = Collision.SlopeCollision(
            probePosition,
            velocity,
            width,
            height,
            gravity: 0f,
            fall: true);
          LegacyVector2 slopePosition = new(slopeResult.X, slopeResult.Y);
          LegacyVector2 positionOffset = projectilePosition - probePosition;
          LegacyVector2 slopeVelocity = new(slopeResult.Z, slopeResult.W);
          collidedWithTile |= slopePosition != probePosition || slopeVelocity != velocity;
          probePosition = slopePosition;
          projectilePosition = probePosition + positionOffset;
          velocity = slopeVelocity;
          totalVelocity += velocity;
        }

        velocity = totalVelocity;
        if (Math.Abs(velocity.X - lastVelocity.X) < 0.0001f)
        {
          velocity.X = lastVelocity.X;
        }

        if (Math.Abs(velocity.Y - lastVelocity.Y) < 0.0001f)
        {
          velocity.Y = lastVelocity.Y;
        }
      }
      else
      {
        velocity = Collision.TileCollision(
          probePosition,
          velocity,
          width,
          height,
          fallThrough: true,
          fall2: true,
          gravDir: 1,
          ignoreDoors: false);
      }

      var finalSlopeResult = Collision.SlopeCollision(
        probePosition,
        velocity,
        width,
        height,
        gravity: 0f,
        fall: true);
      LegacyVector2 finalSlopePosition = new(finalSlopeResult.X, finalSlopeResult.Y);
      LegacyVector2 finalPositionOffset = projectilePosition - probePosition;
      LegacyVector2 finalSlopeVelocity = new(finalSlopeResult.Z, finalSlopeResult.W);
      collidedWithTile |=
        finalSlopePosition != probePosition || finalSlopeVelocity != velocity;
      projectilePosition = finalSlopePosition + finalPositionOffset;
      velocity = finalSlopeVelocity;
      collidedWithTile |= velocity != lastVelocity;

      return (projectilePosition, velocity, wetVelocity, collidedWithTile);
    }

    private void TryHitNpc(long tickNumber, ProjectileTickContext context)
    {
      RuntimeEntityHandle runtimeHandle = context.RuntimeHandle;
      ProjectileDefinitionComponent definition =
        ReadComponent<ProjectileDefinitionComponent>(runtimeHandle);
      ProjectileBehaviorStateComponent behavior =
        ReadComponent<ProjectileBehaviorStateComponent>(runtimeHandle);
      ProjectileKinematicsStateComponent kinematics = CaptureKinematics(runtimeHandle);
      ProjectilePenetrationStateComponent penetration =
        ReadComponent<ProjectilePenetrationStateComponent>(runtimeHandle);
      ProjectileAnimationStateComponent animation =
        ReadComponent<ProjectileAnimationStateComponent>(runtimeHandle);
      ProjectileDamagePayloadComponent damage =
        ReadComponent<ProjectileDamagePayloadComponent>(runtimeHandle);
      ProjectileHitImmunityPolicyComponent immunityPolicy =
        ReadComponent<ProjectileHitImmunityPolicyComponent>(runtimeHandle);
      ProjectileSourceMetadataComponent source =
        ReadComponent<ProjectileSourceMetadataComponent>(runtimeHandle);
      ProjectileIdentityComponent identity =
        ReadComponent<ProjectileIdentityComponent>(runtimeHandle);
      ProjectileDispositionStateComponent disposition =
        ReadComponent<ProjectileDispositionStateComponent>(runtimeHandle);
      ProjectileCollisionPolicyComponent collisionPolicy =
        ReadComponent<ProjectileCollisionPolicyComponent>(runtimeHandle);
      ProjectileTrapCapabilityComponent trap =
        _runtime.Has<ProjectileTrapCapabilityComponent>(runtimeHandle)
          ? ReadComponent<ProjectileTrapCapabilityComponent>(runtimeHandle)
          : default;
      var damageInput = new ProjectileDamageCandidateInput(
        definition,
        behavior,
        kinematics,
        penetration,
        animation,
        damage,
        immunityPolicy,
        source,
        identity,
        disposition,
        collisionPolicy,
        trap);
      if (!ProjectileDamageGateQuery.CanEnterDamagePath(
        damageInput.Definition,
        damageInput.Behavior,
        damageInput.Penetration,
        damageInput.Kinematics,
        damageInput.Animation,
        isProjectilePet: false))
      {
        return;
      }

      if (!_players.TryResolveEntityReference(
            damageInput.Identity.OwnerReference,
            out RuntimePlayerEntity? owner) ||
          owner is null ||
          owner.Slot != damageInput.Identity.OwnerSlot)
      {
        return;
      }

      LocationComponent ownerLocation = owner.Location;
      ColliderComponent ownerCollider = owner.Collider;
      Vector2 ownerCenter = new(
        ownerLocation.X + ownerCollider.OffsetX + ownerCollider.Width * 0.5f,
        ownerLocation.Y + ownerCollider.OffsetY + ownerCollider.Height * 0.5f);
      LocationComponent location = ReadComponent<LocationComponent>(runtimeHandle);
      VelocityComponent velocity = ReadComponent<VelocityComponent>(runtimeHandle);
      ColliderComponent collider = ReadComponent<ColliderComponent>(runtimeHandle);
      float geometryScale = ReadComponent<ProjectileScaleComponent>(runtimeHandle).Scale;
      ProjectileTrajectoryStateComponent trajectory =
        ReadComponent<ProjectileTrajectoryStateComponent>(runtimeHandle);
      ProjectileTrailCacheComponent trail =
        _runtime.Has<ProjectileTrailCacheComponent>(runtimeHandle)
          ? CloneTrail(ReadComponent<ProjectileTrailCacheComponent>(runtimeHandle))
          : default;
      DirectionComponent direction = ReadComponent<DirectionComponent>(runtimeHandle);
      ProjectileCollisionGeometryInput collisionInput = new(
        definition,
        behavior,
        location,
        velocity,
        collider,
        geometryScale,
        penetration,
        trajectory,
        trail,
        direction);
      ProjectileDamageHitbox projectileHitbox = ProjectileDamageHitboxSystem.Build(
        in collisionInput,
        isPhaseblade: false,
        out ProjectileBehaviorStateComponent hitboxBehavior);
      if (hitboxBehavior.LocalAi0 != collisionInput.Behavior.LocalAi0)
      {
        if (!_runtime.TryEdit<ProjectileBehaviorStateComponent>(
          runtimeHandle,
          (ref ProjectileBehaviorStateComponent behavior) =>
            behavior.LocalAi0 = hitboxBehavior.LocalAi0))
        {
          throw new InvalidOperationException(
            "Damage hitbox construction could not commit its behavior transition.");
        }

        collisionInput = collisionInput with { Behavior = hitboxBehavior };
      }

      bool resolvesLocalNpcImmunity = immunityPolicy.UsesLocalNpcImmunity &&
        damageInput.Definition.ProjectileType is not (626 or 627 or 628);
      ProjectileHitImmunityStateComponent localImmunity = default;
      if (resolvesLocalNpcImmunity &&
        !_runtime.TryInspect<ProjectileHitImmunityStateComponent>(
          runtimeHandle,
          (in ProjectileHitImmunityStateComponent state) =>
            localImmunity = state))
      {
        throw new InvalidOperationException(
          "NPC candidate evaluation could not inspect projectile local immunity.");
      }

      foreach (RuntimeNpcProjectileTargetSnapshot npc in _npcs.CreateProjectileTargetSnapshot())
      {
        var target = new ProjectileNpcTargetSnapshot(
          npc.TypeId,
          Active: npc.IsActive,
          Friendly: npc.Friendly,
          DontTakeDamage: false,
          DontTakeDamageFromHostiles: false,
          AiStyle: npc.AiStyle,
          Ai2: npc.Ai2,
          TrapImmune: false,
          Immortal: false,
          NoTileCollide: false,
          IsZappingJellyfish: false,
          OwnerImmune: false,
          NpcSlot: npc.Slot.Value);
        var gate = new ProjectileNpcDamageGateContext(
          IsProjectilePet: false,
          IsDamageOwnerLocalPlayer: true,
          HasLocalNpcImmunity: resolvesLocalNpcImmunity &&
            ProjectileHitImmunitySystem.IsLocalNpcImmune(
              in localImmunity,
              npc.Slot.Value),
          HasStaticNpcImmunity: false,
          OwnerMeleeHitCooldownAllowsTarget: true,
          OwnerDamageRulesAllowTarget: !npc.IsTownNpc,
          OwnerHitCheckAllowsTarget: true,
          OwnerCanDamageGuide: false,
          OwnerCanDamageClothier: false);
        var targetHitbox = new ProjectileCollisionTargetRectangle(
          (int)npc.Hitbox.Position.X,
          (int)npc.Hitbox.Position.Y,
          npc.Hitbox.Width,
          npc.Hitbox.Height);
        ProjectileNpcDamageCollisionResult collision =
          ProjectileDamageCollisionQuery.EvaluateNpc(
            in damageInput,
            in collisionInput,
            in target,
            in gate,
            _staticNpcImmunityRegistry,
            unchecked((uint)_tickNumber),
            in projectileHitbox,
            in targetHitbox,
            collisionInput.Direction.Horizontal,
            ownerCenter,
            canHitLineFromProjectileHitboxCenterToTargetCenter: true,
            canHitFromProjectileCenterToTargetCenter: true);
        if (collision.GeometryResult == ProjectileCollidingGeometryResult.Unsupported)
        {
          throw new NotSupportedException(
            $"Projectile collision geometry for type {damageInput.Definition.ProjectileType} " +
            "is unsupported by the simulation host.");
        }

        if (!collision.CollisionFound)
        {
          continue;
        }

        if (!_npcs.TryApplyProjectileHit(
          npc,
          damageInput.Damage.CurrentDamage,
          owner.Slot,
          tickNumber,
          damageInput.Damage.Knockback,
          collisionInput.Direction.Horizontal,
          out NpcStrikeResult strike,
          out RuntimeNpcStore.NpcDeathDropSnapshot deathDrop))
        {
          continue;
        }

        if (strike.CombatResult.Applied)
        {
          _onAcceptedNpcHit();
          if (!IsCurrentProjectile(context))
          {
            return;
          }

          penetration = damageInput.Penetration;
          int projectileType = damageInput.Definition.ProjectileType;
          if (!strike.CombatResult.DeathTransitioned)
          {
            if (!_runtime.TryEdit<ProjectileHitImmunityStateComponent>(
              runtimeHandle,
              (ref ProjectileHitImmunityStateComponent state) =>
                ProjectileHitImmunitySystem.RecordAcceptedNpcHit(
                  ref state,
                  immunityPolicy,
                  npc.Slot.Value)))
            {
              throw new InvalidOperationException(
                "An accepted NPC hit could not edit projectile local immunity.");
            }

            ProjectileStaticNpcImmunitySystem.RecordAcceptedNpcHit(
              _staticNpcImmunityRegistry,
              in immunityPolicy,
              in penetration,
              projectileType,
              npc.Slot.Value,
              unchecked((uint)_tickNumber));
          }

          if (strike.CombatResult.DeathTransitioned)
          {
            _worldItems.SpawnNpcDeathDrops(
              deathDrop.NetId,
              deathDrop.Position);
            if (!IsCurrentProjectile(context))
            {
              return;
            }
          }

          bool penetrationCommitted = false;
          if (!_runtime.TryEdit<ProjectilePenetrationStateComponent>(
                runtimeHandle,
                (ref ProjectilePenetrationStateComponent state) =>
                  penetrationCommitted =
                    ProjectilePenetrationSystem.CommitAcceptedHit(ref state)) ||
              !penetrationCommitted)
          {
            throw new InvalidOperationException(
              "An accepted NPC hit could not consume projectile penetration.");
          }

          return;
        }
      }
    }

    private bool IsCurrentProjectile(ProjectileTickContext context)
    {
      return _lifecycle.TryGetRuntimeHandle(
          context.Handle,
          out RuntimeEntityHandle currentRuntimeHandle) &&
        currentRuntimeHandle == context.RuntimeHandle;
    }

    private static ProjectileTrailCacheComponent CloneTrail(
      ProjectileTrailCacheComponent source)
    {
      return new ProjectileTrailCacheComponent
      {
        OldPositions = (Vector2[])source.OldPositions.Clone(),
        OldRotations = (float[])source.OldRotations.Clone(),
        OldSpriteDirections = (int[])source.OldSpriteDirections.Clone(),
        WhipPoints = new List<Vector2>(source.WhipPoints),
      };
    }
  }
}
