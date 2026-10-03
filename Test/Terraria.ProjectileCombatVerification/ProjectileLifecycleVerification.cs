using Terraria.Projectile;
using Terraria.Relationships;
using Terraria.WorldStorage;

internal static class ProjectileLifecycleVerification
{
  public static void Run()
  {
    VerifyOrderedAllocationAndGenerationReuse();
    VerifyCapacityAndIdentityRollback();
    VerifyOldestReplacementAndIdentityChange();
    VerifyReplacementMayRetainTheVictimIdentity();
    VerifyProtectedAndIneligiblePoolsRemainUnchanged();
  }

  private static void VerifyOrderedAllocationAndGenerationReuse()
  {
    var world = new WorldStorageRoot();
    var lifecycle = new ProjectileLifecycleSystem(world);
    Assert(
      lifecycle.TryCreate(
        CreateIdentity(1, 10),
        new ProjectileLifetimeStateComponent(active: true),
        out var first),
      "The first projectile should occupy a slot.");
    Assert(
      lifecycle.TryCreate(
        CreateIdentity(1, 11),
        new ProjectileLifetimeStateComponent(active: true),
        out var second),
      "The second projectile should occupy a slot.");
    AssertEqual(new ProjectileSlot(0), first.Slot, "Allocation should start at slot zero.");
    AssertEqual(new ProjectileSlot(1), second.Slot, "Allocation should use ascending slot order.");
    if (!lifecycle.TryGet(first, out ProjectileEntityState? firstState) || firstState is null)
    {
      throw new InvalidOperationException(
        "The current generation should read its projectile state.");
    }

    AssertEqual(
      first.Slot.Value,
      firstState.Identity.SlotIndex,
      "The stored projectile identity should contain its assigned slot.");
    Assert(
      world.ProjectileIdentities.TryGetHandle(
        new OwnerProjectileIdentity(new PlayerSlot(1), 10),
        out var indexed),
      "Creation should register the owner identity.");
    AssertEqual(first, indexed, "The owner identity should resolve to its allocated handle.");

    Assert(
      lifecycle.TryTerminate(first, ProjectileEndReason.DestroyedByCollision),
      "The current handle should terminate.");
    AssertEqual(1, world.Projectiles.ActiveCount, "Termination should release one slot.");
    Assert(
      !world.ProjectileIdentities.TryGetHandle(
        new OwnerProjectileIdentity(new PlayerSlot(1), 10),
        out _),
      "Termination should remove the owner identity mapping.");

    Assert(
      lifecycle.TryCreate(
        CreateIdentity(1, 12),
        new ProjectileLifetimeStateComponent(active: true),
        out var reused),
      "A released slot should be reusable.");
    AssertEqual(first.Slot, reused.Slot, "Reuse should select the lowest available slot.");
    AssertEqual(
      first.Generation + 1,
      reused.Generation,
      "Reuse should advance the slot generation.");
    Assert(!lifecycle.TryGet(first, out _), "A stale generation must not read the reused slot.");
    Assert(!lifecycle.TryTerminate(first, ProjectileEndReason.DestroyedByCollision),
      "A stale generation must not terminate the new projectile.");
    Assert(
      world.ProjectileIdentities.TryGetHandle(
        new OwnerProjectileIdentity(new PlayerSlot(1), 12),
        out indexed),
      "The reused projectile identity should be registered.");
    AssertEqual(reused, indexed, "The new identity should resolve to the incremented generation.");
  }

