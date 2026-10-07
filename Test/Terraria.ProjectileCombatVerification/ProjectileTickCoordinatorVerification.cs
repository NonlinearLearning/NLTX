using System;
using System.Collections.Generic;
using System.Numerics;

using EntityEcs;

using Terraria.Content;
using Terraria.Projectile;
using Terraria.Relationships;
using Terraria.WorldStorage;

internal static class ProjectileTickCoordinatorVerification
{
  public static void Run()
  {
    var definitions = new ProjectileDefinitionCatalog(new[]
    {
      CreateDefinition(typeId: 31, extraUpdates: 2),
      CreateDefinition(typeId: 32, extraUpdates: 0),
      CreateDefinition(typeId: 33, extraUpdates: 2),
      CreateDefinition(typeId: 35, extraUpdates: 2),
      CreateDefinition(typeId: 36, extraUpdates: 0),
      CreateDefinition(typeId: 37, extraUpdates: 0),
    });
    var slots = new EntitySlotStore<WorldEntityState, ProjectileSlot>(
      static slot => slot.Value,
      static value => new ProjectileSlot(value),
      maximumCapacity: 4);
    using var runtime = new EntityRuntime();
    var lifecycle = new ProjectileLifecycleSystem(slots, new ProjectileIdentityIndex(), runtime);
    var context = new ProjectileDefinitionHydrationContext(
      catalogRevision: 1,
      npcCapacity: 4,
      playerCapacity: 4);

    Assert(
      lifecycle.TrySpawn(
        CreateSpawn(typeId: 31, center: new Vector2(10, 10)),
        definitions,
        context,
        out ProjectileHandle first),
      "The first projectile should be available to the ordered pass.");
    Assert(
      lifecycle.TrySpawn(
        CreateSpawn(typeId: 32, center: new Vector2(20, 20)),
        definitions,
        context,
        out ProjectileHandle second),
      "The second projectile should be available to the ordered pass.");

    var adapter = new RecordingAdapter();
    ProjectileTickResult result = new ProjectileTickCoordinator(lifecycle).Tick(adapter);

    AssertEqual(2, result.ActiveProjectileCount, "Both active slots should be visited.");
    AssertEqual(0, result.SkippedInactiveCount, "No active slot should be skipped.");
    AssertEqual(4, result.UpdateStepCount, "ExtraUpdates should add ordered substeps.");
    AssertSequenceEqual(
      new[]
      {
        "pre",
        "slot-0-step-0-of-3",
        "slot-0-step-1-of-3",
        "slot-0-step-2-of-3",
        "slot-1-step-0-of-1",
        "post",
      },
      adapter.Events,
      "The pass must preserve pre, ascending slots, extra updates, and post order.");
    AssertEqual(first.Slot.Value, adapter.Handles[0].Slot.Value,
      "The first callback should retain the first local slot.");
    AssertEqual(second.Slot.Value, adapter.Handles[3].Slot.Value,
      "The second callback should retain the second local slot.");

    Assert(ProjectileVerificationAccess.HasHandle(lifecycle, first),
      "The first projectile should remain active after the first pass.");
    ProjectileVerificationAccess.Edit(
      lifecycle,
      first,
      (ref ProjectileHitImmunityStateComponent immunity) =>
        ProjectileHitImmunitySystem.SetLocalNpcImmunity(ref immunity, 0, 2));

    Assert(
      lifecycle.TrySpawn(
        CreateSpawn(typeId: 33, center: new Vector2(30, 30)),
        definitions,
        context,
        out ProjectileHandle expiring),
      "A short-lived projectile should be available for the next pass.");

    var secondAdapter = new RecordingAdapter();
    ProjectileTickResult secondResult =
      new ProjectileTickCoordinator(lifecycle).Tick(secondAdapter);

    AssertEqual(
      5,
      secondResult.UpdateStepCount,
      "Lifetime expiration should stop the projectile's remaining extra-update substeps.");
    AssertEqual(
      1,
      secondResult.LifetimeExpiredCount,
      "The coordinator should report the projectile released by its lifetime tail.");
    AssertEqual(
      1,
      ProjectileVerificationAccess.Read<ProjectileHitImmunityStateComponent>(
        lifecycle,
        first).LocalNpcImmunityTicks[0],
      "Local immunity must advance once per regular Update, not once per extra substep.");
    Assert(
      !ProjectileVerificationAccess.HasHandle(lifecycle, expiring),
      "A projectile reaching zero timeLeft must be released after its substeps.");
    AssertEqual(2, slots.ActiveCount,
      "Lifetime expiration must release exactly one projectile slot.");

    VerifyAdapterTerminationStopsRemainingSubsteps();
    VerifyReplacementDuringCallbackStopsOldSubsteps();
    VerifyCallbackReplacementInUnvisitedSlotIsScanned();
    VerifyCallbackReplacementInVisitedSlotWaitsUntilNextPass();
    VerifyPreparationContinueAndReturnSkipUpdateAndTail();
    VerifyPreparationChangesPreserveCallbackComponentWrites();
    VerifyType640PreparationContinuesBeforeUpdateAndTail();
    VerifyUpdateContinueAndReturnSkipLifetimeTail();
    VerifyHitLimitTailCleansEntityRoot();
    VerifyWorldBoundaryDeactivationPreservesTimeLeft();
    VerifyOwnerMovementAdapterReceivesReferenceOnlyForModeFour();
    VerifyTrailDustEffectUsesTypedRequest();
  }

