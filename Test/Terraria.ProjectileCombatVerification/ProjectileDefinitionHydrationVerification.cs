using System.Numerics;

using EntityEcs;
using EntityEcs.Components;

using Terraria.Content;
using Terraria.Projectile;
using Terraria.Relationships;
using Terraria.WorldStorage;

internal static class ProjectileDefinitionHydrationVerification
{
  public static void Run()
  {
    ProjectileDefinition firstDefinition = CreateDefinition(12, 11, 6, 1.5f, needsUuid: false);
    ProjectileDefinition secondDefinition = CreateDefinition(
      13,
      8,
      4,
      1.0f,
      needsUuid: true,
      isTrap: true);
    ProjectileDefinition replacementDefinition = CreateDefinition(14, 5, 5, 1.0f, needsUuid: true);
    ProjectileDefinition unknownUuidDefinition = CreateDefinition(15, 5, 5, 1.0f, needsUuid: null);
    var definitions = new ProjectileDefinitionCatalog(
      new[] { firstDefinition, secondDefinition, replacementDefinition, unknownUuidDefinition });
    var slots = new EntitySlotStore<WorldEntityState, ProjectileSlot>(
      static slot => slot.Value,
      static value => new ProjectileSlot(value),
      maximumCapacity: 2);
    var identities = new ProjectileIdentityIndex();
    using var runtime = new EntityRuntime();
    var lifecycle = new ProjectileLifecycleSystem(slots, identities, runtime);
    var context = new ProjectileDefinitionHydrationContext(
      catalogRevision: 7,
      npcCapacity: 200,
      playerCapacity: 8);

    var firstSpawn = new ProjectileSpawnCommand(
      projectileType: 12,
      owner: new ProjectileOwnerReference(EntityReference.None, legacyOwnerSlot: 4),
      center: new Vector2(100, 50),
      velocity: new Vector2(3, -2),
      damage: 24,
      originalDamage: 30,
      knockback: 2.5f,
      ai0: 1.25f,
      ai1: -4.0f,
      ai2: 8.5f);
    Assert(
      lifecycle.TrySpawn(firstSpawn, definitions, context, out ProjectileHandle firstHandle),
      "A catalog-backed projectile spawn should commit.");
    Assert(
      lifecycle.TryGetRuntimeHandle(firstHandle, out RuntimeEntityHandle firstRuntimeHandle),
      "The spawned projectile should be readable through its current handle.");
    ColliderComponent firstCollider = ReadComponent<ColliderComponent>(runtime, firstRuntimeHandle);
    ProjectileKinematicsStateComponent firstKinematics =
      CaptureKinematics(runtime, firstRuntimeHandle);
    ProjectileLifetimeStateComponent firstLifetime =
      ReadComponent<ProjectileLifetimeStateComponent>(runtime, firstRuntimeHandle);
    ProjectileUpdateCadenceComponent firstUpdateCadence =
      ReadComponent<ProjectileUpdateCadenceComponent>(runtime, firstRuntimeHandle);
    ProjectileBehaviorStateComponent firstBehavior =
      ReadComponent<ProjectileBehaviorStateComponent>(runtime, firstRuntimeHandle);
    ProjectileDamagePayloadComponent firstDamage =
      ReadComponent<ProjectileDamagePayloadComponent>(runtime, firstRuntimeHandle);
    ProjectileDefinitionComponent firstDefinitionState =
      ReadComponent<ProjectileDefinitionComponent>(runtime, firstRuntimeHandle);
    ProjectileIdentityComponent firstIdentity =
      ReadComponent<ProjectileIdentityComponent>(runtime, firstRuntimeHandle);
    ProjectileHitImmunityStateComponent firstHitImmunity =
      ReadDetachedHitImmunity(runtime, firstRuntimeHandle);
    ProjectileNetworkStateComponent firstNetwork =
      ReadDetachedNetwork(runtime, firstRuntimeHandle);
    ProjectileTrailCacheComponent firstTrail =
      ReadDetachedTrail(runtime, firstRuntimeHandle);
    ProjectilePresentationStateComponent firstPresentation =
      ReadComponent<ProjectilePresentationStateComponent>(runtime, firstRuntimeHandle);
    AssertEqual(16, (int)firstCollider.Width, "Definition width should be scaled and truncated.");
    AssertEqual(9, (int)firstCollider.Height, "Definition height should be scaled and truncated.");
    AssertEqual(
      new Vector2(92, 45.5f),
      firstKinematics.Position,
      "Spawn center should convert to top-left position after scaled dimensions.");
    AssertEqual(
      new Vector2(3, -2),
      firstKinematics.Velocity,
      "Spawn velocity should override the zero type default.");
    AssertEqual(120, firstLifetime.TimeLeft, "Lifetime should come from the type definition.");
    AssertEqual(2, firstUpdateCadence.ExtraUpdates,
      "Extra updates should hydrate from definition.");
    AssertEqual(1.25f, firstBehavior.GetAi(0), "Spawn AI should override reset AI state.");
    AssertEqual(-4.0f, firstBehavior.GetAi(1), "Spawn AI slots should retain their order.");
    AssertEqual(0.0f, firstBehavior.GetLocalAi(0), "Local AI should reset on initialization.");
    AssertEqual(24, firstDamage.CurrentDamage, "Spawn damage should override definition defaults.");
    AssertEqual(30, firstDamage.OriginalDamage, "Original damage should be retained.");
    Assert(firstDamage.IsRanged, "Ranged content classification should hydrate.");
    AssertEqual(7, firstDefinitionState.CatalogRevision, "The catalog revision should be captured.");
    bool hasMinionCapability = runtime.Has<ProjectileMinionCapabilityComponent>(firstRuntimeHandle);
    ProjectileMinionCapabilityComponent firstMinion = hasMinionCapability
      ? ReadComponent<ProjectileMinionCapabilityComponent>(runtime, firstRuntimeHandle)
      : default;
    ProjectileSentryCapabilityComponent firstSentry =
      ReadComponent<ProjectileSentryCapabilityComponent>(runtime, firstRuntimeHandle);
    bool hasBobberCapability = runtime.Has<ProjectileBobberCapabilityComponent>(firstRuntimeHandle);
    ProjectileBobberCapabilityComponent firstBobber = hasBobberCapability
      ? ReadComponent<ProjectileBobberCapabilityComponent>(runtime, firstRuntimeHandle)
      : default;
    bool hasTrapCapability = runtime.Has<ProjectileTrapCapabilityComponent>(firstRuntimeHandle);
    ProjectileTrapCapabilityComponent firstTrap = hasTrapCapability
      ? ReadComponent<ProjectileTrapCapabilityComponent>(runtime, firstRuntimeHandle)
      : default;
    Assert(!hasMinionCapability && !firstMinion.IsMinion,
      "An absent minion capability should remain distinguishable from a present capability.");
    Assert(runtime.Has<ProjectileSentryCapabilityComponent>(firstRuntimeHandle) && firstSentry.IsSentry,
      "A defined sentry capability should be attached to the entity root.");
    Assert(!hasBobberCapability && !firstBobber.IsBobber,
      "An absent bobber capability should not be attached as an inactive component.");
    Assert(!hasTrapCapability && !firstTrap.IsTrap,
      "An absent trap capability should not be attached as an inactive component.");
    AssertEqual(firstHandle.Slot.Value, firstIdentity.Identity,
      "A local spawn should derive identity from the selected slot.");
    AssertEqual(firstHandle.Slot.Value, firstIdentity.SlotIndex,
      "A local spawn should store its selected slot separately.");
    AssertEqual(-1, firstIdentity.ProjectileUuid,
      "A type without UUID metadata should keep the reset UUID sentinel.");
    AssertEqual(200, firstHitImmunity.LocalNpcImmunityTicks.Length,
      "NPC immunity storage should match the explicit world context.");
    AssertEqual(8, firstNetwork.SectionSyncSkippedForPlayer.Length,
      "Network skip state should match the explicit player capacity.");
    ProjectileNetworkStateComponent detachedNetwork = firstNetwork;
    detachedNetwork.SectionSyncSkippedForPlayer[0] = true;
    Assert(!ReadDetachedNetwork(runtime, firstRuntimeHandle).SectionSyncSkippedForPlayer[0],
      "A network component read must not expose its stored mutable array.");
    AssertEqual(3, firstTrail.OldPositions.Length,
      "Trail arrays should be allocated at the definition length.");
    ProjectileTrailCacheComponent detachedTrail = firstTrail;
    detachedTrail.OldPositions[0] = new Vector2(1, 2);
    detachedTrail.WhipPoints.Add(new Vector2(3, 4));
    ProjectileTrailCacheComponent currentTrail = ReadDetachedTrail(runtime, firstRuntimeHandle);
    AssertEqual(Vector2.Zero, currentTrail.OldPositions[0],
      "A trail component read must not expose its stored mutable arrays.");
    AssertEqual(0, currentTrail.WhipPoints.Count,
      "A trail component read must not expose its stored mutable list.");
    AssertEqual((short)5, firstPresentation.GlowMask,
      "Glow mask should hydrate without changing its sentinel representation.");

    var secondSpawn = new ProjectileSpawnCommand(
      projectileType: 13,
      owner: new ProjectileOwnerReference(EntityReference.None, legacyOwnerSlot: 4),
      center: new Vector2(20, 30),
      velocity: Vector2.Zero,
      damage: 7,
      originalDamage: 7,
      knockback: 0.0f);
    Assert(
      lifecycle.TrySpawn(secondSpawn, definitions, context, out ProjectileHandle secondHandle),
      "A second spawn should allocate the next free slot.");
    AssertEqual(new ProjectileSlot(1), secondHandle.Slot, "A second spawn should use the next free slot.");
    Assert(
      lifecycle.TryGetRuntimeHandle(secondHandle, out RuntimeEntityHandle secondRuntimeHandle),
      "The second spawn should be readable through its handle.");
    ProjectileIdentityComponent secondIdentity =
      ReadComponent<ProjectileIdentityComponent>(runtime, secondRuntimeHandle);
    ProjectileTrapCapabilityComponent secondTrap =
      ReadComponent<ProjectileTrapCapabilityComponent>(runtime, secondRuntimeHandle);
    AssertEqual(secondHandle.Slot.Value, secondIdentity.Identity,
      "A local spawn should derive owner identity from the selected slot.");
    AssertEqual(secondHandle.Slot.Value, secondIdentity.ProjectileUuid,
      "A UUID-requiring type should derive UUID from the selected slot.");
    Assert(runtime.Has<ProjectileTrapCapabilityComponent>(secondRuntimeHandle) && secondTrap.IsTrap,
      "A declared trap capability should be attached to the entity root.");

    var replacementSpawn = new ProjectileSpawnCommand(
      projectileType: 14,
      owner: new ProjectileOwnerReference(EntityReference.None, legacyOwnerSlot: 4),
      center: new Vector2(20, 30),
      velocity: Vector2.Zero,
      damage: 7,
      originalDamage: 7,
      knockback: 0.0f);
    Assert(
      lifecycle.TrySpawn(replacementSpawn, definitions, context, out ProjectileHandle replacementHandle),
      "A valid spawn should replace the oldest eligible full-pool slot.");
    AssertEqual(firstHandle.Slot, replacementHandle.Slot,
      "Full-pool replacement should retain the selected local slot.");
    AssertEqual(firstHandle.Slot.Value, replacementHandle.Slot.Value,
      "Replacement identity should be derived from the reused slot.");
    Assert(!lifecycle.TryGetRuntimeHandle(firstHandle, out _),
      "Replacement should invalidate the old generation.");
    Assert(lifecycle.TryGetRuntimeHandle(secondHandle, out _),
      "Replacement should preserve the non-selected slot.");
    Assert(
      lifecycle.TryGetRuntimeHandle(
        replacementHandle,
        out RuntimeEntityHandle replacementRuntimeHandle),
      "The replacement should be committed as the current slot state.");
    ProjectileNetworkStateComponent replacementNetwork =
      ReadDetachedNetwork(runtime, replacementRuntimeHandle);
    ProjectileIdentityComponent replacementIdentity =
      ReadComponent<ProjectileIdentityComponent>(runtime, replacementRuntimeHandle);
    ProjectileBehaviorStateComponent replacementBehavior =
      ReadComponent<ProjectileBehaviorStateComponent>(runtime, replacementRuntimeHandle);
    ProjectileTrailCacheComponent replacementTrail =
      ReadDetachedTrail(runtime, replacementRuntimeHandle);
    ProjectileHitImmunityStateComponent replacementHitImmunity =
      ReadDetachedHitImmunity(runtime, replacementRuntimeHandle);
    Assert(replacementNetwork.NetworkImportant,
      "Network importance should hydrate from the replacement definition.");
    AssertEqual(replacementHandle.Slot.Value, replacementIdentity.Identity,
      "Replacement state should derive owner identity from the reused slot.");
    AssertEqual(replacementHandle.Slot.Value, replacementIdentity.ProjectileUuid,
      "Replacement state should derive UUID from the reused slot when required.");
    AssertEqual(0.0f, replacementBehavior.GetAi(0),
      "Replacement should not retain prior projectile AI.");
    AssertEqual(0.0f, replacementTrail.OldPositions[0].X,
      "Replacement trail history should be fresh.");
    AssertEqual(0, replacementHitImmunity.PlayerImmunityTicks[0],
      "Replacement player immunity should be fresh.");
    Assert(
      identities.TryGetHandle(
        new OwnerProjectileIdentity(new PlayerSlot(4), replacementHandle.Slot.Value),
        out ProjectileHandle indexedReplacement) &&
      indexedReplacement == replacementHandle,
      "The owner identity index should register the replacement's slot-derived identity.");

    var missingSpawn = new ProjectileSpawnCommand(
      projectileType: 99,
      owner: new ProjectileOwnerReference(EntityReference.None, legacyOwnerSlot: 4),
      center: Vector2.Zero,
      velocity: Vector2.Zero,
      damage: 0,
      originalDamage: 0,
      knockback: 0.0f);
    Assert(
      !lifecycle.TrySpawn(missingSpawn, definitions, context, out _),
      "A missing type definition should reject before slot allocation.");
    AssertEqual(2, slots.ActiveCount,
      "A missing definition must leave the current slot state unchanged.");

    var unknownUuidSpawn = new ProjectileSpawnCommand(
      projectileType: 15,
      owner: new ProjectileOwnerReference(EntityReference.None, legacyOwnerSlot: 4),
      center: Vector2.Zero,
      velocity: Vector2.Zero,
      damage: 0,
      originalDamage: 0,
      knockback: 0.0f);
    Assert(!lifecycle.TrySpawn(unknownUuidSpawn, definitions, context, out _),
      "A definition without known UUID policy should reject before allocation or replacement.");
    AssertEqual(2, slots.ActiveCount,
      "An unknown UUID policy must not modify occupied projectile slots.");
    Assert(lifecycle.TryGetRuntimeHandle(secondHandle, out _) &&
      lifecycle.TryGetRuntimeHandle(replacementHandle, out _),
      "An unknown UUID policy must preserve existing projectile handles.");
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

  private static ProjectileNetworkStateComponent ReadDetachedNetwork(
    EntityRuntime runtime,
    RuntimeEntityHandle runtimeHandle)
  {
    ProjectileNetworkStateComponent component = ReadComponent<ProjectileNetworkStateComponent>(
      runtime,
      runtimeHandle);
    return new ProjectileNetworkStateComponent
    {
      NetworkImportant = component.NetworkImportant,
      PrimaryUpdatePending = component.PrimaryUpdatePending,
      SecondaryUpdatePending = component.SecondaryUpdatePending,
      NetSpam = component.NetSpam,
      SectionSyncSkippedForPlayer = component.SectionSyncSkippedForPlayer is null
        ? null!
        : (bool[])component.SectionSyncSkippedForPlayer.Clone(),
      SendRequested = component.SendRequested,
    };
  }

  private static ProjectileHitImmunityStateComponent ReadDetachedHitImmunity(
    EntityRuntime runtime,
    RuntimeEntityHandle runtimeHandle)
  {
    ProjectileHitImmunityStateComponent component = ReadComponent<ProjectileHitImmunityStateComponent>(
      runtime,
      runtimeHandle);
    return new ProjectileHitImmunityStateComponent
    {
      LocalNpcImmunityTicks = component.LocalNpcImmunityTicks is null
        ? null!
        : (int[])component.LocalNpcImmunityTicks.Clone(),
      PlayerImmunityTicks = component.PlayerImmunityTicks is null
        ? null!
        : (int[])component.PlayerImmunityTicks.Clone(),
      RestrikeDelayTicks = component.RestrikeDelayTicks,
    };
  }

  private static ProjectileTrailCacheComponent ReadDetachedTrail(
    EntityRuntime runtime,
    RuntimeEntityHandle runtimeHandle)
  {
    ProjectileTrailCacheComponent component = ReadComponent<ProjectileTrailCacheComponent>(
      runtime,
      runtimeHandle);
    return new ProjectileTrailCacheComponent
    {
      OldPositions = component.OldPositions is null
        ? null!
        : (Vector2[])component.OldPositions.Clone(),
      OldRotations = component.OldRotations is null
        ? null!
        : (float[])component.OldRotations.Clone(),
      OldSpriteDirections = component.OldSpriteDirections is null
        ? null!
        : (int[])component.OldSpriteDirections.Clone(),
      WhipPoints = component.WhipPoints is null
        ? null!
        : new System.Collections.Generic.List<Vector2>(component.WhipPoints),
    };
  }

  private static ProjectileDefinition CreateDefinition(
    int typeId,
    int width,
    int height,
    float scale,
    bool? needsUuid,
    bool isTrap = false)
  {
    return new ProjectileDefinition(
      new ProjectileIdentityDefinition(typeId, null, needsUuid),
      new ProjectileGeometryDefinition(width, height, scale, true, false)
      {
        OwnerHitCheckDistance = 48.0f,
      },
      new ProjectileBehaviorDefinition(1, 2, 120),
      new ProjectileCombatDefinition(2, 0.5f, true, false)
      {
        Ranged = true,
        Trap = isTrap,
      },
      new ProjectilePenetrationDefinition(3, 5, true)
      {
        UsesLocalNpcImmunity = true,
        UsesIdStaticNpcImmunity = true,
        LocalNpcHitCooldown = 4,
        IdStaticNpcHitCooldown = 10,
      },
      new ProjectileCapabilitiesDefinition(true, false, false, true)
      {
        UsesOwnerLight = true,
        NoEnchantments = true,
      },
      new ProjectilePresentationDefinition(3, 0.25f, true)
      {
        Alpha = 40,
        DrawLayer = 2,
        GlowMaskId = 5,
        TrailCacheLength = 3,
      })
    {
      Network = new ProjectileNetworkDefinition(NetworkImportant: typeId == 14),
    };
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
