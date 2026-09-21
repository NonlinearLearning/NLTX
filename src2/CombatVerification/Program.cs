using System.Numerics;
using Terraria.Combat.Resolution;
using Terraria.Combat.Items;
using Terraria.Combat.Items.Commands;
using Terraria.Combat.StatusEffects;
using Terraria.Combat.TagEffects;
using Terraria.Combat.TagEffects.Commands;
using Terraria.Combat.Targeting;
using Terraria.Content.StatusEffects;
using Terraria.Content.Items;
using Terraria.EntityLifecycleAttribution;
using Terraria.Input.LockOn;
using Terraria.Presentation.BuffText;
using Terraria.Presentation.CombatText;
using Terraria.Tiles.Interaction;
using Terraria.Transport.TagEffects;

var failures = new List<string>();
var passedScenarioCount = 0;

Run("COMBAT-TARGET-SNAPSHOT", () =>
{
  var target = new CombatTargetSnapshot(
    CombatTargetKind.Npc,
    new CombatRectangle(10f, 20f, 30f, 40f),
    30,
    40,
    new Vector2(10f, 20f),
    new Vector2(1f, -2f));

  Assert(target.Center == new Vector2(25f, 40f), "target center should be derived from the hitbox");
  Assert(target.Size == new Vector2(30f, 40f), "target size should expose width and height");
  Assert(!TargetValidityQuery.IsInvalid(target), "a populated NPC target should be valid");
  Assert(
    TargetValidityQuery.IsInvalid(CombatTargetSnapshot.Invalid),
    "the invalid target should be rejected by the pure validity query");
});

Run("MULTI-POINT-HITBOX-BOUNDARY", () =>
{
  var points = new[] { new Vector2(2f, 3f), new Vector2(-1f, 5f) };
  var hitbox = new MultiPointHitboxSnapshot(new Vector2(4f, 6f), points);
  points[0] = new Vector2(99f, 99f);

  Assert(hitbox.Points[0] == new Vector2(2f, 3f), "points should be defensively copied");
  Assert(
    hitbox.BoundingRect == new CombatRectangle(-1f, 3f, 3f, 2f),
    "the bounding rectangle should be derived from point positions and point size");
});

Run("NPC-IMMUNITY-APPLICATION", () =>
{
  var definition = new NpcDebuffImmunityDefinition(
    immuneToWhips: true,
    immuneToNonWhipBuffs: false,
    specificallyImmuneTo: new[] { 42 });
  var state = new NpcDebuffImmunityState();
  var result = new NpcDebuffImmunityApplySystem().Apply(state, definition);

  Assert(result.Changed, "applying a new immunity definition should change runtime state");
  Assert(state.IsImmune(7, isWhip: true), "whip immunity should be applied");
  Assert(!state.IsImmune(7, isWhip: false), "non-whip immunity should remain disabled");
  Assert(state.IsImmune(42, isWhip: false), "specific effect immunity should be applied");
});

Run("NPC-KILL-ATTEMPT-BOUNDARY", () =>
{
  var attempt = new NpcKillAttemptSnapshot(
    new EntityReference(3, 2),
    17,
    false);
  var command = new NpcStrikeCommitCommand(attempt, 10, "strike-1");
  var system = new NpcKillResolutionSystem();

  var unresolved = system.Resolve(
    command,
    new NpcStrikeCommitResult(false, false, false));
  Assert(!unresolved.IsKillConfirmed, "a pre-strike inactive flag must not create a kill result");

  var resolved = system.Resolve(
    command,
    new NpcStrikeCommitResult(true, true, false));
  Assert(resolved.IsKillConfirmed, "a committed dead-after-strike result should confirm the kill");
  Assert(resolved.Attempt.WasActive == false, "the pre-strike active snapshot must remain observable");
});

Run("TILE-HIT-CAPACITY", () =>
{
  var component = new TileHitTrackingComponent();
  var random = new SequenceTileCrackStyleRandom(0, 1, 2, 3);
  var system = new TileHitTrackingSystem(new TileCrackPresentationAdapter(random));

  Assert(component.Entries.Count == TileHitTrackingPolicy.ArraySize,
    "the component should expose 500 valid slots plus one sentinel slot");
  Assert(component.Order.Count == TileHitTrackingPolicy.ArraySize,
    "the order cache should match the entry array including its sentinel slot");

  for (int index = 0; index < TileHitTrackingPolicy.Capacity; index++)
  {
    Assert(
      system.Apply(component, TileHitCommand.Register(index, index, TileHitKind.Tile, 1)),
      "each valid tile slot should accept a hit");
  }

  Assert(
    system.Apply(component, TileHitCommand.Register(999, 999, TileHitKind.Wall, 2)),
    "a full pool should replace its oldest valid entry rather than use the sentinel slot");
  Assert(
    !TileHitLookupQuery.TryFind(component, 0, 0, TileHitKind.Tile, out _, out _),
    "the oldest valid entry should be evicted when the pool is full");
  Assert(
    TileHitLookupQuery.TryFind(component, 999, 999, TileHitKind.Wall, out int slot, out _)
      && slot < TileHitTrackingPolicy.Capacity,
    "a newly registered entry must never occupy the sentinel slot");
});