  private static void VerifyAdapterTerminationStopsRemainingSubsteps()
  {
    var definitions = new ProjectileDefinitionCatalog(new[]
    {
      CreateDefinition(typeId: 34, extraUpdates: 2),
    });
    var slots = new EntitySlotStore<WorldEntityState, ProjectileSlot>(
      static slot => slot.Value,
      static value => new ProjectileSlot(value),
      maximumCapacity: 1);
    using var runtime = new EntityRuntime();
    var lifecycle = new ProjectileLifecycleSystem(slots, new ProjectileIdentityIndex(), runtime);
    var context = new ProjectileDefinitionHydrationContext(
      catalogRevision: 1,
      npcCapacity: 1,
      playerCapacity: 1);
    Assert(
      lifecycle.TrySpawn(
        CreateSpawn(typeId: 34, center: new Vector2(40, 40)),
        definitions,
        context,
        out ProjectileHandle handle),
      "A projectile for adapter termination should be available.");

    var adapter = new TerminatingAdapter(lifecycle);
    ProjectileTickResult result = new ProjectileTickCoordinator(lifecycle).Tick(adapter);

    AssertEqual(1, result.ActiveProjectileCount,
      "The terminating projectile should be counted once before its callback.");
    AssertEqual(1, result.UpdateStepCount,
      "Termination during the first substep must stop remaining extra updates.");
    AssertEqual(1, adapter.UpdateCount,
      "The adapter must receive exactly one callback before lifecycle termination.");
    AssertSequenceEqual(
      new[] { "pre", "update", "post" },
      adapter.Events,
      "PostUpdateAllProjectiles must still run after adapter termination.");
    Assert(!ProjectileVerificationAccess.HasHandle(lifecycle, handle),
      "Adapter termination must release the projectile before the next substep.");
  }

  private static void VerifyReplacementDuringCallbackStopsOldSubsteps()
  {
    var definitions = new ProjectileDefinitionCatalog(new[]
    {
      CreateDefinition(typeId: 35, extraUpdates: 2),
      CreateDefinition(typeId: 36, extraUpdates: 0),
    });
    var slots = new EntitySlotStore<WorldEntityState, ProjectileSlot>(
      static slot => slot.Value,
      static value => new ProjectileSlot(value),
      maximumCapacity: 1);
    using var runtime = new EntityRuntime();
    var lifecycle = new ProjectileLifecycleSystem(
      slots,
      new ProjectileIdentityIndex(),
      runtime);
    var context = new ProjectileDefinitionHydrationContext(
      catalogRevision: 1,
      npcCapacity: 1,
      playerCapacity: 1);
    Assert(
      lifecycle.TrySpawn(
        CreateSpawn(typeId: 35, center: new Vector2(50, 50)),
        definitions,
        context,
        out ProjectileHandle original),
      "A multi-substep projectile should allocate before callback replacement.");
    Assert(lifecycle.TryGetEntityReference(original, out EntityReference originalReference),
      "The original projectile must expose its entity reference before update.");

    var adapter = new ReplacingAdapter(lifecycle, definitions, context);
    ProjectileTickResult result = new ProjectileTickCoordinator(lifecycle).Tick(adapter);

    AssertEqual(1, result.UpdateStepCount,
      "Replacing the current slot from a callback must stop the old projectile's remaining substeps.");
    AssertEqual(1, adapter.UpdateCount,
      "The replaced generation must not receive another callback in the same slot scan.");
    AssertEqual(1, adapter.PostUpdateCount,
      "The pass post callback must still run after in-place slot replacement.");
    AssertEqual(original.Slot, adapter.Replacement.Slot,
      "Full-pool callback spawn should replace the current projectile slot.");
    AssertEqual(original.Generation + 1, adapter.Replacement.Generation,
      "Callback replacement must advance the projectile slot generation.");
    Assert(!ProjectileVerificationAccess.HasHandle(lifecycle, original),
      "The original generation must be stale after callback replacement.");
    Assert(ProjectileVerificationAccess.HasHandle(lifecycle, adapter.Replacement) &&
      ProjectileVerificationAccess.Read<ProjectileDefinitionComponent>(
        lifecycle,
        adapter.Replacement).ProjectileType == 36,
      "The replacement entity must remain active after the old callback returns.");
    Assert(lifecycle.TryGetEntityReference(adapter.Replacement, out EntityReference replacementReference) &&
      replacementReference.EntityId != originalReference.EntityId,
      "Callback replacement must produce a new entity root.");
    AssertEqual(1, runtime.EntityCount,
      "Callback replacement must not leak or retain both entity roots.");
  }

  private static void VerifyCallbackReplacementInUnvisitedSlotIsScanned()
  {
    var slots = CreateTwoProjectileSlots();
    using var runtime = new EntityRuntime();
    var lifecycle = new ProjectileLifecycleSystem(slots, new ProjectileIdentityIndex(), runtime);
    var definitions = CreateCallbackDefinitions();
    var hydrationContext = CreateCallbackHydrationContext();
    Assert(
      lifecycle.TrySpawn(
        CreateSpawn(typeId: 35, center: new Vector2(70, 70)),
        definitions,
        hydrationContext,
        out ProjectileHandle current),
      "The callback source projectile should allocate at slot zero.");
    Assert(
      lifecycle.TrySpawn(
        CreateSpawn(typeId: 37, center: new Vector2(80, 80)),
        definitions,
        hydrationContext,
        out ProjectileHandle future),
      "A short-lived projectile should occupy the unvisited slot.");

    var adapter = new SpawnDuringCallbackAdapter(
      lifecycle,
      definitions,
      hydrationContext,
      triggerSlot: current.Slot.Value);
    ProjectileTickResult result = new ProjectileTickCoordinator(lifecycle).Tick(adapter);

    AssertEqual(4, result.UpdateStepCount,
      "The current projectile's three substeps and its replacement's future-slot step should run.");
    AssertSequenceEqual(
      new[] { 0, 0, 0, 1 },
      adapter.CallbackSlots,
      "A callback replacement in an unvisited slot should be visible later in the same ordered scan.");
    AssertEqual(future.Slot, adapter.Replacement.Slot,
      "The shorter-lived unvisited projectile should be replaced.");
    AssertEqual(future.Generation + 1, adapter.Replacement.Generation,
      "Replacing an unvisited slot should advance its generation.");
  }

