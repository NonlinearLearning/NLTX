using System;
using System.Collections.Generic;
using System.Numerics;
using Terraria.Combat;
using Terraria.Npc;
using Terraria.Npc.Network;
using Terraria.SpatialSimulation.Components;

static void Assert(bool condition, string message)
{
  if (!condition)
  {
    throw new InvalidOperationException(message);
  }
}

NpcTypeId npcType = new(50);
NpcTypeId mobType = new(1);
NpcDamageDefinitionCatalog definitions =
  NpcDamageDefinitionCatalog.CreateVersion4Snapshot();
var statusSystem = new NpcStatusSystem();
StatusEffectSlotsComponent statusSlots = new(
  new[]
  {
    new StatusEffectSlot(20, 2),
    new StatusEffectSlot(30, 1),
    default,
  },
  revision: 4);
NpcStatusFlagsComponent statusFlags = new()
{
  Tipsy = true,
  LifeRegenerationRate = 10,
  LifeRegenerationExpectedLossPerSecond = 5,
};
NpcBuffStateUpdateResult statusTick = statusSystem.ApplyBuffSlotPhase(
  npcType,
  new NpcAiStateComponent(0, 0.0f, 0.0f, 0.0f, 0.0f, 0),
  default,
  ref statusSlots,
  ref statusFlags);
Assert(
  statusTick.Applied &&
    statusTick.BuffSlotsChanged &&
    statusSlots.Revision == 5 &&
    statusSlots.Slots.Span[0].DefinitionId == 20 &&
    statusSlots.Slots.Span[0].RemainingTicks == 1 &&
    statusSlots.Slots.Span[1].DefinitionId == 30 &&
    statusSlots.Slots.Span[1].RemainingTicks == 0 &&
    statusFlags.Poisoned &&
    statusFlags.Bleeding &&
    !statusFlags.Tipsy &&
    statusFlags.LifeRegenerationRate == 0 &&
    statusFlags.LifeRegenerationExpectedLossPerSecond == -1 &&
    statusTick.Effects.Length == 0,
  "The buff flag phase must expose timer-zero slots for the intervening SoulDrain phase.");
NpcBuffStateUpdateResult expiredBuffs = statusSystem.ClearExpiredBuffs(
  ref statusSlots);
Assert(
  expiredBuffs.Applied &&
    expiredBuffs.BuffSlotsChanged &&
    statusSlots.Revision == 6 &&
    statusSlots.Slots.Span[1].DefinitionId == 0 &&
    expiredBuffs.Effects.Length == 1 &&
    expiredBuffs.Effects.Span[0].Kind ==
      NpcBuffStateUpdateResult.EffectKind.BuffSlotSyncRequested,
  "Expired buff cleanup must commit after status flags and request one sync.");

NpcStatusFlagsComponent poisonRegenerationFlags = new()
{
  Poisoned = true,
};
NpcLifeRegenerationStateComponent poisonRegenerationState = new()
{
  LifeRegenerationCount = 119,
};
NpcLifeRegenerationResult poisonRegeneration = statusSystem.ApplyDamageOverTimePhase(
  ref poisonRegenerationFlags,
  ref poisonRegenerationState,
  new NpcLifeRegenerationInput(
    npcType,
    AiState1: 0.0f,
    NpcLegacySlot: 0,
    CurrentLife: 50,
    MaximumLife: 100,
    DamageProtected: false,
    Immortal: false,
    GoodWorld: false,
    LavaWet: false,
    HasRealLifeParent: false,
    InfectedSeed: false,
    DryadBaneProgress: default,
    TownNpcDamageMultiplier: null,
    ProjectileSnapshot: null,
    ProjectileSnapshotComplete: false));
Assert(
  poisonRegeneration.Computed &&
    poisonRegeneration.LifeRegenerationAfter == -12 &&
    poisonRegeneration.LifeRegenerationCountBefore == 119 &&
    poisonRegeneration.LifeRegenerationCountAfter == 107 &&
    poisonRegeneration.DamageEventCount == 0,
  "Poison must subtract 12 regeneration points without crossing the 120 damage threshold.");

NpcStatusFlagsComponent healingRegenerationFlags = new()
{
  LifeRegenerationRate = 1,
};
NpcLifeRegenerationStateComponent healingRegenerationState = new()
{
  LifeRegenerationCount = 119,
};
NpcLifeRegenerationResult healingRegeneration = statusSystem.ApplyDamageOverTimePhase(
  ref healingRegenerationFlags,
  ref healingRegenerationState,
  new NpcLifeRegenerationInput(
    npcType,
    AiState1: 0.0f,
    NpcLegacySlot: 0,
    CurrentLife: 50,
    MaximumLife: 100,
    DamageProtected: false,
    Immortal: false,
    GoodWorld: false,
    LavaWet: false,
    HasRealLifeParent: false,
    InfectedSeed: false,
    DryadBaneProgress: default,
    TownNpcDamageMultiplier: null,
    ProjectileSnapshot: null,
    ProjectileSnapshotComplete: false));
Assert(
  healingRegeneration.Computed &&
    healingRegeneration.HealingPoints == 1 &&
    healingRegeneration.LifeRegenerationCountAfter == 0,
  "Crossing the 120 regeneration threshold must return one healing point and keep the remainder.");

NpcStatusFlagsComponent missingProjectileFlags = new()
{
  Celled = true,
  LifeRegenerationRate = 7,
};
NpcLifeRegenerationStateComponent missingProjectileState = new()
{
  LifeRegenerationCount = 33,
};
NpcLifeRegenerationResult missingProjectileSnapshot =
  statusSystem.ApplyDamageOverTimePhase(
    ref missingProjectileFlags,
    ref missingProjectileState,
    new NpcLifeRegenerationInput(
      npcType,
      AiState1: 0.0f,
      NpcLegacySlot: 0,
      CurrentLife: 50,
      MaximumLife: 100,
      DamageProtected: false,
      Immortal: false,
      GoodWorld: false,
      LavaWet: false,
      HasRealLifeParent: false,
      InfectedSeed: false,
      DryadBaneProgress: default,
      TownNpcDamageMultiplier: null,
      ProjectileSnapshot: null,
      ProjectileSnapshotComplete: false));
Assert(
  !missingProjectileSnapshot.Computed &&
    missingProjectileSnapshot.FailureReason ==
      NpcLifeRegenerationFailureReason.ProjectileSnapshotRequired &&
    missingProjectileFlags.LifeRegenerationRate == 7 &&
    missingProjectileState.LifeRegenerationCount == 33,
  "A missing projectile snapshot must reject the whole DoT phase without partial state changes.");

NpcStatusFlagsComponent shortProjectileFlags = new()
{
  Celled = true,
};
NpcLifeRegenerationStateComponent shortProjectileState = default;
NpcLifeRegenerationResult shortProjectileSnapshot =
  statusSystem.ApplyDamageOverTimePhase(
    ref shortProjectileFlags,
    ref shortProjectileState,
    new NpcLifeRegenerationInput(
      npcType,
      AiState1: 0.0f,
      NpcLegacySlot: 0,
      CurrentLife: 50,
      MaximumLife: 100,
      DamageProtected: false,
      Immortal: false,
      GoodWorld: false,
      LavaWet: false,
      HasRealLifeParent: false,
      InfectedSeed: false,
      DryadBaneProgress: default,
      TownNpcDamageMultiplier: null,
      ProjectileSnapshot: new NpcLifeRegenerationProjectileSnapshot[999],
      ProjectileSnapshotComplete: true));
Assert(
  !shortProjectileSnapshot.Computed &&
    shortProjectileSnapshot.FailureReason == NpcLifeRegenerationFailureReason.InvalidInput,
  "A snapshot shorter than Version4's 1000 projectile slots must not be treated as complete.");

NpcLifeRegenerationProjectileSnapshot[] celledProjectiles =
  new NpcLifeRegenerationProjectileSnapshot[1000];
celledProjectiles[0] = new NpcLifeRegenerationProjectileSnapshot(
  IsActive: true,
  ProjectileType: 614,
  Ai0: 1.0f,
  Ai1: 7.0f);
NpcStatusFlagsComponent celledFlags = new()
{
  Celled = true,
  LifeRegenerationExpectedLossPerSecond = 15,
};
NpcLifeRegenerationStateComponent celledState = default;
NpcLifeRegenerationResult celledRegeneration = statusSystem.ApplyDamageOverTimePhase(
  ref celledFlags,
  ref celledState,
  new NpcLifeRegenerationInput(
    npcType,
    AiState1: 0.0f,
    NpcLegacySlot: 7,
    CurrentLife: 50,
    MaximumLife: 100,
    DamageProtected: false,
    Immortal: false,
    GoodWorld: false,
    LavaWet: false,
    HasRealLifeParent: false,
    InfectedSeed: false,
    DryadBaneProgress: default,
    TownNpcDamageMultiplier: null,
    ProjectileSnapshot: celledProjectiles,
    ProjectileSnapshotComplete: true));
Assert(
  celledRegeneration.Computed &&
    celledRegeneration.LifeRegenerationAfter == -40 &&
    celledRegeneration.EffectiveDamagePerSecond == 10,
  "Celled must replace expected damage with half of projectileCount * 20 when below that threshold.");

NpcStatusFlagsComponent dryadBaneFlags = new()
{
  DryadBane = true,
  LifeRegenerationExpectedLossPerSecond = 5,
};
NpcLifeRegenerationStateComponent dryadBaneState = default;
NpcLifeRegenerationResult dryadBaneRegeneration =
  statusSystem.ApplyDamageOverTimePhase(
    ref dryadBaneFlags,
    ref dryadBaneState,
    new NpcLifeRegenerationInput(
      npcType,
      AiState1: 0.0f,
      NpcLegacySlot: 0,
      CurrentLife: 50,
      MaximumLife: 100,
      DamageProtected: false,
      Immortal: false,
      GoodWorld: false,
      LavaWet: false,
      HasRealLifeParent: false,
      InfectedSeed: false,
      DryadBaneProgress: default,
      TownNpcDamageMultiplier: 1.5f,
      ProjectileSnapshot: null,
      ProjectileSnapshotComplete: false));
Assert(
  dryadBaneRegeneration.Computed &&
    dryadBaneRegeneration.LifeRegenerationAfter == -12 &&
    dryadBaneRegeneration.EffectiveDamagePerSecond == 2,
  "Dryad Bane must scale town NPC damage and replace the lower threshold with townNpcDamage / 3.");