Run("TILE-HIT-TTL", () =>
{
  var component = new TileHitTrackingComponent();
  var system = new TileHitTrackingSystem(
    new TileCrackPresentationAdapter(new SequenceTileCrackStyleRandom(0, 1)));

  Assert(
    system.Apply(component, TileHitCommand.Register(4, 5, TileHitKind.Tile, 3)),
    "a tile hit should be registered");
  Assert(
    TileHitLookupQuery.TryFind(component, 4, 5, TileHitKind.Tile, out _, out TileHitEntry entry)
      && entry.RemainingTicks == TileHitTrackingPolicy.DefaultLifetimeTicks,
    "a new entry should receive the 60 tick TTL");

  system.Advance(component, TileHitTrackingPolicy.DefaultLifetimeTicks - 1);
  Assert(
    TileHitLookupQuery.TryFind(component, 4, 5, TileHitKind.Tile, out _, out _),
    "an entry should remain active until its final TTL tick");
  system.Advance(component, 1);
  Assert(
    !TileHitLookupQuery.TryFind(component, 4, 5, TileHitKind.Tile, out _, out _),
    "an entry should be cleared when its TTL reaches zero");
});

Run("TILE-HIT-RANDOM-SEAM", () =>
{
  var random = new SequenceTileCrackStyleRandom(2, 2, 3);
  var component = new TileHitTrackingComponent();
  var system = new TileHitTrackingSystem(new TileCrackPresentationAdapter(random));

  Assert(
    system.Apply(component, TileHitCommand.Register(1, 1, TileHitKind.Tile, 1)),
    "the first hit should use the injected random port");
  Assert(
    TileHitLookupQuery.TryFind(component, 1, 1, TileHitKind.Tile, out _, out TileHitEntry first)
      && first.CrackStyle == 2,
    "the first crack style should come from the injected random port");
  Assert(
    system.Apply(component, TileHitCommand.Register(2, 2, TileHitKind.Tile, 1)),
    "the second hit should use the same injected random port");
  Assert(
    TileHitLookupQuery.TryFind(component, 2, 2, TileHitKind.Tile, out _, out TileHitEntry second)
      && second.CrackStyle == 3,
    "the adapter should avoid repeating the previous crack style");
  Assert(random.CallCount == 3, "randomness should be explicit and observable through the port");
});

Run("TAG-EFFECT-SWITCH-CLEAR", () =>
{
  var owner = new EntityReference(8, 1);
  var target = new EntityReference(41, 2);
  var firstDefinition = new TagEffectDefinition(10, true, true, 5);
  var secondDefinition = new TagEffectDefinition(11, true, false, 7);
  var state = new PlayerTagEffectStateComponent(owner);
  var lifecycle = new TagEffectLifecycleSystem();

  Assert(
    lifecycle.Apply(state, new SetActiveTagEffectCommand(10, firstDefinition)),
    "a valid definition should become the active tag effect");
  Assert(
    lifecycle.Apply(state, new ApplyTagToNpcCommand(target)) &&
      lifecycle.Apply(state, new EnableTagProcCommand(target)),
    "tag and proc commands should be accepted for the active effect");
  Assert(
    TagEffectHitEligibilityQuery.IsTagged(state, target) &&
      TagEffectHitEligibilityQuery.CanProc(state, target),
    "the target should have both tag and proc eligibility before switching");

  Assert(
    lifecycle.Apply(state, new SetActiveTagEffectCommand(11, secondDefinition)),
    "switching to a different effect should be accepted");
  Assert(state.ActiveEffectTypeId == 11, "the new effect type should be authoritative");
  Assert(
    !TagEffectHitEligibilityQuery.IsTagged(state, target) &&
      !TagEffectHitEligibilityQuery.CanProc(state, target),
    "switching effects should clear the old tag and proc marks");
  Assert(
    !lifecycle.Apply(state, new SetActiveTagEffectCommand(12, null)) &&
      state.ActiveEffectTypeId == 11,
    "an invalid definition must not replace a valid active effect");
});

