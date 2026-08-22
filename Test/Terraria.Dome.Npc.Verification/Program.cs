using System;
using System.Collections.Generic;
using System.Linq;
using Arch.Core;
using Terraria.Dome.Simulation;
using Terraria.Dome.Simulation.Combat;
using Terraria.Dome.Simulation.Combat.Components;
using Terraria.Dome.Simulation.Combat.Systems;
using Terraria.Dome.Simulation.Items;
using Terraria.Dome.Simulation.Npc.Commands;
using Terraria.Dome.Simulation.Npc.Components;
using Terraria.Dome.Simulation.Npc.Definitions;
using Terraria.Dome.Simulation.Npc.Snapshots;
using Terraria.Dome.Simulation.Npc.Systems;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Simulation.WorldModel.Systems;
using Terraria.Dome.Simulation.Physics.Systems;
using SimulationComponents = Terraria.Dome.Simulation.Components;

NpcDefinition ordinaryDefinition = new(
  DefinitionId: 1,
  NetId: 1,
  MaximumHealth: 100,
  Defense: 0,
  ColliderWidth: 1.0f,
  ColliderHeight: 2.0f,
  BehaviorId: NpcBehaviorId.OrdinaryChase,
  LootTableId: 1);
NpcDefinitionRegistry registry = new([ordinaryDefinition]);
if (!registry.TryGet(ordinaryDefinition.DefinitionId, out NpcDefinition resolved) ||
    resolved != ordinaryDefinition)
{
  throw new InvalidOperationException("NPC definition registry did not preserve the definition contract.");
}

try
{
  _ = registry.GetRequired(99);
  throw new InvalidOperationException("NPC definition registry accepted an unknown definition ID.");
}
catch (KeyNotFoundException)
{
}

try
{
  _ = new NpcDefinitionRegistry([ordinaryDefinition, ordinaryDefinition]);
  throw new InvalidOperationException("NPC definition registry accepted a duplicate definition ID.");
}
catch (ArgumentException)
{
}

try
{
  _ = new NpcDefinition(
    DefinitionId: 2,
    NetId: 2,
    MaximumHealth: 0,
    Defense: 0,
    ColliderWidth: 1.0f,
    ColliderHeight: 2.0f,
    BehaviorId: NpcBehaviorId.OrdinaryChase,
    LootTableId: 1);
  throw new InvalidOperationException("NPC definition accepted non-positive maximum health.");
}
catch (ArgumentOutOfRangeException)
{
}

NpcTargetSelectionSystem targetSelection = new();
using World targetWorld = World.Create();
Entity firstPlayer = targetWorld.Create();
Entity secondPlayer = targetWorld.Create();
NpcTargetComponent selectedTarget = targetSelection.SelectTarget(
  new SimulationVector(0.0f, 0.0f),
  [
    new NpcTargetCandidate(secondPlayer, 2, new SimulationVector(5.0f, 0.0f), true, 100),
    new NpcTargetCandidate(firstPlayer, 1, new SimulationVector(-5.0f, 0.0f), true, 100),
    new NpcTargetCandidate(default, 4, new SimulationVector(0.0f, 0.0f), true, 100, true),
    new NpcTargetCandidate(default, 3, new SimulationVector(1.0f, 0.0f), true, 0)
  ]);
if (!selectedTarget.HasTarget || selectedTarget.StableTargetId != 1 ||
    selectedTarget.LockReason != NpcTargetLockReason.NearestActivePlayer)
{
  throw new InvalidOperationException("NPC target selection did not filter dead players or break ties by handle.");
}

NpcTargetComponent noTarget = targetSelection.SelectTarget(
  new SimulationVector(0.0f, 0.0f),
  [new NpcTargetCandidate(firstPlayer, 1, new SimulationVector(1.0f, 0.0f), false, 100)]);
if (noTarget.HasTarget || noTarget.LockReason != NpcTargetLockReason.NoValidTarget)
{
  throw new InvalidOperationException("NPC target selection retained an inactive player.");
}

NpcTargetComponent ghostOnlyTarget = targetSelection.SelectTarget(
  new SimulationVector(0.0f, 0.0f),
  [new NpcTargetCandidate(firstPlayer, 1, new SimulationVector(1.0f, 0.0f), true, 100, true)]);
if (ghostOnlyTarget.HasTarget || ghostOnlyTarget.LockReason != NpcTargetLockReason.NoValidTarget)
{
  throw new InvalidOperationException("NPC target selection retained a ghost player.");
}

NpcTargetComponent invalidTarget = targetSelection.SelectTarget(
  new SimulationVector(0.0f, 0.0f),
  [
    new NpcTargetCandidate(
      firstPlayer,
      0,
      new SimulationVector(float.NaN, 0.0f),
      true,
      100),
    new NpcTargetCandidate(
      firstPlayer,
      1,
      new SimulationVector(1.0f, 0.0f),
      true,
      100)
  ]);