  private static void VerifyCallbackReplacementInVisitedSlotWaitsUntilNextPass()
  {
    var slots = CreateTwoProjectileSlots();
    using var runtime = new EntityRuntime();
    var lifecycle = new ProjectileLifecycleSystem(slots, new ProjectileIdentityIndex(), runtime);
    var definitions = CreateCallbackDefinitions();
    var hydrationContext = CreateCallbackHydrationContext();
    Assert(
      lifecycle.TrySpawn(
        CreateSpawn(typeId: 37, center: new Vector2(90, 90)),
        definitions,
        hydrationContext,
        out ProjectileHandle past),
      "A short-lived projectile should allocate in the slot that will be visited first.");
    Assert(
      lifecycle.TrySpawn(
        CreateSpawn(typeId: 35, center: new Vector2(100, 100)),
        definitions,
        hydrationContext,
        out ProjectileHandle current),
      "The replacement callback source should allocate in slot one.");

    var adapter = new SpawnDuringCallbackAdapter(
      lifecycle,
      definitions,
      hydrationContext,
      triggerSlot: current.Slot.Value);
    ProjectileTickResult result = new ProjectileTickCoordinator(lifecycle).Tick(adapter);

    AssertEqual(4, result.UpdateStepCount,
      "Replacing an already visited slot should not add an update to the current pass.");
    AssertSequenceEqual(
      new[] { 0, 1, 1, 1 },
      adapter.CallbackSlots,
      "An already visited slot should not be revisited after callback replacement.");
    AssertEqual(past.Slot, adapter.Replacement.Slot,
      "The shortest-lived already visited projectile should be replaced.");
    AssertEqual(past.Generation + 1, adapter.Replacement.Generation,
      "Replacing an already visited slot should advance its generation.");
  }

  private static void VerifyPreparationContinueAndReturnSkipUpdateAndTail()
  {
    var slots = CreateTwoProjectileSlots();
    using var runtime = new EntityRuntime();
    var lifecycle = new ProjectileLifecycleSystem(slots, new ProjectileIdentityIndex(), runtime);
    var definitions = CreateCallbackDefinitions();
    var hydrationContext = CreateCallbackHydrationContext();
    Assert(
      lifecycle.TrySpawn(
        CreateSpawn(typeId: 35, center: new Vector2(120, 120)),
        definitions,
        hydrationContext,
        out ProjectileHandle handle),
      "A multi-substep projectile should allocate before preparation early-outs.");
    Assert(ProjectileVerificationAccess.HasHandle(lifecycle, handle),
      "The projectile should resolve before preparation early-outs.");
    int timeLeftBefore = ProjectileVerificationAccess.Read<ProjectileLifetimeStateComponent>(
      lifecycle,
      handle).TimeLeft;
    var adapter = new ControlFlowAdapter(
      new[]
      {
        ProjectileTickPreparationResult.Continued,
        ProjectileTickPreparationResult.Returned,
      },
      Array.Empty<ProjectileTickSubstepResult>());

    ProjectileTickResult result = new ProjectileTickCoordinator(lifecycle).Tick(adapter);

    AssertEqual(2, result.UpdateStepCount,
      "Preparation continue and return paths should each consume one ordered substep.");
    AssertEqual(2, adapter.PreparationCount,
      "The preparation adapter should receive the continued and returned substeps.");
    AssertEqual(0, adapter.UpdateCount,
      "Preparation early-outs should skip the projectile update callback.");
    AssertEqual(1, adapter.PostUpdateCount,
      "The global post callback should run after preparation early-outs.");
    Assert(ProjectileVerificationAccess.HasHandle(lifecycle, handle),
      "The projectile should remain active after preparation early-outs.");
    AssertEqual(timeLeftBefore,
      ProjectileVerificationAccess.Read<ProjectileLifetimeStateComponent>(lifecycle, handle)
        .TimeLeft,
      "Preparation early-outs should skip the lifetime tail.");
  }

  private static void VerifyUpdateContinueAndReturnSkipLifetimeTail()
  {
    var slots = CreateTwoProjectileSlots();
    using var runtime = new EntityRuntime();
    var lifecycle = new ProjectileLifecycleSystem(slots, new ProjectileIdentityIndex(), runtime);
    var definitions = CreateCallbackDefinitions();
    var hydrationContext = CreateCallbackHydrationContext();
    Assert(
      lifecycle.TrySpawn(
        CreateSpawn(typeId: 35, center: new Vector2(130, 130)),
        definitions,
        hydrationContext,
        out ProjectileHandle handle),
      "A multi-substep projectile should allocate before update early-outs.");
    Assert(ProjectileVerificationAccess.HasHandle(lifecycle, handle),
      "The projectile should resolve before update early-outs.");
    int timeLeftBefore = ProjectileVerificationAccess.Read<ProjectileLifetimeStateComponent>(
      lifecycle,
      handle).TimeLeft;
    var adapter = new ControlFlowAdapter(
      Array.Empty<ProjectileTickPreparationResult>(),
      new[]
      {
        ProjectileTickSubstepResult.Continued,
        ProjectileTickSubstepResult.Returned,
      });

    ProjectileTickResult result = new ProjectileTickCoordinator(lifecycle).Tick(adapter);

    AssertEqual(2, result.UpdateStepCount,
      "Update continue and return paths should each consume one ordered substep.");
    AssertEqual(2, adapter.UpdateCount,
      "The projectile update adapter should receive both early-out substeps.");
    AssertEqual(1, adapter.PostUpdateCount,
      "The global post callback should run after update early-outs.");
    Assert(ProjectileVerificationAccess.HasHandle(lifecycle, handle),
      "The projectile should remain active after update early-outs.");
    AssertEqual(timeLeftBefore,
      ProjectileVerificationAccess.Read<ProjectileLifetimeStateComponent>(lifecycle, handle)
        .TimeLeft,
      "Update early-outs should skip the lifetime tail.");
  }

