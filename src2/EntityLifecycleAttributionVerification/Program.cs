using System.Numerics;
using Terraria.EntityLifecycleAttribution;

namespace Terraria.EntityLifecycleAttributionVerification;

internal static class Program
{
  private static int Main()
  {
    TestSlotBindingRejectsStaleHandles();
    TestMotionHistoryCapturesThePreviousFrame();
    TestWorldItemReuseDelayAndGeneration();
    TestProjectileIdentityIndexIsClearedBeforeReuse();
    TestSourceAdapterProducesCoreContextWithoutLegacyObjects();
    TestDeathAttributionQueryAndNetworkRoundTrip();
    TestWorldContainersRequireEmptyRemovalAndSignsInvalidate();
    TestPresentationPoolAndAnimationCatalogHaveIndependentLifetimes();
    TestSpawnAndDespawnSystemsOwnPoolTransitions();
    TestAllocationCommandsRouteToTheirDeclaredPools();
    TestPresentationActivityIsScopedToTheEffectKind();
    Console.WriteLine("P06 verifier passed.");
    return 0;
  }

  private static void TestSlotBindingRejectsStaleHandles()
  {
    EntitySlotBindingComponent binding = new();
    binding.Bind(runtimeEntityId: 42, compatibilitySlot: 3, generation: 7);
    EntitySlotHandle firstHandle = binding.Handle;

    Assert(binding.Matches(firstHandle), "An active binding must match its current handle.");
    binding.Release();
    Assert(!binding.Matches(firstHandle), "A released binding must reject its old handle.");

    binding.Bind(runtimeEntityId: 43, compatibilitySlot: 3, generation: 8);
    Assert(!binding.Matches(firstHandle), "A reused slot must reject a prior generation.");
  }

  private static void TestMotionHistoryCapturesThePreviousFrame()
  {
    LocationComponent location = new(new Vector2(10, 20));
    VelocityComponent velocity = new(new Vector2(2, 3));
    DirectionComponent direction = new(1);
    MotionHistoryComponent history = new();

    new MotionHistoryCommitSystem().Capture(location, velocity, direction, history);

    Assert(history.PreviousPosition == new Vector2(10, 20), "The previous position must be captured.");
    Assert(history.PreviousVelocity == new Vector2(2, 3), "The previous velocity must be captured.");
    Assert(history.PreviousDirection == 1, "The previous direction must be captured.");
  }

  private static void TestWorldItemReuseDelayAndGeneration()
  {
    WorldItemSlotStore store = new(capacity: 1);
    WorldItemComponent item = new(contentType: 5, stack: 2);

    Assert(store.TryAllocate(item, out EntitySlotHandle firstHandle), "The first item slot must allocate.");
    Assert(store.TryRelease(firstHandle, reuseDelayTicks: 2), "The active item must release once.");
    Assert(
      !store.TryAllocate(new WorldItemComponent(contentType: 6, stack: 1), out _),
      "A slot with a reuse delay must not allocate.");

    store.AdvanceTick();
    store.AdvanceTick();
    Assert(store.TryAllocate(new WorldItemComponent(contentType: 6, stack: 1), out EntitySlotHandle secondHandle), "The slot must become reusable.");
    Assert(secondHandle.Generation != firstHandle.Generation, "Reuse must advance the slot generation.");
    Assert(!store.TryGet(firstHandle, out _), "The old item handle must be stale after reuse.");
  }

  private static void TestProjectileIdentityIndexIsClearedBeforeReuse()
  {
    ProjectileEntitySlotStore store = new(capacity: 1);
    Assert(store.TryAllocate(owner: 2, identity: 9, projectileType: 17, out EntitySlotHandle firstHandle), "The projectile slot must allocate.");
    Assert(store.TryResolve(2, 9, out EntitySlotHandle resolved) && resolved == firstHandle, "The owner/identity key must resolve the active projectile.");

    Assert(store.TryRelease(firstHandle), "The projectile must release.");
    Assert(!store.TryResolve(2, 9, out _), "The identity index must be cleared on release.");
    Assert(store.TryAllocate(owner: 2, identity: 9, projectileType: 18, out EntitySlotHandle secondHandle), "The released slot must be reusable.");
    Assert(secondHandle.Generation != firstHandle.Generation, "A reused projectile slot must have a new generation.");
  }