if (!invalidTarget.HasTarget || invalidTarget.StableTargetId != 1)
{
  throw new InvalidOperationException("NPC target selection accepted an invalid identity or position.");
}

NpcTargetComponent defaultEntityTarget = targetSelection.SelectTarget(
  new SimulationVector(0.0f, 0.0f),
  [new NpcTargetCandidate(default, 1, new SimulationVector(1.0f, 0.0f), true, 100)]);
if (defaultEntityTarget.HasTarget)
{
  throw new InvalidOperationException("NPC target selection accepted a default entity identity.");
}

NpcTargetComponent invalidNpcPosition = targetSelection.SelectTarget(
  new SimulationVector(float.NaN, 0.0f),
  [new NpcTargetCandidate(firstPlayer, 1, new SimulationVector(1.0f, 0.0f), true, 100)]);
if (invalidNpcPosition.HasTarget)
{
  throw new InvalidOperationException("NPC target selection accepted a non-finite NPC position.");
}

NpcTargetRoutingSystem targetRouting = new();
NpcTargetRouteCandidate activeNpcTarget = new(secondPlayer, 4, true);
if (!targetRouting.TryResolve(304, true, 10, [activeNpcTarget], out Entity routedTarget) ||
    routedTarget != secondPlayer ||
    targetRouting.TryResolve(299, true, 10, [activeNpcTarget], out _) ||
    targetRouting.TryResolve(310, true, 10, [activeNpcTarget], out _) ||
    targetRouting.TryResolve(304, false, 10, [activeNpcTarget], out _))
{
  throw new InvalidOperationException("NPC encoded target routing did not preserve source bounds.");
}

if (targetRouting.TryResolve(
      304,
      true,
      10,
      [new NpcTargetRouteCandidate(default, 4, true)],
      out _) ||
    targetRouting.TryResolve(
      304,
      true,
      10,
      [new NpcTargetRouteCandidate(secondPlayer, 4, false)],
      out _))
{
  throw new InvalidOperationException("NPC encoded target routing accepted invalid target identity or state.");
}

NpcBehaviorSystem behaviorSystem = new();
NpcBehaviorStateComponent chaseState = new(
  NpcBehaviorId.OrdinaryChase,
  new NpcChaseState(1.0f, 0.0f),
  new NpcTownHomeState(default, true, 0));
NpcBehaviorResult chaseResult = behaviorSystem.Evaluate(
  chaseState,
  selectedTarget,
  new SimulationVector(0.0f, 0.0f),
  new SimulationVector(4.0f, 0.0f),
  isDayTime: true);
if (chaseResult.Velocity.X <= 0.0f || chaseResult.Facing != 1)
{
  throw new InvalidOperationException("Typed ordinary chase state did not produce forward movement.");
}

NpcDefinition trainingDummyDefinition = new(
  DefinitionId: 488,
  NetId: 488,
  MaximumHealth: 1000,
  Defense: 0,
  ColliderWidth: 18.0f,
  ColliderHeight: 40.0f,
  BehaviorId: NpcBehaviorId.TrainingDummy,
  LootTableId: 0,
  AiStyle: 92,
  IsImmortal: true,
  AlwaysReplicate: true);
NpcBehaviorStateComponent trainingDummyState = new(
  NpcBehaviorId.TrainingDummy,
  new NpcChaseState(0.0f, 0.0f),
  new NpcTownHomeState(default, true, 0));
NpcBehaviorResult trainingDummyResult = behaviorSystem.Evaluate(
  trainingDummyState,
  selectedTarget,
  new SimulationVector(0.0f, 0.0f),
  new SimulationVector(4.0f, 0.0f),
  isDayTime: true);
if (trainingDummyDefinition.NetId != 488 ||
    trainingDummyDefinition.AiStyle != 92 ||
    !trainingDummyDefinition.IsImmortal ||
    !trainingDummyDefinition.AlwaysReplicate ||
    trainingDummyResult.Velocity != default ||
    trainingDummyResult.Facing != 0)
{
  throw new InvalidOperationException("Training Dummy definition or stationary behavior is invalid.");
}

NpcBehaviorStateComponent townState = new(
  NpcBehaviorId.TownHome,
  new NpcChaseState(0.0f, 0.0f),
  new NpcTownHomeState(new SimulationVector(5.0f, 0.0f), false, 10));
NpcBehaviorResult townResult = behaviorSystem.Evaluate(
  townState,
  noTarget,
  new SimulationVector(0.0f, 0.0f),
  default,
  isDayTime: true);
if (townResult.Velocity.X <= 0.0f)
{
  throw new InvalidOperationException("Typed town-home state did not return toward home.");
}

try
{
  _ = behaviorSystem.Evaluate(
    townState,
    noTarget,
    new SimulationVector(float.NaN, 0.0f),
    default,
    isDayTime: true);
  throw new InvalidOperationException("NPC town-home behavior accepted a non-finite position.");
}
catch (ArgumentOutOfRangeException)
{
}