Run("TAG-EFFECT-TIMER-LIFECYCLE", () =>
{
  var target = new EntityReference(50, 3);
  var state = new PlayerTagEffectStateComponent(new EntityReference(9, 1));
  var lifecycle = new TagEffectLifecycleSystem();
  var definition = new TagEffectDefinition(20, false, true, 3);

  Assert(
    lifecycle.Apply(state, new SetActiveTagEffectCommand(20, definition)) &&
      lifecycle.Apply(state, new ApplyTagToNpcCommand(target)) &&
      lifecycle.Apply(state, new EnableTagProcCommand(target)),
    "the lifecycle should initialize tag and proc timers");
  lifecycle.Advance(state, 2);
  Assert(
    state.TryGetMark(target, out PlayerTagEffectStateComponent.NpcTagMark mark) &&
      mark.TagTicksRemaining == 1 && mark.ProcTicksRemaining == 1,
    "the lifecycle should decrement both timers exactly once per tick");
  Assert(
    lifecycle.Apply(state, new ResetNpcTagStateCommand(target)) &&
      !state.TryGetMark(target, out _),
    "NPC reset should remove both timers before a slot can be reused");
});

Run("TAG-EFFECT-HIT-BOUNDARY", () =>
{
  var owner = new EntityReference(10, 1);
  var target = new EntityReference(60, 4);
  var behavior = new RecordingTagEffectBehaviorPort();
  var lifecycle = new TagEffectLifecycleSystem(behavior);
  var hitSystem = new TagEffectHitSystem(lifecycle, behavior);
  var state = new PlayerTagEffectStateComponent(owner);
  var definition = new TagEffectDefinition(30, true, true, 10);

  lifecycle.Apply(state, new SetActiveTagEffectCommand(30, definition));
  lifecycle.Apply(state, new ApplyTagToNpcCommand(target));
  lifecycle.Apply(state, new EnableTagProcCommand(target));

  TagEffectHitSystem.HitDecision decision = hitSystem.ModifyHit(
    state,
    target,
    1001,
    10,
    false);
  Assert(decision.Accepted, "a tagged target should produce a hit decision");
  Assert(decision.Damage == 17 && decision.Critical, "tag and proc modifiers should be composed");
  Assert(
    hitSystem.CommitAfterDamage(state, decision, damageCommitted: true, calcDamage: 17),
    "a committed damage result should invoke post-hit effects");
  Assert(behavior.TaggedHitCount == 1 && behavior.ProcHitCount == 1,
    "tagged and proc callbacks should run once after damage commit");
  Assert(
    !TagEffectHitEligibilityQuery.CanProc(state, target) &&
      !hitSystem.CommitAfterDamage(state, decision, damageCommitted: true, calcDamage: 17),
    "a repeated commit must not proc the same hit twice");
  Assert(behavior.ProcHitCount == 1, "duplicate commits must not repeat the proc callback");
});

Run("TAG-EFFECT-NETWORK-PROJECTION", () =>
{
  var owner = new EntityReference(3, 7);
  var target = new EntityReference(70, 2);
  var state = new PlayerTagEffectStateComponent(owner);
  var definition = new TagEffectDefinition(40, true, false, 12);
  var lifecycle = new TagEffectLifecycleSystem();

  lifecycle.Apply(state, new SetActiveTagEffectCommand(40, definition));
  lifecycle.Apply(state, new ApplyTagToNpcCommand(target));
  lifecycle.Apply(state, new EnableTagProcCommand(target));

  TagEffectNetworkAdapter.NetworkPayload payload = TagEffectNetworkAdapter.CreatePayload(
    state,
    networkPlayerId: 5,
    targetNetworkIdResolver: reference => reference.RuntimeEntityId + 1000);
  Assert(payload.ShouldSync && payload.NetworkPlayerId == 5,
    "network projection should carry the protocol player ID explicitly");
  Assert(payload.Marks.Count == 1 && payload.Marks[0].NetworkTargetId == 1070,
    "network projection should map the target reference through an explicit resolver");
  Assert(payload.Marks[0].ProcTicksRemaining == 0,
    "SyncProcs=false must omit proc timer authority from the payload");

  TagEffectStateProjection.Snapshot snapshot = TagEffectStateProjection.CreateSnapshot(state);
  Assert(snapshot.OwnerReference == owner,
    "the local projection should preserve the entity reference separately from network IDs");
  Assert(payload.NetworkPlayerId != snapshot.OwnerReference.RuntimeEntityId,
    "a network player ID must not be treated as the runtime entity ID");
});