NpcStatusFlagsComponent overflowingDryadBaneFlags = new()
{
  DryadBane = true,
  LifeRegenerationRate = 4,
};
NpcLifeRegenerationStateComponent overflowingDryadBaneState = new()
{
  LifeRegenerationCount = 12,
};
NpcLifeRegenerationResult overflowingDryadBane =
  statusSystem.ApplyDamageOverTimePhase(
    ref overflowingDryadBaneFlags,
    ref overflowingDryadBaneState,
    new NpcLifeRegenerationInput(
      npcType,
      AiState1: 0.0f,
      NpcLegacySlot: 0,
      CurrentLife: 50,
      MaximumLife: 100,
      DamageProtected: false,
      Immortal: false,
      GoodWorld: false,
      LavaWet: false,
      HasRealLifeParent: false,
      InfectedSeed: false,
      DryadBaneProgress: default,
      TownNpcDamageMultiplier: 300_000_000.0f,
      ProjectileSnapshot: null,
      ProjectileSnapshotComplete: false));
Assert(
  !overflowingDryadBane.Computed &&
    overflowingDryadBane.FailureReason ==
      NpcLifeRegenerationFailureReason.ArithmeticOverflow &&
    overflowingDryadBaneFlags.LifeRegenerationRate == 4 &&
    overflowingDryadBaneState.LifeRegenerationCount == 12,
  "An unrepresentable Dryad Bane regeneration rate must reject without partial state changes.");

const int onFireBuffType = 24;
const int frostBurnBuffType = 44;
StatusEffectSlotsComponent refreshSlots = new(
  new[]
  {
    new StatusEffectSlot(onFireBuffType, 1),
    new StatusEffectSlot(frostBurnBuffType, 1),
  },
  revision: 1);
NpcStatusFlagsComponent refreshFlags = default;
NpcBuffStateUpdateResult refreshResult = statusSystem.ApplyBuffSlotPhase(
  new NpcTypeId(1),
  new NpcAiStateComponent(0, 0.0f, 9.0f, 0.0f, 0.0f, 0),
  default,
  ref refreshSlots,
  ref refreshFlags);
Assert(
  refreshResult.Applied &&
    refreshSlots.Slots.Span[0].RemainingTicks == 60 &&
    refreshSlots.Slots.Span[1].RemainingTicks == 60 &&
    refreshFlags.OnFire &&
    refreshFlags.OnFrostBurn,
  "Type 1 AI state 9 must refresh On Fire and Frostburn timers after the tick decrement.");

const int shimmeringBuffType = 353;
StatusEffectSlotsComponent missingImmunitySlots = new(
  new[]
  {
    new StatusEffectSlot(shimmeringBuffType, 12),
    new StatusEffectSlot(20, 8),
  },
  revision: 9);
NpcStatusFlagsComponent unchangedStatusFlags = new() { Poisoned = true };
NpcBuffStateUpdateResult missingImmunityResult = statusSystem.ApplyBuffSlotPhase(
  npcType,
  new NpcAiStateComponent(0, 0.0f, 0.0f, 0.0f, 0.0f, 0),
  default,
  ref missingImmunitySlots,
  ref unchangedStatusFlags);
Assert(
  !missingImmunityResult.Applied &&
    missingImmunityResult.RequiredInputMissing &&
    missingImmunitySlots.Revision == 9 &&
    missingImmunitySlots.Slots.Span[0].DefinitionId == shimmeringBuffType &&
    missingImmunitySlots.Slots.Span[0].RemainingTicks == 12 &&
    unchangedStatusFlags.Poisoned &&
    missingImmunityResult.Effects.Length == 0,
  "A shimmer immunity input gap must leave both status flags and buff slots uncommitted.");

var shimmerImmunity = new bool[shimmeringBuffType + 1];
shimmerImmunity[shimmeringBuffType] = true;
StatusEffectSlotsComponent shimmerSlots = new(
  new[]
  {
    new StatusEffectSlot(shimmeringBuffType, 12),
    new StatusEffectSlot(137, 10),
    new StatusEffectSlot(103, 10),
    new StatusEffectSlot(20, 1),
  },
  revision: 2);
NpcStatusFlagsComponent shimmerFlags = default;
NpcBuffStateUpdateResult shimmerResult = statusSystem.ApplyBuffSlotPhase(
  npcType,
  new NpcAiStateComponent(0, 0.0f, 0.0f, 0.0f, 0.0f, 0),
  new StatusEffectImmunityComponent(shimmerImmunity),
  ref shimmerSlots,
  ref shimmerFlags);
Assert(
  shimmerResult.Applied &&
    shimmerSlots.Slots.Span[0].DefinitionId == 137 &&
    shimmerSlots.Slots.Span[1].DefinitionId == 103 &&
    shimmerSlots.Slots.Span[2].DefinitionId == 20 &&
    shimmerSlots.Slots.Span[2].RemainingTicks == 0 &&
    shimmerFlags.Dripping &&
    !shimmerFlags.DrippingSlime &&
    shimmerFlags.Poisoned &&
    shimmerResult.Effects.Length == 2 &&
    shimmerResult.Effects.Span[0].Kind ==
      NpcBuffStateUpdateResult.EffectKind.BuffSlotSyncRequested &&
    shimmerResult.Effects.Span[1].Kind ==
      NpcBuffStateUpdateResult.EffectKind.WaterPerishableCleanupRequested,
  "Status effects must preserve shimmer sync, the SoulDrain insertion point, and cleanup intent order.");
NpcBuffStateUpdateResult shimmerExpiredBuffs = statusSystem.ClearExpiredBuffs(
  ref shimmerSlots);
Assert(
  shimmerSlots.Slots.Span[2].DefinitionId == 0 &&
    shimmerExpiredBuffs.Effects.Length == 1 &&
    shimmerExpiredBuffs.Effects.Span[0].Kind ==
      NpcBuffStateUpdateResult.EffectKind.BuffSlotSyncRequested,
  "Expired cleanup must follow the status phase and emit its sync intent afterward.");

var movementSystem = new NpcMovementSystem();
NpcKnockbackResult lowDamageKnockback = movementSystem.CalculateKnockback(
  new NpcKnockbackRequest(
    new NpcTypeId(50),
    new Vector2(0.0f, 2.0f),
    Knockback: 10.0f,
    HitDirection: 1,
    KnockbackResistance: 0.5f,
    OnFire2: false,
    Critical: false,
    ResolvedDamage: 5,
    LifeMaximum: 100,
    ExpertMode: false,
    NoGravity: false));
Assert(
  lowDamageKnockback.IsEligible &&
    lowDamageKnockback.VelocityBefore == new Vector2(0.0f, 2.0f) &&
  lowDamageKnockback.VelocityAfter == new Vector2(2.5f, -1.875f),
  "Low-damage NPC knockback must replace velocity using the resolved magnitude and resistance.");

NpcKnockbackResult thresholdEqualKnockback = movementSystem.CalculateKnockback(
  new NpcKnockbackRequest(
    new NpcTypeId(185),
    new Vector2(4.0f, 2.0f),
    Knockback: 10.0f,
    HitDirection: -1,
    KnockbackResistance: 1.0f,
    OnFire2: false,
    Critical: false,
    ResolvedDamage: 10,
    LifeMaximum: 100,
    ExpertMode: false,
    NoGravity: false));
Assert(
  thresholdEqualKnockback.IsEligible &&
    Vector2.Distance(
      thresholdEqualKnockback.VelocityAfter,
      new Vector2(-9.8f, -7.35f)) < 0.001f,
  $"Equal-threshold knockback expected (-9.8, -7.35), got " +
    $"eligible={thresholdEqualKnockback.IsEligible}, " +
    $"velocity={thresholdEqualKnockback.VelocityAfter}.");

NpcKnockbackResult highDamageKnockback = movementSystem.CalculateKnockback(
  new NpcKnockbackRequest(
    new NpcTypeId(185),
    new Vector2(5.0f, 0.0f),
    Knockback: 20.0f,
    HitDirection: -1,
    KnockbackResistance: 1.0f,
    OnFire2: true,
    Critical: true,
    ResolvedDamage: 7,
    LifeMaximum: 100,
    ExpertMode: true,
    NoGravity: true));
const float expectedHighDamageKnockback = 21.73024f;
Assert(
  highDamageKnockback.IsEligible &&
    MathF.Abs(highDamageKnockback.VelocityAfter.X + expectedHighDamageKnockback) < 0.001f &&
    MathF.Abs(
      highDamageKnockback.VelocityAfter.Y + expectedHighDamageKnockback * 0.75f) < 0.001f,
  "Expert high-damage knockback must apply the diminishing scale, crit, and special vertical impulse.");

NpcKnockbackResult zeroResistanceKnockback = movementSystem.CalculateKnockback(
  new NpcKnockbackRequest(
    new NpcTypeId(50),
    new Vector2(1.0f, 2.0f),
    Knockback: 10.0f,
    HitDirection: 1,
    KnockbackResistance: 0.0f,
    OnFire2: false,
    Critical: false,
    ResolvedDamage: 50,
    LifeMaximum: 100,
    ExpertMode: false,
    NoGravity: false));
Assert(
  !zeroResistanceKnockback.IsEligible &&
    zeroResistanceKnockback.VelocityBefore == zeroResistanceKnockback.VelocityAfter,
  "NPCs with zero knockback resistance must retain their current velocity.");

var strikeTracking = new NpcDamageTrackingSystem(
  (type, isBoss) => definitions.CreateStrategy(type, isBoss));
var strikeLifecycleSystem = new NpcDeathLifecycleSystem();
var strikeCombatSystem = new NpcCombatSystem(
  strikeTracking,
  strikeLifecycleSystem,
  new RecordingNpcDamageOverTimeTextPort());
var strikeHealth = new NpcHealthComponent(100, 100);
var strikeLifecycle = new NpcLifecycleComponent(
  isActive: true,
  remainingActiveTicks: 600,
  stage: NpcLifecycleStage.Active);
var strikeMovementState = new MovementStateComponent(
  velocity: new Vector2(4.0f, 2.0f));
