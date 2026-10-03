using System.Numerics;

using Terraria.Content;
using Terraria.Projectile;
using Terraria.Relationships;
using Terraria.WorldStorage;

internal static class ProjectileDefinitionHydrationVerification
{
  public static void Run()
  {
    ProjectileDefinition firstDefinition = CreateDefinition(12, 11, 6, 1.5f, needsUuid: false);
    ProjectileDefinition secondDefinition = CreateDefinition(13, 8, 4, 1.0f, needsUuid: true);
    ProjectileDefinition replacementDefinition = CreateDefinition(14, 5, 5, 1.0f, needsUuid: true);
    ProjectileDefinition unknownUuidDefinition = CreateDefinition(15, 5, 5, 1.0f, needsUuid: null);
    var definitions = new ProjectileDefinitionCatalog(
      new[] { firstDefinition, secondDefinition, replacementDefinition, unknownUuidDefinition });
    var slots = new EntitySlotStore<WorldEntityState, ProjectileSlot>(
      static slot => slot.Value,
      static value => new ProjectileSlot(value),
      maximumCapacity: 2);
    var identities = new ProjectileIdentityIndex();
    var lifecycle = new ProjectileLifecycleSystem(slots, identities);
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
      lifecycle.TryGet(firstHandle, out ProjectileEntityState? firstState) &&
        firstState is not null,
      "The spawned projectile should be readable through its current handle.");
    AssertEqual(16, firstState!.Geometry.Width, "Definition width should be scaled and truncated.");
    AssertEqual(9, firstState.Geometry.Height, "Definition height should be scaled and truncated.");
    AssertEqual(
      new Vector2(92, 45.5f),
      firstState.Kinematics.Position,
      "Spawn center should convert to top-left position after scaled dimensions.");
    AssertEqual(
      new Vector2(3, -2),
      firstState.Kinematics.Velocity,
      "Spawn velocity should override the zero type default.");
    AssertEqual(120, firstState.Lifetime.TimeLeft, "Lifetime should come from the type definition.");
    AssertEqual(2, firstState.UpdateCadence.ExtraUpdates,
      "Extra updates should hydrate from definition.");
    AssertEqual(1.25f, firstState.Behavior.GetAi(0), "Spawn AI should override reset AI state.");
    AssertEqual(-4.0f, firstState.Behavior.GetAi(1), "Spawn AI slots should retain their order.");
    AssertEqual(0.0f, firstState.Behavior.GetLocalAi(0), "Local AI should reset on initialization.");
    AssertEqual(24, firstState.Damage.CurrentDamage, "Spawn damage should override definition defaults.");
    AssertEqual(30, firstState.Damage.OriginalDamage, "Original damage should be retained.");
    Assert(firstState.Damage.IsRanged, "Ranged content classification should hydrate.");
    AssertEqual(7, firstState.Definition.CatalogRevision, "The catalog revision should be captured.");
    AssertEqual(firstHandle.Slot.Value, firstState.Identity.Identity,
      "A local spawn should derive identity from the selected slot.");
    AssertEqual(firstHandle.Slot.Value, firstState.Identity.SlotIndex,
      "A local spawn should store its selected slot separately.");
    AssertEqual(-1, firstState.Identity.ProjectileUuid,
      "A type without UUID metadata should keep the reset UUID sentinel.");
    AssertEqual(200, firstState.HitImmunity.LocalNpcImmunityTicks.Length,
      "NPC immunity storage should match the explicit world context.");
    AssertEqual(8, firstState.Network.SectionSyncSkippedForPlayer.Length,
      "Network skip state should match the explicit player capacity.");
    AssertEqual(3, firstState.Trail.OldPositions.Length,
      "Trail arrays should be allocated at the definition length.");
    AssertEqual((short)5, firstState.Presentation.GlowMask,
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
      lifecycle.TryGet(secondHandle, out ProjectileEntityState? secondState) &&
        secondState is not null,
      "The second spawn should be readable through its handle.");
    AssertEqual(secondHandle.Slot.Value, secondState!.Identity.Identity,
      "A local spawn should derive owner identity from the selected slot.");
    AssertEqual(secondHandle.Slot.Value, secondState.Identity.ProjectileUuid,
      "A UUID-requiring type should derive UUID from the selected slot.");

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
    Assert(!lifecycle.TryGet(firstHandle, out _), "Replacement should invalidate the old generation.");
    Assert(lifecycle.TryGet(secondHandle, out _), "Replacement should preserve the non-selected slot.");
    Assert(
      lifecycle.TryGet(replacementHandle, out ProjectileEntityState? replacementState) &&
        replacementState is not null,
      "The replacement should be committed as the current slot state.");
    Assert(replacementState!.Network.NetworkImportant,
      "Network importance should hydrate from the replacement definition.");
    AssertEqual(replacementHandle.Slot.Value, replacementState.Identity.Identity,
      "Replacement state should derive owner identity from the reused slot.");
    AssertEqual(replacementHandle.Slot.Value, replacementState.Identity.ProjectileUuid,
      "Replacement state should derive UUID from the reused slot when required.");
    AssertEqual(0.0f, replacementState.Behavior.GetAi(0),
      "Replacement should not retain prior projectile AI.");
    AssertEqual(0.0f, replacementState.Trail.OldPositions[0].X,
      "Replacement trail history should be fresh.");
    AssertEqual(0, replacementState.HitImmunity.PlayerImmunityTicks[0],
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
    Assert(lifecycle.TryGet(secondHandle, out _) && lifecycle.TryGet(replacementHandle, out _),
      "An unknown UUID policy must preserve existing projectile handles.");
  }

  private static ProjectileDefinition CreateDefinition(
    int typeId,
    int width,
    int height,
    float scale,
    bool? needsUuid)
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