  private static void VerifyCapacityAndIdentityRollback()
  {
    var slots = new EntitySlotStore<WorldEntityState, ProjectileSlot>(
      static slot => slot.Value,
      static value => new ProjectileSlot(value),
      maximumCapacity: 2);
    var identities = new ProjectileIdentityIndex();
    var lifecycle = new ProjectileLifecycleSystem(slots, identities);

    Assert(
      lifecycle.TryCreate(
        CreateIdentity(2, 20),
        new ProjectileLifetimeStateComponent(active: true),
        new ProjectileNetworkStateComponent(networkImportant: true),
        out var handle),
      "The bounded store should allocate within capacity.");
    Assert(
      !lifecycle.TryCreate(
        CreateIdentity(2, 20),
        new ProjectileLifetimeStateComponent(active: true),
        out _),
      "A duplicate identity should reject creation.");
    AssertEqual(1, slots.ActiveCount, "A duplicate identity rejection must not change occupancy.");
    AssertEqual(1, identities.Count, "A duplicate identity must not change the index.");
    Assert(
      lifecycle.TryCreate(
        CreateIdentity(2, 21),
        new ProjectileLifetimeStateComponent(active: true),
        new ProjectileNetworkStateComponent(networkImportant: true),
        out _),
      "A rejected duplicate should release its provisional slot.");
    Assert(
      !lifecycle.TryCreate(
        CreateIdentity(2, 22),
        new ProjectileLifetimeStateComponent(active: true),
        out _),
      "A full pool without a replaceable victim should reject creation.");
    AssertEqual(2, slots.ActiveCount, "No-victim rejection must not change occupancy.");
    AssertEqual(2, identities.Count, "No-victim rejection must not change the identity index.");

    Assert(
      lifecycle.TryTerminate(handle, ProjectileEndReason.LifetimeExpired),
      "Releasing a slot should permit later allocation.");
    Assert(
      lifecycle.TryCreate(
        CreateIdentity(2, 22),
        new ProjectileLifetimeStateComponent(active: true),
        out _),
      "A released slot should make bounded capacity available again.");
    AssertEqual(2, slots.ActiveCount, "A released slot should return to active occupancy.");
    AssertEqual(2, identities.Count, "Allocation after release should register its identity.");
  }

  private static void VerifyOldestReplacementAndIdentityChange()
  {
    var slots = CreateBoundedSlots(2);
    var identities = new ProjectileIdentityIndex();
    var lifecycle = new ProjectileLifecycleSystem(slots, identities);

    Assert(
      lifecycle.TryCreate(
        CreateIdentity(3, 30),
        new ProjectileLifetimeStateComponent(timeLeft: 20),
        out ProjectileHandle first),
      "The first projectile should allocate before the pool fills.");
    Assert(
      lifecycle.TryCreate(
        CreateIdentity(4, 40),
        new ProjectileLifetimeStateComponent(timeLeft: 5),
        out ProjectileHandle oldest),
      "The second projectile should allocate before the pool fills.");
    Assert(
      lifecycle.TryCreate(
        CreateIdentity(5, 50),
        new ProjectileLifetimeStateComponent(timeLeft: 30),
        out ProjectileHandle replacement),
      "A full pool should replace its oldest non-important projectile.");

    AssertEqual(oldest.Slot, replacement.Slot, "The shortest lifetime should be replaced.");
    AssertEqual(
      oldest.Generation + 1,
      replacement.Generation,
      "Replacing a slot should invalidate its previous generation.");
    Assert(lifecycle.TryGet(first, out _), "A non-selected projectile should remain active.");
    Assert(!lifecycle.TryGet(oldest, out _), "The replaced handle must become stale.");
    Assert(
      !identities.TryGetHandle(new OwnerProjectileIdentity(new PlayerSlot(4), 40), out _),
      "Replacement should unregister the old owner identity.");
    Assert(
      identities.TryGetHandle(new OwnerProjectileIdentity(new PlayerSlot(5), 50), out var indexed),
      "Replacement should register the new owner identity.");
    AssertEqual(replacement, indexed, "The new identity should resolve to the new generation.");
    Assert(
      lifecycle.TryGet(replacement, out ProjectileEntityState? replacementState) &&
      replacementState is not null,
      "The replacement generation should expose its state.");
    AssertEqual(
      replacement.Slot.Value,
      replacementState!.Identity.SlotIndex,
      "Replacement state should contain its assigned slot.");
  }