NpcStrikeResult strikeResult = strikeCombatSystem.ResolveAndCommitStrike(
  new NpcTypeId(185),
  strikeHealth,
  strikeLifecycle,
  new NpcDamageRequest(
    CombatContributorId.FromLegacyOwner(255),
    Damage: 10,
    Tick: 0,
    BypassTrackerCredit: true),
  new NpcKnockbackInput(
    Knockback: 10.0f,
    HitDirection: -1,
    KnockbackResistance: 1.0f,
    OnFire2: false,
    ExpertMode: false,
    NoGravity: false),
  movementSystem,
  ref strikeMovementState);
Assert(
    strikeResult.CombatResult.Applied &&
    strikeResult.CombatResult.ResolvedDamage == 10 &&
    strikeResult.KnockbackResult.IsEligible &&
    Vector2.Distance(
      strikeMovementState.Velocity,
      new Vector2(-9.8f, -7.35f)) < 0.001f &&
    !strikeResult.CombatResult.DeathTransitioned,
  $"A strike must commit its movement knockback after damage and before " +
    $"death reconciliation; got applied={strikeResult.CombatResult.Applied}, " +
    $"damage={strikeResult.CombatResult.ResolvedDamage}, " +
    $"eligible={strikeResult.KnockbackResult.IsEligible}, " +
    $"velocity={strikeMovementState.Velocity}, " +
    $"death={strikeResult.CombatResult.DeathTransitioned}.");

Assert(
  CombatContributorId.FromLegacyOwner(255).Kind == CombatContributorKind.World &&
    CombatContributorId.FromLegacyOwner(-1).Kind == CombatContributorKind.World,
  "Legacy world owners 255 and -1 must normalize to world credit.");
Assert(
  CombatContributorId.FromLegacyOwner(7, "player-7").Kind ==
    CombatContributorKind.Player,
  "A legacy player owner must retain player provenance.");
bool mobMappingFound = definitions.TryGetBossTypeForMob(
  mobType,
  out NpcTypeId mappedBossType);
Assert(
  mobMappingFound && mappedBossType == npcType,
  "The Version4 catalog must map the registered mob to its boss type.");

NpcTypeId compositeTrackerType = new(13);
NpcTypeId compositeSegmentType = new(14);
var compositeTracking = new NpcDamageTrackingSystem(
  (type, isBoss) => definitions.CreateStrategy(type, isBoss));
compositeTracking.AdvanceTo(
  0,
  new HashSet<NpcTypeId> { compositeSegmentType });
bool compositeDamageRecorded = compositeTracking.TryRecordAppliedDamage(
  compositeSegmentType,
  new CombatContributorId(CombatContributorKind.Player, "composite-player"),
  appliedAmount: 1,
  tick: 0);
NpcDamageTrackerSnapshot[] compositeSnapshots =
  compositeTracking.GetActiveSnapshots();
Assert(
  compositeDamageRecorded &&
    compositeSnapshots.Length == 1 &&
    compositeSnapshots[0].InitialNpcType == compositeSegmentType &&
    compositeSnapshots[0].TrackerNpcType == compositeTrackerType,
  "Composite boss damage must retain the hit segment and canonical Version4 tracker identity.");

NpcDamageTrackingSystem tracking = new(
  (type, isBoss) => definitions.CreateStrategy(type, isBoss));
tracking.AdvanceTo(0, new HashSet<NpcTypeId> { npcType });

Func<NpcTypeId, INpcDamageTrackingStrategy?> nullStrategyFactory = null!;
bool nullFactoryRejected = false;
try
{
  _ = new NpcDamageTrackingSystem(nullStrategyFactory);
}
catch (ArgumentNullException)
{
  nullFactoryRejected = true;
}

Assert(
  nullFactoryRejected,
  "The legacy strategy factory must reject null during construction.");

var health = new NpcHealthComponent(currentLife: 100, maximumLife: 100);
var lifecycle = new NpcLifecycleComponent(
  isActive: true,
  remainingActiveTicks: 600,
  stage: NpcLifecycleStage.Active);
var combat = new NpcCombatSystem(
  tracking,
  new NpcDeathLifecycleSystem(),
  new RecordingNpcDamageOverTimeTextPort());

NpcCombatResult firstHit = combat.ResolveAndCommit(
  npcType,
  health,
  lifecycle,
  new NpcDamageRequest(
    new CombatContributorId(
      CombatContributorKind.Player,
      "player-1"),
    Damage: 40,
    Tick: 0,
    Defense: 10,
    Critical: true,
    IsBoss: true));

Assert(firstHit.Applied, "The first NPC damage event must commit.");
Assert(firstHit.AppliedDamage == 70, "Version4 defense and critical damage must resolve before commit.");
Assert(health.CurrentLife == 30, "The health owner must receive one life commit.");
Assert(firstHit.TrackerRecorded, "Accepted boss damage must create tracker credit.");
Assert(lifecycle.IsActive, "A non-lethal hit must keep the NPC active.");

var forcedWorldTracking = new NpcDamageTrackingSystem(
  (type, isBoss) => definitions.CreateStrategy(type, isBoss));
forcedWorldTracking.AdvanceTo(0, new HashSet<NpcTypeId> { npcType });
var forcedWorldCombat = new NpcCombatSystem(
  forcedWorldTracking,
  new NpcDeathLifecycleSystem(),
  new RecordingNpcDamageOverTimeTextPort());
var forcedWorldHealth = new NpcHealthComponent(100, 100);
var forcedWorldLifecycle = new NpcLifecycleComponent(
  isActive: true,
  remainingActiveTicks: 600,
  stage: NpcLifecycleStage.Active);
NpcDamageRequest forcedWorldRequest = NpcDamageRequest.FromLegacyOwner(
  damage: 9999,
  tick: 0,
  legacyOwner: 255,
  isBoss: true);
NpcCombatResult forcedWorldHit = forcedWorldCombat.ResolveAndCommit(
  npcType,
  forcedWorldHealth,
  forcedWorldLifecycle,
  forcedWorldRequest);
Assert(
  forcedWorldHit.Applied &&
    forcedWorldHit.AppliedDamage == 100 &&
    !forcedWorldHit.TrackerRecorded &&
    forcedWorldTracking.ActiveTrackerCount == 0,
  "Legacy owner 255 forced world damage must bypass tracker credit while applying life damage.");

var invalidOwnerHealth = new NpcHealthComponent(20, 20);
var invalidOwnerLifecycle = new NpcLifecycleComponent(
  isActive: true,
  remainingActiveTicks: 600,
  stage: NpcLifecycleStage.Active);
NpcCombatResult invalidOwnerResult = combat.ResolveAndCommit(
  npcType,
  invalidOwnerHealth,
  invalidOwnerLifecycle,
  new NpcDamageRequest(
    new CombatContributorId(CombatContributorKind.World, null),
    Damage: 5,
    Tick: 0,
    IsBoss: true,
    LegacyOwner: 0));
Assert(
  !invalidOwnerResult.Applied &&
    invalidOwnerResult.RejectionReason ==
      NpcDamageRejectionReason.InvalidContributor &&
    invalidOwnerHealth.CurrentLife == 20,
  "A legacy owner mismatch must reject without a health or tracker write.");

var parentIdentity = new NpcEntityIdentityComponent(
  new NpcInstanceId(42),
  new NpcSlot(4));
var parentRelation = new NpcParentRelationComponent(
  parentIdentity.InstanceId,
  parentIdentity.LegacySlot,
  attachedAtTick: 0);
var parentHealth = new NpcHealthComponent(200, 250);
var parentLifecycle = new NpcLifecycleComponent(
  isActive: true,
  remainingActiveTicks: 600,
  stage: NpcLifecycleStage.Active);
var parentTarget = new NpcParentHealthTarget(
  parentIdentity,
  npcType,
  parentHealth,
  parentLifecycle,
  isBoss: true);
var segmentHealth = new NpcHealthComponent(140, 150);
var segmentLifecycle = new NpcLifecycleComponent(
  isActive: true,
  remainingActiveTicks: 600,
  stage: NpcLifecycleStage.Active);
var staleParentIdentity = new NpcEntityIdentityComponent(
  new NpcInstanceId(43),
  new NpcSlot(5));
var staleParentTarget = new NpcParentHealthTarget(
  staleParentIdentity,
  npcType,
  new NpcHealthComponent(200, 250),
  new NpcLifecycleComponent(
    isActive: true,
    remainingActiveTicks: 600,
    stage: NpcLifecycleStage.Active),
  isBoss: true);
NpcCombatResult staleParentHit = combat.ResolveAndCommit(
  new NpcTypeId(9000),
  segmentHealth,
  segmentLifecycle,
  new NpcDamageRequest(
    new CombatContributorId(CombatContributorKind.Player, "player-1"),
    Damage: 40,
    Tick: 0,
    Defense: 10),
  parentRelation,
  staleParentTarget);
Assert(
  !staleParentHit.Applied &&
    staleParentHit.RejectionReason ==
      NpcDamageRejectionReason.InvalidParentRelation &&
    parentHealth.CurrentLife == 200 &&
    segmentHealth.CurrentLife == 140 &&
    tracking.GetActiveSnapshots()[0].Credits.Span[0].AppliedDamage == 70,
  "A stale parent identity must reject without parent, mirror, or tracker writes.");

var reusedParentSlotIdentity = new NpcEntityIdentityComponent(
  parentIdentity.InstanceId,
  new NpcSlot(5));
var reusedParentSlotTarget = new NpcParentHealthTarget(
  reusedParentSlotIdentity,
  npcType,
  new NpcHealthComponent(200, 250),
  new NpcLifecycleComponent(
    isActive: true,
    remainingActiveTicks: 600,
    stage: NpcLifecycleStage.Active),
  isBoss: true);
NpcCombatResult reusedParentSlotHit = combat.ResolveAndCommit(
  new NpcTypeId(9000),
  segmentHealth,
  segmentLifecycle,
  new NpcDamageRequest(
    new CombatContributorId(CombatContributorKind.Player, "player-1"),
    Damage: 40,
    Tick: 0,
    Defense: 10),
  parentRelation,
  reusedParentSlotTarget);
Assert(
  !reusedParentSlotHit.Applied &&
    reusedParentSlotHit.RejectionReason ==
      NpcDamageRejectionReason.InvalidParentRelation &&
    parentHealth.CurrentLife == 200 &&
    segmentHealth.CurrentLife == 140 &&
    tracking.GetActiveSnapshots()[0].Credits.Span[0].AppliedDamage == 70,
  "A reused parent slot must reject without parent, mirror, or tracker writes.");

