using System;
using System.Numerics;

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
    var lifecycle = new ProjectileLifecycleSystem(slots, identities);
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
    Assert(lifecycle.TryGet(firstHandle, out ProjectileEntityState? firstState) &&
      firstState is not null, "The packet-created state should be readable.");
    AssertEqual(0, firstHandle.Slot.Value, "The first packet should use the first free slot.");
    AssertEqual(42, firstState!.Identity.Identity,
      "Network identity must remain the packet identity.");
    AssertEqual(0, firstState.Identity.SlotIndex,
      "The local slot must remain a separate field.");
    AssertEqual(900, firstState.Identity.ProjectileUuid,
      "A supplied packet UUID must be retained.");
    AssertEqual(18, firstState.Damage.CurrentDamage,
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
    Assert(lifecycle.TryGet(updatedHandle, out ProjectileEntityState? updatedState) &&
      updatedState is not null, "The updated packet state should be readable.");
    AssertEqual(30.0f, updatedState!.Kinematics.Position.X,
      "Same-type packets should update position in place.");
    AssertEqual(9.0f, updatedState.Behavior.GetAi(0),
      "Same-type packets should update AI in place.");
    AssertEqual(900, updatedState.Identity.ProjectileUuid,
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
    Assert(!lifecycle.TryGet(firstHandle, out _),
      "The replaced packet generation must become stale.");
    Assert(lifecycle.TryGet(replacementHandle, out ProjectileEntityState? replacementState) &&
      replacementState is not null, "The replacement state should be readable.");
    AssertEqual(42, replacementState!.Identity.Identity,
      "Replacement must still preserve the packet identity.");
    AssertEqual(901, replacementState.Identity.ProjectileUuid,
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