  private static void VerifyPreparationChangesPreserveCallbackComponentWrites()
  {
    var definitions = new ProjectileDefinitionCatalog(new[]
    {
      CreateDefinition(typeId: 38, extraUpdates: 0),
    });
    var slots = new EntitySlotStore<WorldEntityState, ProjectileSlot>(
      static slot => slot.Value,
      static value => new ProjectileSlot(value),
      maximumCapacity: 1);
    using var runtime = new EntityRuntime();
    var lifecycle = new ProjectileLifecycleSystem(slots, new ProjectileIdentityIndex(), runtime);
    Assert(
      lifecycle.TrySpawn(
        CreateSpawn(typeId: 38, center: new Vector2(90, 90)),
        definitions,
        CreateCallbackHydrationContext(),
        out ProjectileHandle handle),
      "A projectile should spawn for same-root preparation mutation coverage.");
    Assert(ProjectileVerificationAccess.HasHandle(lifecycle, handle),
      "The projectile should resolve before callback mutation.");
    int timeLeftBefore = ProjectileVerificationAccess.Read<ProjectileLifetimeStateComponent>(
      lifecycle,
      handle).TimeLeft;
    var adapter = new PreparationMutationAdapter(lifecycle);

    ProjectileTickResult result = new ProjectileTickCoordinator(lifecycle).Tick(adapter);

    AssertEqual(3, result.UpdateStepCount,
      "The live NumUpdates value written by preparation should extend this update to three substeps.");
    AssertEqual(3, adapter.PreparationCount,
      "Each live extended substep should run preparation once.");
    AssertEqual(3, adapter.UpdateCount,
      "Each completed preparation step should reach the update adapter.");
    Assert(ProjectileVerificationAccess.HasHandle(lifecycle, handle),
      "The same projectile root should remain active after callback edits.");
    AssertEqual(42.0f,
      ProjectileVerificationAccess.Read<ProjectileBehaviorStateComponent>(lifecycle, handle).Ai0,
      "Applying a bounded prefix change must preserve callback AI edits on the same root.");
    AssertEqual(1,
      ProjectileVerificationAccess.Read<ProjectileUpdateCadenceComponent>(lifecycle, handle)
        .ExtraUpdates,
      "Applying a bounded prefix change must preserve callback cadence edits on the same root.");
    AssertEqual(-1,
      ProjectileVerificationAccess.Read<ProjectileTrajectoryStateComponent>(lifecycle, handle)
        .NumUpdates,
      "The callback's live NumUpdates extension should be consumed by the ordered loop.");
    AssertEqual(timeLeftBefore - 3,
      ProjectileVerificationAccess.Read<ProjectileLifetimeStateComponent>(lifecycle, handle)
        .TimeLeft,
      "The coordinator should run the lifetime tail once for each completed extended substep.");
  }

  private static void VerifyType640PreparationContinuesBeforeUpdateAndTail()
  {
    var definitions = new ProjectileDefinitionCatalog(new[]
    {
      CreateDefinition(typeId: 640, extraUpdates: 0),
    });
    var slots = new EntitySlotStore<WorldEntityState, ProjectileSlot>(
      static slot => slot.Value,
      static value => new ProjectileSlot(value),
      maximumCapacity: 1);
    using var runtime = new EntityRuntime();
    var lifecycle = new ProjectileLifecycleSystem(slots, new ProjectileIdentityIndex(), runtime);
    Assert(
      lifecycle.TrySpawn(
        CreateSpawn(typeId: 640, center: new Vector2(95, 95)),
        definitions,
        CreateCallbackHydrationContext(),
        out ProjectileHandle handle),
      "A type-640 projectile should spawn for countdown branch coverage.");
    Assert(ProjectileVerificationAccess.HasHandle(lifecycle, handle),
      "The type-640 projectile should resolve before the countdown.");
    ProjectileVerificationAccess.Edit(
      lifecycle,
      handle,
      (ref ProjectileBehaviorStateComponent behavior) => behavior.Ai1 = 1.0f);
    int timeLeftBefore = ProjectileVerificationAccess.Read<ProjectileLifetimeStateComponent>(
      lifecycle,
      handle).TimeLeft;
    var adapter = new RecordingAdapter();

    ProjectileTickResult result = new ProjectileTickCoordinator(lifecycle).Tick(adapter);

    AssertEqual(1, result.UpdateStepCount,
      "The type-640 countdown should consume only its current ordered preparation step.");
    AssertSequenceEqual(new[] { "pre", "post" }, adapter.Events,
      "The type-640 countdown should continue before update and trail/lifetime tail.");
    Assert(ProjectileVerificationAccess.HasHandle(lifecycle, handle),
      "The type-640 projectile should remain active after its countdown.");
    AssertEqual(0.0f,
      ProjectileVerificationAccess.Read<ProjectileBehaviorStateComponent>(lifecycle, handle).Ai1,
      "The type-640 preparation branch should decrement AI1 once.");
    AssertEqual(timeLeftBefore,
      ProjectileVerificationAccess.Read<ProjectileLifetimeStateComponent>(lifecycle, handle)
        .TimeLeft,
      "The type-640 continue branch should skip the lifetime tail.");
  }