NpcCombatResult parentHit = combat.ResolveAndCommit(
  new NpcTypeId(9000),
  segmentHealth,
  segmentLifecycle,
  new NpcDamageRequest(
    new CombatContributorId(CombatContributorKind.Player, "player-1"),
    Damage: 40,
    Tick: 0,
    Defense: 10),
  parentRelation,
  parentTarget);
Assert(
  parentHit.Applied &&
    parentHit.AppliedDamage == 35 &&
    parentHit.LifeOwnerInstanceId == parentIdentity.InstanceId &&
    parentHealth.CurrentLife == 165 &&
    parentHealth.MaximumLife == 250 &&
    segmentHealth.CurrentLife == 165 &&
    segmentHealth.MaximumLife == 250 &&
    parentHit.TrackerRecorded &&
    tracking.GetActiveSnapshots()[0].Credits.Span[0].AppliedDamage == 105,
  "A segment hit must credit and damage its resolved parent, then mirror parent life bounds.");

var immortalTracking = new NpcDamageTrackingSystem(
  (type, isBoss) => definitions.CreateStrategy(type, isBoss));
immortalTracking.AdvanceTo(0, new HashSet<NpcTypeId> { npcType });
var immortalCombat = new NpcCombatSystem(
  immortalTracking,
  new NpcDeathLifecycleSystem(),
  new RecordingNpcDamageOverTimeTextPort());
var immortalParentIdentity = new NpcEntityIdentityComponent(
  new NpcInstanceId(46),
  new NpcSlot(8));
var immortalParentRelation = new NpcParentRelationComponent(
  immortalParentIdentity.InstanceId,
  immortalParentIdentity.LegacySlot,
  attachedAtTick: 0);
var immortalParentHealth = new NpcHealthComponent(90, 120);
var immortalParentLifecycle = new NpcLifecycleComponent(
  isActive: true,
  remainingActiveTicks: 600,
  stage: NpcLifecycleStage.Active);
var immortalParentTarget = new NpcParentHealthTarget(
  immortalParentIdentity,
  npcType,
  immortalParentHealth,
  immortalParentLifecycle,
  isBoss: true);
var immortalSegmentHealth = new NpcHealthComponent(20, 30);
var immortalSegmentLifecycle = new NpcLifecycleComponent(
  isActive: true,
  remainingActiveTicks: 600,
  stage: NpcLifecycleStage.Active);
NpcCombatResult immortalStrike = immortalCombat.ResolveAndCommit(
  new NpcTypeId(9000),
  immortalSegmentHealth,
  immortalSegmentLifecycle,
  new NpcDamageRequest(
    new CombatContributorId(CombatContributorKind.Player, "immortal-strike"),
    Damage: 40,
    Tick: 0,
    Defense: 10,
    Immortal: true),
  immortalParentRelation,
  immortalParentTarget);
Assert(
  immortalStrike.Applied &&
    immortalStrike.RejectionReason == NpcDamageRejectionReason.None &&
    immortalStrike.AppliedDamage == 0 &&
    immortalStrike.LifeBefore == 90 &&
    immortalStrike.LifeAfter == 90 &&
    !immortalStrike.TrackerRecorded &&
    !immortalStrike.DeathTransitioned &&
    immortalStrike.LifeOwnerInstanceId == immortalParentIdentity.InstanceId &&
    immortalParentHealth.CurrentLife == 90 &&
    immortalSegmentHealth.CurrentLife == 20 &&
    immortalSegmentHealth.MaximumLife == 30 &&
    immortalTracking.ActiveTrackerCount == 0,
  "An accepted immortal segment strike must skip tracker, parent life, and child mirror writes.");

var firstSegmentHitTracking = new NpcDamageTrackingSystem(
  (type, isBoss) => definitions.CreateStrategy(type, isBoss));
firstSegmentHitTracking.AdvanceTo(0, new HashSet<NpcTypeId> { npcType });
var firstSegmentHitCombat = new NpcCombatSystem(
  firstSegmentHitTracking,
  new NpcDeathLifecycleSystem(),
  new RecordingNpcDamageOverTimeTextPort());
var firstSegmentParentIdentity = new NpcEntityIdentityComponent(
  new NpcInstanceId(45),
  new NpcSlot(7));
var firstSegmentParentRelation = new NpcParentRelationComponent(
  firstSegmentParentIdentity.InstanceId,
  firstSegmentParentIdentity.LegacySlot,
  attachedAtTick: 0);
var firstSegmentParentHealth = new NpcHealthComponent(200, 250);
var firstSegmentParentTarget = new NpcParentHealthTarget(
  firstSegmentParentIdentity,
  npcType,
  firstSegmentParentHealth,
  new NpcLifecycleComponent(
    isActive: true,
    remainingActiveTicks: 600,
    stage: NpcLifecycleStage.Active),
  isBoss: true);
var firstSegmentHealth = new NpcHealthComponent(140, 150);
var firstSegmentLifecycle = new NpcLifecycleComponent(
  isActive: true,
  remainingActiveTicks: 600,
  stage: NpcLifecycleStage.Active);
NpcCombatResult firstSegmentHit = firstSegmentHitCombat.ResolveAndCommit(
  new NpcTypeId(9000),
  firstSegmentHealth,
  firstSegmentLifecycle,
  new NpcDamageRequest(
    new CombatContributorId(CombatContributorKind.Player, "player-segment"),
    Damage: 40,
    Tick: 0,
    Defense: 10),
  firstSegmentParentRelation,
  firstSegmentParentTarget);
NpcDamageTrackerSnapshot[] firstSegmentSnapshots =
  firstSegmentHitTracking.GetActiveSnapshots();
Assert(
  firstSegmentHit.Applied &&
    firstSegmentHit.TrackerRecorded &&
    firstSegmentSnapshots.Length == 1 &&
    firstSegmentSnapshots[0].InitialNpcType == new NpcTypeId(9000) &&
    firstSegmentSnapshots[0].TrackerNpcType == npcType,
  "A first parent-segment hit must preserve the hit type while tracking under the parent type.");

var inactiveParentIdentity = new NpcEntityIdentityComponent(
  new NpcInstanceId(44),
  new NpcSlot(6));
var inactiveParentRelation = new NpcParentRelationComponent(
  inactiveParentIdentity.InstanceId,
  inactiveParentIdentity.LegacySlot,
  attachedAtTick: 0);
var inactiveParentHealth = new NpcHealthComponent(200, 250);
var inactiveParentTarget = new NpcParentHealthTarget(
  inactiveParentIdentity,
  npcType,
  inactiveParentHealth,
  new NpcLifecycleComponent(
    isActive: false,
    remainingActiveTicks: 0,
    stage: NpcLifecycleStage.Despawned),
  isBoss: true);
var inactiveSegmentHealth = new NpcHealthComponent(200, 250);
NpcCombatResult inactiveParentHit = combat.ResolveAndCommit(
  new NpcTypeId(9000),
  inactiveSegmentHealth,
  new NpcLifecycleComponent(
    isActive: true,
    remainingActiveTicks: 600,
    stage: NpcLifecycleStage.Active),
  new NpcDamageRequest(
    new CombatContributorId(CombatContributorKind.Player, "player-1"),
    Damage: 5,
    Tick: 0),
  inactiveParentRelation,
  inactiveParentTarget);
Assert(
  inactiveParentHit.Applied &&
    !inactiveParentHit.TrackerRecorded &&
    inactiveParentHealth.CurrentLife == 195 &&
    inactiveSegmentHealth.CurrentLife == 195 &&
    tracking.GetActiveSnapshots()[0].Credits.Span[0].AppliedDamage == 105,
  "An inactive resolved parent still receives the life commit but not tracker credit.");

var dotTracking = new NpcDamageTrackingSystem(
  (type, isBoss) => definitions.CreateStrategy(type, isBoss));
dotTracking.AdvanceTo(0, new HashSet<NpcTypeId> { npcType });
var dotTextPort = new RecordingNpcDamageOverTimeTextPort();
var dotCombat = new NpcCombatSystem(
  dotTracking,
  new NpcDeathLifecycleSystem(),
  dotTextPort);
var dotHealth = new NpcHealthComponent(100, 100);
var dotLifecycle = new NpcLifecycleComponent(
  isActive: true,
  remainingActiveTicks: 600,
  stage: NpcLifecycleStage.Active);
var dotSourceIdentity = new NpcEntityIdentityComponent(
  new NpcInstanceId(80),
  new NpcSlot(9));
var dotTextBounds = new NpcDamageOverTimeTextBounds(40, 50, 16, 32);
NpcDamageOverTimeResult dotHit = dotCombat.ApplyDamageOverTime(
  npcType,
  dotHealth,
  dotLifecycle,
  new NpcDamageOverTimeRequest(
    Amount: 7,
    Tick: 0,
    TextSourceIdentity: dotSourceIdentity,
    TextBounds: dotTextBounds,
    IsBoss: true));
Assert(
  dotHit.Accepted &&
    dotHit.DirectDamageApplied == 7 &&
    dotHealth.CurrentLife == 93 &&
    dotHit.TrackerRecorded &&
    dotHit.CombatTextPublished &&
    dotTracking.GetActiveSnapshots()[0].WorldDamage == 7 &&
    dotHit.CombatTextIntent is NpcDamageOverTimeTextIntent textIntent &&
    textIntent.SourceInstanceId == dotSourceIdentity.InstanceId &&
    textIntent.Bounds == dotTextBounds &&
    textIntent.Amount == 7 &&
    !textIntent.Dramatic &&
    textIntent.IsDamageOverTime,
  "DoT must commit its direct amount through combat and record world tracker credit.");

NpcDamageOverTimeResult immortalDot = dotCombat.ApplyDamageOverTime(
  npcType,
  dotHealth,
  dotLifecycle,
  new NpcDamageOverTimeRequest(
    Amount: 5,
    Tick: 0,
    TextSourceIdentity: dotSourceIdentity,
    TextBounds: dotTextBounds,
    Immortal: true,
    IsBoss: true));
Assert(
  immortalDot.Accepted &&
    immortalDot.TrackerRecorded &&
    immortalDot.CombatTextRequested &&
    immortalDot.CombatTextPublished &&
    immortalDot.DirectDamageApplied == 0 &&
    dotHealth.CurrentLife == 93 &&
    dotTracking.GetActiveSnapshots()[0].WorldDamage == 12,
  "Immortal DoT must credit world damage and request combat text without changing life.");