if (typeof(NpcBehaviorStateComponent).GetFields().Any(field =>
      field.FieldType.IsArray || typeof(IDictionary<,>).IsAssignableFrom(field.FieldType)))
{
  throw new InvalidOperationException("NPC behavior state must remain typed and bounded, not array-backed.");
}

using World invalidMovementWorld = World.Create();
new NpcMovementIntentSystem().Apply(invalidMovementWorld, default, isDayTime: true);

NpcSpawnEligibilitySystem spawnEligibility = new();
SpawnNpcCommand validSpawn = new(
  DefinitionId: 1,
  Position: new SimulationVector(40.0f, 0.0f),
  Source: NpcSpawnSource.Natural,
  RequestedReplicationId: 8);
IReadOnlyList<SpawnNpcCommand> eligibleSpawns = spawnEligibility.Evaluate(new NpcSpawnSnapshot(
  [
    new NpcSpawnCandidate(validSpawn, IsOccupied: false, IsProtectedSlot: false),
    new NpcSpawnCandidate(validSpawn with { RequestedReplicationId = 7 }, false, false),
    new NpcSpawnCandidate(validSpawn with { DefinitionId = 0 }, false, false),
    new NpcSpawnCandidate(validSpawn with { Position = new SimulationVector(41.0f, 0.0f) }, true, false)
  ],
  activeNpcCount: 0,
  maximumNpcCount: 2,
  protectedSlotCount: 0,
  existingReplicationIds: new HashSet<int> { 7 }));
if (eligibleSpawns.Count != 1 || eligibleSpawns[0] != validSpawn)
{
  throw new InvalidOperationException("NPC spawn eligibility did not reject occupied, invalid, or duplicate candidates.");
}

if (spawnEligibility.Evaluate(new NpcSpawnSnapshot(
      [new NpcSpawnCandidate(validSpawn, false, false)],
      activeNpcCount: 1,
      maximumNpcCount: 1,
      protectedSlotCount: 0,
      existingReplicationIds: new HashSet<int>())).Count != 0 ||
    spawnEligibility.Evaluate(new NpcSpawnSnapshot(
      [new NpcSpawnCandidate(validSpawn, false, true)],
      activeNpcCount: 0,
      maximumNpcCount: 2,
      protectedSlotCount: 0,
      existingReplicationIds: new HashSet<int>())).Count != 0)
{
  throw new InvalidOperationException("NPC spawn eligibility ignored exhausted budget or protected slots.");
}

if (spawnEligibility.Evaluate(new NpcSpawnSnapshot(
      [new NpcSpawnCandidate(validSpawn, false, false)],
      activeNpcCount: 0,
      maximumNpcCount: 2,
      protectedSlotCount: 0,
      existingReplicationIds: new HashSet<int>(),
      spawnAuthorityEnabled: false)).Count != 0)
{
  throw new InvalidOperationException("NPC spawn eligibility ignored a disabled player spawn authority.");
}

if (spawnEligibility.Evaluate(new NpcSpawnSnapshot(
      [new NpcSpawnCandidate(validSpawn, false, false, CanSpawnEnemiesNear: false)],
      activeNpcCount: 0,
      maximumNpcCount: 2,
      protectedSlotCount: 0,
      existingReplicationIds: new HashSet<int>())).Count != 0)
{
  throw new InvalidOperationException(
    "NPC spawn eligibility ignored the source player-readiness predicate.");
}

if (NpcSpawnPlayerReadinessQuery.CanSpawnEnemiesNear(
      new NpcSpawnPlayerReadiness(
        IsActive: false,
        IsDead: false,
        IsJourneyMode: false,
        IsSpawnRateDisabled: false,
        IsNearMoonLord: false)) ||
    NpcSpawnPlayerReadinessQuery.CanSpawnEnemiesNear(
      new NpcSpawnPlayerReadiness(
        IsActive: true,
        IsDead: true,
        IsJourneyMode: false,
        IsSpawnRateDisabled: false,
        IsNearMoonLord: false)) ||
    NpcSpawnPlayerReadinessQuery.CanSpawnEnemiesNear(
      new NpcSpawnPlayerReadiness(
        IsActive: true,
        IsDead: false,
        IsJourneyMode: true,
        IsSpawnRateDisabled: true,
        IsNearMoonLord: false)) ||
    NpcSpawnPlayerReadinessQuery.CanSpawnEnemiesNear(
      new NpcSpawnPlayerReadiness(
        IsActive: true,
        IsDead: false,
        IsJourneyMode: false,
        IsSpawnRateDisabled: false,
        IsNearMoonLord: true)) ||
    !NpcSpawnPlayerReadinessQuery.CanSpawnEnemiesNear(
      new NpcSpawnPlayerReadiness(
        IsActive: true,
        IsDead: false,
        IsJourneyMode: true,
        IsSpawnRateDisabled: false,
        IsNearMoonLord: false)))
{
  throw new InvalidOperationException(
    "NPC spawn player-readiness query did not preserve source CanSpawnEnemiesNear guards.");
}

