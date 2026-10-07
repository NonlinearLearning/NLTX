using System;
using System.Numerics;

using EntityEcs;
using EntityEcs.Components;
using Terraria.Content;
using Terraria.Projectile;
using Terraria.Relationships;
using Terraria.SpatialSimulation;
using Terraria.WorldStorage;

internal static class ProjectileDamageCandidateVerification
{
  public static void Run()
  {
    var definitions = new ProjectileDefinitionCatalog(new[]
    {
      CreateDefinition(),
      CreateStaticImmunityDefinition(),
    });
    var slots = new EntitySlotStore<WorldEntityState, ProjectileSlot>(
      static slot => slot.Value,
      static value => new ProjectileSlot(value),
      maximumCapacity: 3);
    using var runtime = new EntityRuntime();
    var lifecycle = new ProjectileLifecycleSystem(
      slots,
      new ProjectileIdentityIndex(),
      runtime);
    var hydrationContext = new ProjectileDefinitionHydrationContext(
      catalogRevision: 1,
      npcCapacity: 4,
      playerCapacity: 4);
    Assert(
      lifecycle.TrySpawn(
        new ProjectileSpawnCommand(
          projectileType: 31,
          owner: new ProjectileOwnerReference(EntityReference.None, legacyOwnerSlot: 1),
          center: new Vector2(20, 20),
          velocity: Vector2.Zero,
          damage: 10,
          originalDamage: 10,
          knockback: 0.0f),
        definitions,
        hydrationContext,
        out ProjectileHandle handle),
      "The candidate-query projectile should hydrate before evaluation.");
    Assert(lifecycle.TryGetRuntimeHandle(handle, out RuntimeEntityHandle runtimeHandle),
      "The candidate-query projectile should resolve from its live runtime handle.");

    var npcTarget = new ProjectileNpcTargetSnapshot(
      Type: 1,
      Active: true,
      Friendly: false,
      DontTakeDamage: false,
      DontTakeDamageFromHostiles: false,
      AiStyle: 0,
      Ai2: 0.0f,
      TrapImmune: false,
      Immortal: false,
      NoTileCollide: false,
      IsZappingJellyfish: false,
      OwnerImmune: false,
      NpcSlot: 2);
    var npcContext = new ProjectileNpcDamageGateContext(
      IsProjectilePet: false,
      IsDamageOwnerLocalPlayer: true,
      HasLocalNpcImmunity: false,
      HasStaticNpcImmunity: false,
      OwnerMeleeHitCooldownAllowsTarget: true,
      OwnerDamageRulesAllowTarget: true,
      OwnerHitCheckAllowsTarget: true,
      OwnerCanDamageGuide: false,
      OwnerCanDamageClothier: false);
    var staticNpcImmunity = new ProjectileStaticNpcImmunityRegistryComponent(
      projectileTypeCapacity: 64,
      npcCapacity: 4);
    ProjectileDamageCandidateInput input = CaptureDamageInput(runtime, runtimeHandle);
    ProjectileNpcDamageCandidateStatus npcStatus =
      ProjectileNpcDamageCandidateQuery.Evaluate(
        in input,
        in npcTarget,
        in npcContext,
        staticNpcImmunity,
        gameUpdateCount: 1);
    AssertEqual(
      ProjectileNpcDamageCandidateStatus.ReadyForCollisionTest,
      npcStatus,
      "The detached capability input should reach the NPC collision test.");
    AssertEqual(
      npcStatus,
      ProjectileNpcDamageCandidateQuery.Evaluate(
        in input,
        in npcTarget,
        in npcContext,
        staticNpcImmunity,
        gameUpdateCount: 1),
      "The typed candidate input should preserve NPC candidate results across repeated evaluation.");

    ProjectileCollisionGeometryInput collisionInput = CaptureCollisionInput(runtime, runtimeHandle);
    ProjectileDamageHitbox projectileHitbox = ProjectileDamageHitboxSystem.Build(
      in collisionInput,
      isPhaseblade: false,
      out _);
    var targetHitbox = new ProjectileCollisionTargetRectangle(
      projectileHitbox.X,
      projectileHitbox.Y,
      projectileHitbox.Width,
      projectileHitbox.Height);
    ProjectileNpcDamageCollisionResult typedCollision =
      ProjectileDamageCollisionQuery.EvaluateNpc(
        in input,
        in collisionInput,
        in npcTarget,
        in npcContext,
        staticNpcImmunity,
        gameUpdateCount: 1,
        in projectileHitbox,
        in targetHitbox,
        collisionInput.Direction.Horizontal,
        ownerMountedCenter: Vector2.Zero,
        canHitLineFromProjectileHitboxCenterToTargetCenter: true,
        canHitFromProjectileCenterToTargetCenter: true);
    AssertEqual(
      ProjectileCollidingGeometryResult.Collision,
      typedCollision.GeometryResult,
      "Detached candidate and geometry inputs should reach the selected collision branch.");

    Vector2 capturedPosition = collisionInput.Kinematics.Position;
    int capturedWidth = collisionInput.Width;
    Assert(runtime.TryEdit<LocationComponent>(
      runtimeHandle,
      (ref LocationComponent location) => location.X += 100.0f),
      "The spatial fixture should permit a live Location update.");
    Assert(runtime.TryEdit<ColliderComponent>(
      runtimeHandle,
      (ref ColliderComponent collider) => collider = new ColliderComponent(12.0f, 8.0f)),
      "The spatial fixture should permit a live Collider update.");
    AssertEqual(
      capturedPosition,
      collisionInput.Kinematics.Position,
      "A geometry input must remain an immutable position snapshot after the live Location changes.");
    AssertEqual(
      capturedWidth,
      collisionInput.Width,
      "A geometry input must retain its detached collider width after the live Collider changes.");
    ProjectileCollisionGeometryInput movedCollisionInput =
      CaptureCollisionInput(runtime, runtimeHandle);
    Assert(
      movedCollisionInput.Kinematics.Position != capturedPosition &&
      movedCollisionInput.Width == 12,
      "A later geometry capture must observe the shared Location and Collider values.");

    ProjectileHitImmunityStateComponent localImmunityState =
      ReadComponent<ProjectileHitImmunityStateComponent>(runtime, runtimeHandle);
    ProjectileHitImmunitySystem.SetLocalNpcImmunity(
      ref localImmunityState,
      npcTarget.NpcSlot,
      2);
    Assert(runtime.TryEdit<ProjectileHitImmunityStateComponent>(
      runtimeHandle,
      (ref ProjectileHitImmunityStateComponent state) => state = localImmunityState),
      "The authoritative runtime should commit the local-immunity fixture state.");
    input = CaptureDamageInput(runtime, runtimeHandle);
    ProjectileNpcDamageGateContext immuneNpcContext = npcContext with
    {
      HasLocalNpcImmunity = ProjectileHitImmunitySystem.IsLocalNpcImmune(
        in localImmunityState,
        npcTarget.NpcSlot),
    };
    Assert(
      immuneNpcContext.HasLocalNpcImmunity,
      "The typed local-immunity fixture must expose its live cooldown through the gate context.");
    ProjectileNpcDamageCollisionResult typedImmuneCollision =
      ProjectileDamageCollisionQuery.EvaluateNpc(
        in input,
        in collisionInput,
        in npcTarget,
        in immuneNpcContext,
        staticNpcImmunity,
        gameUpdateCount: 1,
        in projectileHitbox,
        in targetHitbox,
        collisionInput.Direction.Horizontal,
        ownerMountedCenter: Vector2.Zero,
        canHitLineFromProjectileHitboxCenterToTargetCenter: true,
        canHitFromProjectileCenterToTargetCenter: true);
    AssertEqual(
      ProjectileNpcDamageCandidateStatus.ProjectileImmunity,
      typedImmuneCollision.CandidateStatus,
      "The typed collision path should honor the explicit target immunity input.");
    AssertEqual(
      ProjectileCollidingGeometryResult.NotEvaluated,
      typedImmuneCollision.GeometryResult,
      "An immune candidate should stop before geometry evaluation.");
    AssertEqual(
      ProjectileNpcDamageCandidateStatus.ProjectileImmunity,
      ProjectileNpcDamageCandidateQuery.Evaluate(
        in input,
        in npcTarget,
        in immuneNpcContext,
        staticNpcImmunity,
        gameUpdateCount: 1),
      "NPC candidate evaluation should use the target-scoped immunity input.");
    AssertEqual(
      ProjectileNpcDamageCandidateStatus.ProjectileImmunity,
      ProjectileNpcDamageCandidateQuery.Evaluate(
        in input,
        in npcTarget,
        in immuneNpcContext,
        staticNpcImmunity,
        gameUpdateCount: 1),
      "The typed candidate query should resolve the same local-immunity result.");

    ProjectileNpcTargetSnapshot invalidNpcSlot = npcTarget with { NpcSlot = 10 };
    ProjectileNpcDamageGateContext nonLocalNpcContext = npcContext with
    {
      IsDamageOwnerLocalPlayer = false,
    };
    AssertEqual(
      ProjectileNpcDamageCandidateStatus.NonLocalOwner,
      ProjectileNpcDamageCandidateQuery.Evaluate(
        in input,
        in invalidNpcSlot,
        in nonLocalNpcContext,
        staticNpcImmunity,
        gameUpdateCount: 1),
      "NPC gate rejection should preserve the owner gate before target immunity lookup.");

    Assert(lifecycle.TrySpawn(
      new ProjectileSpawnCommand(
        projectileType: 31,
        owner: new ProjectileOwnerReference(EntityReference.None, legacyOwnerSlot: 1),
        center: new Vector2(24, 24),
        velocity: Vector2.Zero,
        damage: 10,
        originalDamage: 10,
        knockback: 0.0f),
      definitions,
      hydrationContext,
      out ProjectileHandle sameDefinitionHandle),
      "A second projectile with the same definition should hydrate independently.");
    Assert(lifecycle.TryGetRuntimeHandle(
      sameDefinitionHandle,
      out RuntimeEntityHandle sameDefinitionRuntimeHandle),
      "The same-definition projectile should resolve from its runtime handle.");
    ProjectileDamageCandidateInput sameDefinitionInput = CaptureDamageInput(
      runtime,
      sameDefinitionRuntimeHandle);
    AssertEqual(
      ProjectileNpcDamageCandidateStatus.ReadyForCollisionTest,
      ProjectileNpcDamageCandidateQuery.Evaluate(
        in sameDefinitionInput,
        in npcTarget,
        in npcContext,
        staticNpcImmunity,
        gameUpdateCount: 1),
      "Same-definition projectile instances must keep independent local-immunity state.");

    var pvpTarget = new ProjectilePvpTargetSnapshot(
      new PlayerSlot(2),
      Active: true,
      Dead: false,
      Immune: false,
      Hostile: true,
      Team: 2);
    var pvpContext = new ProjectilePvpDamageGateContext(
      IsProjectilePet: false,
      IsDamageOwnerLocalPlayer: true,
      LocalDamageOwnerIsHostile: true,
      LocalDamageOwnerTeam: 1,
      OwnerHitCheckAllowsTarget: true);
    input = CaptureDamageInput(runtime, runtimeHandle);
    AssertEqual(
      ProjectilePvpDamageCandidateStatus.ReadyForCollisionTest,
      ProjectilePvpDamageCandidateQuery.Evaluate(in input, in pvpTarget, in pvpContext),
      "The detached capability input should reach the PVP collision test.");

    Assert(
      runtime.TryEdit<ProjectileHitImmunityStateComponent>(
        runtimeHandle,
        (ref ProjectileHitImmunityStateComponent immunity) =>
          ProjectileHitImmunitySystem.SetPlayerImmunity(
            ref immunity,
            pvpTarget.Slot.Value,
            2)),
      "The authoritative runtime should commit the player-immunity fixture state.");
    input = CaptureDamageInput(runtime, runtimeHandle);
    ProjectileHitImmunityStateComponent playerImmunity =
      ReadComponent<ProjectileHitImmunityStateComponent>(runtime, runtimeHandle);
    ProjectilePvpDamageGateContext immunePvpContext = pvpContext with
    {
      ProjectileIsImmune = ProjectileHitImmunitySystem.IsPlayerImmune(
        in playerImmunity,
        pvpTarget.Slot.Value),
    };
    AssertEqual(
      ProjectilePvpDamageCandidateStatus.ProjectileImmunity,
      ProjectilePvpDamageCandidateQuery.Evaluate(in input, in pvpTarget, in immunePvpContext),
      "PVP candidate evaluation should use the target-scoped immunity input.");
    var invalidPlayerTarget = new ProjectilePvpTargetSnapshot(
      new PlayerSlot(255),
      Active: true,
      Dead: false,
      Immune: false,
      Hostile: true,
      Team: 2);
    AssertEqual(
      ProjectilePvpDamageCandidateStatus.InvalidTargetSlot,
      ProjectilePvpDamageCandidateQuery.Evaluate(
        in input,
        in invalidPlayerTarget,
        in pvpContext),
      "PVP gate rejection should preserve slot validation before immunity lookup.");

    Assert(lifecycle.TrySpawn(
      new ProjectileSpawnCommand(
        projectileType: 32,
        owner: new ProjectileOwnerReference(EntityReference.None, legacyOwnerSlot: 1),
        center: new Vector2(30, 30),
        velocity: Vector2.Zero,
        damage: 10,
        originalDamage: 10,
        knockback: 0.0f),
      definitions,
      hydrationContext,
      out ProjectileHandle staticHandle),
      "The static-immunity projectile should hydrate before registry evaluation.");
    Assert(lifecycle.TryGetRuntimeHandle(staticHandle, out RuntimeEntityHandle staticRuntimeHandle),
      "The static-immunity projectile should resolve from its live runtime handle.");
    ProjectileHitImmunityPolicyComponent staticImmunityPolicy =
      ReadComponent<ProjectileHitImmunityPolicyComponent>(runtime, staticRuntimeHandle);
    ProjectilePenetrationStateComponent staticPenetration =
      ReadComponent<ProjectilePenetrationStateComponent>(runtime, staticRuntimeHandle);
    int staticProjectileType =
      ReadComponent<ProjectileDefinitionComponent>(runtime, staticRuntimeHandle).ProjectileType;
    Assert(ProjectileStaticNpcImmunitySystem.RecordAcceptedNpcHit(
      staticNpcImmunity,
      in staticImmunityPolicy,
      in staticPenetration,
      staticProjectileType,
      npcTarget.NpcSlot,
      gameUpdateCount: 10),
      "A multi-penetration static-immunity projectile should record its accepted-hit cooldown.");
    ProjectileDamageCandidateInput staticInput =
      CaptureDamageInput(runtime, staticRuntimeHandle);
    AssertEqual(
      ProjectileNpcDamageCandidateStatus.NonLocalOwner,
      ProjectileNpcDamageCandidateQuery.Evaluate(
        in staticInput,
        in invalidNpcSlot,
        in nonLocalNpcContext,
        null!,
        gameUpdateCount: 13),
      "A rejected gate must return before reading a static-immunity registry or target immunity.");
    AssertEqual(
      ProjectileNpcDamageCandidateStatus.ProjectileImmunity,
      ProjectileNpcDamageCandidateQuery.Evaluate(
        in staticInput,
        in npcTarget,
        in npcContext,
        staticNpcImmunity,
        gameUpdateCount: 13),
      "The static registry should block the target before its absolute expiry.");
    AssertEqual(
      ProjectileNpcDamageCandidateStatus.ReadyForCollisionTest,
      ProjectileNpcDamageCandidateQuery.Evaluate(
        in staticInput,
        in npcTarget,
        in npcContext,
        staticNpcImmunity,
        gameUpdateCount: 14),
      "The static registry should allow the target when expiry equals the update count.");

    ProjectileStaticNpcImmunitySystem.ClearStaticNpcSlot(
      staticNpcImmunity,
      npcTarget.NpcSlot);
    var disabledStaticPolicy = new ProjectileHitImmunityPolicyComponent(
      usesStaticNpcImmunity: false,
      staticNpcCooldownTicks: 4);
    Assert(!ProjectileStaticNpcImmunitySystem.RecordAcceptedNpcHit(
      staticNpcImmunity,
      in disabledStaticPolicy,
      in staticPenetration,
      staticProjectileType,
      npcTarget.NpcSlot,
      gameUpdateCount: 20),
      "A disabled static-immunity policy should not record an expiry.");
    Assert(!ProjectileStaticNpcImmunitySystem.IsNpcImmune(
      staticNpcImmunity,
      staticProjectileType,
      npcTarget.NpcSlot,
      gameUpdateCount: 20),
      "A disabled static-immunity policy should leave the registry unchanged.");

    var singleHitPenetration = new ProjectilePenetrationStateComponent(
      remainingHits: 1,
      maximumHits: 1,
      stopsDealingDamageWhenDepleted: true);
    var singleHitStaticPolicy = new ProjectileHitImmunityPolicyComponent(
      usesStaticNpcImmunity: true,
      staticNpcCooldownTicks: 4);
    Assert(!ProjectileStaticNpcImmunitySystem.RecordAcceptedNpcHit(
      staticNpcImmunity,
      in singleHitStaticPolicy,
      in singleHitPenetration,
      staticProjectileType,
      npcTarget.NpcSlot,
      gameUpdateCount: 20),
      "A one-penetration hit should skip static immunity unless configured otherwise.");
    Assert(!ProjectileStaticNpcImmunitySystem.IsNpcImmune(
      staticNpcImmunity,
      staticProjectileType,
      npcTarget.NpcSlot,
      gameUpdateCount: 20),
      "A skipped one-penetration cooldown should leave the registry unchanged.");

    var applySingleHitStaticPolicy = new ProjectileHitImmunityPolicyComponent(
      usesStaticNpcImmunity: true,
      staticNpcCooldownTicks: 4,
      appliesOnSingleHit: true);
    Assert(ProjectileStaticNpcImmunitySystem.RecordAcceptedNpcHit(
      staticNpcImmunity,
      in applySingleHitStaticPolicy,
      in singleHitPenetration,
      staticProjectileType,
      npcTarget.NpcSlot,
      gameUpdateCount: 20),
      "A one-penetration hit should record static immunity when configured.");
    Assert(ProjectileStaticNpcImmunitySystem.IsNpcImmune(
      staticNpcImmunity,
      staticProjectileType,
      npcTarget.NpcSlot,
      gameUpdateCount: 23),
      "The configured single-hit cooldown should block before its expiry.");
    Assert(!ProjectileStaticNpcImmunitySystem.IsNpcImmune(
      staticNpcImmunity,
      staticProjectileType,
      npcTarget.NpcSlot,
      gameUpdateCount: 24),
      "The configured single-hit cooldown should expire at equality.");

    ProjectileCollisionGeometryInput ai19Input = new(
      new ProjectileDefinitionComponent(105, 19, true, false, 0),
      new ProjectileBehaviorStateComponent(),
      new LocationComponent(10.0f, 10.0f),
      new VelocityComponent(1.0f, 0.0f),
      new ColliderComponent(8.0f, 8.0f),
      1.0f,
      new ProjectilePenetrationStateComponent(),
      new ProjectileTrajectoryStateComponent(0.0f, 1, 1.0f, 0, 0.0f),
      default,
      new DirectionComponent(1));
    ProjectileAi19ExtensionSnapshot ai19BeforeExtension =
      ProjectileAi19ExtensionQuery.Evaluate(
        in ai19Input,
        new ProjectileAi19OwnerSnapshot(
          HasResults: true,
          ItemAnimation: 0,
          ItemAnimationMax: 20,
          MeleeSpeed: 1.0f));
    Assert(
      ai19BeforeExtension.HasResults && !ai19BeforeExtension.HasExtensionHitbox,
      "AI19 must retain its pre-extension animation boundary.");
    ProjectileAi19ExtensionSnapshot ai19AtExtensionStart =
      ProjectileAi19ExtensionQuery.Evaluate(
        in ai19Input,
        new ProjectileAi19OwnerSnapshot(
          HasResults: true,
          ItemAnimation: 20 / 3,
          ItemAnimationMax: 20,
          MeleeSpeed: 1.0f));
    Assert(
      ai19AtExtensionStart.HasResults && ai19AtExtensionStart.HasExtensionHitbox,
      "AI19 must produce its extension hitbox at the animation boundary.");

    ProjectileCollisionGeometryInput ai137Input = new(
      new ProjectileDefinitionComponent(700, 137, true, false, 0),
      new ProjectileBehaviorStateComponent(),
      new LocationComponent(0.0f, 0.0f),
      new VelocityComponent(0.0f, 0.0f),
      new ColliderComponent(8.0f, 8.0f),
      1.0f,
      new ProjectilePenetrationStateComponent(),
      new ProjectileTrajectoryStateComponent(0.0f, 1, 1.0f, 0, 0.0f),
      default,
      new DirectionComponent(1));
    var solidTargetTile = new SpatialTileSnapshot(
      0,
      0,
      exists: true,
      isActive: true,
      blocksMovement: true,
      isSolid: true,
      isSolidTop: false,
      isHalfBrick: false,
      slope: 0,
      liquidAmount: 0,
      isInactive: false);
    var ai137Environment = new SpatialCollisionSnapshot(
      revision: 1,
      subject: new SpatialGeometrySnapshot(
        revision: 1,
        position: Vector2.Zero,
        size: new Vector2(8.0f, 8.0f)),
      subjectEntityId: null,
      entities: Array.Empty<SpatialEntitySnapshot>(),
      tiles: new[] { solidTargetTile });
    ProjectileAi137VisibilitySnapshot ai137Visibility =
      ProjectileAi137VisibilityQuery.Evaluate(
        in ai137Input,
        new ProjectileCollisionTargetRectangle(0, 0, 8, 8),
        ai137Environment,
        maxTilesX: 2,
        maxTilesY: 42);
    Assert(
      ai137Visibility.HasTargetCenterResult && !ai137Visibility.TargetCenterCanHit,
      "AI137 must preserve the solid-target visibility boundary.");
    AssertThrows<ArgumentOutOfRangeException>(
      () => ProjectileAi137VisibilityQuery.Evaluate(
        in ai137Input,
        new ProjectileCollisionTargetRectangle(0, 0, 8, 8),
        ai137Environment,
        maxTilesX: 1,
        maxTilesY: 42),
      "AI137 must reject a world width that cannot address tile coordinates.");
  }