  private static void VerifyProtectedAndIneligiblePoolsRemainUnchanged()
  {
    var slots = CreateBoundedSlots(2);
    var identities = new ProjectileIdentityIndex();
    var lifecycle = new ProjectileLifecycleSystem(slots, identities);
    var importantNetwork = new ProjectileNetworkStateComponent(networkImportant: true);

    Assert(
      lifecycle.TryCreate(
        CreateIdentity(6, 60),
        new ProjectileLifetimeStateComponent(timeLeft: 1),
        importantNetwork,
        out ProjectileHandle protectedHandle),
      "The protected projectile should allocate.");
    Assert(
      lifecycle.TryCreate(
        CreateIdentity(7, 70),
        new ProjectileLifetimeStateComponent(timeLeft: 50),
        out ProjectileHandle replaceableHandle),
      "The replaceable projectile should allocate.");
    Assert(
      lifecycle.TryCreate(
        CreateIdentity(8, 80),
        new ProjectileLifetimeStateComponent(timeLeft: 100),
        out ProjectileHandle replacement),
      "A non-important projectile should remain replaceable even when it lives longer.");
    AssertEqual(
      replaceableHandle.Slot,
      replacement.Slot,
      "Network-important state must protect its slot from replacement.");
    Assert(
      lifecycle.TryGet(protectedHandle, out _),
      "A network-important projectile must survive.");

    var duplicateSlots = CreateBoundedSlots(2);
    var duplicateIndex = new ProjectileIdentityIndex();
    var duplicateLifecycle = new ProjectileLifecycleSystem(duplicateSlots, duplicateIndex);
    Assert(
      duplicateLifecycle.TryCreate(
        CreateIdentity(9, 90),
        new ProjectileLifetimeStateComponent(timeLeft: 1),
        out ProjectileHandle existingFirst),
      "The first duplicate-check projectile should allocate.");
    Assert(
      duplicateLifecycle.TryCreate(
        CreateIdentity(10, 100),
        new ProjectileLifetimeStateComponent(timeLeft: 2),
        out ProjectileHandle existingSecond),
      "The second duplicate-check projectile should allocate.");
    Assert(
      !duplicateLifecycle.TryCreate(
        CreateIdentity(10, 100),
        new ProjectileLifetimeStateComponent(timeLeft: 3),
        out _),
      "A duplicate identity owned by a different slot must reject replacement.");
    AssertEqual(
      2,
      duplicateSlots.ActiveCount,
      "Duplicate identity rejection must preserve occupancy.");
    AssertEqual(
      2,
      duplicateIndex.Count,
      "Duplicate identity rejection must preserve both mappings.");
    Assert(
      duplicateLifecycle.TryGet(existingFirst, out _),
      "Rejected replacement must preserve the oldest slot.");
    Assert(
      duplicateLifecycle.TryGet(existingSecond, out _),
      "Rejected replacement must preserve the duplicate owner slot.");

    var importantSlots = CreateBoundedSlots(2);
    var importantIndex = new ProjectileIdentityIndex();
    var importantLifecycle = new ProjectileLifecycleSystem(importantSlots, importantIndex);
    Assert(
      importantLifecycle.TryCreate(
        CreateIdentity(11, 110),
        new ProjectileLifetimeStateComponent(timeLeft: 1),
        importantNetwork,
        out ProjectileHandle firstImportant),
      "The first protected projectile should allocate.");
    Assert(
      importantLifecycle.TryCreate(
        CreateIdentity(12, 120),
        new ProjectileLifetimeStateComponent(timeLeft: 2),
        importantNetwork,
        out ProjectileHandle secondImportant),
      "The second protected projectile should allocate.");
    Assert(
      !importantLifecycle.TryCreate(
        CreateIdentity(13, 130),
        new ProjectileLifetimeStateComponent(timeLeft: 3),
        out _),
      "A pool without an eligible victim must reject creation.");
    AssertEqual(
      2,
      importantSlots.ActiveCount,
      "No-victim rejection must preserve occupancy.");
    AssertEqual(
      2,
      importantIndex.Count,
      "No-victim rejection must preserve the identity index.");
    Assert(
      importantLifecycle.TryGet(firstImportant, out _),
      "No-victim rejection must preserve slot zero.");
    Assert(
      importantLifecycle.TryGet(secondImportant, out _),
      "No-victim rejection must preserve slot one.");

    var highLifetimeSlots = CreateBoundedSlots(2);
    var highLifetimeLifecycle = new ProjectileLifecycleSystem(
      highLifetimeSlots,
      new ProjectileIdentityIndex());
    Assert(
      highLifetimeLifecycle.TryCreate(
        CreateIdentity(17, 170),
        new ProjectileLifetimeStateComponent(timeLeft: 9_999_999),
        out ProjectileHandle highLifetimeFirst),
      "The first high-lifetime projectile should allocate.");
    Assert(
      highLifetimeLifecycle.TryCreate(
        CreateIdentity(18, 180),
        new ProjectileLifetimeStateComponent(timeLeft: 10_000_000),
        out ProjectileHandle highLifetimeSecond),
      "The second high-lifetime projectile should allocate.");
    Assert(
      !highLifetimeLifecycle.TryCreate(
        CreateIdentity(19, 190),
        new ProjectileLifetimeStateComponent(timeLeft: 10_000_001),
        out _),
      "Lifetimes at or above the legacy sentinel must not select a victim.");
    Assert(highLifetimeLifecycle.TryGet(highLifetimeFirst, out _),
      "Sentinel-boundary rejection must preserve the first slot.");
    Assert(highLifetimeLifecycle.TryGet(highLifetimeSecond, out _),
      "Sentinel-boundary rejection must preserve the second slot.");

    var tiedSlots = CreateBoundedSlots(2);
    var tiedLifecycle = new ProjectileLifecycleSystem(tiedSlots, new ProjectileIdentityIndex());
    Assert(
      tiedLifecycle.TryCreate(
        CreateIdentity(14, 140),
        new ProjectileLifetimeStateComponent(timeLeft: 10),
        out ProjectileHandle lowerSlot),
      "The first tied projectile should allocate.");
    Assert(
      tiedLifecycle.TryCreate(
        CreateIdentity(15, 150),
        new ProjectileLifetimeStateComponent(timeLeft: 10),
        out _),
      "The second tied projectile should allocate.");
    Assert(
      tiedLifecycle.TryCreate(
        CreateIdentity(16, 160),
        new ProjectileLifetimeStateComponent(timeLeft: 20),
        out ProjectileHandle tiedReplacement),
      "An eligible tied projectile should be replaced.");
    AssertEqual(
      lowerSlot.Slot,
      tiedReplacement.Slot,
      "Ties should retain ascending slot selection.");
  }