if (spawnEligibility.Evaluate(new NpcSpawnSnapshot(
      [new NpcSpawnCandidate(
        validSpawn with { Position = new SimulationVector(float.NaN, 0.0f) },
        false,
        false)],
      activeNpcCount: 0,
      maximumNpcCount: 2,
      protectedSlotCount: 0,
      existingReplicationIds: new HashSet<int>())).Count != 0)
{
  throw new InvalidOperationException("NPC spawn eligibility accepted a non-finite spawn coordinate.");
}

if (spawnEligibility.Evaluate(new NpcSpawnSnapshot(
      [new NpcSpawnCandidate(
        validSpawn,
        false,
        false,
        IsInvasionCandidate: true)],
      activeNpcCount: 0,
      maximumNpcCount: 2,
      protectedSlotCount: 0,
      existingReplicationIds: new HashSet<int>(),
      invasionState: new NpcInvasionSpawnState(2, 40, 1))).Count != 0 ||
    spawnEligibility.Evaluate(new NpcSpawnSnapshot(
      [new NpcSpawnCandidate(
        validSpawn,
        false,
        false,
        IsInvasionCandidate: true)],
      activeNpcCount: 0,
      maximumNpcCount: 2,
      protectedSlotCount: 0,
      existingReplicationIds: new HashSet<int>(),
      invasionState: new NpcInvasionSpawnState(2, 40, 0))).Count != 1 ||
    spawnEligibility.Evaluate(new NpcSpawnSnapshot(
      [new NpcSpawnCandidate(
        validSpawn,
        false,
        false,
        IsInvasionCandidate: true)],
      activeNpcCount: 0,
      maximumNpcCount: 2,
      protectedSlotCount: 0,
      existingReplicationIds: new HashSet<int>())).Count != 0)
{
  throw new InvalidOperationException(
    "NPC spawn eligibility did not consume authoritative invasion delay facts.");
}

NpcSpawnCommitSystem spawnCommit = new();
using World spawnWorld = World.Create();
if (spawnCommit.TryCommit(
      spawnWorld,
      registry,
      validSpawn,
      replicationId: 8,
      out NpcSpawnCommitResult spawnResult,
      out _))
{
  NpcDefinitionComponent definitionComponent = spawnWorld.Get<NpcDefinitionComponent>(spawnResult.Entity);
  NpcAuthorityComponent authorityComponent =
    spawnWorld.Get<NpcAuthorityComponent>(spawnResult.Entity);
  NpcLifecycleComponent lifecycleComponent = spawnWorld.Get<NpcLifecycleComponent>(spawnResult.Entity);
  NpcReplicationComponent replicationComponent =
    spawnWorld.Get<NpcReplicationComponent>(spawnResult.Entity);
  if (definitionComponent.DefinitionId != 1 || !lifecycleComponent.IsActive ||
      replicationComponent.ReplicationId != 8 || authorityComponent.IsImmortal ||
      authorityComponent.AlwaysReplicate)
  {
    throw new InvalidOperationException("NPC spawn commit did not attach identity and lifecycle components.");
  }
}
else
{
  throw new InvalidOperationException("Valid NPC spawn command was rejected.");
}

NpcDefinitionRegistry trainingRegistry = new([trainingDummyDefinition]);
if (!spawnCommit.TryCommit(
      spawnWorld,
      trainingRegistry,
      new SpawnNpcCommand(
        DefinitionId: 488,
        Position: new SimulationVector(42.0f, 0.0f),
        Source: NpcSpawnSource.TileEntity,
        RequestedReplicationId: 488),
      replicationId: 488,
      out NpcSpawnCommitResult trainingSpawn,
      out _))
{
  throw new InvalidOperationException("Training Dummy NPC definition was rejected by spawn commit.");
}

NpcAuthorityComponent trainingAuthority = spawnWorld.Get<NpcAuthorityComponent>(trainingSpawn.Entity);
if (trainingAuthority.AiStyle != 92 ||
    !trainingAuthority.IsImmortal ||
    !trainingAuthority.AlwaysReplicate)
{
  throw new InvalidOperationException("Training Dummy authority flags were not committed.");
}

if (spawnCommit.TryCommit(
      spawnWorld,
      registry,
      validSpawn with { DefinitionId = 99 },
      replicationId: 9,
      out _,
      out _))
{
  throw new InvalidOperationException("NPC spawn commit accepted an unknown definition.");
}

if (spawnCommit.TryCommit(
      spawnWorld,
      registry,
      validSpawn with { Position = new SimulationVector(float.NaN, 0.0f) },
      replicationId: 11,
      out _,
      out _))
{
  throw new InvalidOperationException("NPC spawn commit accepted a non-finite position.");
}