  private static void VerifyWorldBoundaryDeactivationPreservesTimeLeft()
  {
    var slots = CreateTwoProjectileSlots();
    using var runtime = new EntityRuntime();
    var lifecycle = new ProjectileLifecycleSystem(slots, new ProjectileIdentityIndex(), runtime);
    var definitions = new ProjectileDefinitionCatalog(new[]
    {
      CreateDefinition(typeId: 31, extraUpdates: 2),
    });
    var hydrationContext = CreateCallbackHydrationContext();
    Assert(
      lifecycle.TrySpawn(
        CreateSpawn(typeId: 31, center: new Vector2(-20, -20)),
        definitions,
        hydrationContext,
        out ProjectileHandle handle),
      "An out-of-bounds projectile should allocate before boundary deactivation.");
    Assert(ProjectileVerificationAccess.HasHandle(lifecycle, handle),
      "The projectile should resolve before boundary deactivation.");
    int timeLeftBefore = ProjectileVerificationAccess.Read<ProjectileLifetimeStateComponent>(
      lifecycle,
      handle).TimeLeft;
    var adapter = new RecordingAdapter();

    ProjectileTickResult result = new ProjectileTickCoordinator(lifecycle).Tick(
      adapter,
      new ProjectileWorldBounds(0, 0, 100, 100));

    AssertEqual(1, result.UpdateStepCount,
      "World-boundary deactivation should stop before the projectile callback.");
    AssertSequenceEqual(new[] { "pre", "post" }, adapter.Events,
      "World-boundary deactivation should skip projectile behavior and still run the post callback.");
    Assert(!ProjectileVerificationAccess.HasHandle(lifecycle, handle),
      "Boundary deactivation should release its slot and invalidate the old handle.");
    var boundaryLifetime = new ProjectileLifetimeStateComponent(timeLeft: timeLeftBefore);
    Assert(ProjectileLifetimeSystem.CommitWorldBoundaryDeactivation(ref boundaryLifetime),
      "World-boundary deactivation should transition an active lifetime state.");
    Assert(!boundaryLifetime.Active &&
      boundaryLifetime.EndReason == ProjectileEndReason.WorldBoundary,
      "The lifetime owner should record non-Kill boundary deactivation.");
    AssertEqual(timeLeftBefore, boundaryLifetime.TimeLeft,
      "World-boundary deactivation must preserve the remaining time-left value.");
  }

  private static void VerifyHitLimitTailCleansEntityRoot()
  {
    var definitions = new ProjectileDefinitionCatalog(new[]
    {
      CreateDefinition(
        typeId: 39,
        extraUpdates: 0,
        stopsDealingDamageWhenDepleted: false),
    });
    var slots = new EntitySlotStore<WorldEntityState, ProjectileSlot>(
      static slot => slot.Value,
      static value => new ProjectileSlot(value),
      maximumCapacity: 1);
    using var runtime = new EntityRuntime();
    var identities = new ProjectileIdentityIndex();
    var lifecycle = new ProjectileLifecycleSystem(slots, identities, runtime);
    Assert(lifecycle.TrySpawn(
      CreateSpawn(typeId: 39, center: new Vector2(180, 180)),
      definitions,
      CreateCallbackHydrationContext(),
      out ProjectileHandle handle),
      "A one-hit projectile should spawn for hit-limit tail coverage.");

    var adapter = new PenetrationExhaustingAdapter(lifecycle);
    ProjectileTickResult result = new ProjectileTickCoordinator(lifecycle).Tick(adapter);

    AssertEqual(1, adapter.UpdateCount,
      "The accepted hit should be submitted once before the hit-limit tail.");
    AssertEqual(1, result.UpdateStepCount,
      "The depleted projectile should complete the hit substep before tail termination.");
    AssertEqual(0, result.LifetimeExpiredCount,
      "Hit-limit termination should remain distinct from lifetime expiration.");
    Assert(!ProjectileVerificationAccess.HasHandle(lifecycle, handle),
      "Hit-limit termination should invalidate the depleted projectile handle.");
    AssertEqual(0, slots.ActiveCount,
      "Hit-limit termination should release the projectile slot.");
    AssertEqual(0, identities.Count,
      "Hit-limit termination should unregister the projectile identity.");
    Assert(!identities.TryGetIdentity(handle, out _),
      "The terminated handle should no longer appear in the identity index.");
    AssertEqual(0, runtime.EntityCount,
      "Hit-limit termination should remove the projectile entity root.");
  }

  private static void VerifyOwnerMovementAdapterReceivesReferenceOnlyForModeFour()
  {
    var definitions = new ProjectileDefinitionCatalog(new[]
    {
      CreateDefinition(typeId: 933, extraUpdates: 0),
      CreateDefinition(typeId: 31, extraUpdates: 0),
    });
    var slots = CreateTwoProjectileSlots();
    using var runtime = new EntityRuntime();
    var lifecycle = new ProjectileLifecycleSystem(
      slots,
      new ProjectileIdentityIndex(),
      runtime);
    var hydrationContext = CreateCallbackHydrationContext();
    var ownerHandle = runtime.CreateEntity();
    Assert(runtime.TryPublishEntity(ownerHandle),
      "The owner entity should publish before the Projectile references it.");
    Assert(runtime.TryGetReference(
      ownerHandle,
      EntityReferenceScope.Player,
      out EntityReference ownerReference),
      "The owner entity should expose its Player-scoped reference.");

    Assert(lifecycle.TrySpawn(
      CreateSpawn(933, new Vector2(150, 150), ownerReference),
      definitions,
      hydrationContext,
      out _),
      "The mode-four trail projectile should spawn with the owner reference.");
    Assert(lifecycle.TrySpawn(
      CreateSpawn(31, new Vector2(160, 160), ownerReference),
      definitions,
      hydrationContext,
      out _),
      "A non-mode-four projectile should spawn with the same owner reference.");

    var adapter = new OwnerMovementAdapter(ownerReference);
    new ProjectileTickCoordinator(lifecycle).Tick(adapter);

    AssertEqual(1, adapter.OwnerMovementRequestCount,
      "Only trailing mode four should request the owner movement value.");
    AssertEqual(ownerReference, adapter.ReceivedOwnerReference,
      "The adapter should receive the owner's scoped reference without a projectile facade.");
  }