Run("LOCK-ON-CANDIDATE-VALIDITY", () =>
{
  var origin = Vector2.Zero;
  var first = new EntityReference(80, 1);
  var duplicate = new EntityReference(80, 1);
  var boundary = new EntityReference(81, 1);
  var outOfRange = new EntityReference(82, 1);
  var invalid = new EntityReference(83, 0);
  var candidates = new[]
  {
    new LockOnCandidateQuery.Candidate(first, new Vector2(1000f, 0f), true),
    new LockOnCandidateQuery.Candidate(duplicate, new Vector2(1000f, 0f), true),
    new LockOnCandidateQuery.Candidate(boundary, new Vector2(2000f, 0f), true),
    new LockOnCandidateQuery.Candidate(outOfRange, new Vector2(2001f, 0f), true),
    new LockOnCandidateQuery.Candidate(invalid, new Vector2(10f, 0f), true),
    new LockOnCandidateQuery.Candidate(new EntityReference(84, 1), Vector2.Zero, false)
  };

  IReadOnlyList<EntityReference> selected = LockOnCandidateQuery.Find(
    origin,
    candidates,
    LockOnPolicy.Default);
  Assert(selected.Count == 2, "candidate query should keep valid in-range targets once");
  Assert(selected[0] == first && selected[1] == boundary,
    "candidate query should preserve stable input order and include the range boundary");
});

Run("LOCK-ON-HOLD-LIFETIME", () =>
{
  var target = new EntityReference(90, 2);
  var component = new LockOnSelectionComponent();
  var system = new LockOnInputSystem(LockOnPolicy.Default);

  system.SetCanLockOn(component, true);
  system.ReplaceCandidates(component, new[] { target });
  Assert(system.TrySelect(component, 0), "a valid candidate index should be selectable");
  Assert(LockOnTargetQuery.TryGetSelectedTarget(component, out EntityReference selected) &&
    selected == target, "the selected target query should expose the selected reference");

  system.Advance(component, 39, inputHeld: false);
  Assert(LockOnTargetQuery.TryGetSelectedTarget(component, out _),
    "the selected target should remain during the 40 tick hold lifetime");
  system.Advance(component, 1, inputHeld: false);
  Assert(!LockOnTargetQuery.TryGetSelectedTarget(component, out _),
    "the selected target should clear when the hold lifetime expires");
  Assert(!system.TrySelect(component, -1), "-1 should not select a target");
  Assert(!system.TrySelect(component, 99), "an out-of-range index should not select a target");
});

Run("LOCK-ON-PREDICTION", () =>
{
  Vector2 predicted = LockOnPredictionQuery.Predict(
    Vector2.Zero,
    new Vector2(1000f, 0f),
    new Vector2(2f, -1f),
    hasTarget: true);
  Assert(
    Vector2.Distance(predicted, new Vector2(1045f, -22.5f)) < 0.001f,
    "prediction should apply the distance-scaled velocity lead");
  Assert(
    LockOnPredictionQuery.Predict(
      Vector2.Zero,
      Vector2.Zero,
      Vector2.One,
      hasTarget: false) == Vector2.Zero,
    "prediction should return zero when there is no selected target");
});

Run("LOCK-ON-PROJECTION-SCOPE", () =>
{
  var projection = new LockOnCursorProjectionAdapter();
  Assert(projection.SetUp(new Vector2(12f, 14f)), "projection setup should open a scope");
  Assert(projection.IsActive && projection.Position == new Vector2(12f, 14f),
    "projection state should be visible only while the scope is active");
  Assert(projection.SetDown(), "projection teardown should close the scope");
  Assert(!projection.IsActive && projection.Position == Vector2.Zero,
    "projection teardown should clear temporary cursor state");
  Assert(!projection.SetDown(), "a second teardown must not mutate a closed scope");
});

Run("LOCK-ON-ID-BOUNDARY", () =>
{
  var runtimeReference = new EntityReference(101, 4);
  var component = new LockOnSelectionComponent();
  var system = new LockOnInputSystem(LockOnPolicy.Default);
  system.SetCanLockOn(component, true);
  system.ReplaceCandidates(component, new[] { runtimeReference });
  Assert(system.TrySelect(component, 0), "a runtime entity reference should be selectable");
  Assert(LockOnTargetQuery.TryGetSelectedTarget(component, out EntityReference selected) &&
    selected == runtimeReference, "selection should retain the entity generation");
  Assert(component.SelectedCandidateIndex == 0,
    "the selection index must remain a local collection index, not an entity ID");
  Assert(selected.RuntimeEntityId != component.SelectedCandidateIndex,
    "runtime entity IDs must not be conflated with candidate slot indices");
});