  private static ProjectileDamageCandidateInput CaptureDamageInput(
    EntityRuntime runtime,
    RuntimeEntityHandle runtimeHandle)
  {
    ProjectileTrapCapabilityComponent trap = runtime.Has<ProjectileTrapCapabilityComponent>(runtimeHandle)
      ? ReadComponent<ProjectileTrapCapabilityComponent>(runtime, runtimeHandle)
      : default;
    return new ProjectileDamageCandidateInput(
      ReadComponent<ProjectileDefinitionComponent>(runtime, runtimeHandle),
      ReadComponent<ProjectileBehaviorStateComponent>(runtime, runtimeHandle),
      CaptureKinematics(runtime, runtimeHandle),
      ReadComponent<ProjectilePenetrationStateComponent>(runtime, runtimeHandle),
      ReadComponent<ProjectileAnimationStateComponent>(runtime, runtimeHandle),
      ReadComponent<ProjectileDamagePayloadComponent>(runtime, runtimeHandle),
      ReadComponent<ProjectileHitImmunityPolicyComponent>(runtime, runtimeHandle),
      ReadComponent<ProjectileSourceMetadataComponent>(runtime, runtimeHandle),
      ReadComponent<ProjectileIdentityComponent>(runtime, runtimeHandle),
      ReadComponent<ProjectileDispositionStateComponent>(runtime, runtimeHandle),
      ReadComponent<ProjectileCollisionPolicyComponent>(runtime, runtimeHandle),
      trap);
  }