WorldGrid occupiedWorld = new(200, 150);
_ = occupiedWorld.TrySetTile(40, 0, new WorldTile(true, 1));
if (spawnCommit.TryCommit(
      spawnWorld,
      occupiedWorld,
      registry,
      validSpawn,
      replicationId: 10,
      out _,
      out _))
{
  throw new InvalidOperationException("NPC spawn commit accepted an occupied spawn tile.");
}

NpcContactEffectSystem contactEffects = new();
IReadOnlyList<Terraria.Dome.Simulation.Player.Commands.DamagePlayerCommand> contactCommands =
  contactEffects.ProduceDamageCommands(
    [new NpcContactCandidate(
      new NpcHandle(1),
      new SimulationVector(0.0f, 0.0f),
      new SimulationComponents.ColliderComponent(1.0f, 2.0f),
      IsActive: true)],
    [
      new PlayerContactCandidate(
        new PlayerHandle(1),
        new SimulationVector(0.5f, 0.0f),
        new SimulationComponents.ColliderComponent(1.0f, 2.0f),
        IsActive: true,
        CooldownTicks: 0),
      new PlayerContactCandidate(
        new PlayerHandle(2),
        new SimulationVector(0.5f, 0.0f),
        new SimulationComponents.ColliderComponent(1.0f, 2.0f),
        IsActive: true,
        CooldownTicks: 1)
    ],
    damage: 25);
if (contactCommands.Count != 1 || contactCommands[0].Player.Value != 1)
{
  throw new InvalidOperationException("NPC contact effects did not honor overlap and cooldown state.");
}

if (contactEffects.ProduceDamageCommands(
      [new NpcContactCandidate(
        new NpcHandle(1),
        new SimulationVector(float.NaN, 0.0f),
        new SimulationComponents.ColliderComponent(1.0f, 2.0f),
        IsActive: true)],
      [new PlayerContactCandidate(
        new PlayerHandle(1),
        new SimulationVector(0.0f, 0.0f),
        new SimulationComponents.ColliderComponent(1.0f, 2.0f),
        IsActive: true,
        CooldownTicks: 0)],
      damage: 25).Count != 0 ||
    contactEffects.ProduceDamageCommands(
      [new NpcContactCandidate(
        new NpcHandle(1),
        new SimulationVector(0.0f, 0.0f),
        new SimulationComponents.ColliderComponent(float.NaN, 2.0f),
        IsActive: true)],
      [new PlayerContactCandidate(
        new PlayerHandle(1),
        new SimulationVector(0.0f, 0.0f),
        new SimulationComponents.ColliderComponent(1.0f, 2.0f),
        IsActive: true,
        CooldownTicks: 0)],
      damage: 25).Count != 0)
{
  throw new InvalidOperationException("NPC contact effects accepted non-finite contact geometry.");
}

DamageResolutionSystem damageResolution = new();
DamageCalculationSystem damageCalculation = new();
DamageVariationSystem damageVariation = new();
WorldTimeClassificationSystem worldTimeClassification = new();
WorldRuleState normalRules = new();
if (damageCalculation.CalculateRawAmount(
      rawAmount: 20,
      defense: 5,
      DamageTargetKind.Npc,
      normalRules) != 17.5 ||
    damageCalculation.CalculateAppliedAmount(
      rawAmount: 20,
      defense: 5,
      DamageTargetKind.Npc,
      normalRules) != 17 ||
    damageCalculation.CalculateRawAmount(
      rawAmount: 20,
      defense: 5,
      DamageTargetKind.Player,
      new WorldRuleState(isExpertMode: true)) != 16.25 ||
    damageCalculation.CalculateRawAmount(
      rawAmount: 20,
      defense: 5,
      DamageTargetKind.Player,
      new WorldRuleState(isExpertMode: true, isMasterMode: true)) != 15.0 ||
    damageCalculation.CalculateRawAmount(
      rawAmount: 20,
      defense: 5,
      DamageTargetKind.PlayerPvp,
      new WorldRuleState(isExpertMode: true, isMasterMode: true)) != 17.5 ||
    damageCalculation.CalculateAppliedAmount(
      rawAmount: 1,
      defense: 5,
      DamageTargetKind.Npc,
      normalRules) != 1)
{
  throw new InvalidOperationException(
    "Damage calculation did not preserve legacy mitigation, difficulty, PvP, or minimum damage.");
}

ScriptedDamageVariationRandom noLuckRandom = new([-15], []);
if (damageVariation.Calculate(100.0f, 0.0f, false, noLuckRandom) != 85 ||
    noLuckRandom.StepReadCount != 1 || noLuckRandom.ChanceReadCount != 0)
{
  throw new InvalidOperationException("Damage variation did not consume exactly one base roll.");
}