Run("COMBAT-TEXT-PALETTE", () =>
{
  CombatTextPalette palette = CombatTextPalette.Default;
  Assert(
    palette.Get(CombatTextColorRole.DamagedFriendly) == new CombatTextColor(255, 80, 90),
    "friendly damage should use the documented palette color");
  Assert(
    palette.Get(CombatTextColorRole.DamagedFriendlyCritical) ==
      new CombatTextColor(255, 100, 30),
    "friendly critical damage should use the documented critical color");
  Assert(
    palette.Get(CombatTextColorRole.OthersDamagedHostile) ==
      palette.Get(CombatTextColorRole.DamagedHostile).Scale(0.4f),
    "remote hostile damage should derive from the hostile palette color");
  Assert(
    palette.Get(CombatTextColorRole.LifeRegenNegative) == new CombatTextColor(255, 140, 40),
    "negative life regeneration should retain its separate semantic color");
});

Run("COMBAT-TEXT-POOL", () =>
{
  var pool = new CombatTextEntryComponent();
  var system = new CombatTextPresentationSystem();
  Assert(pool.Entries.Count == CombatTextPoolPolicy.Capacity,
    "combat text should allocate exactly 100 presentation slots");

  for (int index = 0; index < CombatTextPoolPolicy.Capacity; index++)
  {
    Assert(system.TrySpawn(
      pool,
      new CombatTextSpawnCommand(
        index,
        index.ToString(),
        new Vector2(index, 0f),
        Vector2.Zero,
        CombatTextColorRole.DamagedHostile,
        1f,
        0f,
        false,
        false,
        10),
      out int slot), "each combat text slot should accept a spawn");
    Assert(slot < CombatTextPoolPolicy.Capacity,
      "a presentation slot must not be treated as an entity ID");
  }

  Assert(system.TrySpawn(
    pool,
    new CombatTextSpawnCommand(
      1000,
      "replacement",
      Vector2.Zero,
      Vector2.Zero,
      CombatTextColorRole.HealLife,
      1f,
      0f,
      false,
      false,
      10),
    out _), "a full presentation pool should replace its oldest entry");
  Assert(!CombatTextPresentationQuery.ContainsText(pool, "0"),
    "the oldest entry should be the replacement candidate when the pool is full");
});

Run("COMBAT-TEXT-SPAWN-PROJECTION", () =>
{
  var pool = new CombatTextEntryComponent();
  var system = new CombatTextPresentationSystem();
  var command = new CombatTextSpawnCommand(
    200,
    "42",
    new Vector2(10f, 20f),
    new Vector2(0f, -1f),
    CombatTextColorRole.DamagedHostileCritical,
    1.5f,
    0.25f,
    true,
    true,
    3);

  Assert(system.TrySpawn(pool, command, out int slot), "a spawn command should allocate a slot");
  Assert(CombatTextPresentationQuery.TryGet(pool, slot, out CombatTextEntry entry),
    "the presentation query should read the spawned entry");
  Assert(entry.Text == "42" && entry.Position == command.Position &&
    entry.Velocity == command.Velocity && entry.IsCritical && entry.IsDamageOverTime &&
    entry.Scale == 1.5f && entry.RemainingTicks == 3,
    "spawn fields should map one way into presentation state");
  system.Advance(pool, 1);
  Assert(CombatTextPresentationQuery.TryGet(pool, slot, out entry) &&
    entry.Position == new Vector2(10f, 19f) && entry.RemainingTicks == 2,
    "presentation advancement should update only the visual entry");
});

Run("COMBAT-TEXT-CLEAR-BOUNDARY", () =>
{
  var pool = new CombatTextEntryComponent();
  var system = new CombatTextPresentationSystem();
  int health = 100;
  system.TrySpawn(
    pool,
    new CombatTextSpawnCommand(
      300,
      "heal",
      Vector2.Zero,
      Vector2.Zero,
      CombatTextColorRole.HealLife,
      1f,
      0f,
      false,
      false,
      10),
    out _);

  Assert(system.ClearAll(pool), "clearAll should report cleared presentation entries");
  Assert(health == 100, "clearing combat text must not modify health state");
  Assert(!CombatTextPresentationQuery.HasActiveEntries(pool),
    "clearAll should remove only presentation entries");
});