  private static void TestSourceAdapterProducesCoreContextWithoutLegacyObjects()
  {
    LegacyEntitySourceRecord source = LegacyEntitySourceRecord.OnHit(
      striking: new EntityReference(10, 3),
      struck: new EntityReference(11, 4));

    Assert(LegacyEntitySourceAdapter.TryAdapt(source, out EntitySpawnSourceContext context), "A supported legacy source must adapt.");
    Assert(context.Kind == EntitySpawnSourceKind.OnHit, "The source kind must be preserved.");
    Assert(context.PrimaryEntity?.RuntimeEntityId == 10, "The striking entity must become a core reference.");
    Assert(context.SecondaryEntity?.RuntimeEntityId == 11, "The struck entity must become a core reference.");

    EntityProvenanceComponent provenance = new();
    Assert(new EntityProvenanceCommitSystem().Commit(provenance, context), "A source context must commit once.");
    Assert(provenance.Context == context, "Committed provenance must retain the immutable context value.");
    Assert(!new EntityProvenanceCommitSystem().Commit(provenance, context), "A committed provenance value must not be overwritten silently.");
  }

  private static void TestDeathAttributionQueryAndNetworkRoundTrip()
  {
    PlayerDeathAttributionSnapshot snapshot = new(
      sourcePlayerIndex: 1,
      sourceNpcIndex: 2,
      sourceProjectileLocalIndex: 4,
      sourceOtherIndex: -1,
      sourceProjectileType: 88,
      sourceItemType: 99,
      sourceItemPrefix: 3,
      customReason: "fell");

    Assert(DeathAttributionQuery.GetSourceProjectileType(snapshot) == 88, "A local projectile source must expose its projectile type.");
    Assert(new DeathAttributionCaptureSystem().Capture(snapshot) == snapshot, "Death capture must return an immutable snapshot.");

    byte[] encoded = LegacyDeathReasonNetworkAdapter.Serialize(snapshot);
    Assert(LegacyDeathReasonNetworkAdapter.TryDeserialize(encoded, out PlayerDeathAttributionSnapshot decoded), "The death attribution payload must decode.");
    Assert(decoded == snapshot, "Death attribution network round-trip must preserve all fields.");
    Assert(DeathCauseProjection.Create(snapshot).CustomReason == "fell", "The death projection must expose presentation data only.");
  }

  private static void TestWorldContainersRequireEmptyRemovalAndSignsInvalidate()
  {
    WorldContainerStore containers = new();
    Assert(containers.TryCreateChest(slot: 5, capacity: 2, out WorldChestState chest), "A chest slot must be creatable.");
    Assert(chest.TrySetItem(0, 100), "The chest must accept an item.");
    Assert(!containers.TryRemoveEmptyChest(5), "A non-empty chest must not be removed.");
    chest.Clear();
    Assert(containers.TryRemoveEmptyChest(5), "An empty chest must be removable.");

    WorldSignStore signs = new();
    TileCoordinate coordinate = new(12, 14);
    WorldSignState sign = signs.GetOrCreate(coordinate);
    Assert(signs.TrySetText(coordinate, "hello"), "A sign text update must be accepted.");
    Assert(sign.Text == "hello", "The sign text must be stored in its coordinate slot.");
    Assert(signs.Invalidate(coordinate), "A sign must be invalidatable when its tile is no longer valid.");
    Assert(!signs.TryGetActive(coordinate, out _), "An invalidated sign must not remain active.");
  }

  private static void TestPresentationPoolAndAnimationCatalogHaveIndependentLifetimes()
  {
    PresentationEffectPoolStore effects = new(capacityPerKind: 1);
    Assert(effects.TryAllocate(PresentationEffectKind.Dust, lifetimeTicks: 1, out EntitySlotHandle effectHandle), "A presentation effect must allocate.");
    effects.AdvanceTick();
    Assert(!effects.IsActive(PresentationEffectKind.Dust, effectHandle), "An expired presentation effect must be recycled.");

    ItemAnimationCatalog catalog = new();
    Assert(catalog.Register(new ItemAnimationDefinition(contentType: 7, frameCount: 4, frameDurationTicks: 3)), "An item animation definition must register.");
    Assert(catalog.TryGet(7, out ItemAnimationDefinition definition) && definition.FrameCount == 4, "The catalog must return registered definitions.");
    Assert(new ItemAnimationCatalogSystem(catalog).Clear(), "The catalog system must clear registration bookkeeping.");
    Assert(!catalog.TryGet(7, out _), "Clearing the catalog must remove the definition.");
  }