  private static ProjectileCollisionGeometryInput CaptureCollisionInput(
    EntityRuntime runtime,
    RuntimeEntityHandle runtimeHandle)
  {
    ProjectileDefinitionComponent definition =
      ReadComponent<ProjectileDefinitionComponent>(runtime, runtimeHandle);
    LocationComponent location = ReadComponent<LocationComponent>(runtime, runtimeHandle);
    VelocityComponent velocity = ReadComponent<VelocityComponent>(runtime, runtimeHandle);
    ColliderComponent collider = ReadComponent<ColliderComponent>(runtime, runtimeHandle);
    float scale = ReadComponent<ProjectileScaleComponent>(runtime, runtimeHandle).Scale;
    return new ProjectileCollisionGeometryInput(
      definition,
      ReadComponent<ProjectileBehaviorStateComponent>(runtime, runtimeHandle),
      location,
      velocity,
      collider,
      scale,
      ReadComponent<ProjectilePenetrationStateComponent>(runtime, runtimeHandle),
      ReadComponent<ProjectileTrajectoryStateComponent>(runtime, runtimeHandle),
      default,
      ReadComponent<DirectionComponent>(runtime, runtimeHandle));
  }

  private static ProjectileKinematicsStateComponent CaptureKinematics(
    EntityRuntime runtime,
    RuntimeEntityHandle runtimeHandle)
  {
    LocationComponent location = ReadComponent<LocationComponent>(runtime, runtimeHandle);
    VelocityComponent velocity = ReadComponent<VelocityComponent>(runtime, runtimeHandle);
    return new ProjectileKinematicsStateComponent(
      new Vector2(location.X, location.Y),
      new Vector2(velocity.X, velocity.Y));
  }