  private static void VerifyTrailDustEffectUsesTypedRequest()
  {
    var definitions = new ProjectileDefinitionCatalog(new[]
    {
      CreateDefinition(typeId: 466, extraUpdates: 0),
    });
    using var runtime = new EntityRuntime();
    var lifecycle = new ProjectileLifecycleSystem(
      CreateTwoProjectileSlots(),
      new ProjectileIdentityIndex(),
      runtime);
    Assert(
      lifecycle.TrySpawn(
        CreateSpawn(typeId: 466, center: new Vector2(170, 180)),
        definitions,
        CreateCallbackHydrationContext(),
        out ProjectileHandle handle),
      "A stationary mode-one trail projectile should allocate for its effect callback.");
    Assert(
      lifecycle.TryGetRuntimeHandle(handle, out RuntimeEntityHandle projectileRuntimeHandle),
      "The mode-one trail projectile should resolve before its update.");
    Vector2 previousTrailPosition = new(160, 170);
    Assert(
      runtime.TryEdit<ProjectileTrailCacheComponent>(
        projectileRuntimeHandle,
        (ref ProjectileTrailCacheComponent trail) =>
          trail.OldPositions[^2] = previousTrailPosition),
      "The prior trail point should be seeded through the owning EntityRuntime.");
    var adapter = new TrailDustAdapter();

    ProjectileTickResult result = new ProjectileTickCoordinator(lifecycle).Tick(adapter);

    AssertEqual(1, result.UpdateStepCount,
      "The stationary mode-one projectile should complete one update before its trail effect.");
    AssertSequenceEqual(
      new[] { "pre", "update", "dust", "post" },
      adapter.Events,
      "The typed dust request should execute after update and trail recording.");
    Assert(adapter.Request.HasValue,
      "A stationary projectile with the mode-one sparse trail should produce a dust request.");
    ProjectileTrailDustRequest request = adapter.Request ??
      throw new InvalidOperationException(
        "The mode-one trail callback did not capture its dust request.");
    AssertEqual(466, request.ProjectileType,
      "The effect callback should receive the projectile type as a value.");
    AssertEqual(previousTrailPosition, request.Position,
      "The effect callback should receive the oldest shifted trail position as a value.");
  }

  private static EntitySlotStore<WorldEntityState, ProjectileSlot> CreateTwoProjectileSlots()
  {
    return new EntitySlotStore<WorldEntityState, ProjectileSlot>(
      static slot => slot.Value,
      static value => new ProjectileSlot(value),
      maximumCapacity: 2);
  }

  private static ProjectileDefinitionCatalog CreateCallbackDefinitions()
  {
    return new ProjectileDefinitionCatalog(new[]
    {
      CreateDefinition(typeId: 35, extraUpdates: 2),
      CreateDefinition(typeId: 36, extraUpdates: 0),
      CreateDefinition(typeId: 37, extraUpdates: 0),
    });
  }

  private static ProjectileDefinitionHydrationContext CreateCallbackHydrationContext()
  {
    return new ProjectileDefinitionHydrationContext(
      catalogRevision: 1,
      npcCapacity: 2,
      playerCapacity: 2);
  }

  private static ProjectileSpawnCommand CreateSpawn(int typeId, Vector2 center)
  {
    return new ProjectileSpawnCommand(
      typeId,
      new ProjectileOwnerReference(EntityReference.None, legacyOwnerSlot: 1),
      center,
      Vector2.Zero,
      damage: 1,
      originalDamage: 1,
      knockback: 0.0f);
  }

  private static ProjectileSpawnCommand CreateSpawn(
    int typeId,
    Vector2 center,
    EntityReference ownerReference)
  {
    return new ProjectileSpawnCommand(
      typeId,
      new ProjectileOwnerReference(ownerReference, legacyOwnerSlot: 1),
      center,
      Vector2.Zero,
      damage: 1,
      originalDamage: 1,
      knockback: 0.0f);
  }

  private static ProjectileDefinition CreateDefinition(
    int typeId,
    int extraUpdates,
    bool stopsDealingDamageWhenDepleted = true)
  {
    var penetration = new ProjectilePenetrationDefinition(
      1,
      1,
      stopsDealingDamageWhenDepleted)
    {
      UsesLocalNpcImmunity = true,
    };
    return new ProjectileDefinition(
      new ProjectileIdentityDefinition(typeId, null, NeedsUuid: false),
      new ProjectileGeometryDefinition(8, 8, 1.0f, true, false),
      new ProjectileBehaviorDefinition(
        1,
        extraUpdates,
        typeId == 33 ? 1 : typeId == 37 ? 5 : 30),
      new ProjectileCombatDefinition(0, 0.0f, true, false),
      penetration,
      new ProjectileCapabilitiesDefinition(false, false, false, false),
      new ProjectilePresentationDefinition(1, 0.0f, false));
  }