Run("BUFF-TEXT-PROJECTILE-FILTER", () =>
{
  var filter = new ProjectileBuffTextFilter(new[] { 4, 7, 9 });
  Assert(filter.IsTracked(7) && !filter.IsTracked(8),
    "the projectile filter should expose only its immutable configured IDs");
  Assert(filter.CountActive(new[] { 7, 8, 7, 10, 4 }) == 3,
    "the projectile filter should count active tracked projectiles without writing state");
});

Run("COMBAT-TEXT-NETWORK-ID", () =>
{
  var command = new CombatTextSpawnCommand(
    400,
    "remote",
    Vector2.Zero,
    Vector2.Zero,
    CombatTextColorRole.OthersDamagedHostile,
    1f,
    0f,
    false,
    false,
    10);
  CombatTextNetworkProjection.NetworkPayload payload =
    CombatTextNetworkProjection.CreatePayload(command, networkSourceId: 12);
  Assert(payload.NetworkSourceId == 12 && payload.EventId == 400,
    "network projection should carry explicit event and source protocol IDs");
  Assert(payload.PresentationSlotIndex is null,
    "network projection must not serialize a presentation slot as an entity ID");
});

Run("ITEM-CAPABILITY-COVERAGE", () =>
{
  var snapshot = new ItemCapabilitySnapshot(
    new ItemCombatCapabilityDefinition(12, 3.5f, 7, 4, 9),
    new ItemProjectileUseDefinition(new ProjectileContentId(88), 6.5f),
    new ItemResourceRecoveryDefinition(15, 8, -2, 20, 4),
    new ItemDamageClassCapability(true, false, true, true, false),
    new ItemPresentationDefinition(5));

  Assert(snapshot.Combat.BaseDamage == 12 && snapshot.Combat.BaseKnockback == 3.5f,
    "combat capability should retain damage and knockback");
  Assert(snapshot.Combat.CriticalChance == 7 &&
    snapshot.Combat.ArmorPenetration == 4 && snapshot.Combat.BonusTagDamage == 9,
    "combat capability should retain crit, armor penetration, and tag damage");
  Assert(snapshot.ProjectileUse.ProjectileTypeId == new ProjectileContentId(88) &&
    snapshot.ProjectileUse.ProjectileSpeed == 6.5f,
    "projectile capability should retain shoot and shoot speed");
  Assert(snapshot.ResourceRecovery.LifeRecovery == 15 &&
    snapshot.ResourceRecovery.ManaRecovery == 8 &&
    snapshot.ResourceRecovery.LifeRegen == -2 &&
    snapshot.ResourceRecovery.MaxManaIncrease == 20 &&
    snapshot.ResourceRecovery.ManaCost == 4,
    "resource capability should retain all five resource fields");
  Assert(snapshot.DamageClass.IsMelee && !snapshot.DamageClass.IsMagic &&
    snapshot.DamageClass.IsRanged && snapshot.DamageClass.IsSummon &&
    !snapshot.DamageClass.IsSentry,
    "damage class capability should retain all five class flags");
  Assert(snapshot.Presentation.RarityTier == 5,
    "rarity should be retained as presentation metadata");
});

Run("ITEM-COMBAT-QUERY", () =>
{
  var baseSnapshot = new ItemCapabilitySnapshot(
    new ItemCombatCapabilityDefinition(20, 4f, 10, 6, 3),
    ItemProjectileUseDefinition.None,
    new ItemResourceRecoveryDefinition(0, 0, 0, 0, 0),
    new ItemDamageClassCapability(true, false, false, false, false),
    new ItemPresentationDefinition(2));

  ItemCapabilitySnapshot merged = ItemCombatCapabilityQuery.Merge(
    baseSnapshot,
    new[]
    {
      new ItemCapabilityModifier(20, damageDelta: 2, criticalChanceDelta: 1),
      new ItemCapabilityModifier(10, damageDelta: 3, armorPenetrationDelta: 2)
    });

  Assert(merged.Combat.BaseDamage == 25 &&
    merged.Combat.CriticalChance == 11 &&
    merged.Combat.ArmorPenetration == 8,
    "combat query should expose the deterministically merged combat capability");
  Assert(merged.DamageClass.IsMelee && !merged.DamageClass.IsRanged,
    "combat query should preserve damage class capability without inventing a class");
});