  private static TComponent ReadComponent<TComponent>(
    EntityRuntime runtime,
    RuntimeEntityHandle runtimeHandle)
    where TComponent : notnull
  {
    TComponent component = default!;
    Assert(
      runtime.TryInspect<TComponent>(
        runtimeHandle,
        (in TComponent value) => component = value),
      $"The runtime fixture should expose {typeof(TComponent).Name}.");
    return component;
  }

  private static void AssertThrows<TException>(
    Action action,
    string message)
    where TException : Exception
  {
    try
    {
      action();
    }
    catch (TException)
    {
      return;
    }

    throw new InvalidOperationException(message);
  }

  private static ProjectileDefinition CreateDefinition()
  {
    return new ProjectileDefinition(
      new ProjectileIdentityDefinition(31, null, NeedsUuid: false),
      new ProjectileGeometryDefinition(8, 8, 1.0f, true, false),
      new ProjectileBehaviorDefinition(1, 0, 30),
      new ProjectileCombatDefinition(1, 0.0f, true, false),
      new ProjectilePenetrationDefinition(1, 1, true)
      {
        UsesLocalNpcImmunity = true,
      },
      new ProjectileCapabilitiesDefinition(false, false, false, false),
      new ProjectilePresentationDefinition(1, 0.0f, false));
  }

  private static ProjectileDefinition CreateStaticImmunityDefinition()
  {
    return new ProjectileDefinition(
      new ProjectileIdentityDefinition(32, null, NeedsUuid: false),
      new ProjectileGeometryDefinition(8, 8, 1.0f, true, false),
      new ProjectileBehaviorDefinition(1, 0, 30),
      new ProjectileCombatDefinition(1, 0.0f, true, false),
      new ProjectilePenetrationDefinition(2, 2, true)
      {
        UsesIdStaticNpcImmunity = true,
        IdStaticNpcHitCooldown = 4,
      },
      new ProjectileCapabilitiesDefinition(false, false, false, false),
      new ProjectilePresentationDefinition(1, 0.0f, false));
  }

  private static void Assert(bool condition, string message)
  {
    if (!condition)
    {
      throw new InvalidOperationException(message);
    }
  }

  private static void AssertEqual<T>(T expected, T actual, string message)
  {
    if (!EqualityComparer<T>.Default.Equals(expected, actual))
    {
      throw new InvalidOperationException(
        $"{message} Expected '{expected}', actual '{actual}'.");
    }
  }
}