ScriptedDamageVariationRandom positiveLuckRandom = new([-15, 15], [0.2f]);
if (damageVariation.Calculate(100.0f, 0.25f, false, positiveLuckRandom) != 115 ||
    positiveLuckRandom.StepReadCount != 2 || positiveLuckRandom.ChanceReadCount != 1)
{
  throw new InvalidOperationException("Positive luck did not select the larger eligible variation.");
}

ScriptedDamageVariationRandom negativeLuckRandom = new([15, -15], [0.2f]);
if (damageVariation.Calculate(100.0f, -0.25f, false, negativeLuckRandom) != 85 ||
    negativeLuckRandom.StepReadCount != 2 || negativeLuckRandom.ChanceReadCount != 1)
{
  throw new InvalidOperationException("Negative luck did not select the smaller eligible variation.");
}

ScriptedDamageVariationRandom disabledVariationRandom = new([], []);
if (damageVariation.Calculate(10.9f, 0.5f, true, disabledVariationRandom) != 10 ||
    disabledVariationRandom.StepReadCount != 0 || disabledVariationRandom.ChanceReadCount != 0)
{
  throw new InvalidOperationException("Disabled variation consumed a random value or changed truncation.");
}

if (worldTimeClassification.GetMoonPhase(0) != WorldMoonPhase.Full ||
    worldTimeClassification.GetMoonPhase(4) != WorldMoonPhase.Empty ||
    worldTimeClassification.GetMoonPhase(7) != WorldMoonPhase.ThreeQuartersAtRight ||
    !worldTimeClassification.IsGameplayDayTime(isDayTime: true, isRemixWorld: false) ||
    worldTimeClassification.IsGameplayDayTime(isDayTime: false, isRemixWorld: false) ||
    worldTimeClassification.IsGameplayDayTime(isDayTime: true, isRemixWorld: true))
{
  throw new InvalidOperationException("World time classification did not preserve moon phase or remix day rules.");
}

try
{
  _ = worldTimeClassification.GetMoonPhase(8);
  throw new InvalidOperationException("World time classification accepted an invalid moon phase.");
}
catch (ArgumentOutOfRangeException)
{
}

SimulationComponents.HealthComponent damageHealth = new(100, 100);
ImmunityComponent damageImmunity = new();
if (!damageResolution.TryResolve(
      ref damageHealth,
      new DefenseComponent(5),
      ref damageImmunity,
      rawAmount: 20,
      DamageTargetKind.Npc,
      normalRules,
      out int appliedDamage) || appliedDamage != 17 || damageHealth.Current != 83)
{
  throw new InvalidOperationException("NPC damage resolution did not apply legacy defense mitigation.");
}

damageImmunity.RemainingTicks = 2;
if (damageResolution.TryResolve(
      ref damageHealth,
      new DefenseComponent(0),
      ref damageImmunity,
      rawAmount: 20,
      DamageTargetKind.Npc,
      normalRules,
      out _))
{
  throw new InvalidOperationException("NPC damage resolution ignored hit immunity.");
}

if (damageResolution.TryResolve(
      ref damageHealth,
      new DefenseComponent(0),
      ref damageImmunity,
      rawAmount: 0,
      DamageTargetKind.Npc,
      normalRules,
      out _))
{
  throw new InvalidOperationException("NPC damage resolution accepted zero damage.");
}

NpcLifecycleSystem lifecycleSystem = new();
NpcLifecycleComponent deathLifecycle = new(isActive: true, timeLeft: 10);
NpcLifecycleResult lifecycleResult = lifecycleSystem.Advance(ref deathLifecycle, currentHealth: 0);
if (!lifecycleResult.BecameInactive || !lifecycleResult.IsDead || deathLifecycle.IsActive)
{
  throw new InvalidOperationException("NPC lifecycle did not publish a single death transition.");
}

NpcLifecycleComponent expiredLifecycle = new(isActive: true, timeLeft: -1);
NpcLifecycleResult expiredResult = lifecycleSystem.Advance(
  ref expiredLifecycle,
  currentHealth: 100);
if (!expiredResult.BecameInactive || expiredResult.IsDead || expiredLifecycle.IsActive ||
    expiredLifecycle.DespawnReason != NpcDespawnReason.TimedOut)
{
  throw new InvalidOperationException("NPC lifecycle retained an active entity with a negative timer.");
}

NpcLifecycleComponent immortalLifecycle = new(isActive: true, timeLeft: 0);
NpcLifecycleResult immortalResult = lifecycleSystem.Advance(
  ref immortalLifecycle,
  currentHealth: 0,
  isImmortal: true);
if (immortalResult.BecameInactive || immortalResult.IsDead || !immortalLifecycle.IsActive)
{
  throw new InvalidOperationException("Immortal NPC lifecycle entered a death or timeout transition.");
}

NpcDeathSystem deathSystem = new();
NpcDeathResult deathResult = deathSystem.Evaluate(new NpcDeathInput(
  new NpcHandle(1),
  Health: 0,
  WasActive: true,
  new SimulationVector(4.0f, 0.0f),
  LootTableId: 1));