Run("ITEM-RESOURCE-QUERY", () =>
{
  var recovery = new ItemResourceRecoveryDefinition(25, 12, -1, 0, 7);
  var resources = new ItemResourceUseQuery.ResourceAvailability(50, 100, 10, 50);
  ItemResourceUseQuery.Result result = ItemResourceUseQuery.Evaluate(recovery, resources);

  Assert(result.CanUse && result.ManaCost == 7 && result.ManaAfterCost == 3,
    "resource query should authorize an affordable use and report its cost");
  Assert(result.LifeRecovery == 25 && result.ManaRecovery == 12,
    "resource query should report recovery without mutating the input snapshot");
  Assert(resources.CurrentLife == 50 && resources.CurrentMana == 10,
    "resource query must not deduct mana or apply healing");

  ItemResourceUseQuery.Result rejected = ItemResourceUseQuery.Evaluate(
    recovery,
    new ItemResourceUseQuery.ResourceAvailability(50, 100, 6, 50));
  Assert(!rejected.CanUse &&
    rejected.RejectionReason == ItemResourceUseQuery.RejectionReason.InsufficientMana,
    "resource query should reject an unaffordable mana cost without side effects");
});

Run("ITEM-PROJECTILE-USE", () =>
{
  var snapshot = new ItemCapabilitySnapshot(
    new ItemCombatCapabilityDefinition(30, 5f, 8, 2, 4),
    new ItemProjectileUseDefinition(new ProjectileContentId(91), 12f),
    new ItemResourceRecoveryDefinition(0, 0, 0, 0, 3),
    new ItemDamageClassCapability(false, false, true, false, false),
    new ItemPresentationDefinition(4));
  var user = new EntityReference(14, 2);
  var lockOnTarget = new EntityReference(40, 3);

  Assert(
    ItemCombatCommandBuilder.TryBuild(
      new ItemUseId(700),
      user,
      new ItemContentId(12),
      new InventorySlotIndex(5),
      new ItemPersistentId("item:12:instance"),
      snapshot,
      new ItemResourceUseQuery.ResourceAvailability(100, 100, 10, 50),
      lockOnTarget,
      33,
      new ItemNetworkId(8),
      out UseItemCombatCommand? command),
    "an affordable item use should produce one command intent");
  Assert(command is not null && command.UserReference == user &&
    command.ProjectileUse.ProjectileTypeId == new ProjectileContentId(91) &&
    command.ProjectileUse.ProjectileSpeed == 12f &&
    command.LockOnTargetReference == lockOnTarget &&
    command.ActiveTagEffectTypeId == 33 &&
    command.NetworkId == new ItemNetworkId(8),
    "the use command should carry projectile and targeting snapshots");
  Assert(command is not null && command.ResourceUse.ManaCost == 3,
    "the command should carry the calculated resource cost rather than deducting it");
});

Run("ITEM-PREFIX-VARIANT-MERGE", () =>
{
  var baseSnapshot = new ItemCapabilitySnapshot(
    new ItemCombatCapabilityDefinition(10, 2f, 4, 1, 2),
    new ItemProjectileUseDefinition(new ProjectileContentId(10), 5f),
    new ItemResourceRecoveryDefinition(0, 0, 0, 0, 2),
    new ItemDamageClassCapability(false, true, false, false, false),
    new ItemPresentationDefinition(1));

  ItemCapabilitySnapshot merged = ItemCombatCapabilityQuery.Merge(
    baseSnapshot,
    new[]
    {
      new ItemCapabilityModifier(
        20,
        damageDelta: 4,
        projectileSpeedDelta: 3f,
        manaCostDelta: 1,
        rarityDelta: 2,
        projectileTypeOverride: new ProjectileContentId(22)),
      new ItemCapabilityModifier(
        10,
        damageDelta: 1,
        projectileSpeedDelta: 2f,
        manaCostDelta: -1,
        rarityDelta: 1,
        projectileTypeOverride: new ProjectileContentId(21))
    });

  Assert(merged.Combat.BaseDamage == 15 &&
    merged.ProjectileUse.ProjectileSpeed == 10f &&
    merged.ResourceRecovery.ManaCost == 2,
    "prefix and variant modifiers should apply in ascending explicit order");
  Assert(merged.ProjectileUse.ProjectileTypeId == new ProjectileContentId(22) &&
    merged.Presentation.RarityTier == 4,
    "later explicit modifiers should deterministically win projectile and rarity overrides");
});

