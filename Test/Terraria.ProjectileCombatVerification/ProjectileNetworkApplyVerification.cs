using System;
using System.Numerics;

using EntityEcs;
using EntityEcs.Components;

using Terraria.Content;
using Terraria.Projectile;
using Terraria.Relationships;
using Terraria.WorldStorage;

internal static class ProjectileNetworkApplyVerification
{
  public static void Run()
  {
    var definitions = new ProjectileDefinitionCatalog(new[]
    {
      CreateDefinition(625, true),
      CreateDefinition(626, true),
      CreateDefinition(627, false),
      CreateDefinition(628, null),
    });
    var slots = new EntitySlotStore<WorldEntityState, ProjectileSlot>(
      static slot => slot.Value,
      static value => new ProjectileSlot(value),
      maximumCapacity: 2);
    var identities = new ProjectileIdentityIndex();
    using var runtime = new EntityRuntime();
    var lifecycle = new ProjectileLifecycleSystem(slots, identities, runtime);
    var context = new ProjectileDefinitionHydrationContext(
      catalogRevision: 9,
      npcCapacity: 64,
      playerCapacity: 8);

    var first = new ProjectileNetworkApplyCommand(
      ownerSlot: 7,
      identity: 42,
      projectileType: 625,
      position: new Vector2(12, 24),
      velocity: new Vector2(2, -3),
      damage: 18,
      originalDamage: 21,
      knockback: 1.5f,
      ai0: 4,
      ai1: 5,
      ai2: 6,
      projectileUuid: 900);
    Assert(
      lifecycle.TryApplyNetwork(first, definitions, context, out ProjectileHandle firstHandle),
      "A valid packet state should allocate a local slot.");
    Assert(
      lifecycle.TryGetRuntimeHandle(firstHandle, out RuntimeEntityHandle firstRuntimeHandle),
      "The packet-created state should be readable.");
    Assert(
      lifecycle.TryGetEntityReference(firstHandle, out EntityReference firstReference),
      "The packet-created runtime root should publish a projectile reference.");
    ProjectileIdentityComponent firstIdentity =
      ReadComponent<ProjectileIdentityComponent>(runtime, firstRuntimeHandle);
    ProjectileDamagePayloadComponent firstDamage =
      ReadComponent<ProjectileDamagePayloadComponent>(runtime, firstRuntimeHandle);
    AssertEqual(0, firstHandle.Slot.Value, "The first packet should use the first free slot.");
    AssertEqual(42, firstIdentity.Identity,
      "Network identity must remain the packet identity.");
    AssertEqual(0, firstIdentity.SlotIndex,
      "The local slot must remain a separate field.");
    AssertEqual(900, firstIdentity.ProjectileUuid,
      "A supplied packet UUID must be retained.");
    AssertEqual(18, firstDamage.CurrentDamage,
      "Packet damage should be applied during hydration.");

    var update = new ProjectileNetworkApplyCommand(
      ownerSlot: 7,
      identity: 42,
      projectileType: 625,
      position: new Vector2(30, 40),
      velocity: new Vector2(-1, 2),
      damage: 7,
      originalDamage: 8,
      knockback: 0.5f,
      ai0: 9,
      projectileUuid: -1);
    Assert(
      lifecycle.TryApplyNetwork(update, definitions, context, out ProjectileHandle updatedHandle),
      "A same-type packet should update the active instance.");
    AssertEqual(firstHandle, updatedHandle,
      "A same-type packet must not replace the active generation.");
    Assert(
      lifecycle.TryGetRuntimeHandle(updatedHandle, out RuntimeEntityHandle updatedRuntimeHandle),
      "The updated packet state should be readable.");
    Assert(
      lifecycle.TryGetEntityReference(updatedHandle, out EntityReference updatedReference),
      "The updated runtime root should publish a projectile reference.");
    ProjectileKinematicsStateComponent updatedKinematics =
      CaptureKinematics(runtime, updatedRuntimeHandle);
    ProjectileBehaviorStateComponent updatedBehavior =
      ReadComponent<ProjectileBehaviorStateComponent>(runtime, updatedRuntimeHandle);
    ProjectileIdentityComponent updatedIdentity =
      ReadComponent<ProjectileIdentityComponent>(runtime, updatedRuntimeHandle);
    AssertEqual(firstReference, updatedReference,
      "A same-type packet must retain the projectile entity root.");
    AssertEqual(30.0f, updatedKinematics.Position.X,
      "Same-type packets should update position in place.");
    AssertEqual(9.0f, updatedBehavior.GetAi(0),
      "Same-type packets should update AI in place.");
    AssertEqual(900, updatedIdentity.ProjectileUuid,
      "An omitted UUID must not clear the existing UUID.");

    var replacement = new ProjectileNetworkApplyCommand(
      ownerSlot: 7,
      identity: 42,
      projectileType: 626,
      position: Vector2.Zero,
      velocity: Vector2.One,
      damage: 2,
      originalDamage: 2,
      knockback: 0,
      projectileUuid: 901);
    Assert(
      lifecycle.TryApplyNetwork(replacement, definitions, context, out ProjectileHandle replacementHandle),
      "A changed packet type should replace the existing local state.");
    AssertEqual(firstHandle.Slot, replacementHandle.Slot,
      "A type change should retain the selected local slot.");
    AssertEqual(firstHandle.Generation + 1, replacementHandle.Generation,
      "A type change should advance the local generation.");
    Assert(!lifecycle.TryGetRuntimeHandle(firstHandle, out _),
      "The replaced packet generation must become stale.");
    Assert(
      lifecycle.TryGetRuntimeHandle(
        replacementHandle,
        out RuntimeEntityHandle replacementRuntimeHandle),
      "The replacement state should be readable.");
    Assert(
      lifecycle.TryGetEntityReference(replacementHandle, out EntityReference replacementReference),
      "The replacement runtime root should publish a projectile reference.");
    ProjectileIdentityComponent replacementIdentity =
      ReadComponent<ProjectileIdentityComponent>(runtime, replacementRuntimeHandle);
    Assert(replacementReference.EntityId != firstReference.EntityId,
      "A changed packet type must publish a new projectile entity root.");
    AssertEqual(42, replacementIdentity.Identity,
      "Replacement must still preserve the packet identity.");
    AssertEqual(901, replacementIdentity.ProjectileUuid,
      "Replacement must retain the packet UUID.");

    var malformed = new ProjectileNetworkApplyCommand(
      ownerSlot: 1,
      identity: 3,
      projectileType: 627,
      position: Vector2.Zero,
      velocity: Vector2.Zero,
      damage: 0,
      originalDamage: 0,
      knockback: 0,
      projectileUuid: 3);
    Assert(!lifecycle.TryApplyNetwork(malformed, definitions, context, out _),
      "A UUID on a type without UUID policy must be rejected before commit.");
    AssertEqual(1, slots.ActiveCount,
      "Malformed network state must not allocate a slot.");

    var unknown = new ProjectileNetworkApplyCommand(
      ownerSlot: 1,
      identity: 4,
      projectileType: 628,
      position: Vector2.Zero,
      velocity: Vector2.Zero,
      damage: 0,
      originalDamage: 0,
      knockback: 0);
    Assert(!lifecycle.TryApplyNetwork(unknown, definitions, context, out _),
      "An unknown UUID policy must reject network apply.");
    AssertEqual(1, slots.ActiveCount,
      "Unknown network policy must not modify active slots.");

    var missingIdentityTermination = new ProjectileNetworkTerminateCommand(
      ownerSlot: 7,
      identity: 43);
    Assert(!lifecycle.TryTerminateNetwork(missingIdentityTermination),
      "A packet-29 request for a missing identity must be a no-op.");
    Assert(lifecycle.TryGetRuntimeHandle(replacementHandle, out _),
      "A missing-identity packet-29 request must preserve other projectiles.");

    var missingTermination = new ProjectileNetworkTerminateCommand(
      ownerSlot: 8,
      identity: 42);
    Assert(!lifecycle.TryTerminateNetwork(missingTermination),
      "A packet-29 request with the wrong owner must be a no-op.");
    Assert(lifecycle.TryGetRuntimeHandle(replacementHandle, out _),
      "A wrong-owner packet-29 request must preserve the projectile.");
    AssertEqual(1, slots.ActiveCount,
      "A wrong-owner packet-29 request must not release a slot.");

    var termination = new ProjectileNetworkTerminateCommand(
      ownerSlot: 7,
      identity: 42);
    Assert(lifecycle.TryTerminateNetwork(termination),
      "A packet-29 request matching the active owner identity should terminate it.");
    Assert(!lifecycle.TryGetRuntimeHandle(replacementHandle, out _),
      "An accepted packet-29 request should release the projectile generation.");
    Assert(!lifecycle.TryTerminateNetwork(termination),
      "A repeated packet-29 request after release should be a no-op.");
    AssertEqual(0, slots.ActiveCount,
      "Accepted packet-29 termination should release exactly one slot.");
    AssertEqual(0, identities.Count,
      "Accepted packet-29 termination should unregister the protocol identity.");
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

  private static ProjectileDefinition CreateDefinition(int typeId, bool? needsUuid)
  {
    return new ProjectileDefinition(
      new ProjectileIdentityDefinition(typeId, null, needsUuid),
      new ProjectileGeometryDefinition(8, 8, 1.0f, true, false),
      new ProjectileBehaviorDefinition(1, 0, 120),
      new ProjectileCombatDefinition(0, 0.0f, true, false),
      new ProjectilePenetrationDefinition(1, 1, true),
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