var dotParentIdentity = new NpcEntityIdentityComponent(
  new NpcInstanceId(90),
  new NpcSlot(10));
var dotParentRelation = new NpcParentRelationComponent(
  dotParentIdentity.InstanceId,
  dotParentIdentity.LegacySlot,
  attachedAtTick: 0);
var dotParentHealth = new NpcHealthComponent(4, 20);
var dotParentLifecycle = new NpcLifecycleComponent(
  isActive: true,
  remainingActiveTicks: 600,
  stage: NpcLifecycleStage.Active);
var dotParentTarget = new NpcParentHealthTarget(
  dotParentIdentity,
  npcType,
  dotParentHealth,
  dotParentLifecycle,
  isBoss: true);
var dotSegmentIdentity = new NpcEntityIdentityComponent(
  new NpcInstanceId(91),
  new NpcSlot(11));
var dotSegmentTextBounds = new NpcDamageOverTimeTextBounds(12, 14, 24, 36);
var dotSegmentHealth = new NpcHealthComponent(0, 50);
var dotSegmentLifecycle = new NpcLifecycleComponent(
  isActive: true,
  remainingActiveTicks: 600,
  stage: NpcLifecycleStage.Active);
int? parentLifeAtTextPublication = null;
bool? parentActiveAtTextPublication = null;
dotTextPort.TryPublishHandler = _ =>
{
  parentLifeAtTextPublication = dotParentHealth.CurrentLife;
  parentActiveAtTextPublication = dotParentLifecycle.IsActive;
  return false;
};
NpcDamageOverTimeResult lethalParentDot = dotCombat.ApplyDamageOverTime(
  new NpcTypeId(9000),
  dotSegmentHealth,
  dotSegmentLifecycle,
  new NpcDamageOverTimeRequest(
    Amount: 8,
    Tick: 0,
    TextSourceIdentity: dotSegmentIdentity,
    TextBounds: dotSegmentTextBounds),
  dotParentRelation,
  dotParentTarget);
Assert(lethalParentDot.Accepted, "A valid parent DoT must be accepted.");
Assert(
  lethalParentDot.LifeOwnerInstanceId == dotParentIdentity.InstanceId,
  "Parent DoT must retain the resolved parent identity.");
Assert(
  lethalParentDot.DirectDamageApplied == 8 &&
    lethalParentDot.LifeAfterDirectStage == 1 &&
    lethalParentDot.RequiresForcedDeathStrike &&
    lethalParentDot.ForcedDeathStrikeResult is NpcCombatResult forcedStrike &&
    forcedStrike.Applied &&
    forcedStrike.AppliedDamage == 1 &&
    !forcedStrike.TrackerRecorded &&
    forcedStrike.DeathTransitioned &&
    !lethalParentDot.CombatTextPublished &&
    lethalParentDot.DeathPacket28Requested &&
    lethalParentDot.CombatTextRequested,
  $"Unexpected DoT result: direct={lethalParentDot.DirectDamageApplied}, " +
    $"life={lethalParentDot.LifeAfterDirectStage}, " +
    $"strike={lethalParentDot.RequiresForcedDeathStrike}, " +
    $"text={lethalParentDot.CombatTextRequested}.");
Assert(
  parentLifeAtTextPublication == 1 &&
    parentActiveAtTextPublication == true &&
    dotTextPort.PublishedIntents.Count == 3,
  "CombatText must publish after the direct stage but before forced death resolution.");
Assert(
  lethalParentDot.CombatTextIntent is NpcDamageOverTimeTextIntent parentTextIntent &&
    parentTextIntent.SourceInstanceId == dotSegmentIdentity.InstanceId &&
    parentTextIntent.Bounds == dotSegmentTextBounds &&
    parentTextIntent.Amount == 8,
  "DoT CombatText intent must preserve the child identity, bounds, and raw amount.");
Assert(
  lethalParentDot.DeathPacket28Intent is NpcDeathPacket28Intent packetIntent &&
    packetIntent.TargetInstanceId == dotParentIdentity.InstanceId &&
    packetIntent.TargetLegacySlot == dotParentIdentity.LegacySlot &&
    packetIntent.Damage == 9999,
  "Lethal parent DoT must target packet 28 at the parent legacy slot.");
Assert(
  dotParentHealth.CurrentLife == 0 &&
    lethalParentDot.LifeAfterForcedDeathStrike == 0 &&
    dotSegmentHealth.CurrentLife == 0 &&
    !dotParentLifecycle.IsActive,
  "Lethal parent DoT must use parent life despite dead child life and avoid child mirroring.");
int worldDamageAfterLethalDot = dotTracking.GetActiveSnapshots()[0].WorldDamage;
Assert(
  worldDamageAfterLethalDot == 16,
  $"Tracker world credit must clamp to parent life; observed {worldDamageAfterLethalDot}.");

var redHatHealth = new NpcHealthComponent(currentLife: 100, maximumLife: 100);
var redHatLifecycle = new NpcLifecycleComponent(
  isActive: true,
  remainingActiveTicks: 600,
  stage: NpcLifecycleStage.Active);
NpcCombatResult redHatHit = combat.ResolveAndCommit(
  npcType,
  redHatHealth,
  redHatLifecycle,
  new NpcDamageRequest(
    new CombatContributorId(
      CombatContributorKind.Player,
      "formula-check"),
    Damage: 40,
    Tick: 0,
    Defense: 10,
    Critical: true,
    BypassTrackerCredit: true,
    RedHatSkeletronAdjustmentEnabled: true));

Assert(
  redHatHit.AppliedDamage == 48 && redHatHealth.CurrentLife == 52,
  "Red Hat Skeletron adjustment must truncate after its multiplier.");

var subOneMultiplierHealth = new NpcHealthComponent(
  currentLife: 100,
  maximumLife: 100);
var subOneMultiplierLifecycle = new NpcLifecycleComponent(
  isActive: true,
  remainingActiveTicks: 600,
  stage: NpcLifecycleStage.Active);
NpcCombatResult subOneMultiplierHit = combat.ResolveAndCommit(
  npcType,
  subOneMultiplierHealth,
  subOneMultiplierLifecycle,
  new NpcDamageRequest(
    new CombatContributorId(
      CombatContributorKind.Player,
      "formula-check"),
    Damage: 10,
    Tick: 0,
    TakenDamageMultiplier: 0.5f,
    BypassTrackerCredit: true));

Assert(
  subOneMultiplierHit.AppliedDamage == 10 &&
    subOneMultiplierHealth.CurrentLife == 90,
  "Version4 must ignore taken-damage multipliers at or below one.");

var mobHealth = new NpcHealthComponent(currentLife: 20, maximumLife: 20);
var mobLifecycle = new NpcLifecycleComponent(
  isActive: true,
  remainingActiveTicks: 600,
  stage: NpcLifecycleStage.Active);
NpcCombatResult mobHit = combat.ResolveAndCommit(
  mobType,
  mobHealth,
  mobLifecycle,
  new NpcDamageRequest(
    new CombatContributorId(
      CombatContributorKind.Player,
      "player-2"),
    Damage: 7,
    Tick: 0));

Assert(
  mobHit.Applied && mobHit.TrackerRecorded,
  "A registered mob hit must join the active boss tracker.");
NpcDamageTrackerSnapshot[] activeBossTrackers = tracking.GetActiveSnapshots();
Assert(
  activeBossTrackers.Length == 1,
  "The registered mob must reuse the boss encounter tracker.");
Assert(
  activeBossTrackers[0].TrackerNpcType == npcType,
  "The active tracker must retain the boss as its tracker NPC type.");
Assert(
  activeBossTrackers[0].Credits.Length == 2,
  "Boss and registered mob damage must share one tracker credit report.");

NpcCombatResult rejectedMultiplier = combat.ResolveAndCommit(
  npcType,
  health,
  lifecycle,
  new NpcDamageRequest(
    new CombatContributorId(
      CombatContributorKind.Player,
      "player-2"),
    Damage: 10,
    Tick: 0,
    TakenDamageMultiplier: float.NaN));

Assert(!rejectedMultiplier.Applied, "An invalid multiplier must not commit damage.");
Assert(
  rejectedMultiplier.RejectionReason ==
    NpcDamageRejectionReason.InvalidMultiplier,
  "An invalid multiplier must report its rejection reason.");
Assert(health.CurrentLife == 30, "A rejected hit must not write health.");
Assert(
  tracking.GetActiveSnapshots()[0].Credits.Length == 2,
  "A rejected hit must not create tracker credit.");

tracking.AdvanceTo(1, new HashSet<NpcTypeId> { npcType });
NpcCombatResult lethalHit = combat.ResolveAndCommit(
  npcType,
  health,
  lifecycle,
  new NpcDamageRequest(
    new CombatContributorId(CombatContributorKind.World, null),
    Damage: 9999,
    Tick: 1));

Assert(lethalHit.Applied, "The lethal NPC damage event must commit.");
Assert(lethalHit.AppliedDamage == 30, "Damage must clamp to remaining life.");
Assert(health.IsDead, "The health owner must reach zero exactly once.");
Assert(lethalHit.DeathTransitioned, "The death lifecycle must transition once.");
Assert(!lifecycle.IsActive, "A lethal commit must deactivate the NPC.");

NpcDeathLifecycleResult repeatedDeath = new NpcDeathLifecycleSystem().Reconcile(
  health,
  lifecycle);
Assert(
  repeatedDeath.AlreadyTerminal && !repeatedDeath.Transitioned,
  "A repeated death reconciliation must not commit a second transition.");

var deathPhaseLifecycleSystem = new NpcDeathLifecycleSystem();
var splitPhaseAi = new NpcAiStateComponent(0, 0.0f, 0.0f, 0.0f, 6.5f, 0);
var splitPhaseHealth = new NpcHealthComponent(currentLife: 0, maximumLife: 100);
var splitPhaseLifecycle = new NpcLifecycleComponent(
  isActive: true,
  remainingActiveTicks: 600,
  stage: NpcLifecycleStage.Active);
var splitPhaseDamagePolicy = new DamageAcceptancePolicyComponent(
  rejectAllDamage: false,
  rejectHostileDamage: false,
  rejectTrapDamage: false,
  isImmortal: false);