if (!deathResult.Published)
{
  throw new InvalidOperationException("NPC death system did not publish the death event.");
}

if (deathSystem.Evaluate(new NpcDeathInput(
      default,
      Health: 0,
      WasActive: true,
      new SimulationVector(4.0f, 0.0f),
      LootTableId: 1)).Published ||
    deathSystem.Evaluate(new NpcDeathInput(
      new NpcHandle(1),
      Health: 0,
      WasActive: true,
      new SimulationVector(float.NaN, 0.0f),
      LootTableId: 1)).Published ||
    deathSystem.Evaluate(new NpcDeathInput(
      new NpcHandle(1),
      Health: 0,
      WasActive: true,
      new SimulationVector(4.0f, 0.0f),
      LootTableId: 0)).Published)
{
  throw new InvalidOperationException("NPC death system accepted forged identity, position or loot facts.");
}

NpcLootSystem lootSystem = new(
  new NpcLootDefinitionRegistry([new NpcLootDefinition(1, 1, 1, 2)]),
  new WorldSeed(1));
if (lootSystem.Roll(1, 5) != lootSystem.Roll(1, 5))
{
  throw new InvalidOperationException("NPC loot was not deterministic for the same seed and identity.");
}

WorldGrid collisionWorld = new(200, 150);
_ = collisionWorld.TrySetTile(2, 0, new WorldTile(true, 1));
SimulationComponents.TransformComponent collisionTransform = new(0.0f, 0.0f);
SimulationComponents.VelocityComponent collisionVelocity = new(3.0f, 0.0f);
SimulationComponents.PhysicsStateComponent collisionPhysics = new();
new TileCollisionSystem().MoveAndResolve(
  collisionWorld,
  ref collisionTransform,
  ref collisionVelocity,
  ref collisionPhysics,
  new SimulationComponents.ColliderComponent(1.0f, 1.0f));
if (collisionTransform.X >= 2.0f || collisionVelocity.X != 0.0f)
{
  throw new InvalidOperationException("NPC tile collision did not stop at a solid tile.");
}

using DomeSimulation queuedSimulation = new();
string[] expectedNpcPipeline =
[
  "NpcSpawnEligibilitySystem",
  "NpcSpawnCommitSystem",
  "NpcTargetSelectionSystem",
  "NpcBehaviorSystem",
  "NpcMovementIntentSystem",
  "MovementSystem/TileCollisionSystem",
  "NpcContactEffectSystem",
  "DamageResolutionSystem",
  "NpcLifecycleSystem",
  "NpcDeathSystem",
  "NpcLootSystem",
  "NpcReplicationSystem"
];
if (!queuedSimulation.NpcPipelineSystemNames.SequenceEqual(expectedNpcPipeline))
{
  throw new InvalidOperationException("NPC system registration order is not explicit or stable.");
}

queuedSimulation.QueueNpcSpawn(validSpawn with { RequestedReplicationId = 4 });
queuedSimulation.Tick(new SimulationInputBatch());
if (!queuedSimulation.LastNpcPipelineSystemNames.SequenceEqual(expectedNpcPipeline))
{
  throw new InvalidOperationException("NPC system pipeline was not registered for the simulation tick.");
}
NpcReplicationSnapshot queuedReplication = queuedSimulation.CreateNpcReplicationSnapshots()
  .Single(snapshot => snapshot.ReplicationId == 4);
if (!queuedReplication.IsActive || queuedReplication.Revision != 1)
{
  throw new InvalidOperationException("Queued NPC spawn did not commit a stable active revision.");
}

queuedSimulation.QueueNpcDespawn(new DespawnNpcCommand(
  new NpcHandle(4),
  NpcDespawnReason.OutOfRange));
queuedSimulation.Tick(new SimulationInputBatch());
NpcReplicationSnapshot despawnedReplication = queuedSimulation.CreateNpcReplicationSnapshots()
  .Single(snapshot => snapshot.ReplicationId == 4);
if (despawnedReplication.IsActive || despawnedReplication.Revision <= queuedReplication.Revision)
{
  throw new InvalidOperationException("Queued NPC despawn did not publish exactly one inactive revision.");
}
long inactiveRevision = despawnedReplication.Revision;
queuedSimulation.Tick(new SimulationInputBatch());
NpcReplicationSnapshot stableInactiveReplication = queuedSimulation.CreateNpcReplicationSnapshots()
  .Single(snapshot => snapshot.ReplicationId == 4);
if (stableInactiveReplication.IsActive || stableInactiveReplication.Revision != inactiveRevision)
{
  throw new InvalidOperationException("Inactive NPC replication revision changed more than once.");
}

