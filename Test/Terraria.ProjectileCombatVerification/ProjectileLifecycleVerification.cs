using System.Reflection;

using EntityEcs;

using Terraria.Content;
using Terraria.Projectile;
using Terraria.Relationships;
using Terraria.WorldStorage;

internal static class ProjectileLifecycleVerification
{
  public static void Run()
  {
    VerifyOrderedAllocationAndGenerationReuse();
    VerifyCapacityAndIdentityRollback();
    VerifyGenerationExhaustionPreventsReplacement();
    VerifyProjectileWorldCapacityAndRootCleanup();
    VerifyOldestReplacementAndIdentityChange();
    VerifyReplacementMayRetainTheVictimIdentity();
    VerifyProtectedAndIneligiblePoolsRemainUnchanged();
    VerifyBorrowedFullPoolRejectsCreateWithoutLeakingCandidate();
    VerifyBorrowedTerminationIsNoOpAndCanRetry();
    VerifyBorrowedNetworkTypeReplacementIsNoOpAndCanRetry();
  }

  private static void VerifyOrderedAllocationAndGenerationReuse()
  {
    using var entityRuntime = new EntityRuntime();
    using var world = new WorldStorageRoot(entityRuntime);
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
    if (!lifecycle.TryGetRuntimeHandle(first, out RuntimeEntityHandle firstRuntimeHandle))
    {
      throw new InvalidOperationException(
        "The current generation should read its projectile state.");
    }

    ProjectileIdentityComponent firstIdentity =
      ProjectileVerificationAccess.Read<ProjectileIdentityComponent>(lifecycle, first);
    AssertEqual(
      first.Slot.Value,
      firstIdentity.SlotIndex,
      "The stored projectile identity should contain its assigned slot.");
    Assert(lifecycle.TryGetEntityReference(first, out EntityReference firstReference),
      "The current projectile should expose its entity reference.");
    AssertEqual(EntityReferenceScope.Projectile, firstReference.Scope,
      "Each projectile should publish its unified entity reference.");
    Assert(
      world.ProjectileRuntime.TryResolve(firstReference, out RuntimeEntityHandle firstRoot) &&
      firstRoot == firstRuntimeHandle,
      "The projectile reference should resolve to its component root.");
    AssertEqual(2, world.ProjectileRuntime.EntityCount,
      "Each occupied projectile should own a registered entity root.");
    Assert(
      world.ProjectileIdentities.TryGetHandle(
        new OwnerProjectileIdentity(new PlayerSlot(1), 10),
        out var indexed),
      "Creation should register the owner identity.");
    AssertEqual(first, indexed, "The owner identity should resolve to its allocated handle.");

    Assert(
      lifecycle.TryTerminate(first, ProjectileEndReason.DestroyedByCollision),
      "The current handle should terminate.");
    Assert(
      !world.ProjectileRuntime.TryResolve(firstReference, out _),
      "Termination should unregister the projectile entity root.");
    AssertEqual(1, world.Projectiles.ActiveCount, "Termination should release one slot.");
    Assert(
      !world.ProjectileIdentities.TryGetHandle(
        new OwnerProjectileIdentity(new PlayerSlot(1), 10),
        out _),
      "Termination should remove the owner identity mapping.");
    AssertEqual(1, world.ProjectileRuntime.EntityCount,
      "Termination should remove the projectile root and its components.");

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
    Assert(
      lifecycle.TryGetEntityReference(reused, out EntityReference reusedReference) &&
      reusedReference.EntityId != firstReference.EntityId,
      "Reusing a slot should create a new entity identity.");
    Assert(!ProjectileVerificationAccess.HasHandle(lifecycle, first),
      "A stale generation must not read the reused slot.");
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
    using var runtime = new EntityRuntime();
    var lifecycle = new ProjectileLifecycleSystem(slots, identities, runtime);

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
    AssertEqual(1, runtime.EntityCount,
      "A rejected duplicate should remove its provisional entity root.");
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
    AssertEqual(2, runtime.EntityCount,
      "A rejected full-pool create should remove its provisional entity root.");
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

  private static void VerifyGenerationExhaustionPreventsReplacement()
  {
    var slots = CreateBoundedSlots(1);
    var identities = new ProjectileIdentityIndex();
    using var runtime = new EntityRuntime();
    var lifecycle = new ProjectileLifecycleSystem(slots, identities, runtime);
    OwnerProjectileIdentity originalIdentity = new(new PlayerSlot(24), 240);

    Assert(
      lifecycle.TryCreate(
        CreateIdentity(24, 240),
        new ProjectileLifetimeStateComponent(timeLeft: 10),
        out ProjectileHandle original),
      "The projectile for the generation exhaustion boundary should allocate.");

    SetSlotGenerationForBoundaryTest(slots, original.Slot.Value, uint.MaxValue);
    var exhaustedHandle = new ProjectileHandle(original.Slot, uint.MaxValue);
    Assert(
      identities.TryReplace(originalIdentity, original, exhaustedHandle),
      "The protocol index should track the slot's exhausted generation.");
    Assert(
      ProjectileVerificationAccess.HasHandle(lifecycle, exhaustedHandle),
      "The occupied slot should remain addressable at its final generation.");

    Assert(
      !lifecycle.TryCreate(
        CreateIdentity(25, 250),
        new ProjectileLifetimeStateComponent(timeLeft: 20),
        out _),
      "A full pool must reject replacement when the oldest slot has exhausted its generation.");
    AssertEqual(1, slots.ActiveCount,
      "Generation exhaustion must leave the original slot occupied.");
    AssertEqual(1, identities.Count,
      "Generation exhaustion must leave the original identity mapping intact.");
    AssertEqual(1, runtime.EntityCount,
      "Generation exhaustion must remove the rejected candidate's provisional entity root.");
    Assert(
      identities.TryGetHandle(originalIdentity, out ProjectileHandle indexed) &&
      indexed == exhaustedHandle,
      "The identity index must retain the original final-generation handle.");

    Assert(
      lifecycle.TryTerminate(exhaustedHandle, ProjectileEndReason.LifetimeExpired),
      "The final-generation projectile should still terminate normally.");
    AssertEqual(0, slots.ActiveCount,
      "Terminating a final-generation projectile should release its occupied slot.");
    AssertEqual(0, identities.Count,
      "Terminating a final-generation projectile should unregister its identity.");
    AssertEqual(0, runtime.EntityCount,
      "Terminating a final-generation projectile should remove its entity root.");
    Assert(
      !lifecycle.TryCreate(
        CreateIdentity(25, 250),
        new ProjectileLifetimeStateComponent(timeLeft: 20),
        out _),
      "A released slot at generation exhaustion must never be reused.");
    AssertEqual(0, slots.ActiveCount,
      "An exhausted slot must stay unavailable to later allocations.");
    AssertEqual(0, identities.Count,
      "Rejected creation after exhaustion must not add an identity mapping.");
    AssertEqual(0, runtime.EntityCount,
      "Rejected creation after exhaustion must clean up its provisional root.");
  }

  private static void SetSlotGenerationForBoundaryTest(
    EntitySlotStore<WorldEntityState, ProjectileSlot> slots,
    int slotIndex,
    uint generation)
  {
    FieldInfo entriesField = typeof(EntitySlotStore<WorldEntityState, ProjectileSlot>)
      .GetField("_entries", BindingFlags.Instance | BindingFlags.NonPublic) ??
      throw new InvalidOperationException(
        "The slot-store generation boundary could not access its private entry table.");
    if (entriesField.GetValue(slots) is not Array entries ||
      entries.GetValue(slotIndex) is not EntitySlotEntry<WorldEntityState> entry)
    {
      throw new InvalidOperationException(
        "The slot-store generation boundary did not find its occupied entry.");
    }

    PropertyInfo generationProperty = typeof(EntitySlotEntry<WorldEntityState>)
      .GetProperty(nameof(EntitySlotEntry<WorldEntityState>.Generation)) ??
      throw new InvalidOperationException(
        "The slot-store generation boundary could not access the generation property.");
    MethodInfo generationSetter = generationProperty.GetSetMethod(nonPublic: true) ??
      throw new InvalidOperationException(
        "The slot-store generation boundary could not access its internal setter.");
    generationSetter.Invoke(entry, [generation]);
  }

  private static void VerifyProjectileWorldCapacityAndRootCleanup()
  {
    using var entityRuntime = new EntityRuntime();
    using var world = new WorldStorageRoot(entityRuntime);
    var lifecycle = new ProjectileLifecycleSystem(world);
    var handles = new ProjectileHandle[1000];
    for (int index = 0; index < handles.Length; index++)
    {
      Assert(
        lifecycle.TryCreate(
          CreateIdentity(23, index),
          new ProjectileLifetimeStateComponent(timeLeft: 1),
          new ProjectileNetworkStateComponent(networkImportant: true),
          out handles[index]),
        $"World projectile slot {index} should allocate before the pool reaches capacity.");
      AssertEqual(index, handles[index].Slot.Value,
        "The world projectile pool must allocate slots in ascending order.");
    }

    AssertEqual(1000, world.Projectiles.ActiveCount,
      "The world projectile store must expose exactly 1000 ordinary slots.");
    AssertEqual(1000, world.ProjectileRuntime.EntityCount,
      "Each occupied world slot must own exactly one live entity root.");

    Assert(
      !lifecycle.TryCreate(
        CreateIdentity(23, 1000),
        new ProjectileLifetimeStateComponent(timeLeft: 1),
        out _),
      "A full all-important pool must reject a create without replacing a protected slot.");
    AssertEqual(1000, world.Projectiles.ActiveCount,
      "A rejected full-pool create must preserve slot occupancy.");
    AssertEqual(1000, world.ProjectileIdentities.Count,
      "A rejected full-pool create must preserve the protocol index.");
    AssertEqual(1000, world.ProjectileRuntime.EntityCount,
      "A rejected full-pool create must remove its provisional entity root.");

    Assert(lifecycle.TryTerminate(handles[^1], ProjectileEndReason.DestroyedByCollision),
      "A live projectile at the last world slot should terminate.");
    AssertEqual(999, world.ProjectileRuntime.EntityCount,
      "Termination at the last slot must remove its root.");
    Assert(
      lifecycle.TryCreate(
        CreateIdentity(23, 1000),
        new ProjectileLifetimeStateComponent(timeLeft: 10),
        out ProjectileHandle reused),
      "Releasing the last world slot must make capacity reusable.");
    AssertEqual(handles[^1].Slot, reused.Slot,
      "The released highest slot must remain reusable under ascending allocation.");
    AssertEqual(1000, world.ProjectileRuntime.EntityCount,
      "Reallocation must register one new root after the released root is removed.");

    using var tieRuntime = new EntityRuntime();
    using var tieWorld = new WorldStorageRoot(tieRuntime);
    var tieLifecycle = new ProjectileLifecycleSystem(tieWorld);
    var tiedHandles = new ProjectileHandle[1000];
    for (int index = 0; index < tiedHandles.Length; index++)
    {
      Assert(
        tieLifecycle.TryCreate(
          CreateIdentity(23, index),
          new ProjectileLifetimeStateComponent(timeLeft: 10),
          out tiedHandles[index]),
        $"Tied projectile slot {index} should fill the ordinary world pool.");
    }

    Assert(
      tieLifecycle.TryCreate(
        CreateIdentity(23, 1000),
        new ProjectileLifetimeStateComponent(timeLeft: 30),
        out ProjectileHandle tiedReplacement),
      "A full ordinary pool should replace one of its equally old projectiles.");
    AssertEqual(tiedHandles[0].Slot, tiedReplacement.Slot,
      "A 1000-slot lifetime tie should choose the lowest eligible slot.");
    AssertEqual(tiedHandles[0].Generation + 1, tiedReplacement.Generation,
      "The 1000-slot tie replacement should advance the selected slot generation.");
    Assert(!ProjectileVerificationAccess.HasHandle(tieLifecycle, tiedHandles[0]),
      "The 1000-slot tie replacement should stale the selected handle.");
    AssertEqual(1000, tieWorld.Projectiles.ActiveCount,
      "The full-pool tie replacement should retain exactly 1000 active slots.");
    AssertEqual(1000, tieWorld.ProjectileIdentities.Count,
      "The full-pool tie replacement should retain exactly 1000 protocol mappings.");
    AssertEqual(1000, tieWorld.ProjectileRuntime.EntityCount,
      "The full-pool tie replacement should retain exactly 1000 entity roots.");
  }

  private static void VerifyOldestReplacementAndIdentityChange()
  {
    var slots = CreateBoundedSlots(2);
    var identities = new ProjectileIdentityIndex();
    using var runtime = new EntityRuntime();
    var lifecycle = new ProjectileLifecycleSystem(slots, identities, runtime);

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
    Assert(ProjectileVerificationAccess.HasHandle(lifecycle, first),
      "A non-selected projectile should remain active.");
    Assert(!ProjectileVerificationAccess.HasHandle(lifecycle, oldest),
      "The replaced handle must become stale.");
    Assert(
      !identities.TryGetHandle(new OwnerProjectileIdentity(new PlayerSlot(4), 40), out _),
      "Replacement should unregister the old owner identity.");
    Assert(
      identities.TryGetHandle(new OwnerProjectileIdentity(new PlayerSlot(5), 50), out var indexed),
      "Replacement should register the new owner identity.");
    AssertEqual(replacement, indexed, "The new identity should resolve to the new generation.");
    Assert(
      ProjectileVerificationAccess.HasHandle(lifecycle, replacement),
      "The replacement generation should expose its state.");
    AssertEqual(
      replacement.Slot.Value,
      ProjectileVerificationAccess.Read<ProjectileIdentityComponent>(lifecycle, replacement)
        .SlotIndex,
      "Replacement state should contain its assigned slot.");
  }

  private static void VerifyProtectedAndIneligiblePoolsRemainUnchanged()
  {
    var slots = CreateBoundedSlots(2);
    var identities = new ProjectileIdentityIndex();
    using var runtime = new EntityRuntime();
    var lifecycle = new ProjectileLifecycleSystem(slots, identities, runtime);
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
      ProjectileVerificationAccess.HasHandle(lifecycle, protectedHandle),
      "A network-important projectile must survive.");

    var duplicateSlots = CreateBoundedSlots(2);
    var duplicateIndex = new ProjectileIdentityIndex();
    using var duplicateRuntime = new EntityRuntime();
    var duplicateLifecycle = new ProjectileLifecycleSystem(duplicateSlots, duplicateIndex, duplicateRuntime);
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
      ProjectileVerificationAccess.HasHandle(duplicateLifecycle, existingFirst),
      "Rejected replacement must preserve the oldest slot.");
    Assert(
      ProjectileVerificationAccess.HasHandle(duplicateLifecycle, existingSecond),
      "Rejected replacement must preserve the duplicate owner slot.");