NpcDeathLifecycleResult splitPhase = deathPhaseLifecycleSystem.Reconcile(
  new NpcTypeId(396),
  splitPhaseHealth,
  splitPhaseLifecycle,
  isLifeOwner: true,
  new Vector2(12.75f, -3.75f),
  splitPhaseAi);
Assert(
  !splitPhase.IsTerminal &&
    splitPhase.PhaseDecision is NpcDeathPhaseDecision splitDecision &&
    splitDecision.PhaseKind == NpcDeathPhaseKind.Type396Or397SpawnPhase &&
    splitDecision.StateChanged &&
    splitDecision.AiAfter.State0 == -2.0f &&
    splitDecision.RestoreLifeToMaximum &&
    splitDecision.RejectAllDamage &&
    splitDecision.ReplicationSyncRequested &&
    splitDecision.SpawnIntent is NpcDeathPhaseSpawnIntent splitSpawn &&
    splitSpawn.NpcType == new NpcTypeId(400) &&
    splitSpawn.PositionX == 12 &&
    splitSpawn.PositionY == -3 &&
    splitSpawn.Ai3 == 6.5f &&
    splitPhaseAi.State0 == 0.0f &&
    splitPhaseHealth.CurrentLife == 0 &&
    !splitPhaseDamagePolicy.RejectAllDamage,
  "Types 396/397 must return a phase decision and type 400 intent without writing owner state.");

var heldSplitPhaseAi = new NpcAiStateComponent(0, -2.0f, 0.0f, 0.0f, 6.5f, 0);
var heldSplitPhaseHealth = new NpcHealthComponent(currentLife: 0, maximumLife: 100);
var heldSplitPhaseLifecycle = new NpcLifecycleComponent(
  isActive: true,
  remainingActiveTicks: 600,
  stage: NpcLifecycleStage.Active);
NpcDeathLifecycleResult heldSplitPhase = deathPhaseLifecycleSystem.Reconcile(
  new NpcTypeId(397),
  heldSplitPhaseHealth,
  heldSplitPhaseLifecycle,
  isLifeOwner: true,
  Vector2.Zero,
  heldSplitPhaseAi);
Assert(
  !heldSplitPhase.IsTerminal &&
    heldSplitPhase.PhaseDecision is NpcDeathPhaseDecision heldDecision &&
    heldDecision.PhaseKind == NpcDeathPhaseKind.Type396Or397PhaseHeld &&
    !heldDecision.StateChanged &&
    heldDecision.SpawnIntent is null &&
    heldSplitPhaseHealth.CurrentLife == 0,
  "A repeated type 396/397 phase check must not spawn a second type 400 NPC.");

var type398Ai = new NpcAiStateComponent(0, 0.0f, 0.0f, 0.0f, 0.0f, 0);
var type398Health = new NpcHealthComponent(currentLife: 0, maximumLife: 80);
var type398Lifecycle = new NpcLifecycleComponent(
  isActive: true,
  remainingActiveTicks: 600,
  stage: NpcLifecycleStage.Active);
var type398Policy = new DamageAcceptancePolicyComponent(false, true, false, false);
NpcDeathLifecycleResult type398Phase = deathPhaseLifecycleSystem.Reconcile(
  new NpcTypeId(398),
  type398Health,
  type398Lifecycle,
  isLifeOwner: true,
  Vector2.Zero,
  type398Ai);
Assert(
  !type398Phase.IsTerminal &&
    type398Phase.PhaseDecision is NpcDeathPhaseDecision type398Decision &&
    type398Decision.PhaseKind == NpcDeathPhaseKind.Type398EnterAi0Two &&
    type398Decision.AiAfter.State0 == 2.0f &&
    type398Decision.RestoreLifeToMaximum &&
    type398Decision.RejectAllDamage &&
    type398Decision.ReplicationSyncRequested &&
    type398Ai.State0 == 0.0f &&
    type398Health.CurrentLife == 0 &&
    !type398Policy.RejectAllDamage &&
    type398Policy.RejectHostileDamage,
  "Type 398 must return its phase decision without directly writing owner state.");

var type398CombatTracking = new NpcDamageTrackingSystem(
  (type, isBoss) => definitions.CreateStrategy(type, isBoss));
type398CombatTracking.AdvanceTo(0, new HashSet<NpcTypeId>());
var type398CombatSystem = new NpcCombatSystem(
  type398CombatTracking,
  deathPhaseLifecycleSystem,
  new RecordingNpcDamageOverTimeTextPort());
var type398CombatHealth = new NpcHealthComponent(5, 80);
var type398CombatLifecycle = new NpcLifecycleComponent(
  isActive: true,
  remainingActiveTicks: 600,
  stage: NpcLifecycleStage.Active);
var type398Behavior = new NpcBehaviorComponent(0, 0, new float[4]);
var type398SyncIntent = new NpcNetworkSyncIntentComponent();
var authoritySystem = new NpcAuthoritySystem();
NpcCombatResult type398CombatResult = type398CombatSystem.ResolveAndCommit(
  new NpcTypeId(398),
  type398CombatHealth,
  type398CombatLifecycle,
  new NpcDamageRequest(
    new CombatContributorId(CombatContributorKind.World, null),
    Damage: 5,
    Tick: 0,
    BypassTrackerCredit: true),
  deathPhaseContext: new NpcDeathPhaseContext(
    new NpcAiStateComponent(0, 0.0f, 0.0f, 0.0f, 0.0f, 0),
    Vector2.Zero,
    IsLifeOwner: true,
    IsGoodWorld: false,
    BottomY: null));
bool type398PhaseCommitted = authoritySystem.CommitDeathPhaseDecision(
  type398CombatResult.DeathLifecycle,
  type398Behavior,
  ref type398Policy,
  type398SyncIntent);
Assert(
  type398CombatResult.Applied &&
    type398CombatResult.DeathLifecycle is NpcDeathLifecycleResult combatDeath &&
    !combatDeath.IsTerminal &&
    combatDeath.PhaseDecision is NpcDeathPhaseDecision combatDecision &&
    combatDecision.PhaseKind == NpcDeathPhaseKind.Type398EnterAi0Two &&
    !type398CombatResult.DeathTransitioned &&
    type398CombatResult.LifeAfter == type398CombatHealth.MaximumLife &&
    type398CombatHealth.CurrentLife == type398CombatHealth.MaximumLife &&
    type398CombatLifecycle.IsActive &&
    type398PhaseCommitted &&
    type398Behavior.AiSlots[0] == 2.0f &&
    type398Policy.RejectAllDamage &&
    type398Policy.RejectHostileDamage &&
    type398SyncIntent.IsPending,
  "Combat must restore phase health and return a non-terminal decision.");

NpcCombatResult rejectedPhaseHit = type398CombatSystem.ResolveAndCommit(
  new NpcTypeId(398),
  type398CombatHealth,
  type398CombatLifecycle,
  new NpcDamageRequest(
    new CombatContributorId(CombatContributorKind.World, null),
    Damage: 1,
    Tick: 0,
    BypassTrackerCredit: true),
  damagePolicy: type398Policy);
Assert(
  !rejectedPhaseHit.Applied &&
    rejectedPhaseHit.RejectionReason == NpcDamageRejectionReason.DamagePolicy &&
    type398CombatHealth.CurrentLife == type398CombatHealth.MaximumLife &&
    type398CombatLifecycle.IsActive,
  "The committed type 398 damage policy must reject later damage.");

var type398SegmentParentIdentity = new NpcEntityIdentityComponent(
  new NpcInstanceId(3981),
  new NpcSlot(398));
var type398SegmentParentHealth = new NpcHealthComponent(5, 80);
var type398SegmentParentLifecycle = new NpcLifecycleComponent(
  isActive: true,
  remainingActiveTicks: 600,
  stage: NpcLifecycleStage.Active);
var type398SegmentTarget = new NpcParentHealthTarget(
  type398SegmentParentIdentity,
  new NpcTypeId(398),
  type398SegmentParentHealth,
  type398SegmentParentLifecycle,
  isBoss: false);
var type398SegmentHealth = new NpcHealthComponent(5, 80);
var type398SegmentLifecycle = new NpcLifecycleComponent(
  isActive: true,
  remainingActiveTicks: 600,
  stage: NpcLifecycleStage.Active);
NpcCombatResult type398SegmentResult = type398CombatSystem.ResolveAndCommit(
  new NpcTypeId(9000),
  type398SegmentHealth,
  type398SegmentLifecycle,
  new NpcDamageRequest(
    new CombatContributorId(CombatContributorKind.World, null),
    Damage: 5,
    Tick: 0,
    BypassTrackerCredit: true),
  new NpcParentRelationComponent(
    type398SegmentParentIdentity.InstanceId,
    type398SegmentParentIdentity.LegacySlot,
    attachedAtTick: 0),
  type398SegmentTarget,
  new NpcDeathPhaseContext(
    new NpcAiStateComponent(0, 0.0f, 0.0f, 0.0f, 0.0f, 0),
    Vector2.Zero,
    IsLifeOwner: true,
    IsGoodWorld: false,
    BottomY: null));
Assert(
  type398SegmentResult.Applied &&
    type398SegmentResult.LifeAfter ==
      type398SegmentParentHealth.MaximumLife &&
    type398SegmentResult.DeathLifecycle is
      NpcDeathLifecycleResult segmentDeath &&
    segmentDeath.PhaseDecision is
      NpcDeathPhaseDecision segmentDecision &&
    segmentDecision.PhaseKind == NpcDeathPhaseKind.Type398EnterAi0Two &&
    !type398SegmentResult.DeathTransitioned &&
    type398SegmentParentHealth.CurrentLife ==
      type398SegmentParentHealth.MaximumLife &&
    type398SegmentHealth.CurrentLife ==
      type398SegmentParentHealth.CurrentLife &&
    type398SegmentHealth.MaximumLife ==
      type398SegmentParentHealth.MaximumLife &&
    type398SegmentParentLifecycle.IsActive &&
    type398SegmentLifecycle.IsActive,
  "Type 398 phase health restore must synchronize the struck segment.");

var type398DotTracking = new NpcDamageTrackingSystem(
  (type, isBoss) => definitions.CreateStrategy(type, isBoss));
type398DotTracking.AdvanceTo(
  0,
  new HashSet<NpcTypeId> { new NpcTypeId(398) });