  private sealed class RecordingAdapter : IProjectileTickAdapter
  {
    public List<string> Events { get; } = new();

    public List<ProjectileHandle> Handles { get; } = new();

    public void PreUpdateAllProjectiles()
    {
      Events.Add("pre");
    }

    public void UpdateProjectile(ProjectileTickContext context)
    {
      Events.Add(
        $"slot-{context.SlotIndex}-step-{context.SubstepIndex}-of-{context.SubstepCount}");
      Handles.Add(context.Handle);
      AssertEqual(
        context.Handle.Slot.Value,
        context.SlotIndex,
        "The callback context must match the current lifecycle slot.");
      Assert(context.RuntimeHandle.IsAssigned,
        "The callback context must carry the resolved runtime handle.");
    }

    public void PostUpdateAllProjectiles()
    {
      Events.Add("post");
    }
  }

  private sealed class OwnerMovementAdapter : IProjectileTickAdapter
  {
    private readonly EntityReference _expectedOwnerReference;

    public OwnerMovementAdapter(EntityReference expectedOwnerReference)
    {
      _expectedOwnerReference = expectedOwnerReference;
    }

    public int OwnerMovementRequestCount { get; private set; }

    public EntityReference ReceivedOwnerReference { get; private set; }

    public void PreUpdateAllProjectiles()
    {
    }

    public bool TryGetOwnerMovementDelta(
      ProjectileTickContext context,
      EntityReference ownerReference,
      out Vector2 movementDelta)
    {
      OwnerMovementRequestCount++;
      ReceivedOwnerReference = ownerReference;
      movementDelta = new Vector2(2, 3);
      return ownerReference == _expectedOwnerReference;
    }

    public void UpdateProjectile(ProjectileTickContext context)
    {
    }

    public void PostUpdateAllProjectiles()
    {
    }
  }

  private sealed class TrailDustAdapter : IProjectileTickAdapter
  {
    public List<string> Events { get; } = new();

    public ProjectileTrailDustRequest? Request { get; private set; }

    public void PreUpdateAllProjectiles()
    {
      Events.Add("pre");
    }

    public void UpdateProjectile(ProjectileTickContext context)
    {
      Events.Add("update");
    }

    public void EmitTrailDust(
      ProjectileTickContext context,
      ProjectileTrailDustRequest request)
    {
      Events.Add("dust");
      Request = request;
    }

    public void PostUpdateAllProjectiles()
    {
      Events.Add("post");
    }
  }

  private sealed class TerminatingAdapter : IProjectileTickAdapter
  {
    private readonly ProjectileLifecycleSystem _lifecycle;

    public TerminatingAdapter(ProjectileLifecycleSystem lifecycle)
    {
      _lifecycle = lifecycle;
    }

    public List<string> Events { get; } = new();

    public int UpdateCount { get; private set; }

    public void PreUpdateAllProjectiles()
    {
      Events.Add("pre");
    }

    public void UpdateProjectile(ProjectileTickContext context)
    {
      Events.Add("update");
      UpdateCount++;
      Assert(
        _lifecycle.TryTerminate(
          context.Handle,
          ProjectileEndReason.DestroyedByCollision),
        "The adapter should be able to submit a collision termination.");
    }

    public void PostUpdateAllProjectiles()
    {
      Events.Add("post");
    }
  }

  private sealed class ReplacingAdapter : IProjectileTickAdapter
  {
    private readonly ProjectileLifecycleSystem _lifecycle;
    private readonly IProjectileDefinitionQuery _definitions;
    private readonly ProjectileDefinitionHydrationContext _context;

    public ReplacingAdapter(
      ProjectileLifecycleSystem lifecycle,
      IProjectileDefinitionQuery definitions,
      ProjectileDefinitionHydrationContext context)
    {
      _lifecycle = lifecycle;
      _definitions = definitions;
      _context = context;
    }

    public int UpdateCount { get; private set; }

    public int PostUpdateCount { get; private set; }

    public ProjectileHandle Replacement { get; private set; }

    public void PreUpdateAllProjectiles()
    {
    }

    public void UpdateProjectile(ProjectileTickContext context)
    {
      UpdateCount++;
      Assert(context.RuntimeHandle.IsAssigned,
        "The callback context must carry the current runtime handle.");
      Assert(
        _lifecycle.TrySpawn(
          CreateSpawn(typeId: 36, center: new Vector2(60, 60)),
          _definitions,
          _context,
          out ProjectileHandle replacement),
        "The callback should be able to replace the current full-pool projectile.");
      Replacement = replacement;
    }

    public void PostUpdateAllProjectiles()
    {
      PostUpdateCount++;
    }
  }

  private sealed class SpawnDuringCallbackAdapter : IProjectileTickAdapter
  {
    private readonly ProjectileLifecycleSystem _lifecycle;
    private readonly IProjectileDefinitionQuery _definitions;
    private readonly ProjectileDefinitionHydrationContext _hydrationContext;
    private readonly int _triggerSlot;
    private bool _spawned;

    public SpawnDuringCallbackAdapter(
      ProjectileLifecycleSystem lifecycle,
      IProjectileDefinitionQuery definitions,
      ProjectileDefinitionHydrationContext hydrationContext,
      int triggerSlot)
    {
      _lifecycle = lifecycle;
      _definitions = definitions;
      _hydrationContext = hydrationContext;
      _triggerSlot = triggerSlot;
    }

    public List<int> CallbackSlots { get; } = new();

    public ProjectileHandle Replacement { get; private set; }

    public void PreUpdateAllProjectiles()
    {
    }