    var importantSlots = CreateBoundedSlots(2);
    var importantIndex = new ProjectileIdentityIndex();
    using var importantRuntime = new EntityRuntime();
    var importantLifecycle = new ProjectileLifecycleSystem(importantSlots, importantIndex, importantRuntime);
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
      ProjectileVerificationAccess.HasHandle(importantLifecycle, firstImportant),
      "No-victim rejection must preserve slot zero.");
    Assert(
      ProjectileVerificationAccess.HasHandle(importantLifecycle, secondImportant),
      "No-victim rejection must preserve slot one.");

    var highLifetimeSlots = CreateBoundedSlots(2);
    using var highLifetimeRuntime = new EntityRuntime();
    var highLifetimeLifecycle = new ProjectileLifecycleSystem(
      highLifetimeSlots,
      new ProjectileIdentityIndex(),
      highLifetimeRuntime);
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
    Assert(ProjectileVerificationAccess.HasHandle(highLifetimeLifecycle, highLifetimeFirst),
      "Sentinel-boundary rejection must preserve the first slot.");
    Assert(ProjectileVerificationAccess.HasHandle(highLifetimeLifecycle, highLifetimeSecond),
      "Sentinel-boundary rejection must preserve the second slot.");

    var tiedSlots = CreateBoundedSlots(2);
    using var tiedRuntime = new EntityRuntime();
    var tiedLifecycle = new ProjectileLifecycleSystem(tiedSlots, new ProjectileIdentityIndex(), tiedRuntime);
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
    using var runtime = new EntityRuntime();
    var lifecycle = new ProjectileLifecycleSystem(slots, identities, runtime);
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
    Assert(!ProjectileVerificationAccess.HasHandle(lifecycle, previous),
      "The previous handle must become stale.");
    Assert(
      identities.TryGetHandle(reusedIdentity, out ProjectileHandle indexed),
      "The reused owner identity should remain registered.");
    AssertEqual(replacement, indexed, "The reused identity should resolve to the new handle.");
  }

  private static void VerifyBorrowedFullPoolRejectsCreateWithoutLeakingCandidate()
  {
    var slots = CreateBoundedSlots(1);
    var identities = new ProjectileIdentityIndex();
    using var runtime = new EntityRuntime();
    var lifecycle = new ProjectileLifecycleSystem(slots, identities, runtime);
    Assert(lifecycle.TryCreate(
      CreateIdentity(22, 220),
      new ProjectileLifetimeStateComponent(timeLeft: 5),
      out ProjectileHandle original),
      "The full-pool borrow fixture should allocate its original projectile.");
    Assert(lifecycle.TryGetRuntimeHandle(original, out RuntimeEntityHandle originalRoot),
      "The original full-pool root should resolve.");
    Assert(lifecycle.TryGetEntityReference(original, out EntityReference originalReference),
      "The original full-pool root should expose its reference.");

    bool replacementRejected = false;
    Assert(runtime.TryInspect<ProjectileIdentityComponent>(
      originalRoot,
      (in ProjectileIdentityComponent borrowedIdentity) =>
      {
        _ = borrowedIdentity;
        try
        {
          _ = lifecycle.TryCreate(
            CreateIdentity(23, 230),
            new ProjectileLifetimeStateComponent(timeLeft: 20),
            out _);
        }
        catch (InvalidOperationException)
        {
          replacementRejected = true;
        }
      }),
      "The original root should remain inspectable while its replacement is rejected.");

    Assert(replacementRejected,
      "Oldest selection should reject a root whose component access is borrowed.");
    AssertEqual(1, slots.ActiveCount,
      "A borrowed-root rejection must preserve slot occupancy.");
    AssertEqual(1, identities.Count,
      "A borrowed-root rejection must preserve the identity index.");
    AssertEqual(1, runtime.EntityCount,
      "A borrowed-root rejection must remove its provisional candidate root.");
    Assert(identities.TryGetHandle(
      new OwnerProjectileIdentity(new PlayerSlot(22), 220),
      out ProjectileHandle current) && current == original,
      "A borrowed-root rejection must preserve the original identity mapping.");
    Assert(lifecycle.TryGetRuntimeHandle(original, out RuntimeEntityHandle currentRoot) &&
      currentRoot == originalRoot,
      "A borrowed-root rejection must preserve the original runtime root.");
    Assert(lifecycle.TryGetEntityReference(original, out EntityReference currentReference) &&
      currentReference == originalReference,
      "A borrowed-root rejection must preserve the original entity reference.");

    Assert(lifecycle.TryCreate(
      CreateIdentity(23, 230),
      new ProjectileLifetimeStateComponent(timeLeft: 20),
      out ProjectileHandle retried),
      "The full-pool replacement should be retryable after the borrow ends.");
    AssertEqual(original.Slot, retried.Slot,
      "A successful retry should replace the same bounded slot.");
    AssertEqual(original.Generation + 1, retried.Generation,
      "A successful retry should advance the slot generation exactly once.");
    AssertEqual(1, slots.ActiveCount,
      "A successful retry should retain the pool capacity.");
    AssertEqual(1, identities.Count,
      "A successful retry should retain one identity mapping.");
    AssertEqual(1, runtime.EntityCount,
      "A successful retry should remove the previous root and keep one new root.");
  }

  private static void VerifyBorrowedTerminationIsNoOpAndCanRetry()
  {
    var slots = CreateBoundedSlots(1);
    var identities = new ProjectileIdentityIndex();
    using var runtime = new EntityRuntime();
    var lifecycle = new ProjectileLifecycleSystem(slots, identities, runtime);
    Assert(lifecycle.TryCreate(
      CreateIdentity(24, 240),
      new ProjectileLifetimeStateComponent(timeLeft: 17),
      out ProjectileHandle handle),
      "The borrowed-termination fixture should create its projectile.");
    Assert(lifecycle.TryGetRuntimeHandle(handle, out RuntimeEntityHandle runtimeHandle),
      "The borrowed-termination root should resolve.");

    bool terminationAccepted = true;
    Assert(runtime.TryInspect<ProjectileIdentityComponent>(
      runtimeHandle,
      (in ProjectileIdentityComponent borrowedIdentity) =>
      {
        _ = borrowedIdentity;
        terminationAccepted = lifecycle.TryTerminate(
          handle,
          ProjectileEndReason.DestroyedByCollision);
      }),
      "The root should remain inspectable during the borrowed termination attempt.");

    Assert(!terminationAccepted,
      "A borrowed root should reject termination before lifecycle state changes.");
    AssertEqual(1, slots.ActiveCount,
      "A borrowed termination rejection must preserve the occupied slot.");
    AssertEqual(1, identities.Count,
      "A borrowed termination rejection must preserve the identity index.");
    AssertEqual(1, runtime.EntityCount,
      "A borrowed termination rejection must preserve the runtime root.");
    Assert(ProjectileVerificationAccess.HasHandle(lifecycle, handle),
      "The original handle must remain readable after borrowed termination is rejected.");
    AssertEqual(17,
      ProjectileVerificationAccess.Read<ProjectileLifetimeStateComponent>(lifecycle, handle)
        .TimeLeft,
      "A borrowed termination rejection must preserve lifetime state.");

    Assert(lifecycle.TryTerminate(handle, ProjectileEndReason.DestroyedByCollision),
      "Termination should succeed after the component borrow ends.");
    AssertEqual(0, slots.ActiveCount,
      "A successful retry should release the projectile slot.");
    AssertEqual(0, identities.Count,
      "A successful retry should unregister the projectile identity.");
    AssertEqual(0, runtime.EntityCount,
      "A successful retry should remove the root.");
  }

  private static void VerifyBorrowedNetworkTypeReplacementIsNoOpAndCanRetry()
  {
    var definitions = new ProjectileDefinitionCatalog(new[]
    {
      CreateNetworkDefinition(625),
      CreateNetworkDefinition(626),
    });
    var slots = CreateBoundedSlots(1);
    var identities = new ProjectileIdentityIndex();
    using var runtime = new EntityRuntime();
    var lifecycle = new ProjectileLifecycleSystem(slots, identities, runtime);
    var hydration = new ProjectileDefinitionHydrationContext(
      catalogRevision: 1,
      npcCapacity: 4,
      playerCapacity: 4);
    var initial = CreateNetworkCommand(625);
    Assert(lifecycle.TryApplyNetwork(initial, definitions, hydration, out ProjectileHandle original),
      "The network replacement fixture should create its initial root.");
    Assert(lifecycle.TryGetRuntimeHandle(original, out RuntimeEntityHandle originalRoot),
      "The initial network root should resolve.");
    Assert(lifecycle.TryGetEntityReference(original, out EntityReference originalReference),
      "The initial network root should expose its entity reference.");

    ProjectileNetworkApplyCommand typeReplacement = CreateNetworkCommand(626);
    bool replacementApplied = true;
    Assert(runtime.TryInspect<ProjectileIdentityComponent>(
      originalRoot,
      (in ProjectileIdentityComponent borrowedIdentity) =>
      {
        _ = borrowedIdentity;
        replacementApplied = lifecycle.TryApplyNetwork(
          typeReplacement,
          definitions,
          hydration,
          out _);
      }),
      "The initial network root should remain inspectable during a borrowed replacement attempt.");

    Assert(!replacementApplied,
      "A type replacement should decline while its current root is borrowed.");
    AssertEqual(1, slots.ActiveCount,
      "A borrowed type replacement must preserve slot occupancy.");
    AssertEqual(1, identities.Count,
      "A borrowed type replacement must preserve the owner identity index.");
    AssertEqual(1, runtime.EntityCount,
      "A borrowed type replacement must not publish or leak a candidate root.");
    Assert(lifecycle.TryGetRuntimeHandle(original, out RuntimeEntityHandle currentRoot) &&
      currentRoot == originalRoot,
      "A borrowed type replacement must preserve the original runtime root.");
    Assert(lifecycle.TryGetEntityReference(original, out EntityReference currentReference) &&
      currentReference == originalReference,
      "A borrowed type replacement must preserve the original entity reference.");
    AssertEqual(625,
      ProjectileVerificationAccess.Read<ProjectileDefinitionComponent>(lifecycle, original)
        .ProjectileType,
      "A borrowed type replacement must preserve the original projectile type.");

    Assert(lifecycle.TryApplyNetwork(
      typeReplacement,
      definitions,
      hydration,
      out ProjectileHandle retried),
      "The type replacement should be retryable after the borrow ends.");
    AssertEqual(original.Slot, retried.Slot,
      "A successful type replacement should retain the selected local slot.");
    AssertEqual(original.Generation + 1, retried.Generation,
      "A successful type replacement should advance the slot generation exactly once.");
    Assert(!ProjectileVerificationAccess.HasHandle(lifecycle, original),
      "The original generation should become stale after the successful retry.");
    AssertEqual(626,
      ProjectileVerificationAccess.Read<ProjectileDefinitionComponent>(lifecycle, retried)
        .ProjectileType,
      "The retry should publish the requested replacement type.");
    AssertEqual(1, slots.ActiveCount,
      "A successful type replacement should retain one occupied slot.");
    AssertEqual(1, identities.Count,
      "A successful type replacement should retain one owner identity mapping.");
    AssertEqual(1, runtime.EntityCount,
      "A successful type replacement should remove the previous root.");
  }

  private static ProjectileDefinition CreateNetworkDefinition(int projectileType)
  {
    return new ProjectileDefinition(
      new ProjectileIdentityDefinition(projectileType, null, NeedsUuid: false),
      new ProjectileGeometryDefinition(8, 8, 1.0f, true, false),
      new ProjectileBehaviorDefinition(1, 0, 120),
      new ProjectileCombatDefinition(0, 0.0f, true, false),
      new ProjectilePenetrationDefinition(1, 1, true),
      new ProjectileCapabilitiesDefinition(false, false, false, false),
      new ProjectilePresentationDefinition(1, 0.0f, false));
  }

  private static ProjectileNetworkApplyCommand CreateNetworkCommand(int projectileType)
  {
    return new ProjectileNetworkApplyCommand(
      ownerSlot: 25,
      identity: 250,
      projectileType: projectileType,
      position: System.Numerics.Vector2.Zero,
      velocity: System.Numerics.Vector2.One,
      damage: 4,
      originalDamage: 4,
      knockback: 0.0f,
      projectileUuid: -1);
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