var type398DotCombat = new NpcCombatSystem(
  type398DotTracking,
  deathPhaseLifecycleSystem,
  new RecordingNpcDamageOverTimeTextPort());
var type398DotHealth = new NpcHealthComponent(4, 80);
var type398DotLifecycle = new NpcLifecycleComponent(
  isActive: true,
  remainingActiveTicks: 600,
  stage: NpcLifecycleStage.Active);
NpcDamageOverTimeResult type398DotResult = type398DotCombat.ApplyDamageOverTime(
  new NpcTypeId(398),
  type398DotHealth,
  type398DotLifecycle,
  new NpcDamageOverTimeRequest(
    Amount: 4,
    Tick: 0,
    TextSourceIdentity: new NpcEntityIdentityComponent(
      new NpcInstanceId(3980),
      new NpcSlot(398)),
    TextBounds: new NpcDamageOverTimeTextBounds(0, 0, 16, 16)),
  deathPhaseContext: new NpcDeathPhaseContext(
    new NpcAiStateComponent(0, 0.0f, 0.0f, 0.0f, 0.0f, 0),
    Vector2.Zero,
    IsLifeOwner: true,
    IsGoodWorld: false,
    BottomY: null));
Assert(
  type398DotResult.RequiresForcedDeathStrike &&
    type398DotResult.ForcedDeathStrikeResult is NpcCombatResult type398ForcedStrike &&
    type398ForcedStrike.DeathLifecycle is NpcDeathLifecycleResult type398DotDeath &&
    type398DotDeath.PhaseDecision is NpcDeathPhaseDecision type398DotDecision &&
    type398DotDecision.PhaseKind == NpcDeathPhaseKind.Type398EnterAi0Two &&
    !type398ForcedStrike.DeathTransitioned &&
    type398DotHealth.CurrentLife == type398DotHealth.MaximumLife &&
    type398DotLifecycle.IsActive,
  "A lethal DoT forced strike must forward phase context and restore owner health.");

var ai2Phase = new NpcAiStateComponent(0, 0.0f, 4.0f, 0.0f, 0.0f, 0);
var ai2Health = new NpcHealthComponent(currentLife: 0, maximumLife: 60);
var ai2Lifecycle = new NpcLifecycleComponent(
  isActive: true,
  remainingActiveTicks: 600,
  stage: NpcLifecycleStage.Active);
var ai2Policy = new DamageAcceptancePolicyComponent(false, false, false, false);
NpcDeathLifecycleResult ai2PhaseResult = deathPhaseLifecycleSystem.Reconcile(
  new NpcTypeId(493),
  ai2Health,
  ai2Lifecycle,
  isLifeOwner: true,
  Vector2.Zero,
  ai2Phase);
Assert(
  !ai2PhaseResult.IsTerminal &&
    ai2PhaseResult.PhaseDecision is NpcDeathPhaseDecision ai2Decision &&
    ai2Decision.PhaseKind == NpcDeathPhaseKind.Type517422507493EnterAi2One &&
    ai2Decision.AiAfter.State2 == 1.0f &&
    ai2Decision.AiAfter.State1 == 0.0f &&
    ai2Decision.RestoreLifeToMaximum &&
    ai2Decision.RejectAllDamage &&
    ai2Phase.State1 == 4.0f &&
    ai2Health.CurrentLife == 0 &&
    !ai2Policy.RejectAllDamage,
  "Types 517, 422, 507, and 493 must return their AI and damage-policy decisions.");

var type548Ai = new NpcAiStateComponent(0, 5.0f, 0.0f, 0.0f, 0.0f, 0);
var type548Health = new NpcHealthComponent(currentLife: 0, maximumLife: 40);
var type548Lifecycle = new NpcLifecycleComponent(
  isActive: true,
  remainingActiveTicks: 600,
  stage: NpcLifecycleStage.Active);
var type548Policy = new DamageAcceptancePolicyComponent(false, false, false, false);
NpcDeathLifecycleResult type548Phase = deathPhaseLifecycleSystem.Reconcile(
  new NpcTypeId(548),
  type548Health,
  type548Lifecycle,
  isLifeOwner: true,
  Vector2.Zero,
  type548Ai);
Assert(
  !type548Phase.IsTerminal &&
    type548Phase.PhaseDecision is NpcDeathPhaseDecision type548Decision &&
    type548Decision.PhaseKind == NpcDeathPhaseKind.Type548EnterAi1One &&
    type548Decision.AiAfter.State0 == 0.0f &&
    type548Decision.AiAfter.State1 == 1.0f &&
    type548Decision.RestoreLifeToMaximum &&
    type548Decision.RejectHostileDamage &&
    !type548Decision.RejectAllDamage &&
    type548Ai.State0 == 5.0f &&
    type548Health.CurrentLife == 0 &&
  !type548Policy.RejectHostileDamage,
  "Type 548 must return its AI and hostile-damage policy decisions.");

var type548Behavior = new NpcBehaviorComponent(0, 0, new float[4]);
var type548SyncIntent = new NpcNetworkSyncIntentComponent();
bool type548PhaseCommitted = authoritySystem.CommitDeathPhaseDecision(
  type548Phase,
  type548Behavior,
  ref type548Policy,
  type548SyncIntent);
Assert(
  type548PhaseCommitted &&
    type548Behavior.AiSlots[0] == 0.0f &&
    type548Behavior.AiSlots[1] == 1.0f &&
    type548Policy.RejectHostileDamage &&
  type548SyncIntent.IsPending,
  "The authority owner must commit type 548 AI, hostile policy, and sync intent.");

var type548ExistingPolicy = new DamageAcceptancePolicyComponent(true, false, true, false);
var type548ExistingBehavior = new NpcBehaviorComponent(0, 0, new float[4]);
var type548ExistingSyncIntent = new NpcNetworkSyncIntentComponent();
bool type548ExistingPolicyCommitted = authoritySystem.CommitDeathPhaseDecision(
  type548Phase,
  type548ExistingBehavior,
  ref type548ExistingPolicy,
  type548ExistingSyncIntent);
Assert(
  type548ExistingPolicyCommitted &&
    type548ExistingPolicy.RejectAllDamage &&
    type548ExistingPolicy.RejectHostileDamage &&
    type548ExistingPolicy.RejectTrapDamage,
  "A phase decision must not clear unrelated existing damage restrictions.");

var type548CombatTracking = new NpcDamageTrackingSystem(
  (type, isBoss) => definitions.CreateStrategy(type, isBoss));
type548CombatTracking.AdvanceTo(0, new HashSet<NpcTypeId>());
var type548CombatSystem = new NpcCombatSystem(
  type548CombatTracking,
  deathPhaseLifecycleSystem,
  new RecordingNpcDamageOverTimeTextPort());
var type548CombatHealth = new NpcHealthComponent(40, 40);
var type548CombatLifecycle = new NpcLifecycleComponent(
  isActive: true,
  remainingActiveTicks: 600,
  stage: NpcLifecycleStage.Active);
NpcCombatResult rejectedHostileHit = type548CombatSystem.ResolveAndCommit(
  new NpcTypeId(548),
  type548CombatHealth,
  type548CombatLifecycle,
  new NpcDamageRequest(
    new CombatContributorId(CombatContributorKind.Player, "hostile"),
    Damage: 5,
    Tick: 0,
    BypassTrackerCredit: true,
    IsHostileDamage: true),
  damagePolicy: type548Policy);
NpcCombatResult acceptedNonHostileHit = type548CombatSystem.ResolveAndCommit(
  new NpcTypeId(548),
  type548CombatHealth,
  type548CombatLifecycle,
  new NpcDamageRequest(
    new CombatContributorId(CombatContributorKind.Player, "non-hostile"),
    Damage: 5,
    Tick: 0,
    BypassTrackerCredit: true),
  damagePolicy: type548Policy);
Assert(
  !rejectedHostileHit.Applied &&
    rejectedHostileHit.RejectionReason == NpcDamageRejectionReason.DamagePolicy &&
    acceptedNonHostileHit.Applied &&
    type548CombatHealth.CurrentLife == 35,
  "The committed type 548 policy must reject hostile hits and accept other hits.");

NpcDeathPhaseInput goodWorldType13Input = new(
  new NpcTypeId(13),
  IsActive: true,
  IsLifeOwner: true,
  CurrentLife: 0,
  new NpcAiStateComponent(0, 0.0f, 0.0f, 0.0f, 0.0f, 0),
  new Vector2(12.75f, -3.75f),
  IsGoodWorld: true,
  BottomY: 80.5f);
NpcDeathPhaseDecision goodWorldType13Decision =
  NpcDeathPhaseQuery.Evaluate(in goodWorldType13Input);
Assert(
  !goodWorldType13Decision.SuppressTerminalDeath &&
    goodWorldType13Decision.PhaseKind == NpcDeathPhaseKind.GoodWorldType13Spawn &&
    goodWorldType13Decision.WorldEffectIntent is NpcDeathWorldEffectIntent type13Spawn &&
    type13Spawn.Kind == NpcDeathWorldEffectKind.FixedPositionSpawn &&
    type13Spawn.SpawnNpcType == new NpcTypeId(-12) &&
    type13Spawn.FixedPositionPixels == new Vector2(12.0f, 80.0f) &&
    type13Spawn.SpawnCount == 1 &&
    type13Spawn.RequestReplicationSyncAfterSpawn,
  "GoodWorld type 13 death must describe its fixed spawn and follow-up sync request.");

NpcDeathPhaseInput goodWorldType13MissingBottomY = goodWorldType13Input with
{
  BottomY = null,
};
NpcDeathPhaseDecision missingBottomYDecision =
  NpcDeathPhaseQuery.Evaluate(in goodWorldType13MissingBottomY);
Assert(
  missingBottomYDecision.SuppressTerminalDeath &&
    missingBottomYDecision.RequiredInputMissing &&
    missingBottomYDecision.PhaseKind ==
      NpcDeathPhaseKind.GoodWorldType13MissingBottomY &&
    missingBottomYDecision.WorldEffectIntent is null,
  "GoodWorld type 13 death must retain terminal state when the spawn Y input is missing.");

var missingBottomYLifecycle = new NpcLifecycleComponent(
  isActive: true,
  remainingActiveTicks: 600,
  stage: NpcLifecycleStage.Active);