  private static void VerifyReplacementMayRetainTheVictimIdentity()
  {
    var slots = CreateBoundedSlots(2);
    var identities = new ProjectileIdentityIndex();
    var lifecycle = new ProjectileLifecycleSystem(slots, identities);
    OwnerProjectileIdentity reusedIdentity = new(new PlayerSlot(20), 200);

    Assert(
      lifecycle.TryCreate(
        CreateIdentity(20, 200),
        new ProjectileLifetimeStateComponent(timeLeft: 1),
        out ProjectileHandle previous),
      "The identity that will be reused should allocate.");
    Assert(
      lifecycle.TryCreate(
        CreateIdentity(21, 210),
        new ProjectileLifetimeStateComponent(timeLeft: 50),
        out _),
      "A second projectile should fill the bounded pool.");
    Assert(
      lifecycle.TryCreate(
        CreateIdentity(20, 200),
        new ProjectileLifetimeStateComponent(timeLeft: 60),
        out ProjectileHandle replacement),
      "The victim identity may be reused while its slot is replaced.");

    AssertEqual(previous.Slot, replacement.Slot, "The victim slot should be recycled.");
    AssertEqual(
      previous.Generation + 1,
      replacement.Generation,
      "Recycling the same identity must still advance its generation.");
    Assert(!lifecycle.TryGet(previous, out _), "The previous handle must become stale.");
    Assert(
      identities.TryGetHandle(reusedIdentity, out ProjectileHandle indexed),
      "The reused owner identity should remain registered.");
    AssertEqual(replacement, indexed, "The reused identity should resolve to the new handle.");
  }

  private static EntitySlotStore<WorldEntityState, ProjectileSlot> CreateBoundedSlots(int capacity)
  {
    return new EntitySlotStore<WorldEntityState, ProjectileSlot>(
      static slot => slot.Value,
      static value => new ProjectileSlot(value),
      maximumCapacity: capacity);
  }

  private static ProjectileIdentityComponent CreateIdentity(int ownerSlot, int identity)
  {
    return new ProjectileIdentityComponent(
      EntityReference.None,
      identity: identity,
      ownerSlot: ownerSlot);
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