using DomeSimulation replayA = new();
using DomeSimulation replayB = new();
SpawnNpcCommand replayCommand = validSpawn with { RequestedReplicationId = 5 };
replayA.QueueNpcSpawn(replayCommand);
replayB.QueueNpcSpawn(replayCommand);
replayA.Tick(new SimulationInputBatch());
replayB.Tick(new SimulationInputBatch());
if (!replayA.CreateNpcReplicationSnapshots().SequenceEqual(replayB.CreateNpcReplicationSnapshots()))
{
  throw new InvalidOperationException("NPC spawn replay diverged for identical command inputs.");
}

using DomeSimulation contactSimulation = new();
PlayerHandle contactPlayer = contactSimulation.CreatePlayer(new SimulationVector(0.0f, 0.0f));
_ = contactSimulation.CreateNpc(new SimulationVector(0.0f, 0.0f));
contactSimulation.Tick(new SimulationInputBatch());
int firstContactHealth = contactSimulation.CreateSnapshot().FindPlayer(contactPlayer).Health;
contactSimulation.Tick(new SimulationInputBatch());
int cooldownContactHealth = contactSimulation.CreateSnapshot().FindPlayer(contactPlayer).Health;
contactSimulation.Tick(new SimulationInputBatch());
int secondContactHealth = contactSimulation.CreateSnapshot().FindPlayer(contactPlayer).Health;
if (firstContactHealth != 75 || cooldownContactHealth != 75 || secondContactHealth != 50)
{
  throw new InvalidOperationException(
    $"NPC contact damage did not honor the configured immunity cooldown ticks: " +
    $"First={firstContactHealth}, Cooldown={cooldownContactHealth}, Second={secondContactHealth}.");
}

using DomeSimulation simulation = new();
PlayerHandle player = simulation.CreatePlayer(new SimulationVector(10.0f, 0.0f));
NpcHandle npc = simulation.CreateNpc(new SimulationVector(30.0f, 0.0f));
SimulationSnapshot before = simulation.CreateSnapshot();
simulation.Tick(new SimulationInputBatch());
SimulationSnapshot after = simulation.CreateSnapshot();
NpcSnapshot beforeNpc = before.FindNpc(npc);
NpcSnapshot afterNpc = after.FindNpc(npc);
if (!afterNpc.HasTarget || afterNpc.Position.X >= beforeNpc.Position.X)
{
  throw new InvalidOperationException("NPC must select the live player and move toward it.");
}

simulation.QueueNpcDamage(npc, 100);
simulation.Tick(new SimulationInputBatch());
NpcReplicationSnapshot replication = simulation.CreateNpcReplicationSnapshots()
  .Single(snapshot => snapshot.ReplicationId == npc.Value);
if (replication.IsActive || replication.Revision <= 1)
{
  throw new InvalidOperationException("NPC death must publish an inactive replication revision.");
}

if (simulation.CreateWorldItemSnapshots().Count == 0)
{
  throw new InvalidOperationException("NPC death must create a deterministic world-item drop.");
}

Console.WriteLine("PASS: NPC source contract, target/chase baseline, death revision and deterministic loot");

using DomeSimulation deathReplaySource = new();
NpcHandle replayNpc = deathReplaySource.CreateNpc(new SimulationVector(30.0f, 0.0f));
deathReplaySource.Tick(new SimulationInputBatch());
deathReplaySource.QueueNpcDamage(replayNpc, 100);
deathReplaySource.Tick(new SimulationInputBatch());
int droppedBeforeRestore = deathReplaySource.CreateWorldItemSnapshots().Count;
if (droppedBeforeRestore != 1)
{
  throw new InvalidOperationException("NPC death replay fixture did not produce one committed drop.");
}

WorldMetadata deathReplayMetadata = new("NpcDeathReplay", new WorldSeed(123), 4200, 1200);
using DomeSimulation deathReplayRestored = new(
  deathReplaySource.CreatePersistenceSnapshot(deathReplayMetadata));
deathReplayRestored.Tick(new SimulationInputBatch());
if (deathReplayRestored.CreateWorldItemSnapshots().Count != droppedBeforeRestore)
{
  throw new InvalidOperationException(
    "Restoring an already committed NPC death published duplicate loot.");
}

Console.WriteLine("PASS: committed NPC death does not publish duplicate loot after snapshot restore");

file sealed class ScriptedDamageVariationRandom : IDamageVariationRandom
{
  private readonly Queue<float> _chanceValues;
  private readonly Queue<int> _stepValues;

  public ScriptedDamageVariationRandom(IEnumerable<int> stepValues, IEnumerable<float> chanceValues)
  {
    _stepValues = new Queue<int>(stepValues);
    _chanceValues = new Queue<float>(chanceValues);
  }

  public int ChanceReadCount { get; private set; }

  public int StepReadCount { get; private set; }

  public float NextChance()
  {
    ChanceReadCount++;
    return _chanceValues.Dequeue();
  }

  public int NextDamageStep()
  {
    StepReadCount++;
    return _stepValues.Dequeue();
  }
}