var missingBottomYHealth = new NpcHealthComponent(currentLife: 0, maximumLife: 100);
NpcDeathLifecycleResult missingBottomYResult = deathPhaseLifecycleSystem.Reconcile(
  new NpcTypeId(13),
  missingBottomYHealth,
  missingBottomYLifecycle,
  isLifeOwner: true,
  new Vector2(12.75f, -3.75f),
  goodWorldType13Input.Ai,
  isGoodWorld: true);
Assert(
  !missingBottomYResult.IsTerminal &&
    !missingBottomYResult.Transitioned &&
    missingBottomYLifecycle.IsActive &&
    missingBottomYResult.PhaseDecision is NpcDeathPhaseDecision blockedType13Decision &&
    blockedType13Decision.RequiredInputMissing,
  "The lifecycle must stay active when GoodWorld type 13 lacks its required spawn Y input.");
NpcDeathLifecycleResult ignoredMissingInputCommit =
  deathPhaseLifecycleSystem.CommitAfterPreTerminalEffects(
    missingBottomYHealth,
    missingBottomYLifecycle,
    missingBottomYResult);
Assert(
  !ignoredMissingInputCommit.IsTerminal &&
    !ignoredMissingInputCommit.Transitioned &&
    missingBottomYLifecycle.IsActive,
  "The explicit effect commit must reject a result that has no pending terminal effects.");

var goodWorldType36Lifecycle = new NpcLifecycleComponent(
  isActive: true,
  remainingActiveTicks: 600,
  stage: NpcLifecycleStage.Active);
var goodWorldType36Health = new NpcHealthComponent(currentLife: 0, maximumLife: 100);
NpcDeathLifecycleResult goodWorldType36Death = deathPhaseLifecycleSystem.Reconcile(
  new NpcTypeId(36),
  goodWorldType36Health,
  goodWorldType36Lifecycle,
  isLifeOwner: true,
  new Vector2(800.0f, 1200.0f),
  new NpcAiStateComponent(0, 0.0f, 0.0f, 0.0f, 0.0f, 0),
  isGoodWorld: true);
Assert(
  !goodWorldType36Death.IsTerminal &&
    goodWorldType36Death.TerminalCommitPending &&
    goodWorldType36Lifecycle.IsActive &&
    goodWorldType36Death.PhaseDecision is NpcDeathPhaseDecision type36Decision &&
    type36Decision.PhaseKind == NpcDeathPhaseKind.GoodWorldType36SpawnSearch &&
    type36Decision.WorldEffectIntent is NpcDeathWorldEffectIntent type36Spawn &&
    type36Spawn.Kind == NpcDeathWorldEffectKind.SurfaceSearchSpawn &&
    type36Spawn.SpawnNpcType == new NpcTypeId(32) &&
    type36Spawn.OriginCenter == new Vector2(800.0f, 1200.0f) &&
    type36Spawn.FixedPositionPixels is null &&
    type36Spawn.SpawnCount == 3 &&
    type36Spawn.RandomOffsetRadiusInTiles == 50 &&
    type36Spawn.SearchAttemptsPerSpawn == 1000 &&
    type36Spawn.WorldBottomMarginInTiles == 200 &&
    type36Spawn.RequestReplicationSyncAfterSpawn,
  "GoodWorld type 36 death must defer terminal commit with its surface-search spawn plan.");

var childDeathAi = new NpcAiStateComponent(0, 0.0f, 0.0f, 0.0f, 1.0f, 0);
var childDeathHealth = new NpcHealthComponent(currentLife: 0, maximumLife: 20);
var childDeathLifecycle = new NpcLifecycleComponent(
  isActive: true,
  remainingActiveTicks: 600,
  stage: NpcLifecycleStage.Active);
var childDeathPolicy = new DamageAcceptancePolicyComponent(false, false, false, false);
NpcDeathLifecycleResult childDeathGuard = deathPhaseLifecycleSystem.Reconcile(
  new NpcTypeId(35),
  childDeathHealth,
  childDeathLifecycle,
  isLifeOwner: false,
  Vector2.Zero,
  childDeathAi);
Assert(
  !childDeathGuard.IsTerminal &&
    childDeathGuard.PhaseDecision is NpcDeathPhaseDecision childDecision &&
    childDecision.PhaseKind == NpcDeathPhaseKind.EntryGuardRejected &&
    childDecision.AnnouncementIntent is null &&
    childDeathAi.State0 == 0.0f &&
    childDeathHealth.CurrentLife == 0 &&
    !childDeathPolicy.RejectAllDamage,
  "A non-owner segment must not transition or commit the terminal death state.");

var skeletronLifecycle = new NpcLifecycleComponent(
  isActive: true,
  remainingActiveTicks: 600,
  stage: NpcLifecycleStage.Active);
var skeletronHealth = new NpcHealthComponent(currentLife: 0, maximumLife: 100);
NpcDeathLifecycleResult skeletronDeath = deathPhaseLifecycleSystem.Reconcile(
  new NpcTypeId(35),
  skeletronHealth,
  skeletronLifecycle,
  isLifeOwner: true,
  Vector2.Zero,
  new NpcAiStateComponent(0, 0.0f, 0.0f, 0.0f, 1.0f, 0));
Assert(
  !skeletronDeath.IsTerminal &&
    skeletronDeath.TerminalCommitPending &&
    skeletronLifecycle.IsActive &&
    skeletronDeath.PhaseDecision is NpcDeathPhaseDecision skeletronDecision &&
    skeletronDecision.AnnouncementIntent is NpcDeathAnnouncementIntent announcement &&
    announcement.LocalizationKey == "SkeletronText.Taunt1" &&
    announcement.Red == 255 && announcement.Green == 0 && announcement.Blue == 0 &&
    skeletronDecision.LadyBugKilledIntent is null,
  "Type 35 death must return its red Skeletron taunt before terminal commit.");
NpcDeathLifecycleResult committedSkeletronDeath =
  deathPhaseLifecycleSystem.CommitAfterPreTerminalEffects(
    skeletronHealth,
    skeletronLifecycle,
    skeletronDeath);
Assert(
  committedSkeletronDeath.IsTerminal && !skeletronLifecycle.IsActive,
  "Type 35 terminal commit must remain available as a follow-up call.");

foreach ((int type, bool isGold) in new[] { (604, false), (605, true) })
{
  var ladyBugLifecycle = new NpcLifecycleComponent(
    isActive: true,
    remainingActiveTicks: 600,
    stage: NpcLifecycleStage.Active);
  var ladyBugHealth = new NpcHealthComponent(currentLife: 0, maximumLife: 100);
  NpcDeathLifecycleResult ladyBugDeath = deathPhaseLifecycleSystem.Reconcile(
    new NpcTypeId(type),
    ladyBugHealth,
    ladyBugLifecycle,
    isLifeOwner: true,
    new Vector2(40.0f, 50.0f),
    new NpcAiStateComponent(0, 0.0f, 0.0f, 0.0f, 0.0f, 0));
  Assert(
    !ladyBugDeath.IsTerminal &&
      ladyBugDeath.TerminalCommitPending &&
      ladyBugLifecycle.IsActive &&
      ladyBugDeath.PhaseDecision is NpcDeathPhaseDecision ladyBugDecision &&
      ladyBugDecision.LadyBugKilledIntent is NpcLadyBugKilledIntent ladyBugIntent &&
      ladyBugIntent.Position == new Vector2(40.0f, 50.0f) &&
      ladyBugIntent.GoldLadyBug == isGold &&
      ladyBugDecision.AnnouncementIntent is null,
    $"Type {type} death must return its source-defined effect intent before terminal commit.");
  NpcDeathLifecycleResult committedLadyBugDeath =
    deathPhaseLifecycleSystem.CommitAfterPreTerminalEffects(
      ladyBugHealth,
      ladyBugLifecycle,
      ladyBugDeath);
  Assert(
    committedLadyBugDeath.IsTerminal && !ladyBugLifecycle.IsActive,
    $"Type {type} terminal commit must remain available as a follow-up call.");
}

var inactivePendingDespawn = new NpcLifecycleComponent(
  isActive: false,
  remainingActiveTicks: 0,
  stage: NpcLifecycleStage.PendingDespawn);
NpcDeathLifecycleResult ignoredInactiveDeath =
  new NpcDeathLifecycleSystem().Reconcile(
    new NpcHealthComponent(currentLife: 0, maximumLife: 10),
    inactivePendingDespawn);
Assert(
  !ignoredInactiveDeath.IsTerminal &&
    !ignoredInactiveDeath.Transitioned &&
    !ignoredInactiveDeath.AlreadyTerminal &&
    inactivePendingDespawn.Stage == NpcLifecycleStage.PendingDespawn,
  "An inactive pending-despawn NPC must not be reconciled as terminal.");

NpcCombatResult duplicateHit = combat.ResolveAndCommit(
  npcType,
  health,
  lifecycle,
  new NpcDamageRequest(
    new CombatContributorId(
      CombatContributorKind.Player,
      "player-1"),
    Damage: 10,
    Tick: 1));

Assert(!duplicateHit.Applied, "A post-death hit must be rejected.");
Assert(
  duplicateHit.RejectionReason == NpcDamageRejectionReason.Inactive,
  "A post-death hit must report the lifecycle rejection.");
Assert(
  tracking.GetActiveSnapshots().Length == 1,
  "The encounter remains active until the activity phase closes it.");

tracking.AdvanceTo(2, new HashSet<NpcTypeId>());
NpcDamageTrackerSnapshot[] recent = tracking.GetRecentSnapshots();
Assert(recent.Length == 1, "Closing the NPC activity must retain one recent tracker.");
Assert(recent[0].IsKilled, "The lethal commit must mark the tracker killed.");
Assert(recent[0].Credits.Length == 3,
  "Three accepted hits must produce three contributors.");

Console.WriteLine("PASS: P12 combat, life, tracker and death core smoke");

sealed class RecordingNpcDamageOverTimeTextPort : INpcDamageOverTimeTextPort
{
  public List<NpcDamageOverTimeTextIntent> PublishedIntents { get; } = new();

  public Func<NpcDamageOverTimeTextIntent, bool>? TryPublishHandler { get; set; }

  public bool TryPublish(in NpcDamageOverTimeTextIntent intent)
  {
    PublishedIntents.Add(intent);
    return TryPublishHandler?.Invoke(intent) ?? true;
  }
}