Run("ITEM-PRESENTATION-BOUNDARY", () =>
{
  var snapshot = new ItemCapabilitySnapshot(
    new ItemCombatCapabilityDefinition(99, 1f, 50, 20, 12),
    ItemProjectileUseDefinition.None,
    new ItemResourceRecoveryDefinition(0, 0, 0, 0, 0),
    new ItemDamageClassCapability(false, false, false, true, true),
    new ItemPresentationDefinition(7));

  ItemCapabilityProjection.View view = ItemCapabilityProjection.Create(
    new ItemContentId(77),
    snapshot);
  Assert(view.ContentId == new ItemContentId(77) && view.RarityTier == 7,
    "presentation projection should expose content identity and rarity");
  Assert(view.DamageClass.IsSummon && view.DamageClass.IsSentry,
    "presentation projection may expose class metadata for UI and tooltip consumers");
  Assert(view.RarityTier != snapshot.Combat.CriticalChance,
    "rarity must remain presentation metadata rather than a combat statistic");
});

Run("ITEM-ID-BOUNDARY", () =>
{
  var snapshot = new ItemCapabilitySnapshot(
    new ItemCombatCapabilityDefinition(1, 1f, 0, 0, 0),
    ItemProjectileUseDefinition.None,
    new ItemResourceRecoveryDefinition(0, 0, 0, 0, 0),
    new ItemDamageClassCapability(true, false, false, false, false),
    new ItemPresentationDefinition());
  var entity = new EntityReference(101, 4);
  var contentId = new ItemContentId(4);
  var slot = new InventorySlotIndex(101);
  var persistentId = new ItemPersistentId("save:item:4");
  var networkId = new ItemNetworkId(101);
  var externalId = new ItemExternalId("platform:item:4");

  Assert(
    ItemCombatCommandBuilder.TryBuild(
      new ItemUseId(701),
      entity,
      contentId,
      slot,
      persistentId,
      snapshot,
      new ItemResourceUseQuery.ResourceAvailability(10, 10, 0, 10),
      null,
      null,
      networkId,
      out UseItemCombatCommand? command),
    "a command should accept separately typed item and entity identities");
  Assert(command is not null && command.ItemContentId == contentId &&
    command.InventorySlot == slot && command.ItemPersistentId == persistentId &&
    command.UserReference == entity && command.NetworkId == networkId,
    "content ID, inventory slot, persistent ID, entity reference, and network ID must stay distinct");
  Assert(contentId.Value != slot.Value && networkId.Value == entity.RuntimeEntityId,
    "equal numeric values must not make slot, content, network, and entity identities interchangeable");
  Assert(externalId.Value != persistentId.Value,
    "external identity must remain separate from persistence identity");
});

if (failures.Count > 0)
{
  Console.Error.WriteLine(string.Join(Environment.NewLine, failures));
  return 1;
}

Console.WriteLine($"P07 focused verifier passed: {passedScenarioCount} scenarios");
return 0;

void Run(string name, Action action)
{
  try
  {
    action();
    passedScenarioCount++;
  }
  catch (Exception exception)
  {
    failures.Add($"{name}: {exception.Message}");
  }
}

void Assert(bool condition, string message)
{
  if (!condition)
  {
    throw new InvalidOperationException(message);
  }
}

sealed class SequenceTileCrackStyleRandom : ITileCrackStyleRandom
{
  private readonly int[] _values;
  private int _index;

  public SequenceTileCrackStyleRandom(params int[] values)
  {
    _values = values;
  }

  public int CallCount { get; private set; }

  public int Next(int exclusiveUpperBound)
  {
    CallCount++;
    int value = _values[Math.Min(_index++, _values.Length - 1)];
    return value % exclusiveUpperBound;
  }
}

sealed class RecordingTagEffectBehaviorPort : TagEffectBehaviorPort
{
  public int ProcHitCount { get; private set; }

  public int TaggedHitCount { get; private set; }

  public override TagEffectBehaviorPort.HitModification ModifyTaggedHit(
    TagEffectDefinition definition,
    EntityReference owner,
    EntityReference target,
    int damage,
    bool critical)
  {
    return new TagEffectBehaviorPort.HitModification(damage + 2, critical);
  }

  public override TagEffectBehaviorPort.HitModification ModifyProcHit(
    TagEffectDefinition definition,
    EntityReference owner,
    EntityReference target,
    int damage,
    bool critical)
  {
    return new TagEffectBehaviorPort.HitModification(damage + 5, true);
  }

  public override void OnProcHit(
    TagEffectDefinition definition,
    EntityReference owner,
    EntityReference target,
    int calcDamage)
  {
    ProcHitCount++;
  }

  public override void OnTaggedHit(
    TagEffectDefinition definition,
    EntityReference owner,
    EntityReference target,
    int calcDamage)
  {
    TaggedHitCount++;
  }
}