    public void UpdateProjectile(ProjectileTickContext context)
    {
      Assert(context.RuntimeHandle.IsAssigned,
        "Each callback context must carry the scanned runtime generation.");
      CallbackSlots.Add(context.SlotIndex);
      if (_spawned || context.SlotIndex != _triggerSlot)
      {
        return;
      }

      Assert(
        _lifecycle.TrySpawn(
          CreateSpawn(typeId: 36, center: new Vector2(110, 110)),
          _definitions,
          _hydrationContext,
          out ProjectileHandle replacement),
        "The callback should replace the shortest-lived projectile in the full pool.");
      Replacement = replacement;
      _spawned = true;
    }

    public void PostUpdateAllProjectiles()
    {
    }
  }

  private sealed class ControlFlowAdapter : IProjectileTickAdapter
  {
    private readonly IReadOnlyList<ProjectileTickPreparationResult> _preparationResults;
    private readonly IReadOnlyList<ProjectileTickSubstepResult> _updateResults;

    public ControlFlowAdapter(
      IReadOnlyList<ProjectileTickPreparationResult> preparationResults,
      IReadOnlyList<ProjectileTickSubstepResult> updateResults)
    {
      _preparationResults = preparationResults;
      _updateResults = updateResults;
    }

    public int PreparationCount { get; private set; }

    public int UpdateCount { get; private set; }

    public int PostUpdateCount { get; private set; }

    public void PreUpdateAllProjectiles()
    {
    }

    public ProjectileTickPreparationResult PrepareProjectileStep(
      ProjectileTickContext context,
      in ProjectileTickPreparationInput input,
      out ProjectileTickPreparationChanges changes)
    {
      changes = default;
      int resultIndex = PreparationCount++;
      return resultIndex < _preparationResults.Count
        ? _preparationResults[resultIndex]
        : ProjectileTickPreparationResult.Ready;
    }

    public void UpdateProjectile(ProjectileTickContext context)
    {
    }

    public ProjectileTickSubstepResult UpdateProjectileStep(ProjectileTickContext context)
    {
      int resultIndex = UpdateCount++;
      return resultIndex < _updateResults.Count
        ? _updateResults[resultIndex]
        : ProjectileTickSubstepResult.Completed;
    }

    public void PostUpdateAllProjectiles()
    {
      PostUpdateCount++;
    }
  }

  private sealed class PenetrationExhaustingAdapter : IProjectileTickAdapter
  {
    private readonly ProjectileLifecycleSystem _lifecycle;

    public PenetrationExhaustingAdapter(ProjectileLifecycleSystem lifecycle)
    {
      _lifecycle = lifecycle;
    }

    public int UpdateCount { get; private set; }

    public void PreUpdateAllProjectiles()
    {
    }

    public void UpdateProjectile(ProjectileTickContext context)
    {
      UpdateCount++;
      bool acceptedHit = false;
      ProjectileVerificationAccess.Edit(
        _lifecycle,
        context.Handle,
        (ref ProjectilePenetrationStateComponent penetration) =>
          acceptedHit = ProjectilePenetrationSystem.CommitAcceptedHit(ref penetration));
      Assert(acceptedHit,
        "The one-hit projectile should accept its final penetration hit.");
    }

    public void PostUpdateAllProjectiles()
    {
    }
  }

  private sealed class PreparationMutationAdapter : IProjectileTickAdapter
  {
    private readonly ProjectileLifecycleSystem _lifecycle;
    private bool _hasMutated;

    public PreparationMutationAdapter(ProjectileLifecycleSystem lifecycle)
    {
      _lifecycle = lifecycle;
    }

    public int PreparationCount { get; private set; }

    public int UpdateCount { get; private set; }

    public void PreUpdateAllProjectiles()
    {
    }

    public ProjectileTickPreparationResult PrepareProjectileStep(
      ProjectileTickContext context,
      in ProjectileTickPreparationInput input,
      out ProjectileTickPreparationChanges changes)
    {
      PreparationCount++;
      ProjectileTickPreparationResult result =
        ProjectileTickPreparationSystem.PrepareProjectileStep(
          context,
          in input,
          out changes);
      if (!_hasMutated)
      {
        Assert(_lifecycle.TryGetRuntimeHandle(context.Handle, out _),
          "The same-root callback should resolve its current projectile before editing it.");
        ProjectileVerificationAccess.Edit(
          _lifecycle,
          context.Handle,
          (ref ProjectileBehaviorStateComponent behavior) => behavior.Ai0 = 42.0f);
        ProjectileVerificationAccess.Edit(
          _lifecycle,
          context.Handle,
          (ref ProjectileUpdateCadenceComponent cadence) => cadence.ExtraUpdates = 1);
        ProjectileVerificationAccess.Edit(
          _lifecycle,
          context.Handle,
          (ref ProjectileTrajectoryStateComponent trajectory) => trajectory.NumUpdates = 1);
        _hasMutated = true;
      }

      return result;
    }

    public void UpdateProjectile(ProjectileTickContext context)
    {
      UpdateCount++;
    }

    public void PostUpdateAllProjectiles()
    {
    }
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

  private static void AssertSequenceEqual<T>(
    IReadOnlyList<T> expected,
    IReadOnlyList<T> actual,
    string message)
  {
    if (expected.Count != actual.Count)
    {
      throw new InvalidOperationException(
        $"{message} Expected count '{expected.Count}', actual '{actual.Count}'.");
    }

    for (int index = 0; index < expected.Count; index++)
    {
      if (!EqualityComparer<T>.Default.Equals(expected[index], actual[index]))
      {
        throw new InvalidOperationException(
          $"{message} Difference at index {index}: expected '{expected[index]}', " +
          $"actual '{actual[index]}'.");
      }
    }
  }
}