  private static void TestSpawnAndDespawnSystemsOwnPoolTransitions()
  {
    EntityPoolSet pools = new EntityPoolInitializationSystem().Initialize(
      worldItemCapacity: 1,
      npcCapacity: 1,
      projectileCapacity: 1,
      presentationCapacityPerKind: 1);
    EntitySpawnCommitSystem spawn = new(pools);
    EntityDespawnRecycleSystem despawn = new(pools);

    AllocateEntitySlotCommand command = AllocateEntitySlotCommand.ForProjectile(
      runtimeEntityId: 100,
      owner: 3,
      identity: 5,
      projectileType: 12);
    Assert(spawn.TryCommit(command, out EntitySlotHandle handle), "The spawn system must commit a projectile allocation.");
    Assert(pools.Projectiles.TryResolve(3, 5, out _), "A committed projectile must be indexed.");
    Assert(despawn.TryCommit(new ReleaseEntitySlotCommand(EntitySlotPoolKind.Projectile, handle)), "The despawn system must release the projectile.");
    Assert(!pools.Projectiles.TryResolve(3, 5, out _), "Despawn must detach the projectile identity index.");
  }

  private static void TestAllocationCommandsRouteToTheirDeclaredPools()
  {
    EntityPoolSet pools = new EntityPoolInitializationSystem().Initialize(
      worldItemCapacity: 1,
      npcCapacity: 1,
      projectileCapacity: 1,
      presentationCapacityPerKind: 1);
    EntitySpawnCommitSystem spawn = new(pools);

    AllocateEntitySlotCommand worldItemCommand = AllocateEntitySlotCommand.ForWorldItem(
      runtimeEntityId: 200,
      item: new WorldItemComponent(contentType: 3, stack: 1));
    Assert(spawn.TryCommit(worldItemCommand, out EntitySlotHandle worldItemHandle), "The world item factory must route to the world item pool.");
    Assert(worldItemHandle.RuntimeEntityId == 200, "The world item command must preserve its runtime entity ID.");
    Assert(pools.WorldItems.TryGet(worldItemHandle, out _), "The world item command must create a world item entry.");

    AllocateEntitySlotCommand presentationCommand = AllocateEntitySlotCommand.ForPresentation(
      runtimeEntityId: 201,
      kind: PresentationEffectKind.Dust,
      lifetimeTicks: 1);
    Assert(spawn.TryCommit(presentationCommand, out EntitySlotHandle presentationHandle), "The presentation factory must route to the presentation pool.");
    Assert(
      pools.PresentationEffects.IsActive(PresentationEffectKind.Dust, presentationHandle),
      "The presentation command must create a presentation effect entry.");
  }

  private static void TestPresentationActivityIsScopedToTheEffectKind()
  {
    PresentationEffectPoolStore store = new(capacityPerKind: 1);
    Assert(
      store.TryAllocate(PresentationEffectKind.Dust, runtimeEntityId: 300, lifetimeTicks: 2, out EntitySlotHandle dustHandle),
      "The dust effect must allocate.");
    Assert(
      store.TryAllocate(PresentationEffectKind.Star, runtimeEntityId: 300, lifetimeTicks: 2, out EntitySlotHandle starHandle),
      "The star effect must allocate independently.");
    Assert(store.IsActive(PresentationEffectKind.Dust, dustHandle), "The dust handle must be active in the dust pool.");
    Assert(store.IsActive(PresentationEffectKind.Star, starHandle), "The star handle must be active in the star pool.");
    Assert(!store.IsActive(PresentationEffectKind.Star, dustHandle), "A dust handle must not resolve in the star pool.");
  }

  private static void Assert(bool condition, string message)
  {
    if (!condition)
    {
      throw new InvalidOperationException(message);
    }
  }
}
