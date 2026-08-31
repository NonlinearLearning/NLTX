using System;
using System.Collections.Generic;
using System.Linq;
using Arch.Core;
using Terraria.Dome.Simulation;
using Terraria.Dome.Simulation.Combat;
using Terraria.Dome.Simulation.Combat.Commands;
using Terraria.Dome.Simulation.Combat.Components;
using Terraria.Dome.Simulation.Combat.Systems;
using Terraria.Dome.Simulation.Items;
using Terraria.Dome.Simulation.Movement.Components;
using Terraria.Dome.Simulation.Npc;
using Terraria.Dome.Simulation.Npc.Commands;
using Terraria.Dome.Simulation.Npc.Components;
using Terraria.Dome.Simulation.Npc.Definitions;
using Terraria.Dome.Simulation.Npc.Events;
using Terraria.Dome.Simulation.Npc.Snapshots;
using Terraria.Dome.Simulation.Npc.Systems;
using Terraria.Dome.Simulation.Inventory.Components;
using Terraria.Dome.Simulation.Items.Components;
using Terraria.Dome.Simulation.Items.Definitions;
using Terraria.Dome.Simulation.Liquid.Components;
using Terraria.Dome.Simulation.Player.Components;
using Terraria.Dome.Simulation.Player.Definitions;
using Terraria.Dome.Simulation.Player.Systems;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Simulation.WorldModel.Systems;
using Terraria.Dome.Simulation.Physics.Systems;
using Terraria.Dome.Simulation.Projectile.Definitions;
using Terraria.Dome.Simulation.Projectile.Systems;
using Terraria.Dome.Protocol.V1456.Packets;
using Terraria.Dome.Protocol.V1456.Compatibility;
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

NpcDefinition multiplierDefinition = new(
  DefinitionId: 3,
  NetId: 3,
  MaximumHealth: 100,
  Defense: 4,
  ColliderWidth: 1.0f,
  ColliderHeight: 2.0f,
  BehaviorId: NpcBehaviorId.OrdinaryChase,
  LootTableId: 1,
  TakenDamageMultiplier: 3.0f);
NpcAuthorityComponent multiplierAuthority = new(
  multiplierDefinition.AiStyle,
  multiplierDefinition.IsImmortal,
  multiplierDefinition.AlwaysReplicate,
  multiplierDefinition.TakenDamageMultiplier);
SimulationComponents.HealthComponent multiplierHealth = new(100, 100);
ImmunityComponent multiplierImmunity = default;
DamageResolutionSystem multiplierResolution = new();
if (!multiplierResolution.TryResolve(
      ref multiplierHealth,
      new DefenseComponent(multiplierDefinition.Defense),
      ref multiplierImmunity,
      rawAmount: 10,
      DamageTargetKind.Npc,
      new WorldRuleState(),
      out int multipliedAmount,
      multiplierAuthority.TakenDamageMultiplier) ||
    multipliedAmount != 24 || multiplierHealth.Current != 76)
{
  throw new InvalidOperationException(
    "NPC taken-damage multiplier did not apply after source-compatible defense resolution.");
}

SimulationComponents.HealthComponent fractionalMultiplierHealth = new(100, 100);
ImmunityComponent fractionalMultiplierImmunity = default;
if (!multiplierResolution.TryResolve(
      ref fractionalMultiplierHealth,
      new DefenseComponent(3),
      ref fractionalMultiplierImmunity,
      rawAmount: 10,
      DamageTargetKind.Npc,
      new WorldRuleState(),
      out int fractionalAmount,
      multiplierAuthority.TakenDamageMultiplier) ||
    fractionalAmount != 25 || fractionalMultiplierHealth.Current != 75)
{
  throw new InvalidOperationException(
    "NPC taken-damage multiplier was truncated before post-defense scaling.");
}

SimulationComponents.HealthComponent defaultMultiplierHealth = new(100, 100);
ImmunityComponent defaultMultiplierImmunity = default;
if (!multiplierResolution.TryResolve(
      ref defaultMultiplierHealth,
      new DefenseComponent(multiplierDefinition.Defense),
      ref defaultMultiplierImmunity,
      rawAmount: 10,
      DamageTargetKind.Npc,
      new WorldRuleState(),
      out int defaultAmount) ||
    defaultAmount != 8 || defaultMultiplierHealth.Current != 92)
{
  throw new InvalidOperationException(
    "NPC default taken-damage multiplier did not preserve unscaled damage.");
}

try
{
  _ = new NpcDefinition(
    DefinitionId: 4,
    NetId: 4,
    MaximumHealth: 100,
    Defense: 0,
    ColliderWidth: 1.0f,
    ColliderHeight: 2.0f,
    BehaviorId: NpcBehaviorId.OrdinaryChase,
    LootTableId: 1,
    TakenDamageMultiplier: 0.5f);
  throw new InvalidOperationException("NPC definition accepted a sub-unit damage multiplier.");
}
catch (ArgumentOutOfRangeException)
{
}

try
{
  _ = new NpcAuthorityComponent(0, false, false, 1.0f, -1.0f);
  throw new InvalidOperationException("NPC authority accepted a negative slot cost.");
}
catch (ArgumentOutOfRangeException)
{
}

NpcDefinition weightedDefinition = new(
  DefinitionId: 5,
  NetId: 5,
  MaximumHealth: 100,
  Defense: 0,
  ColliderWidth: 1.0f,
  ColliderHeight: 2.0f,
  BehaviorId: NpcBehaviorId.OrdinaryChase,
  LootTableId: 1,
  NpcSlotCost: 2.5f);
NpcAuthorityComponent weightedAuthority = new(
  weightedDefinition.AiStyle,
  weightedDefinition.IsImmortal,
  weightedDefinition.AlwaysReplicate,
  weightedDefinition.TakenDamageMultiplier,
  weightedDefinition.NpcSlotCost);
if (weightedAuthority.NpcSlotCost != 2.5f)
{
  throw new InvalidOperationException("NPC slot cost was not preserved by the authority owner.");
}

NpcSlotAccountingSystem slotAccounting = new();
if (slotAccounting.CalculateActiveSlots([
      new NpcSlotAccount(IsActive: true, NpcSlotCost: 2.5f),
      new NpcSlotAccount(IsActive: false, NpcSlotCost: 100.0f),
      new NpcSlotAccount(IsActive: true, NpcSlotCost: 0.5f)]) != 3.0f)
{
  throw new InvalidOperationException("NPC slot accounting did not sum active weighted costs.");
}

try
{
  _ = new NpcDefinition(
    DefinitionId: 6,
    NetId: 6,
    MaximumHealth: 100,
    Defense: 0,
    ColliderWidth: 1.0f,
    ColliderHeight: 2.0f,
    BehaviorId: NpcBehaviorId.OrdinaryChase,
    LootTableId: 1,
    NpcSlotCost: float.NaN);
  throw new InvalidOperationException("NPC definition accepted a non-finite slot cost.");
}
catch (ArgumentOutOfRangeException)
{
}

try
{
  _ = new NpcAuthorityComponent(0, false, false, float.NaN);
  throw new InvalidOperationException("NPC authority accepted a non-finite damage multiplier.");
}
catch (ArgumentOutOfRangeException)
{
}

NpcTargetCapabilityRegistry targetCapabilities = NpcTargetCapabilityRegistry.CreateVersion1456();
if (targetCapabilities.Count != 33 ||
    !targetCapabilities.Supports(547) ||
    !targetCapabilities.Supports(668) ||
    targetCapabilities.Supports(1) ||
    targetCapabilities.Supports(453))
{
  throw new InvalidOperationException(
    "NPC UsesNewTargeting capability registry did not preserve the source-backed type set.");
}

try
{
  _ = new NpcTargetCapabilityRegistry([1, 1]);
  throw new InvalidOperationException("NPC target capability registry accepted a duplicate ID.");
}
catch (ArgumentException)
{
}

try
{
  _ = new NpcTargetCapabilityRegistry([0]);
  throw new InvalidOperationException("NPC target capability registry accepted a non-positive ID.");
}
catch (ArgumentException)
{
}

NpcDefinition tableBackedDefinition = new(
  DefinitionId: 547,
  NetId: 547,
  MaximumHealth: 100,
  Defense: 0,
  ColliderWidth: 1.0f,
  ColliderHeight: 2.0f,
  BehaviorId: NpcBehaviorId.OrdinaryChase,
  LootTableId: 1);
NpcDefinitionRegistry tableBackedRegistry = new([tableBackedDefinition], targetCapabilities);
if (!tableBackedRegistry.GetRequired(547).SupportsNpcTargets)
{
  throw new InvalidOperationException(
    "NPC definition registry did not bind the source-backed target capability.");
}

if (!targetCapabilities.Supports(tableBackedDefinition) ||
    !targetCapabilities.Supports(ordinaryDefinition.WithSupportsNpcTargets(true)))
{
  throw new InvalidOperationException(
    "NPC target capability registry did not unify definition and table capability checks.");
}

NpcCombatClassificationSystem combatClassification = new();
if (!combatClassification.CountsAsCritter(5, 0, 1) ||
    combatClassification.CountsAsCritter(6, 0, 1) ||
    combatClassification.CountsAsCritter(5, 1, 1) ||
    combatClassification.CountsAsCritter(5, 0, 594) ||
    combatClassification.CountsAsCritter(5, 0, 686) ||
    combatClassification.CountsAsCritter(-1, 0, 1))
{
  throw new InvalidOperationException(
    "NPC critter classification did not preserve the source-backed predicate.");
}

if (!combatClassification.CanBeChasedBy(
      isActive: true,
      isChaseable: true,
      maximumHealth: 6,
      doesNotTakeDamage: false,
      isFriendly: false,
      isImmortal: false) ||
    combatClassification.CanBeChasedBy(true, true, 5, false, false, false) ||
    combatClassification.CanBeChasedBy(false, true, 6, false, false, false) ||
    combatClassification.CanBeChasedBy(true, true, 6, false, true, false) ||
    combatClassification.CanBeChasedBy(true, true, 6, false, false, true) ||
    combatClassification.CanBeChasedBy(true, false, 6, false, false, false) ||
    !combatClassification.CanBeChasedBy(
      true,
      true,
      6,
      true,
      false,
      false,
      ignoreDoesNotTakeDamage: true) ||
    !combatClassification.CanBeChasedBy(
      true,
      true,
      6,
      false,
      false,
      true,
      allowImmortalTargetDummy: true))
{
  throw new InvalidOperationException(
    "NPC chaseability did not preserve the source-backed predicate.");
}

NpcBehaviorStateComponent chaseabilityState = new(
  NpcBehaviorId.OrdinaryChase,
  new NpcChaseState(1.0f, 0.0f),
  new NpcTownHomeState(default, true, 0),
  isChaseable: false,
  doesNotTakeDamage: true);
if (chaseabilityState.IsChaseable || !chaseabilityState.DoesNotTakeDamage ||
    combatClassification.CanBeChasedBy(
      true,
      chaseabilityState.IsChaseable,
      6,
      chaseabilityState.DoesNotTakeDamage,
      false,
      false))
{
  throw new InvalidOperationException(
    "NPC typed behavior state did not carry chaseability into the combat predicate.");
}

NpcTownVariantSystem townVariant = new();
if (!townVariant.IsShimmerVariant(1, true) ||
    townVariant.IsShimmerVariant(0, true) ||
    townVariant.IsShimmerVariant(1, false) ||
    townVariant.IsShimmerVariant(-1, true))
{
  throw new InvalidOperationException(
    "NPC shimmer variant query did not preserve the source-backed variation boundary.");
}

NpcTargetRoutingSystem rangeRouting = new();
if (!rangeRouting.IsWithinTargetRange(
      new SimulationVector(0.0f, 0.0f),
      new SimulationVector(3.0f, 4.0f),
      6.0f) ||
    rangeRouting.IsWithinTargetRange(
      new SimulationVector(0.0f, 0.0f),
      new SimulationVector(3.0f, 4.0f),
      5.0f) ||
    rangeRouting.IsWithinTargetRange(
      new SimulationVector(float.NaN, 0.0f),
      new SimulationVector(0.0f, 0.0f),
      6.0f) ||
    rangeRouting.IsWithinTargetRange(
      new SimulationVector(0.0f, 0.0f),
      new SimulationVector(0.0f, 0.0f),
      -1.0f))
{
  throw new InvalidOperationException(
    "NPC target range query did not preserve the strict finite distance boundary.");
}

if (registry.OrderedDefinitions.Count != 1 ||
    registry.OrderedDefinitions[0] != ordinaryDefinition)
{
  throw new InvalidOperationException("NPC definition registry did not preserve registration order.");
}

try
{
  ((IDictionary<int, NpcDefinition>)registry.Definitions)[1] = ordinaryDefinition;
  throw new InvalidOperationException("NPC definition projection was mutable.");
}
catch (NotSupportedException)
{
}

VerifyNpcSnapshotIdentityOverflow();
VerifyLegacyAiStyle2Type501Explosion();
VerifyLegacyAiStyle2Type240Explosion();
VerifyLegacyNpcInactivityRegistry();
VerifyNpcCheckActivePixelRangePolicy();
VerifyNpcCheckActiveSlotContributionPolicy();
VerifyNpcInvasionBossCapPolicy();
VerifyNpcCheckActiveKeepAlivePolicy();
VerifyNpcCheckActiveTimerRefreshPolicy();
VerifyNpcCheckActiveDeactivationPolicy();
VerifyNpcCheckDeadQualificationPolicy();
VerifyNpcCheckDeadSpecialTransitionPolicy();
VerifyNpcCheckDeadInvasionProgressPolicy();
VerifyNpcCheckDeadInvasionProgressCommandPolicy();
VerifyLegacyNpcInvasionGroupRegistry();
VerifyNpcTombstoneProjectileTypePolicy();
VerifyNpcCheckDeadSpawnCyclePolicy();
VerifyNpcCheckDeadGoodWorldProjectilePolicy();
VerifyNpcCheckActiveWormSegmentPolicy();
VerifyLegacyNpcTownRegistry();

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

try
{
  _ = new NpcDefinitionComponent(
    DefinitionId: 1,
    NetId: 697,
    Faction: NpcFaction.Hostile,
    Category: NpcCategory.Enemy);
  throw new InvalidOperationException("NPC definition component accepted an out-of-range net ID.");
}
catch (ArgumentOutOfRangeException)
{
}

try
{
  _ = new NpcDefinitionComponent(
    DefinitionId: 1,
    NetId: 1,
    Faction: (NpcFaction)99,
    Category: NpcCategory.Enemy);
  throw new InvalidOperationException("NPC definition component accepted an undefined faction.");
}
catch (ArgumentOutOfRangeException)
{
}

try
{
  _ = new NpcDefinitionComponent(
    DefinitionId: 1,
    NetId: 1,
    Faction: NpcFaction.Hostile,
    Category: (NpcCategory)99);
  throw new InvalidOperationException("NPC definition component accepted an undefined category.");
}
catch (ArgumentOutOfRangeException)
{
}

NpcDefinition maxNetIdDefinition = new(
  DefinitionId: 10,
  NetId: 696,
  MaximumHealth: 100,
  Defense: 0,
  ColliderWidth: 1.0f,
  ColliderHeight: 2.0f,
  BehaviorId: NpcBehaviorId.OrdinaryChase,
  LootTableId: 1);
if (maxNetIdDefinition.NetId != 696)
{
  throw new InvalidOperationException("NPC definition did not accept the NPCID.Count upper boundary.");
}

try
{
  _ = new NpcDefinition(
    DefinitionId: 9,
    NetId: 697,
    MaximumHealth: 100,
    Defense: 0,
    ColliderWidth: 1.0f,
    ColliderHeight: 2.0f,
    BehaviorId: NpcBehaviorId.OrdinaryChase,
    LootTableId: 1);
  throw new InvalidOperationException("NPC definition accepted a net ID outside NPCID.Count.");
}
catch (ArgumentOutOfRangeException)
{
}

try
{
  _ = new NpcDefinition(
    DefinitionId: 8,
    NetId: 8,
    MaximumHealth: 100,
    Defense: 0,
    ColliderWidth: 1.0f,
    ColliderHeight: 2.0f,
    BehaviorId: NpcBehaviorId.OrdinaryChase,
    LootTableId: 1,
    AiStyle: 128);
  throw new InvalidOperationException("NPC definition accepted an out-of-range AI style.");
}
catch (ArgumentOutOfRangeException)
{
}

try
{
  _ = new NpcDefinition(
    DefinitionId: 7,
    NetId: 7,
    MaximumHealth: 100,
    Defense: 0,
    ColliderWidth: 1.0f,
    ColliderHeight: 2.0f,
    BehaviorId: NpcBehaviorId.OrdinaryChase,
    LootTableId: 1,
    AiStyle: -1);
  throw new InvalidOperationException("NPC definition accepted a negative AI style.");
}
catch (ArgumentOutOfRangeException)
{
}

try
{
  _ = new NpcDefinition(
    DefinitionId: 6,
    NetId: 6,
    MaximumHealth: 100,
    Defense: -1,
    ColliderWidth: 1.0f,
    ColliderHeight: 2.0f,
    BehaviorId: NpcBehaviorId.OrdinaryChase,
    LootTableId: 1);
  throw new InvalidOperationException("NPC definition accepted negative defense.");
}
catch (ArgumentOutOfRangeException)
{
}

try
{
  _ = new NpcDefinition(
    DefinitionId: 2,
    NetId: 2,
    MaximumHealth: 100,
    Defense: 0,
    ColliderWidth: 1.0f,
    ColliderHeight: 2.0f,
    BehaviorId: (NpcBehaviorId)99,
    LootTableId: 1);
  throw new InvalidOperationException("NPC definition accepted an unknown behavior ID.");
}
catch (ArgumentOutOfRangeException)
{
}

try
{
  _ = new NpcDefinition(
    DefinitionId: 3,
    NetId: 3,
    MaximumHealth: 100,
    Defense: 0,
    ColliderWidth: 1.0f,
    ColliderHeight: 2.0f,
    BehaviorId: NpcBehaviorId.OrdinaryChase,
    LootTableId: -1);
  throw new InvalidOperationException("NPC definition accepted a negative loot table ID.");
}
catch (ArgumentOutOfRangeException)
{
}

try
{
  _ = new NpcDefinition(
    DefinitionId: 4,
    NetId: 4,
    MaximumHealth: 100,
    Defense: 0,
    ColliderWidth: 1.0f,
    ColliderHeight: 2.0f,
    BehaviorId: NpcBehaviorId.TownHome,
    LootTableId: 1,
    Faction: NpcFaction.Hostile,
    Category: NpcCategory.Enemy);
  throw new InvalidOperationException("NPC definition accepted contradictory town-home authority.");
}
catch (ArgumentException)
{
}

NpcTargetSelectionSystem targetSelection = new();
if (!targetSelection.TryCalculateTargetPriority(
      new SimulationVector(0.0f, 0.0f),
      new SimulationVector(3.0f, 4.0f),
      2,
      false,
      false,
      out float aggroPriority) ||
    aggroPriority != 5.0f ||
    !targetSelection.TryCalculateTargetPriority(
      new SimulationVector(0.0f, 0.0f),
      new SimulationVector(3.0f, 4.0f),
      0,
      true,
      true,
      out float noAggroPriority) ||
    noAggroPriority != 1007.0f ||
    targetSelection.TryCalculateTargetPriority(
      new SimulationVector(float.NaN, 0.0f),
      new SimulationVector(0.0f, 0.0f),
      0,
      false,
      false,
      out _))
{
  throw new InvalidOperationException("NPC target priority query did not preserve source-backed scoring.");
}

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

NpcTargetComponent aggroSelectedTarget = targetSelection.SelectTarget(
  new SimulationVector(0.0f, 0.0f),
  [
    new NpcTargetCandidate(firstPlayer, 1, new SimulationVector(2.0f, 0.0f), true, 100,
      TargetPriority: 2.0f),
    new NpcTargetCandidate(secondPlayer, 2, new SimulationVector(1.0f, 0.0f), true, 100,
      TargetPriority: -10.0f)
  ]);
if (!aggroSelectedTarget.HasTarget || aggroSelectedTarget.StableTargetId != 2)
{
  throw new InvalidOperationException("NPC target selection did not consume target priority.");
}

PlayerTargetingStateComponent targetingState = new();
targetingState.SetNoAggroNpcTypes([547, 668]);
if (!targetingState.IsNoAggroNpc(547) || targetingState.IsNoAggroNpc(1))
{
  throw new InvalidOperationException("Player NPC no-aggro targeting state did not preserve its type set.");
}
NpcNoAggroCapabilityRegistry noAggroRegistry =
  NpcNoAggroCapabilityRegistry.CreateVersion1456Item3090();
if (noAggroRegistry.Item3090Count != 23 ||
    !noAggroRegistry.IsItem3090NoAggroNpc(1) ||
    !noAggroRegistry.IsItem3090NoAggroNpc(676) ||
    noAggroRegistry.IsItem3090NoAggroNpc(547))
{
  throw new InvalidOperationException("NPC item-3090 no-aggro capability registry did not preserve the source set.");
}
PlayerNpcTargetingSystem npcTargetingSystem = new();
PlayerStealthStateComponent stealthState = new();
PlayerMountStateComponent mountState = new();
if (!MountTypeRegistry.IsValid(-1) || !MountTypeRegistry.IsDefined(0) ||
    !MountTypeRegistry.IsDefined(63) || MountTypeRegistry.IsValid(64))
{
  throw new InvalidOperationException("Mount type registry did not preserve the source-backed Count=64 boundary.");
}
if (!MountCapabilityRegistry.IsCart(6) || MountCapabilityRegistry.IsCart(7) ||
    !MountCapabilityRegistry.CanFly(56) || MountCapabilityRegistry.CanFly(48) ||
    !MountCapabilityRegistry.CanFly(54, true) || MountCapabilityRegistry.CanFly(54, false) ||
    !MountCapabilityRegistry.CanUseFlightFrame(56, 2) ||
    MountCapabilityRegistry.CanUseFlightFrame(56, 4) ||
    !MountCapabilityRegistry.CanUseFlightFrame(61, 4) ||
    MountCapabilityRegistry.CanUseFlightFrame(48, 4) ||
    !MountCapabilityRegistry.CanDash(63) || MountCapabilityRegistry.CanDash(55) ||
    !MountCapabilityRegistry.DoesNotHoldItems(55) || MountCapabilityRegistry.DoesNotHoldItems(54) ||
    !MountCapabilityRegistry.DismountsOnItemUse(56) ||
    MountCapabilityRegistry.DismountsOnItemUse(54) ||
    !MountCapabilityRegistry.IsTransformationMount(52) ||
    MountCapabilityRegistry.IsTransformationMount(53) ||
    !MountCapabilityRegistry.HidesPlayer(61) ||
    MountCapabilityRegistry.HidesPlayer(51) ||
    !MountCapabilityRegistry.CanUseHooks(54) ||
    !MountCapabilityRegistry.CanUseHooks(60) ||
    MountCapabilityRegistry.CanUseHooks(56) ||
    !MountCapabilityRegistry.DoesNotDismountWhenCrowdControlled(55) ||
    MountCapabilityRegistry.DoesNotDismountWhenCrowdControlled(54) ||
    !MountCapabilityRegistry.DoesNotOverrideBodyFrames(57) ||
    !MountCapabilityRegistry.DoesNotOverrideLegFrames(60) ||
    !MountCapabilityRegistry.DoesNotOverrideBackpackDraw(59) ||
    MountCapabilityRegistry.DoesNotOverrideBodyFrames(56) ||
    !MountCapabilityRegistry.IsRollerSkates(58) ||
    !MountCapabilityRegistry.IsRollerSkates(60) ||
    MountCapabilityRegistry.IsRollerSkates(56) ||
    !MountCapabilityRegistry.CanUseWings(57) ||
    MountCapabilityRegistry.CanUseWings(63) ||
    !MountCapabilityRegistry.CanRideMinecartTracks(60) ||
    MountCapabilityRegistry.CanRideMinecartTracks(6) ||
    !MountCapabilityRegistry.BlocksExtraJumps(5) ||
    !MountCapabilityRegistry.BlocksExtraJumps(61) ||
    MountCapabilityRegistry.BlocksExtraJumps(9) ||
    !MountCapabilityRegistry.UsesHover(48) ||
    !MountCapabilityRegistry.UsesHover(61) ||
    MountCapabilityRegistry.UsesHover(6) ||
    !MountCapabilityRegistry.CanUseHoverFrame(48, 0) ||
    !MountCapabilityRegistry.CanUseHoverFrame(49, 4) ||
    MountCapabilityRegistry.CanUseHoverFrame(49, 0) ||
    MountCapabilityRegistry.CanUseHoverFrame(6, 4) ||
    MountCapabilityRegistry.GetFlightTimeMax(0) != 160 ||
    MountCapabilityRegistry.GetFlightTimeMax(50) != 80 ||
    MountCapabilityRegistry.GetFlightTimeMax(56) != 320 ||
    MountCapabilityRegistry.GetFlightTimeMax(6) != 0 ||
    MountCapabilityRegistry.GetFatigueMax(5) != 320 ||
    MountCapabilityRegistry.GetFatigueMax(49) != 320 ||
    MountCapabilityRegistry.GetFatigueMax(9) != 0 ||
    MountCapabilityRegistry.GetFallDamageMultiplier(1) != 0.8f ||
    MountCapabilityRegistry.GetFallDamageMultiplier(43) != 0.25f ||
    MountCapabilityRegistry.GetFallDamageMultiplier(56) != 0.0f ||
    MountCapabilityRegistry.GetFallDamageMultiplier(40) != 0.5f ||
    MountCapabilityRegistry.GetFallDamageMultiplier(57) != 1.0f ||
    MountCapabilityRegistry.GetFallDamageMultiplier(63) != 0.5f ||
    MountCapabilityRegistry.GetRunSpeed(6) != 13.0f ||
    MountCapabilityRegistry.GetRunSpeed(39) != 6.0f ||
    MountCapabilityRegistry.GetRunSpeed(40) != 3.0f ||
    MountCapabilityRegistry.GetRunSpeed(57) != 7.5f ||
    MountCapabilityRegistry.GetRunSpeed(62) != 3.0f ||
    MountCapabilityRegistry.GetDashSpeed(1) != 7.8f ||
    MountCapabilityRegistry.GetDashSpeed(45) != 16.0f ||
    MountCapabilityRegistry.GetDashSpeed(57) != 7.5f ||
    MountCapabilityRegistry.GetDashSpeed(14) != 0.0f ||
    MountCapabilityRegistry.GetAcceleration(39) != 0.02f ||
    MountCapabilityRegistry.GetAcceleration(57) != 0.3f ||
    MountCapabilityRegistry.GetAcceleration(63) != 0.32f ||
    MountCapabilityRegistry.GetAcceleration(14) != 0.25f ||
    MountCapabilityRegistry.GetJumpHeight(9) != 22 ||
    MountCapabilityRegistry.GetJumpHeight(57) != 14 ||
    MountCapabilityRegistry.GetJumpHeight(62) != 8 ||
    MountCapabilityRegistry.GetJumpHeight(49) != 4 ||
    MountCapabilityRegistry.GetJumpHeight(40) != 6 ||
    MountCapabilityRegistry.GetJumpSpeed(0) != 5.31f ||
    MountCapabilityRegistry.GetJumpSpeed(57) != 7.0f ||
    MountCapabilityRegistry.GetJumpSpeed(42) != 7.01f ||
    MountCapabilityRegistry.GetJumpSpeed(63) != 8.01f ||
    MountCapabilityRegistry.GetSwimSpeed(4) != 10.0f ||
    MountCapabilityRegistry.GetSwimSpeed(12) != 16.0f ||
    MountCapabilityRegistry.GetSwimSpeed(49) != 14.0f ||
    MountCapabilityRegistry.GetSwimSpeed(6) != 0.0f ||
    MountCapabilityRegistry.GetHeightBoost(4) != 26 ||
    MountCapabilityRegistry.GetHeightBoost(40) != 34 ||
    MountCapabilityRegistry.GetHeightBoost(62) != 4 ||
    MountCapabilityRegistry.GetHeightBoost(57) != 0)
{
  throw new InvalidOperationException("Mount capability registry did not preserve source-backed sets.");
}
if (mountState.IsMounted || mountState.MountType != -1)
{
  throw new InvalidOperationException("Player mount state default did not represent an unmounted player.");
}
mountState.Set(3);
if (!mountState.IsMounted || mountState.MountType != 3)
{
  throw new InvalidOperationException("Player mount state did not preserve the explicit mount type.");
}
mountState.Set(-1);
if (mountState.IsMounted || mountState.MountType != -1)
{
  throw new InvalidOperationException("Player mount state did not clear the explicit mount type.");
}
mountState.Set(50);
if (mountState.FlightTimeRemaining != 80 || !mountState.ConsumeFlightTime(true) ||
    mountState.FlightTimeRemaining != 79 || mountState.ConsumeFlightTime(false))
{
  throw new InvalidOperationException("Player mount state did not preserve the source-backed flight-time budget.");
}
mountState.RestoreFlightTime();
if (mountState.FlightTimeRemaining != 80)
{
  throw new InvalidOperationException("Player mount state did not restore the source-backed flight-time budget.");
}
mountState.Set(-1);
if (mountState.FlightTimeRemaining != 0)
{
  throw new InvalidOperationException("Player dismount did not clear the flight-time budget.");
}
mountState.Set(49);
if (mountState.IsHoverActive)
{
  throw new InvalidOperationException(
    "Type 49 hover became active outside its source-backed frame.");
}
mountState.SetFlightFrameState(4);
if (!mountState.IsHoverActive)
{
  throw new InvalidOperationException("Type 49 hover did not activate in its source-backed frame.");
}
mountState.SetFlightFrameState(0);
mountState.Set(-1);
mountState.Set(48);
if (!mountState.IsHoverActive)
{
  throw new InvalidOperationException(
    "Ordinary hover mount did not preserve its frame-independent capability.");
}
mountState.Set(-1);
mountState.Set(49);
if (mountState.FatigueRemaining != 320 || !mountState.ConsumeFatigue(true) ||
    mountState.FatigueRemaining != 319 || mountState.ConsumeFatigue(false))
{
  throw new InvalidOperationException("Player mount state did not preserve the source-backed fatigue budget.");
}

using DomeSimulation gravityContractSimulation = new(new WorldGrid(400, 300));
PlayerHandle gravityContractPlayer = gravityContractSimulation.CreatePlayer(
  new SimulationVector(20.0f, 20.0f));
gravityContractSimulation.ApplyPlayerMountControl(gravityContractPlayer, 49);
gravityContractSimulation.Tick(new SimulationInputBatch());
PlayerSnapshot gravityContractSnapshot = gravityContractSimulation.CreateSnapshot()
  .FindPlayer(gravityContractPlayer);
if (gravityContractSnapshot.Velocity.Y != -1.0f)
{
  throw new InvalidOperationException(
    "Type 49 frame-zero hover incorrectly bypassed the source gravity gate.");
}

gravityContractSimulation.ApplyPlayerMountControl(gravityContractPlayer, 48);
gravityContractSimulation.Tick(new SimulationInputBatch());
PlayerSnapshot ordinaryHoverSnapshot = gravityContractSimulation.CreateSnapshot()
  .FindPlayer(gravityContractPlayer);
if (ordinaryHoverSnapshot.Velocity.Y != -1.0f)
{
  throw new InvalidOperationException(
    "Ordinary hover mount did not preserve the source gravity gate.");
}

mountState.RestoreFatigue();
if (mountState.FatigueRemaining != 320)
{
  throw new InvalidOperationException("Player mount state did not restore the source-backed fatigue budget.");
}
mountState.Set(-1);
if (mountState.FatigueRemaining != 0)
{
  throw new InvalidOperationException("Player dismount did not clear the fatigue budget.");
}
mountState.SetFlightFrameState(4);
if (mountState.FlightFrameState != 4)
{
  throw new InvalidOperationException("Player mount state did not preserve the bounded flight frame state.");
}
try
{
  mountState.SetFlightFrameState(5);
  throw new InvalidOperationException("Player mount state accepted an unrepresentable flight frame state.");
}
catch (ArgumentOutOfRangeException)
{
}
mountState.Set(50);
mountState.ConsumeFlightTime(true);
mountState.RechargeFlightTime(10);
if (mountState.FlightTimeRemaining != 80)
{
  throw new InvalidOperationException("Player mount resource recovery did not clamp to source-backed maxima.");
}
mountState.Set(49);
mountState.ConsumeFatigue(true);
mountState.RecoverFatigue(1000);
if (mountState.FatigueRemaining != 320)
{
  throw new InvalidOperationException("Player fatigue recovery did not clamp to source-backed maxima.");
}
try
{
  mountState.RechargeFlightTime(-1);
  throw new InvalidOperationException("Player mount flight recovery accepted a negative amount.");
}
catch (ArgumentOutOfRangeException)
{
}
try
{
  mountState.RecoverFatigue(-1);
  throw new InvalidOperationException("Player mount fatigue recovery accepted a negative amount.");
}
catch (ArgumentOutOfRangeException)
{
}
mountState.Set(56);
if (!npcTargetingSystem.TryConsumeFlightInput(ref mountState, true) ||
    mountState.FlightTimeRemaining != 319 || mountState.FatigueRemaining != 319 ||
    npcTargetingSystem.TryConsumeFlightInput(ref mountState, false))
{
  throw new InvalidOperationException("Flight input authority did not consume the eligible mount budget.");
}
mountState.Set(54);
if (npcTargetingSystem.TryConsumeFlightInput(ref mountState, true) ||
    mountState.FlightTimeRemaining != 0)
{
  throw new InvalidOperationException("Flight input authority ignored the mount capability boundary.");
}
mountState.Set(48);
if (npcTargetingSystem.TryConsumeFlightInput(ref mountState, true))
{
  throw new InvalidOperationException("Flight input authority bypassed the legacy type 48 CanFly exclusion.");
}
mountState.Set(56);
if (!npcTargetingSystem.TryConsumeFlightInput(ref mountState, true, 2) ||
    npcTargetingSystem.TryConsumeFlightInput(ref mountState, true, 4))
{
  throw new InvalidOperationException("Flight input authority did not consume the source-backed frame-state boundary.");
}
mountState.Set(56);
for (int tick = 0; tick < 320; tick++)
{
  if (!mountState.ConsumeFlightTime(true))
  {
    throw new InvalidOperationException("Flight budget exhausted before its source-backed maximum.");
  }
}
if (!mountState.IsFlightExhausted || mountState.ConsumeFlightTime(true) ||
    npcTargetingSystem.TryConsumeFlightInput(ref mountState, true))
{
  throw new InvalidOperationException("Exhausted flight budget remained consumable.");
}
mountState.Set(49);
for (int tick = 0; tick < 320; tick++)
{
  if (!mountState.ConsumeFatigue(true))
  {
    throw new InvalidOperationException("Fatigue budget exhausted before its source-backed maximum.");
  }
}
if (!mountState.IsFatigueExhausted || mountState.ConsumeFatigue(true))
{
  throw new InvalidOperationException("Exhausted fatigue budget remained consumable.");
}
mountState.Set(56);
for (int tick = 0; tick < 320; tick++)
{
  if (!mountState.ConsumeFatigue(true))
  {
    throw new InvalidOperationException("Flight input fatigue gate exhausted before its source-backed maximum.");
  }
}
if (mountState.FlightTimeRemaining != 320 ||
    !mountState.IsFatigueExhausted ||
    npcTargetingSystem.TryConsumeFlightInput(ref mountState, true))
{
  throw new InvalidOperationException("Flight input bypassed an exhausted fatigue budget.");
}
mountState.Set(56);
if (!npcTargetingSystem.TryConsumeFlightInput(ref mountState, true) ||
    !npcTargetingSystem.TryRecoverFlightResources(ref mountState, true, 1) ||
    mountState.FlightTimeRemaining != 320 || mountState.FatigueRemaining != 320 ||
    npcTargetingSystem.TryRecoverFlightResources(ref mountState, false, 1) ||
    npcTargetingSystem.TryRecoverFlightResources(ref mountState, true, 0))
{
  throw new InvalidOperationException("Flight resource recovery authority did not preserve grounded boundaries.");
}
try
{
  mountState.Set(ushort.MaxValue + 1);
  throw new InvalidOperationException("Player mount state accepted an unrepresentable mount type.");
}
catch (ArgumentOutOfRangeException)
{
}
if (!npcTargetingSystem.TryApplyMountSummon(
      ref mountState,
      new ItemSummoningDefinition(MountType: 7)) ||
    !mountState.IsMounted || mountState.MountType != 7 ||
    npcTargetingSystem.TryApplyMountSummon(ref mountState, null))
{
  throw new InvalidOperationException("Player mount summon did not preserve explicit item metadata.");
}
if (!npcTargetingSystem.TryApplyMountControl(ref mountState, 9) ||
    !mountState.IsMounted || mountState.MountType != 9 ||
    !npcTargetingSystem.TryApplyMountControl(ref mountState, null) ||
    mountState.IsMounted || mountState.MountType != -1)
{
  throw new InvalidOperationException("Player mount control did not preserve explicit dismount semantics.");
}
mountState.Set(55);
if (!npcTargetingSystem.TryApplyEarlyDismount(ref mountState, true) || mountState.IsMounted)
{
  throw new InvalidOperationException("Player early dismount did not consume the source-backed item-use capability.");
}
mountState.Set(55);
if (npcTargetingSystem.TryApplyEarlyDismount(ref mountState, false) ||
    !mountState.IsMounted || mountState.MountType != 55)
{
  throw new InvalidOperationException("Player early dismount ignored the collision/space eligibility result.");
}
mountState.Set(54);
if (npcTargetingSystem.TryApplyEarlyDismount(ref mountState, true) ||
    !mountState.IsMounted || mountState.MountType != 54)
{
  throw new InvalidOperationException("Player early dismount ignored the mount capability boundary.");
}
byte[] encodedMountControls = TerrariaPacketCodec.EncodePlayerControls(
  new PlayerControlIntent(1, false, false, false, false, true, 0),
  0.0f,
  0.0f,
  9);
LegacyPlayerControlsProjection decodedMountControls =
  TerrariaPacketCodec.DecodePlayerControlsCompatibility(encodedMountControls);
if (decodedMountControls.State.MountType != 9)
{
  throw new InvalidOperationException("Player mount protocol projection did not preserve mount type.");
}
byte[] encodedDismountControls = TerrariaPacketCodec.EncodePlayerControls(
  new PlayerControlIntent(1, false, false, false, false, true, 0),
  0.0f,
  0.0f);
if (TerrariaPacketCodec.DecodePlayerControlsCompatibility(encodedDismountControls).State.MountType is not null)
{
  throw new InvalidOperationException("Player mount protocol projection did not preserve dismount state.");
}
using DomeSimulation mountMovementSimulation = new();
PlayerHandle mountMovementPlayer = mountMovementSimulation.CreatePlayer(
  new SimulationVector(0.0f, 0.0f));
mountMovementSimulation.ApplyPlayerMountControl(mountMovementPlayer, 40);
mountMovementSimulation.Tick(new SimulationInputBatch(
  new PlayerInput(mountMovementPlayer, false, true, false, false)));
if (mountMovementSimulation.CreateSnapshot().FindPlayer(mountMovementPlayer).Velocity.X != 0.25f)
{
  throw new InvalidOperationException("Player movement did not consume the mounted run-speed definition.");
}
using DomeSimulation mountAccelerationSimulation = new();
PlayerHandle mountAccelerationPlayer = mountAccelerationSimulation.CreatePlayer(
  new SimulationVector(0.0f, 0.0f));
mountAccelerationSimulation.ApplyPlayerMountControl(mountAccelerationPlayer, 57);
mountAccelerationSimulation.Tick(new SimulationInputBatch(
  new PlayerInput(mountAccelerationPlayer, false, true, false, false)));
float firstMountedSpeed = mountAccelerationSimulation.CreateSnapshot()
  .FindPlayer(mountAccelerationPlayer).Velocity.X;
mountAccelerationSimulation.Tick(new SimulationInputBatch(
  new PlayerInput(mountAccelerationPlayer, false, true, false, false)));
float secondMountedSpeed = mountAccelerationSimulation.CreateSnapshot()
  .FindPlayer(mountAccelerationPlayer).Velocity.X;
mountAccelerationSimulation.Tick(new SimulationInputBatch(
  new PlayerInput(mountAccelerationPlayer, false, false, false, false)));
float releasedMountedSpeed = mountAccelerationSimulation.CreateSnapshot()
  .FindPlayer(mountAccelerationPlayer).Velocity.X;
if (firstMountedSpeed != 0.3f || secondMountedSpeed != 0.6f || releasedMountedSpeed != 0.3f)
{
  throw new InvalidOperationException(
    "Mounted movement did not preserve source-backed acceleration and deceleration boundaries.");
}
using DomeSimulation mountDashSimulation = new();
PlayerHandle mountDashPlayer = mountDashSimulation.CreatePlayer(new SimulationVector(0.0f, 0.0f));
mountDashSimulation.ApplyPlayerMountControl(mountDashPlayer, 57);
for (int tick = 0; tick < 25; tick++)
{
  mountDashSimulation.Tick(new SimulationInputBatch(
    new PlayerInput(mountDashPlayer, false, true, false, false, Dash: true)));
}
if (mountDashSimulation.CreateSnapshot().FindPlayer(mountDashPlayer).Velocity.X != 7.5f)
{
  throw new InvalidOperationException("Mounted dash input did not consume the source-backed dash-speed definition.");
}
using DomeSimulation mountJumpSimulation = new();
PlayerHandle mountJumpPlayer = mountJumpSimulation.CreatePlayer(new SimulationVector(0.0f, 0.0f));
mountJumpSimulation.ApplyPlayerMountControl(mountJumpPlayer, 57);
mountJumpSimulation.Tick(new SimulationInputBatch(
  new PlayerInput(mountJumpPlayer, false, false, true, false)));
if (mountJumpSimulation.CreateSnapshot().FindPlayer(mountJumpPlayer).Velocity.Y != 6.0f)
{
  throw new InvalidOperationException("Mounted jump input did not consume the source-backed jump-speed definition.");
}
using DomeSimulation hoverSimulation = new();
PlayerHandle hoverPlayer = hoverSimulation.CreatePlayer(new SimulationVector(0.0f, 10.0f));
hoverSimulation.ApplyPlayerMountControl(hoverPlayer, 48);
hoverSimulation.Tick(new SimulationInputBatch());
if (hoverSimulation.CreateSnapshot().FindPlayer(hoverPlayer).Velocity.Y != 0.0f)
{
  throw new InvalidOperationException("Hover mount did not preserve the source-backed gravity gate.");
}
using DomeSimulation gravityMountSimulation = new();
PlayerHandle gravityMountPlayer = gravityMountSimulation.CreatePlayer(new SimulationVector(0.0f, 10.0f));
gravityMountSimulation.ApplyPlayerMountControl(gravityMountPlayer, 57);
gravityMountSimulation.Tick(new SimulationInputBatch());
if (gravityMountSimulation.CreateSnapshot().FindPlayer(gravityMountPlayer).Velocity.Y != -1.0f)
{
  throw new InvalidOperationException("Non-hover mount unexpectedly bypassed the gravity path.");
}
using DomeSimulation flightMovementSimulation = new();
PlayerHandle flightMovementPlayer = flightMovementSimulation.CreatePlayer(
  new SimulationVector(0.0f, 10.0f));
flightMovementSimulation.ApplyPlayerMountControl(flightMovementPlayer, 56);
flightMovementSimulation.Tick(new SimulationInputBatch(
  new PlayerInput(flightMovementPlayer, false, false, false, false, Up: true)));
if (flightMovementSimulation.CreateSnapshot().FindPlayer(flightMovementPlayer).Velocity.Y != 5.0f)
{
  throw new InvalidOperationException("Mounted flight input did not apply the source-backed vertical speed.");
}
using DomeSimulation mountSnapshotSimulation = new();
PlayerHandle mountSnapshotPlayer = mountSnapshotSimulation.CreatePlayer(
  new SimulationVector(0.0f, 0.0f));
mountSnapshotSimulation.ApplyPlayerMountControl(mountSnapshotPlayer, 11);
if (mountSnapshotSimulation.CreateSnapshot().FindPlayer(mountSnapshotPlayer).MountType != 11 ||
    mountSnapshotSimulation.CreatePlayerStateSnapshot(mountSnapshotPlayer).MountType != 11)
{
  throw new InvalidOperationException("Player mount state did not survive authoritative snapshot projection.");
}
mountSnapshotSimulation.ApplyPlayerMountControl(mountSnapshotPlayer, null);
if (mountSnapshotSimulation.CreateSnapshot().FindPlayer(mountSnapshotPlayer).MountType is not null ||
    mountSnapshotSimulation.CreatePlayerStateSnapshot(mountSnapshotPlayer).MountType is not null)
{
  throw new InvalidOperationException("Player dismount state did not survive authoritative snapshot projection.");
}
mountSnapshotSimulation.ApplyPlayerMountControl(mountSnapshotPlayer, 12);
mountSnapshotSimulation.QueuePlayerDamage(mountSnapshotPlayer, 100);
mountSnapshotSimulation.Tick(new SimulationInputBatch());
if (mountSnapshotSimulation.CreateSnapshot().FindPlayer(mountSnapshotPlayer).MountType is not null ||
    mountSnapshotSimulation.CreatePlayerStateSnapshot(mountSnapshotPlayer).MountType is not null)
{
  throw new InvalidOperationException("Player death did not clear authoritative mount state.");
}
try
{
  mountSnapshotSimulation.ApplyPlayerMountControl(mountSnapshotPlayer, null);
  throw new InvalidOperationException("Inactive player accepted a mount control replay.");
}
catch (ArgumentException)
{
}
if (mountSnapshotSimulation.CreateSnapshot().FindPlayer(mountSnapshotPlayer).MountType is not null)
{
  throw new InvalidOperationException("Rejected inactive mount control changed dismounted authoritative state.");
}
for (int tick = 0; tick < 3; tick++)
{
  mountSnapshotSimulation.Tick(new SimulationInputBatch());
}
if (!mountSnapshotSimulation.CreatePlayerStateSnapshot(mountSnapshotPlayer).IsActive ||
    mountSnapshotSimulation.CreateSnapshot().FindPlayer(mountSnapshotPlayer).MountType is not null)
{
  throw new InvalidOperationException("Player respawn did not preserve the dismounted lifecycle state.");
}
stealthState.Set(false, 0.0f, true, false);
npcTargetingSystem.AdvanceStealth(ref stealthState, false, 0.0f, 0.0f, false);
if (stealthState.Stealth != 0.0f)
{
  throw new InvalidOperationException("Player stealth state did not preserve the stationary floor.");
}
stealthState.Set(false, 0.5f, true, false);
npcTargetingSystem.AdvanceStealth(ref stealthState, false, 1.0f, 0.0f, false);
if (stealthState.Stealth <= 0.5f)
{
  throw new InvalidOperationException("Player stealth state did not recover while moving.");
}
npcTargetingSystem.AdvanceStealth(ref stealthState, false, 0.0f, 0.0f, true);
if (stealthState.Stealth != 1.0f)
{
  throw new InvalidOperationException("Player stealth state did not reset on mounting.");
}
stealthState.Set(false, 0.5f, true, false);
npcTargetingSystem.AdvanceStealth(ref stealthState, true, 0.0f, 0.0f, false);
if (stealthState.StealthTimer != 4)
{
  throw new InvalidOperationException("Player stealth state did not preserve item-use timer semantics.");
}
stealthState.Set(false, 0.0f, true, false);
npcTargetingSystem.RefreshAggro(ref targetingState, stealthState, 0);
if (targetingState.Aggro != -750)
{
  throw new InvalidOperationException("Player stealth state did not refresh aggro through the typed owner.");
}
npcTargetingSystem.SetAggro(ref targetingState, 0);
npcTargetingSystem.SetAggro(ref targetingState, npcTargetingSystem.CalculateAggro(
  0,
  false,
  0.0f,
  true,
  false));
if (targetingState.Aggro != -750 ||
    npcTargetingSystem.CalculateAggro(0, false, 0.0f, false, true) != -1200)
{
  throw new InvalidOperationException("Player stealth aggro resolver did not preserve source-backed reductions.");
}
npcTargetingSystem.SetAggro(ref targetingState, -750);
if (targetingState.Aggro != -750)
{
  throw new InvalidOperationException("Player NPC targeting did not accept the explicit aggro result.");
}
npcTargetingSystem.RefreshNoAggroCapabilities(
  ref targetingState,
  noAggroRegistry,
  hasItem3090Effect: true);
if (!targetingState.IsNoAggroNpc(1) || !targetingState.IsNoAggroNpc(676))
{
  throw new InvalidOperationException("Player NPC targeting refresh did not apply the item-3090 capability.");
}
npcTargetingSystem.RefreshNoAggroCapabilities(
  ref targetingState,
  noAggroRegistry,
  hasItem3090Effect: false);
if (targetingState.IsNoAggroNpc(1) || targetingState.NoAggroNpcTypes?.Count != 0)
{
  throw new InvalidOperationException("Player NPC targeting refresh did not clear stale no-aggro state.");
}
InventoryComponent targetingInventory = new();
targetingInventory.SetSlot(0, new ItemStack(3090, 1));
EquipmentStateCollectionComponent targetingEquipment = new();
targetingEquipment.Add(new ItemEquipmentStateComponent(
  ItemEquipmentSlot.Accessory,
  0,
  false));
npcTargetingSystem.RefreshFromEquipment(
  ref targetingState,
  targetingEquipment,
  targetingInventory,
  noAggroRegistry);
if (!targetingState.IsNoAggroNpc(1))
{
  throw new InvalidOperationException("Player NPC targeting did not derive no-aggro from equipped item 3090.");
}
targetingEquipment.Remove(ItemEquipmentSlot.Accessory);
npcTargetingSystem.RefreshFromEquipment(
  ref targetingState,
  targetingEquipment,
  targetingInventory,
  noAggroRegistry);
if (targetingState.IsNoAggroNpc(1))
{
  throw new InvalidOperationException("Player NPC targeting retained removed item-3090 capability.");
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

NpcTargetComponent overflowTarget = targetSelection.SelectTarget(
  new SimulationVector(float.MaxValue, 0.0f),
  [
    new NpcTargetCandidate(firstPlayer, 1, new SimulationVector(-float.MaxValue, 0.0f), true, 100),
    new NpcTargetCandidate(secondPlayer, 2, new SimulationVector(float.MaxValue - 1.0f, 0.0f), true, 100)
  ]);
if (!overflowTarget.HasTarget || overflowTarget.StableTargetId != 2)
{
  throw new InvalidOperationException("NPC target selection accepted an overflowing distance result.");
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
if (!targetRouting.HasPlayerTarget(0, 255) ||
    !targetRouting.HasPlayerTarget(254, 255) ||
    targetRouting.HasPlayerTarget(255, 255) ||
    targetRouting.HasPlayerTarget(-1, 255) ||
    targetRouting.HasPlayerTarget(int.MaxValue, int.MaxValue) ||
    !targetRouting.HasNpcTarget(304, 10) ||
    targetRouting.HasNpcTarget(299, 10) ||
    targetRouting.HasNpcTarget(310, 10) ||
    targetRouting.HasNpcTarget(int.MaxValue, 10) ||
    !targetRouting.TryTranslateNpcTargetIndex(304, 10, out int translatedTargetIndex) ||
    translatedTargetIndex != 4 ||
    targetRouting.TryTranslateNpcTargetIndex(299, 10, out _) ||
    !targetRouting.TryTranslateNpcTargetIndex(
      int.MaxValue,
      int.MaxValue,
      out int maximumTranslatedTargetIndex) ||
    maximumTranslatedTargetIndex != int.MaxValue - 300)
{
  throw new InvalidOperationException(
    "NPC encoded target property queries did not preserve source bounds.");
}

if (!targetRouting.TryTranslateTargetIndex(
      304,
      255,
      10,
      out int translatedNpcTarget,
      out bool isNpcTarget) ||
    translatedNpcTarget != 4 ||
    !isNpcTarget ||
    !targetRouting.TryTranslateTargetIndex(
      12,
      255,
      10,
      out int translatedPlayerTarget,
      out bool playerTargetIsNpc) ||
    translatedPlayerTarget != 12 ||
    playerTargetIsNpc ||
    targetRouting.TryTranslateTargetIndex(299, 255, 10, out _, out _))
{
  throw new InvalidOperationException(
    "NPC translated target index did not preserve target-family precedence.");
}

if (!targetRouting.HasValidTarget(
      12,
      true,
      255,
      10,
      [new PlayerTargetRouteCandidate(firstPlayer, 12, true, false, false)],
      []) ||
    targetRouting.HasValidTarget(
      12,
      true,
      255,
      10,
      [new PlayerTargetRouteCandidate(firstPlayer, 12, true, true, false)],
      []) ||
    !targetRouting.HasValidTarget(
      304,
      true,
      255,
      10,
      [],
      [activeNpcTarget]) ||
    targetRouting.HasValidTarget(304, false, 255, 10, [], [activeNpcTarget]))
{
  throw new InvalidOperationException(
    "NPC valid-target routing did not preserve player-first fallback semantics.");
}

if (!targetRouting.TryResolve(304, true, 10, [activeNpcTarget], out Entity routedTarget) ||
    routedTarget != secondPlayer ||
    targetRouting.TryResolve(299, true, 10, [activeNpcTarget], out _) ||
    targetRouting.TryResolve(310, true, 10, [activeNpcTarget], out _) ||
    targetRouting.TryResolve(304, false, 10, [activeNpcTarget], out _))
{
  throw new InvalidOperationException("NPC encoded target routing did not preserve source bounds.");
}

if (!targetRouting.TryResolve(
      304,
      new NpcDefinition(
        DefinitionId: 25,
        NetId: 25,
        MaximumHealth: 100,
        Defense: 0,
        ColliderWidth: 1.0f,
        ColliderHeight: 2.0f,
        BehaviorId: NpcBehaviorId.OrdinaryChase,
        LootTableId: 1,
        SupportsNpcTargets: true),
      10,
      [activeNpcTarget],
      out Entity definitionRoutedTarget) ||
    definitionRoutedTarget != secondPlayer ||
    targetRouting.TryResolve(
      304,
      ordinaryDefinition,
      10,
      [activeNpcTarget],
      out _))
{
  throw new InvalidOperationException(
    "NPC encoded target routing did not consume the definition capability boundary.");
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
NpcBehaviorRegistry orderedBehaviorRegistry = new(new Dictionary<NpcBehaviorId, NpcBehaviorHandler>
{
  [NpcBehaviorId.FloatingEye] = _ => default,
  [NpcBehaviorId.OrdinaryChase] = _ => default,
  [NpcBehaviorId.TownHome] = _ => default
});
if (!orderedBehaviorRegistry.RegisteredBehaviorIds.SequenceEqual(
      [NpcBehaviorId.OrdinaryChase, NpcBehaviorId.TownHome, NpcBehaviorId.FloatingEye]))
{
  throw new InvalidOperationException("NPC behavior registry did not expose deterministic behavior ordering.");
}
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

bool rejectedUnregisteredBehavior = false;
try
{
  _ = behaviorSystem.Evaluate(
    new NpcBehaviorStateComponent(
      (NpcBehaviorId)99,
      new NpcChaseState(1.0f, 0.0f),
      new NpcTownHomeState(default, true, 0)),
    selectedTarget,
    new SimulationVector(0.0f, 0.0f),
    new SimulationVector(1.0f, 0.0f),
    isDayTime: true);
}
catch (InvalidOperationException)
{
  rejectedUnregisteredBehavior = true;
}

if (!rejectedUnregisteredBehavior)
{
  throw new InvalidOperationException("NPC behavior system accepted an unregistered behavior.");
}

NpcBehaviorResult invalidChaseResult = behaviorSystem.Evaluate(
  new NpcBehaviorStateComponent(
    NpcBehaviorId.OrdinaryChase,
    new NpcChaseState(float.NaN, -1.0f),
    new NpcTownHomeState(default, true, 0)),
  selectedTarget,
  new SimulationVector(0.0f, 0.0f),
  new SimulationVector(4.0f, 0.0f),
  isDayTime: true);
if (!float.IsFinite(invalidChaseResult.Velocity.X) ||
    !float.IsFinite(invalidChaseResult.Velocity.Y) ||
    invalidChaseResult.Velocity.X != 0.0f ||
    invalidChaseResult.Velocity.Y != 0.0f ||
    invalidChaseResult.Facing != 0)
{
  throw new InvalidOperationException("NPC chase behavior did not fail closed for invalid state.");
}

NpcBehaviorStateComponent flyingState = new(
  NpcBehaviorId.FloatingEye,
  new NpcChaseState(0.0f, 0.0f),
  new NpcTownHomeState(default, true, 0),
  new NpcFlyingState(0.1f, 0.15f, 4.0f, 2.5f));
NpcBehaviorResult flyingResult = behaviorSystem.Evaluate(
  flyingState,
  selectedTarget,
  new SimulationVector(0.0f, 5.0f),
  new SimulationVector(4.0f, 0.0f),
  isDayTime: true);
if (flyingResult.Velocity.X <= 0.0f || flyingResult.Velocity.Y >= 0.0f ||
    flyingResult.Velocity.X > 4.0f || flyingResult.Velocity.Y < -2.5f ||
    flyingResult.Facing != 1)
{
  throw new InvalidOperationException("Typed floating-eye behavior did not clamp pursuit velocity.");
}

NpcBehaviorResult invalidFlyingResult = behaviorSystem.Evaluate(
  flyingState with
  {
    Flying = new NpcFlyingState(float.NaN, 0.15f, 4.0f, 2.5f)
  },
  selectedTarget,
  new SimulationVector(0.0f, 5.0f),
  new SimulationVector(4.0f, 0.0f),
  isDayTime: true);
if (!float.IsFinite(invalidFlyingResult.Velocity.X) ||
    !float.IsFinite(invalidFlyingResult.Velocity.Y) ||
    invalidFlyingResult.Velocity.X != 0.0f ||
    invalidFlyingResult.Velocity.Y != 0.0f ||
    invalidFlyingResult.Facing != 0)
{
  throw new InvalidOperationException("Floating-eye behavior did not fail closed for NaN state.");
}

NpcSegmentFollowSystem segmentFollowSystem = new();
NpcSegmentComponent segmentFollowState = new(
  new NpcHandle(20),
  new NpcHandle(19),
  default,
  segmentIndex: 1,
  isRoot: false,
  NpcSegmentLifePolicy.RootShared);
NpcSegmentFollowResult segmentFollowResult = segmentFollowSystem.Evaluate(
  segmentFollowState,
  new NpcChaseState(2.0f, 0.0f),
  new SimulationVector(0.0f, 0.0f),
  new SimulationVector(3.0f, 4.0f));
if (segmentFollowResult.Velocity != new SimulationVector(1.2f, 1.6f) ||
    segmentFollowResult.Facing != 1)
{
  throw new InvalidOperationException("Segment follow did not produce deterministic parent movement.");
}

bool rejectedInvalidSegmentFollow = false;
try
{
  _ = segmentFollowSystem.Evaluate(
    segmentFollowState,
    new NpcChaseState(float.NaN, 0.0f),
    new SimulationVector(0.0f, 0.0f),
    new SimulationVector(3.0f, 4.0f));
}
catch (ArgumentOutOfRangeException)
{
  rejectedInvalidSegmentFollow = true;
}

if (!rejectedInvalidSegmentFollow)
{
  throw new InvalidOperationException("Segment follow accepted a non-finite speed.");
}

NpcRangedAttackSystem rangedAttackSystem = new();
NpcRangedAttackState rangedState = new(
  projectileType: 7,
  damage: 20,
  projectileSpeed: 5.0f,
  cooldownTicks: 2);
if (!rangedAttackSystem.TryProduce(
      new NpcHandle(2),
      ref rangedState,
      new SimulationVector(0.0f, 0.0f),
      new SimulationVector(3.0f, 4.0f),
      hasTarget: true,
      hasLineOfSight: true,
      sequence: 1,
      out NpcRangedAttackCommand rangedCommand) ||
    rangedCommand.Velocity != new SimulationVector(3.0f, 4.0f) ||
    rangedState.RemainingCooldownTicks != 2 ||
    rangedAttackSystem.TryProduce(
      new NpcHandle(2),
      ref rangedState,
      new SimulationVector(0.0f, 0.0f),
      new SimulationVector(3.0f, 4.0f),
      hasTarget: true,
      hasLineOfSight: true,
      sequence: 2,
      out _))
{
  throw new InvalidOperationException("NPC ranged attack did not emit or enforce cooldown correctly.");
}

NpcRangedAttackCommitSystem rangedCommitSystem = new();
NpcProjectileSpawnRequest rangedRequest = default;
if (rangedCommitSystem.TryCommit(
      rangedCommand,
      ProjectileDefinitionRegistry.CreateDefault(),
      out rangedRequest) ||
    rangedRequest.SourceNpc.IsValid)
{
  throw new InvalidOperationException(
    "NPC ranged commit accepted a projectile definition without hostile NPC ownership.");
}

NpcRangedAttackCommand hostileRangedCommand = rangedCommand with { ProjectileType = 3 };
ProjectileDefinitionRegistry hostileProjectileDefinitions = new([
  new ProjectileDefinition(
    ProjectileType: 3,
    BehaviorId: 1,
    Damage: hostileRangedCommand.Damage,
    LifetimeTicks: 30,
    Collider: new SimulationComponents.ColliderComponent(0.5f, 0.5f),
    Friendly: false,
    Hostile: true,
    MaximumPenetration: 1,
    PlayerDamagePolicy: PlayerDamagePolicy.HostileNonPvp)]);
if (!rangedCommitSystem.TryCommit(
      hostileRangedCommand,
      hostileProjectileDefinitions,
      out rangedRequest) ||
    rangedRequest.SourceNpc != hostileRangedCommand.SourceNpc ||
    rangedRequest.Definition.ProjectileType != hostileRangedCommand.ProjectileType)
{
  throw new InvalidOperationException("NPC ranged commit did not preserve the typed hostile request.");
}

NpcProjectileSpawnRequest pvpRequest = default;
if (rangedCommitSystem.TryCommit(
      hostileRangedCommand with { ProjectileType = 6 },
      ProjectileDefinitionRegistry.CreateDefault(),
      out pvpRequest) ||
    pvpRequest.SourceNpc.IsValid)
{
  throw new InvalidOperationException(
    "NPC ranged commit admitted a PVP-only projectile definition.");
}

NpcProjectileSpawnSystem npcProjectileSpawnSystem = new();
using World npcProjectileWorld = World.Create();
Entity npcProjectile = npcProjectileSpawnSystem.Spawn(npcProjectileWorld, rangedRequest, identity: 12);
if (!npcProjectileWorld.IsAlive(npcProjectile) ||
    npcProjectileWorld.Get<SimulationComponents.NpcProjectileOwnerComponent>(npcProjectile).Owner !=
      hostileRangedCommand.SourceNpc ||
    npcProjectileWorld.Get<SimulationComponents.NpcProjectileNetworkIdentityComponent>(npcProjectile).Identity != 12)
{
  throw new InvalidOperationException("NPC projectile spawn did not preserve typed ownership.");
}

NpcProjectileReplicationSnapshot npcProjectileReplication =
  new NpcProjectileReplicationSystem().Project(
    npcProjectile,
    npcProjectileWorld,
    replicationId: 12,
    revision: 1,
    new WorldSectionCoordinates(0, 0));
if (npcProjectileReplication.Owner != hostileRangedCommand.SourceNpc ||
    npcProjectileReplication.Identity != 12 ||
    npcProjectileReplication.ProjectileUuid == null ||
    npcProjectileReplication.ProjectileUuid == Guid.Empty ||
    npcProjectileReplication.ProjectileType != hostileRangedCommand.ProjectileType ||
    npcProjectileReplication.Damage != hostileRangedCommand.Damage)
{
  throw new InvalidOperationException("NPC projectile replication did not preserve typed state.");
}

NpcProjectileLifetimeSystem npcProjectileLifetimeSystem = new();
NpcProjectileReplicationSystem npcProjectileReplicationSystem = new();
while (!npcProjectileLifetimeSystem.Advance(npcProjectile, npcProjectileWorld))
{
}

NpcProjectileReplicationSnapshot npcProjectileTombstone =
  npcProjectileReplicationSystem.ProjectTombstone(
    npcProjectile,
    npcProjectileWorld,
    replicationId: 12,
    revision: 2,
    currentTick: 100,
    new WorldSectionCoordinates(0, 0),
    ProjectileTombstoneReason.Expired);
if (npcProjectileTombstone.IsActive ||
    npcProjectileTombstone.RemainingLifetime != 0 ||
    npcProjectileTombstone.TombstoneReason != ProjectileTombstoneReason.Expired ||
    npcProjectileTombstone.TombstoneRetainedUntilTick <= 100 ||
    npcProjectileTombstone.Owner != hostileRangedCommand.SourceNpc ||
    npcProjectileTombstone.Identity != 12)
{
  throw new InvalidOperationException(
    "NPC projectile expiry did not preserve a typed retained tombstone.");
}

using DomeSimulation npcLifecycleSimulation = new();
NpcHandle simulationNpc = npcLifecycleSimulation.CreateNpc(
  new SimulationVector(20.0f, 20.0f),
  definitionId: 1);
NpcProjectileSpawnRequest simulationRequest = rangedRequest with
{
  SourceNpc = simulationNpc,
  Position = new SimulationVector(20.0f, 20.0f),
  Velocity = new SimulationVector(0.0f, 0.0f)
};
NpcProjectileReplicationSnapshot simulationProjectile =
  npcLifecycleSimulation.CreateNpcProjectile(simulationRequest);
for (int tick = 0; tick < simulationRequest.Definition.LifetimeTicks; tick++)
{
  npcLifecycleSimulation.Tick(new SimulationInputBatch());
}

NpcProjectileReplicationSnapshot simulationTombstone =
  npcLifecycleSimulation.CreateNpcProjectileReplicationSnapshots().Single();
if (simulationProjectile.ReplicationId != simulationTombstone.ReplicationId ||
    simulationTombstone.IsActive ||
    simulationTombstone.ProjectileUuid != simulationProjectile.ProjectileUuid ||
    simulationTombstone.TombstoneReason != ProjectileTombstoneReason.Expired ||
    simulationTombstone.Owner != simulationNpc)
{
  throw new InvalidOperationException(
    "DomeSimulation did not advance NPC projectile expiry into typed replication.");
}

using DomeSimulation npcBehaviorLifecycleSimulation = new();
NpcHandle behaviorNpc = npcBehaviorLifecycleSimulation.CreateNpc(
  new SimulationVector(20.0f, 20.0f),
  definitionId: 1);
NpcProjectileSpawnRequest gravityRequest = simulationRequest with
{
  SourceNpc = behaviorNpc,
  Definition = simulationRequest.Definition with
  {
    BehaviorId = 2,
    LifetimeTicks = 1
  },
  Position = new SimulationVector(20.0f, 20.0f),
  Velocity = new SimulationVector(0.0f, 0.0f)
};
_ = npcBehaviorLifecycleSimulation.CreateNpcProjectile(gravityRequest);
npcBehaviorLifecycleSimulation.Tick(new SimulationInputBatch());
NpcProjectileReplicationSnapshot gravityTombstone = npcBehaviorLifecycleSimulation
  .CreateNpcProjectileReplicationSnapshots()
  .Single();
if (gravityTombstone.IsActive ||
    gravityTombstone.TombstoneReason != ProjectileTombstoneReason.Expired ||
    gravityTombstone.Position.Y != 19.75f ||
    gravityTombstone.Ai0 != -0.25f || gravityTombstone.Ai1 != 1.0f ||
    gravityTombstone.Ai2 != 0.0f)
{
  throw new InvalidOperationException(
    "NPC projectile lifecycle did not retain the authoritative Gravity AI state.");
}

WorldGrid npcTilePassWorld = new(400, 300);
_ = npcTilePassWorld.TrySetTile(24, 20, new WorldTile(IsActive: true, Type: 1));
using DomeSimulation npcTilePassSimulation = new(npcTilePassWorld);
NpcHandle tilePassNpc = npcTilePassSimulation.CreateNpc(
  new SimulationVector(20.0f, 20.0f),
  definitionId: 1);
if (!rangedCommitSystem.TryCommit(
      hostileRangedCommand with { ProjectileType = 300 },
      ProjectileDefinitionRegistry.CreateDefault(),
      out NpcProjectileSpawnRequest tilePassRequest))
{
  throw new InvalidOperationException("NPC ranged commit rejected the type 300 source definition.");
}

_ = npcTilePassSimulation.CreateNpcProjectile(tilePassRequest with
{
  SourceNpc = tilePassNpc,
  Position = new SimulationVector(20.0f, 20.0f),
  Velocity = new SimulationVector(4.0f, 0.0f)
});
npcTilePassSimulation.Tick(new SimulationInputBatch());
NpcProjectileReplicationSnapshot tilePassingNpcProjectile = npcTilePassSimulation
  .CreateNpcProjectileReplicationSnapshots()
  .Single();
if (!tilePassingNpcProjectile.IsActive ||
    tilePassingNpcProjectile.TombstoneReason != ProjectileTombstoneReason.None)
{
  throw new InvalidOperationException(
    "NPC type 300 projectile did not preserve its source tileCollide false contract.");
}

using DomeSimulation npcHitSimulation = new();
NpcHandle hitNpc = npcHitSimulation.CreateNpc(new SimulationVector(10.0f, 0.0f), definitionId: 1);
PlayerHandle hitPlayer = npcHitSimulation.CreatePlayer(new SimulationVector(1.0f, 0.0f));
NpcProjectileReplicationSnapshot hitProjectile = npcHitSimulation.CreateNpcProjectile(
  simulationRequest with
  {
    SourceNpc = hitNpc,
    Position = new SimulationVector(0.0f, 0.0f),
    Velocity = new SimulationVector(1.0f, 0.0f)
  });
npcHitSimulation.Tick(new SimulationInputBatch());
if (npcHitSimulation.CreatePlayerStateSnapshot(hitPlayer).Health >= 100 ||
    npcHitSimulation.CreateNpcProjectileReplicationSnapshots().Single().TombstoneReason !=
      ProjectileTombstoneReason.Penetrated ||
    hitProjectile.Owner != hitNpc)
{
  throw new InvalidOperationException(
    $"NPC projectile did not damage a player and retain typed penetration termination: " +
    $"health={npcHitSimulation.CreatePlayerStateSnapshot(hitPlayer).Health}, " +
    $"tombstone={npcHitSimulation.CreateNpcProjectileReplicationSnapshots().Single().TombstoneReason}, " +
    $"active={npcHitSimulation.CreateNpcProjectileReplicationSnapshots().Single().IsActive}");
}

try
{
  _ = ProjectileStateProjection.Project(npcProjectileReplication);
  throw new InvalidOperationException(
    "V1456 projection silently converted an NPC-owned projectile to player ownership.");
}
catch (NotSupportedException)
{
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

using World staleTargetWorld = World.Create();
Entity staleTargetNpc = staleTargetWorld.Create(
  new SimulationComponents.TransformComponent(8.0f, 0.0f),
  new Terraria.Dome.Simulation.Movement.Components.MovementIntentComponent(),
  new NpcBehaviorStateComponent(
    NpcBehaviorId.OrdinaryChase,
    new NpcChaseState(1.0f, 0.0f),
    new NpcTownHomeState(default, true, 0)),
  new NpcTargetComponent(default, 1, NpcTargetLockReason.RetainedValidTarget));
new NpcMovementIntentSystem().Apply(staleTargetWorld, staleTargetNpc, isDayTime: true);
Terraria.Dome.Simulation.Movement.Components.MovementIntentComponent staleTargetIntent =
  staleTargetWorld.Get<Terraria.Dome.Simulation.Movement.Components.MovementIntentComponent>(
    staleTargetNpc);
if (!staleTargetIntent.HasNpcIntent || staleTargetIntent.NpcHorizontalVelocity != 0.0f ||
    staleTargetIntent.NpcVerticalVelocity != 0.0f || staleTargetIntent.HorizontalDirection != 0)
{
  throw new InvalidOperationException("An invalid NPC target continued to drive movement intent.");
}

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

SpawnNpcCommand halfSlotSpawn = validSpawn with { RequestedReplicationId = 9 };
SpawnNpcCommand fullSlotSpawn = validSpawn with { RequestedReplicationId = 10 };
IReadOnlyList<SpawnNpcCommand> weightedEligibleSpawns = spawnEligibility.Evaluate(
  new NpcSpawnSnapshot(
    [
      new NpcSpawnCandidate(
        halfSlotSpawn,
        IsOccupied: false,
        IsProtectedSlot: false,
        NpcSlotCost: 0.5f),
      new NpcSpawnCandidate(
        fullSlotSpawn,
        IsOccupied: false,
        IsProtectedSlot: false,
        NpcSlotCost: 1.0f)],
    activeNpcCount: 0,
    maximumNpcCount: 2,
    protectedSlotCount: 0,
    existingReplicationIds: new HashSet<int>(),
    activeNpcSlots: 1.5f));
if (weightedEligibleSpawns.Count != 1 || weightedEligibleSpawns[0] != halfSlotSpawn)
{
  throw new InvalidOperationException("NPC spawn eligibility did not enforce the weighted slot budget.");
}

if (spawnEligibility.Evaluate(new NpcSpawnSnapshot(
      [new NpcSpawnCandidate(
        validSpawn,
        IsOccupied: false,
        IsProtectedSlot: false,
        NpcSlotCost: float.NaN)],
      activeNpcCount: 0,
      maximumNpcCount: 2,
      protectedSlotCount: 0,
      existingReplicationIds: new HashSet<int>())).Count != 0)
{
  throw new InvalidOperationException("NPC spawn eligibility accepted a non-finite candidate slot cost.");
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
      authorityComponent.AlwaysReplicate || lifecycleComponent.DoesNotCountMe)
  {
    throw new InvalidOperationException("NPC spawn commit did not attach identity and lifecycle components.");
  }
}
else
{
  throw new InvalidOperationException("Valid NPC spawn command was rejected.");
}

using World homelessSpawnWorld = World.Create();
SpawnNpcCommand homelessSpawn = validSpawn with { HomelessDespawn = true };
if (!spawnCommit.TryCommit(
      homelessSpawnWorld,
      registry,
      homelessSpawn,
      replicationId: 15,
      out NpcSpawnCommitResult homelessSpawnResult,
      out _))
{
  throw new InvalidOperationException("NPC homeless-despawn spawn command was rejected.");
}

NpcLifecycleComponent homelessLifecycle = homelessSpawnWorld
  .Get<NpcLifecycleComponent>(homelessSpawnResult.Entity);
if (!homelessLifecycle.HomelessDespawn)
{
  throw new InvalidOperationException(
    "NPC spawn commit did not preserve the explicit homeless-despawn lifecycle flag.");
}

homelessLifecycle.ClearHomelessDespawn();
homelessLifecycle.MarkHomelessDespawn();
if (!homelessLifecycle.HomelessDespawn)
{
  throw new InvalidOperationException(
    "NPC lifecycle did not preserve the explicit homeless-despawn mark and clear operations.");
}

NpcDefinitionRegistry multiplierRegistry = new([multiplierDefinition]);
using World multiplierSpawnWorld = World.Create();
if (!spawnCommit.TryCommit(
      multiplierSpawnWorld,
      multiplierRegistry,
      new SpawnNpcCommand(
        DefinitionId: multiplierDefinition.DefinitionId,
        Position: new SimulationVector(5.0f, 5.0f),
        Source: NpcSpawnSource.Command),
      replicationId: 9,
      out NpcSpawnCommitResult multiplierSpawn,
      out _))
{
  throw new InvalidOperationException("NPC multiplier definition spawn was rejected.");
}

NpcAuthorityComponent spawnedMultiplierAuthority = multiplierSpawnWorld
  .Get<NpcAuthorityComponent>(multiplierSpawn.Entity);
if (spawnedMultiplierAuthority.TakenDamageMultiplier != 3.0f)
{
  throw new InvalidOperationException(
    "NPC spawn commit did not transfer the source-backed taken-damage multiplier.");
}

NpcReplicationComponent dirtyReplication = new(1, 1);
dirtyReplication.ClearDirty();
dirtyReplication.MarkDirty();
if (!dirtyReplication.IsDirty)
{
  throw new InvalidOperationException("NPC replication dirty state did not preserve update intent.");
}

if (spawnCommit.TryCommit(
      spawnWorld,
      registry,
      validSpawn,
      replicationId: int.MaxValue,
      out _,
      out _))
{
  throw new InvalidOperationException(
    "NPC spawn commit accepted a replication identity that cannot advance.");
}

Console.WriteLine("PASS: NPC spawn rejects max replication identity before allocator overflow");

NpcReplicationSnapshot unrepresentableHealthNpc = new(
  ReplicationId: 1,
  NpcType: 1,
  Position: default,
  Velocity: default,
  Health: int.MaxValue,
  IsActive: true,
  Revision: 1,
  Section: default);
bool rejectedUnrepresentableHealth = false;
try
{
  _ = NpcStateSnapshot.FromReplication(unrepresentableHealthNpc);
}
catch (ArgumentOutOfRangeException)
{
  rejectedUnrepresentableHealth = true;
}

if (!rejectedUnrepresentableHealth)
{
  throw new InvalidOperationException(
    "NPC persistence derived an unrepresentable maximum health value.");
}

Console.WriteLine("PASS: NPC persistence rejects unrepresentable health fallback");

NpcDefinitionRegistry trainingRegistry = new([trainingDummyDefinition]);
if (!spawnCommit.TryCommit(
      spawnWorld,
      trainingRegistry,
      new SpawnNpcCommand(
        DefinitionId: 488,
        Position: new SimulationVector(42.0f, 0.0f),
        Source: NpcSpawnSource.TileEntity,
         RequestedReplicationId: 488,
         GivenName: "Target Dummy",
         CanBeReplaced: true,
         DoesNotCountMe: true),
      replicationId: 488,
      out NpcSpawnCommitResult trainingSpawn,
      out _))
{
  throw new InvalidOperationException("Training Dummy NPC definition was rejected by spawn commit.");
}

NpcAuthorityComponent trainingAuthority = spawnWorld.Get<NpcAuthorityComponent>(trainingSpawn.Entity);
NpcLifecycleComponent trainingLifecycle = spawnWorld.Get<NpcLifecycleComponent>(trainingSpawn.Entity);
if (!trainingLifecycle.CanBeReplaced || !trainingLifecycle.DoesNotCountMe)
{
  throw new InvalidOperationException(
    "NPC spawn did not preserve the explicit lifecycle ownership inputs.");
}
if (trainingAuthority.AiStyle != 92 ||
    !trainingAuthority.IsImmortal ||
    !trainingAuthority.AlwaysReplicate ||
    trainingAuthority.NpcSlotCost != trainingDummyDefinition.NpcSlotCost)
{
  throw new InvalidOperationException("Training Dummy authority flags or slot cost were not committed.");
}

using World weightedSpawnWorld = World.Create();
if (!spawnCommit.TryCommit(
      weightedSpawnWorld,
      new NpcDefinitionRegistry([weightedDefinition]),
      new SpawnNpcCommand(
        DefinitionId: weightedDefinition.DefinitionId,
        Position: new SimulationVector(50.0f, 0.0f),
        Source: NpcSpawnSource.Natural),
      replicationId: 777,
      out NpcSpawnCommitResult weightedSpawn,
      out _) ||
    weightedSpawnWorld.Get<NpcAuthorityComponent>(weightedSpawn.Entity).NpcSlotCost !=
      weightedDefinition.NpcSlotCost)
{
  throw new InvalidOperationException("NPC spawn commit did not propagate the weighted slot cost.");
}

NpcGivenNameComponent trainingName = spawnWorld.Get<NpcGivenNameComponent>(trainingSpawn.Entity);
if (!trainingName.HasGivenName || trainingName.GivenName != "Target Dummy")
{
  throw new InvalidOperationException("NPC spawn command did not preserve its explicit given name.");
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

if (spawnCommit.TryCommit(
      spawnWorld,
      registry,
      validSpawn with { DifficultyScale = float.NaN },
      replicationId: 12,
      out _,
      out _) ||
    spawnCommit.TryCommit(
      spawnWorld,
      registry,
      validSpawn with { Source = (NpcSpawnSource)99 },
      replicationId: 13,
      out _,
      out _) ||
    spawnCommit.TryCommit(
      spawnWorld,
      registry,
      validSpawn with { ReleaseOwner = byte.MaxValue + 1 },
      replicationId: 14,
      out _,
      out _))
{
  throw new InvalidOperationException(
    "NPC spawn commit accepted a forged source, difficulty or release owner.");
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
      NpcFaction.Hostile,
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
        NpcFaction.Hostile,
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
        NpcFaction.Hostile,
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

NpcDefinition defaultLavaDefinition = new(
  DefinitionId: 614,
  NetId: 614,
  MaximumHealth: 100,
  Defense: 0,
  ColliderWidth: 1.0f,
  ColliderHeight: 2.0f,
  BehaviorId: NpcBehaviorId.OrdinaryChase,
  LootTableId: 0);
if (defaultLavaDefinition.IsLavaImmune)
{
  throw new InvalidOperationException(
    "The source-default NPC definition unexpectedly enabled lava immunity.");
}

NpcDefinition lavaImmuneDefinition = new(
  DefinitionId: 690,
  NetId: 690,
  MaximumHealth: 100,
  Defense: 0,
  ColliderWidth: 1.0f,
  ColliderHeight: 2.0f,
  BehaviorId: NpcBehaviorId.OrdinaryChase,
  LootTableId: 0,
  IsLavaImmune: true);
WorldGrid lavaWorld = new(200, 150);
if (!lavaWorld.TrySetLiquid(10, 0, byte.MaxValue, (byte)LiquidType.Lava))
{
  throw new InvalidOperationException("The lava fixture could not be initialized.");
}

using World lavaOwnerWorld = World.Create();
if (!spawnCommit.TryCommit(
      lavaOwnerWorld,
      lavaWorld,
      new NpcDefinitionRegistry([lavaImmuneDefinition]),
      new SpawnNpcCommand(
        lavaImmuneDefinition.DefinitionId,
        new SimulationVector(10.0f, 0.0f),
        NpcSpawnSource.Command),
      replicationId: 1,
      out NpcSpawnCommitResult lavaSpawn,
      out string lavaFailureReason))
{
  throw new InvalidOperationException($"Lava-immune NPC spawn was rejected: {lavaFailureReason}");
}

NpcAuthorityComponent lavaAuthority = lavaOwnerWorld.Get<NpcAuthorityComponent>(lavaSpawn.Entity);
if (!lavaAuthority.IsLavaImmune)
{
  throw new InvalidOperationException(
    "The NPC spawn commit did not preserve the explicit lava-immunity capability.");
}

NpcLavaContactSystem lavaContactSystem = new();
NpcLavaContactCandidate lavaCandidate = new(
  new NpcHandle(1),
  new SimulationVector(10.0f, 0.0f),
  new SimulationComponents.ColliderComponent(1.0f, 2.0f),
  lavaAuthority,
  IsActive: true,
  DoesNotTakeDamage: false,
  Health: 100,
  Immunity: default);
if (lavaContactSystem.ProduceDamageCommands(lavaWorld, [lavaCandidate], damage: 50).Count != 0)
{
  throw new InvalidOperationException(
    "Lava contact produced damage for a lava-immune NPC.");
}

NpcAuthorityComponent ordinaryLavaAuthority = new(0, false, false, isLavaImmune: false);
NpcLavaContactCandidate ordinaryLavaCandidate = lavaCandidate with
{
  Authority = ordinaryLavaAuthority
};
IReadOnlyList<DamageNpcCommand> lavaCommands = lavaContactSystem.ProduceDamageCommands(
  lavaWorld,
  [ordinaryLavaCandidate],
  damage: 50);
if (lavaCommands.Count != 1 || lavaCommands[0].Amount != 50 ||
    lavaCommands[0].SourceKind != NpcDamageSourceKind.Lava)
{
  throw new InvalidOperationException(
    "Lava contact did not produce the bounded typed NPC damage command.");
}

if (lavaContactSystem.ProduceDamageCommands(
      lavaWorld,
      [ordinaryLavaCandidate with
      {
        Position = new SimulationVector(10.0f, 0.0f),
        Collider = new SimulationComponents.ColliderComponent(float.NaN, 2.0f)
      }],
      damage: 50).Count != 0 ||
    lavaContactSystem.ProduceDamageCommands(
      lavaWorld,
      [ordinaryLavaCandidate with { Position = new SimulationVector(12.0f, 0.0f) }],
      damage: 50).Count != 0 ||
    lavaContactSystem.ProduceDamageCommands(
      lavaWorld,
      [ordinaryLavaCandidate with
      {
        Position = new SimulationVector(float.MaxValue, float.MaxValue)
      }],
      damage: 50).Count != 0)
{
  throw new InvalidOperationException(
    "Lava contact accepted invalid geometry or a non-overlapping NPC.");
}

WorldGrid waterWorld = new(200, 150);
if (!waterWorld.TrySetLiquid(10, 0, byte.MaxValue, (byte)LiquidType.Water) ||
    lavaContactSystem.ProduceDamageCommands(
      waterWorld,
      [ordinaryLavaCandidate],
      damage: 50).Count != 0)
{
  throw new InvalidOperationException(
    "Lava contact treated a non-lava liquid as lava damage.");
}

WorldGrid runtimeLavaWorld = new(200, 150);
if (!runtimeLavaWorld.TrySetLiquid(20, 0, byte.MaxValue, (byte)LiquidType.Lava))
{
  throw new InvalidOperationException("The runtime lava fixture could not be initialized.");
}

using DomeSimulation runtimeLavaSimulation = new(runtimeLavaWorld);
NpcHandle runtimeLavaNpc = runtimeLavaSimulation.CreateNpc(new SimulationVector(20.0f, 0.0f));
runtimeLavaSimulation.Tick(new SimulationInputBatch());
if (runtimeLavaSimulation.CreateNpcReplicationSnapshots()
      .Single(snapshot => snapshot.ReplicationId == runtimeLavaNpc.Value)
      .Health != 50)
{
  throw new InvalidOperationException(
    "The authoritative NPC pipeline did not apply bounded lava contact damage.");
}

QueryDescription runtimeLavaQuery = new QueryDescription()
  .WithAll<SimulationComponents.NpcTagComponent, NpcReplicationComponent>();
Entity runtimeLavaEntity = default;
runtimeLavaSimulation.World.Query(
  in runtimeLavaQuery,
  (Entity entity, ref NpcReplicationComponent replication) =>
  {
    if (replication.ReplicationId == runtimeLavaNpc.Value)
    {
      runtimeLavaEntity = entity;
    }
  });
if (!runtimeLavaSimulation.World.IsAlive(runtimeLavaEntity))
{
  throw new InvalidOperationException("The runtime lava fixture lost its NPC entity.");
}

NpcAuthorityComponent runtimeLavaAuthority =
  runtimeLavaSimulation.World.Get<NpcAuthorityComponent>(runtimeLavaEntity);
runtimeLavaSimulation.World.Set(
  runtimeLavaEntity,
  new NpcAuthorityComponent(
    runtimeLavaAuthority.AiStyle,
    runtimeLavaAuthority.IsImmortal,
    runtimeLavaAuthority.AlwaysReplicate,
    runtimeLavaAuthority.TakenDamageMultiplier,
    runtimeLavaAuthority.NpcSlotCost,
    runtimeLavaAuthority.IsTrapImmune,
    isLavaImmune: true));
runtimeLavaSimulation.World.Set(runtimeLavaEntity, new ImmunityComponent());
runtimeLavaSimulation.QueueNpcDamage(new DamageNpcCommand(
  runtimeLavaNpc,
  50,
  SourceKind: NpcDamageSourceKind.Lava));
runtimeLavaSimulation.Tick(new SimulationInputBatch());
if (runtimeLavaSimulation.CreateNpcReplicationSnapshots()
      .Single(snapshot => snapshot.ReplicationId == runtimeLavaNpc.Value)
      .Health != 50)
{
  throw new InvalidOperationException(
    "A forged Lava source handle bypassed the authoritative damage boundary.");
}

Console.WriteLine(
  "PASS: NPC lava-immunity owner, liquid contact producer and settlement gate");

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

NpcLifecycleComponent despawnPolicy = new(isActive: true, timeLeft: 100);
despawnPolicy.EncourageDespawn(10);
despawnPolicy.EncourageDespawn(20);
if (despawnPolicy.TimeLeft != 10 || !despawnPolicy.DespawnEncouraged)
{
  throw new InvalidOperationException(
    "NPC lifecycle did not preserve source-backed despawn encouragement semantics.");
}

despawnPolicy.DiscourageDespawn(20);
despawnPolicy.DiscourageDespawn(5);
if (despawnPolicy.TimeLeft != 20 || despawnPolicy.DespawnEncouraged)
{
  throw new InvalidOperationException(
    "NPC lifecycle did not preserve source-backed despawn discouragement semantics.");
}

despawnPolicy.EncourageDespawn(20);
if (despawnPolicy.TimeLeft != 20 || !despawnPolicy.DespawnEncouraged)
{
  throw new InvalidOperationException(
    "NPC lifecycle did not update the despawn flag at an equal threshold.");
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
      new NpcHandle(1),
      Health: 0,
      WasActive: true,
      new SimulationVector(4.0f, 0.0f),
      LootTableId: 1,
      SpawnedFromStatue: true,
      SuppressLootWhenSpawnedFromStatue: true)).Published ||
    !deathSystem.Evaluate(new NpcDeathInput(
      new NpcHandle(1),
      Health: 0,
      WasActive: true,
      new SimulationVector(4.0f, 0.0f),
      LootTableId: 1,
      SpawnedFromStatue: false,
      SuppressLootWhenSpawnedFromStatue: true)).Published)
{
  throw new InvalidOperationException("Statue-spawn loot suppression did not preserve its scoped boundary.");
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

ItemDefinitionRegistry lootItemDefinitions = new([
  new ItemDefinition(1, 2)]);
try
{
  _ = new NpcLootDefinitionRegistry(
    [new NpcLootDefinition(2, 600, 1, 1)],
    lootItemDefinitions);
  throw new InvalidOperationException(
    "NPC loot registration accepted an item type missing from the authoritative definitions.");
}
catch (ArgumentException)
{
}

try
{
  _ = new NpcLootDefinitionRegistry(
    [new NpcLootDefinition(3, 1, 1, 3)],
    lootItemDefinitions);
  throw new InvalidOperationException(
    "NPC loot registration accepted a stack above the authoritative item limit.");
}
catch (ArgumentException)
{
}

NpcLootSystem lootSystem = new(
  new NpcLootDefinitionRegistry([new NpcLootDefinition(1, 1, 1, 2)], lootItemDefinitions),
  new WorldSeed(1));
if (lootSystem.Roll(1, 5) != lootSystem.Roll(1, 5))
{
  throw new InvalidOperationException("NPC loot was not deterministic for the same seed and identity.");
}

try
{
  _ = lootSystem.Roll(1, 0);
  throw new InvalidOperationException("NPC loot accepted an invalid replication identity.");
}
catch (ArgumentOutOfRangeException)
{
}

NpcLootCommand lootCommand = lootSystem.CreateDrop(new NpcDeathEvent(
  new NpcHandle(5),
  1,
  new SimulationVector(4.0f, 0.0f),
  new WorldSectionCoordinates(0, 0),
  Tick: 3));
if (lootCommand.SourceNpc.Value != 5 || lootCommand.WorldItem.SpawnSource != 5 ||
    lootCommand.WorldItem.Stack != lootSystem.Roll(1, 5) || !lootCommand.IsConsistent)
{
  throw new InvalidOperationException("NPC loot command did not preserve source and deterministic roll.");
}

NpcLootCommand forgedLootCommand = lootCommand with
{
  WorldItem = lootCommand.WorldItem with { SpawnSource = 6 }
};
if (forgedLootCommand.IsConsistent)
{
  throw new InvalidOperationException("NPC loot command accepted a forged world-item source.");
}

NpcLootEmissionLedger lootLedger = new();
if (!lootLedger.TryEmit(
      lootSystem,
      new NpcDeathEvent(
        new NpcHandle(5),
        1,
        new SimulationVector(4.0f, 0.0f),
        new WorldSectionCoordinates(0, 0),
        Tick: 3),
      out _) ||
    lootLedger.TryEmit(
      lootSystem,
      new NpcDeathEvent(
        new NpcHandle(5),
        1,
        new SimulationVector(4.0f, 0.0f),
        new WorldSectionCoordinates(0, 0),
        Tick: 3),
      out _))
{
  throw new InvalidOperationException("NPC loot ledger emitted duplicate death output.");
}

try
{
  _ = lootLedger.TryEmit(
    lootSystem,
    new NpcDeathEvent(
      new NpcHandle(5),
      99,
      new SimulationVector(4.0f, 0.0f),
      new WorldSectionCoordinates(0, 0),
      Tick: 5),
    out _);
  throw new InvalidOperationException("NPC loot ledger did not surface a failed drop roll.");
}
catch (KeyNotFoundException)
{
}

if (lootLedger.TryEmit(
      lootSystem,
      new NpcDeathEvent(
        new NpcHandle(5),
        1,
        new SimulationVector(4.0f, 0.0f),
        new WorldSectionCoordinates(0, 0),
        Tick: 3),
      out _))
{
  throw new InvalidOperationException(
    "NPC loot ledger lost the previous idempotence marker after a failed retry.");
}

if (!lootLedger.TryEmit(
      lootSystem,
      new NpcDeathEvent(
        new NpcHandle(5),
        1,
        new SimulationVector(4.0f, 0.0f),
        new WorldSectionCoordinates(0, 0),
        Tick: 4),
      out _))
{
  throw new InvalidOperationException(
    "NPC loot ledger rejected a later death cycle for the same NPC identity.");
}

try
{
  _ = lootSystem.CreateDrop(new NpcDeathEvent(
    default,
    1,
    new SimulationVector(4.0f, 0.0f),
    new WorldSectionCoordinates(0, 0),
    Tick: 3));
  throw new InvalidOperationException("NPC loot command accepted an invalid death identity.");
}
catch (ArgumentOutOfRangeException)
{
}

try
{
  _ = lootSystem.CreateDrop(new NpcDeathEvent(
    new NpcHandle(5),
    1,
    new SimulationVector(4.0f, 0.0f),
    new WorldSectionCoordinates(0, 0),
    Tick: -1));
  throw new InvalidOperationException("NPC loot command accepted a negative death tick.");
}
catch (ArgumentOutOfRangeException)
{
}

NpcEventSpawnSystem eventSpawnSystem = new();
NpcEventSpawnDefinition eventDefinition = new(2, 1, 2);
NpcEventSpawnTable eventTable = new([
  eventDefinition,
  new NpcEventSpawnDefinition(2, 2, 2)]);
IReadOnlyList<SpawnNpcCommand> eventSpawns = eventSpawnSystem.Evaluate(
  new WorldProgressionState(invasionType: 2, invasionSize: 40),
  eventDefinition,
  [new WorldInvasionPlayerSnapshot(IsActive: true, MaximumHealth: 200)],
  new SimulationVector(10.0f, 1.0f),
  activeNpcCount: 1,
  maximumNpcCount: 2);
if (eventSpawns.Count != 1 || eventSpawns[0].Source != NpcSpawnSource.Event ||
    eventSpawns[0].DefinitionId != 1)
{
  throw new InvalidOperationException("NPC event spawning did not honor the bounded event owner.");
}

if (eventSpawnSystem.Evaluate(
      new WorldProgressionState(invasionType: 2, invasionSize: 40),
      eventDefinition,
      registry,
      [new WorldInvasionPlayerSnapshot(IsActive: true, MaximumHealth: 400)],
      new SimulationVector(10.0f, 1.0f),
      activeNpcCount: 0,
      maximumNpcCount: 2).Count != 2)
{
  throw new InvalidOperationException("NPC event spawning rejected a registered definition.");
}

IReadOnlyList<SpawnNpcCommand> tableSpawns = eventSpawnSystem.EvaluateTable(
  new WorldProgressionState(invasionType: 2, invasionSize: 40),
  eventTable,
  new NpcDefinitionRegistry([
    ordinaryDefinition,
    new NpcDefinition(
      DefinitionId: 2,
      NetId: 2,
      MaximumHealth: 100,
      Defense: 0,
      ColliderWidth: 1.0f,
      ColliderHeight: 2.0f,
      BehaviorId: NpcBehaviorId.OrdinaryChase,
      LootTableId: 1)]),
  [new WorldInvasionPlayerSnapshot(IsActive: true, MaximumHealth: 400)],
  new SimulationVector(10.0f, 1.0f),
  activeNpcCount: 0,
  maximumNpcCount: 3);
if (tableSpawns.Count != 3 || tableSpawns[0].DefinitionId != 1 ||
    tableSpawns[1].DefinitionId != 1 || tableSpawns[2].DefinitionId != 2)
{
  throw new InvalidOperationException("NPC event spawn table did not preserve stable order and budget.");
}

try
{
  _ = new NpcEventSpawnTable([
    eventDefinition,
    eventDefinition]);
  throw new InvalidOperationException("NPC event spawn table accepted a duplicate definition.");
}
catch (ArgumentException)
{
}

NpcEventSpawnDefinition unknownEventDefinition = new(2, 99, 2);
if (eventSpawnSystem.Evaluate(
      new WorldProgressionState(invasionType: 2, invasionSize: 40),
      unknownEventDefinition,
      registry,
      [new WorldInvasionPlayerSnapshot(IsActive: true, MaximumHealth: 400)],
      new SimulationVector(10.0f, 1.0f),
      activeNpcCount: 0,
      maximumNpcCount: 2).Count != 0)
{
  throw new InvalidOperationException("NPC event spawning accepted an unknown definition.");
}

if (eventSpawnSystem.Evaluate(
      new WorldProgressionState(invasionType: 3, invasionSize: 40),
      eventDefinition,
      [new WorldInvasionPlayerSnapshot(IsActive: true, MaximumHealth: 400)],
      new SimulationVector(10.0f, 1.0f),
      activeNpcCount: 0,
      maximumNpcCount: 2).Count != 0 ||
    eventSpawnSystem.Evaluate(
      new WorldProgressionState(invasionType: 2, invasionSize: 40, invasionDelayTicks: 1),
      eventDefinition,
      [new WorldInvasionPlayerSnapshot(IsActive: true, MaximumHealth: 400)],
      new SimulationVector(10.0f, 1.0f),
      activeNpcCount: 0,
      maximumNpcCount: 2).Count != 0 ||
    eventSpawnSystem.Evaluate(
      new WorldProgressionState(invasionType: 2, invasionSize: 40),
      eventDefinition,
      [new WorldInvasionPlayerSnapshot(IsActive: false, MaximumHealth: 400)],
      new SimulationVector(10.0f, 1.0f),
      activeNpcCount: 0,
      maximumNpcCount: 2).Count != 0)
{
  throw new InvalidOperationException("NPC event spawning accepted an invalid world event input.");
}

NpcEscapeSystem escapeSystem = new();
NpcEscapeResult escapeResult = escapeSystem.Evaluate(
  new NpcHandle(3),
  new SimulationVector(2.0f, 0.0f),
  new SimulationVector(0.0f, 0.0f),
  hasTarget: true,
  timeLeft: 10,
  maximumDistance: 20.0f,
  escapeSpeed: 2.0f,
  faction: NpcFaction.Hostile);
if (escapeResult.DespawnCommand.HasValue || escapeResult.Velocity.X <= 0.0f ||
    escapeResult.Facing != 1)
{
  throw new InvalidOperationException("NPC escape behavior did not produce outward movement.");
}

NpcEscapeResult outOfRangeEscape = escapeSystem.Evaluate(
  new NpcHandle(3),
  new SimulationVector(21.0f, 0.0f),
  default,
  hasTarget: true,
  timeLeft: 10,
  maximumDistance: 20.0f,
  escapeSpeed: 2.0f,
  faction: NpcFaction.Hostile);
if (outOfRangeEscape.DespawnCommand?.Reason != NpcDespawnReason.OutOfRange ||
    escapeSystem.Evaluate(
      new NpcHandle(3),
      default,
      default,
      hasTarget: false,
      timeLeft: 0,
      maximumDistance: 20.0f,
      escapeSpeed: 2.0f,
      faction: NpcFaction.Hostile).DespawnCommand?.Reason != NpcDespawnReason.TimedOut)
{
  throw new InvalidOperationException("NPC escape behavior did not emit auditable despawn reasons.");
}
NpcEscapeResult largeCoordinateEscape = escapeSystem.Evaluate(
  new NpcHandle(3),
  new SimulationVector(float.MaxValue, float.MaxValue),
  default,
  hasTarget: true,
  timeLeft: 10,
  maximumDistance: 20.0f,
  escapeSpeed: 2.0f,
  faction: NpcFaction.Hostile);
if (largeCoordinateEscape.DespawnCommand?.Reason != NpcDespawnReason.OutOfRange)
{
  throw new InvalidOperationException("NPC escape did not classify large finite coordinates as out of range.");
}
NpcEscapeResult verticalEscape = escapeSystem.Evaluate(
  new NpcHandle(3),
  new SimulationVector(0.0f, 2.0f),
  new SimulationVector(0.0f, 0.0f),
  hasTarget: true,
  timeLeft: 10,
  maximumDistance: 20.0f,
  escapeSpeed: 2.0f,
  faction: NpcFaction.Hostile);
if (verticalEscape.Facing != 0 || verticalEscape.Velocity.Y <= 0.0f)
{
  throw new InvalidOperationException("NPC vertical escape did not preserve a neutral horizontal facing.");
}

try
{
  _ = escapeSystem.Evaluate(
    new NpcHandle(3),
    new SimulationVector(0.0f, 0.0f),
    new SimulationVector(10.0f, 0.0f),
    hasTarget: true,
    timeLeft: 10,
    maximumDistance: 5.0f,
    escapeSpeed: 1.0f,
    faction: NpcFaction.Town);
  throw new InvalidOperationException("NPC escape accepted a town faction.");
}
catch (ArgumentOutOfRangeException)
{
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
using DomeSimulation definitionSimulation = new();
NpcHandle trainingDummy = definitionSimulation.CreateNpc(
  new SimulationVector(30.0f, 0.0f),
  488);
NpcReplicationSnapshot trainingDummySnapshot = definitionSimulation
  .CreateNpcReplicationSnapshots()
  .Single(snapshot => snapshot.ReplicationId == trainingDummy.Value);
if (trainingDummySnapshot.Health != 1000 ||
    trainingDummySnapshot.NpcType != 488 ||
    trainingDummySnapshot.DefinitionId != 488 ||
    trainingDummySnapshot.MaximumHealth != 1000 ||
    trainingDummySnapshot.Faction != NpcFaction.Neutral ||
    trainingDummySnapshot.Category != NpcCategory.Town)
{
  throw new InvalidOperationException(
    "NPC creation did not project the registered definition into replication.");
}

using DomeSimulation failedCreateSimulation = new();
bool rejectedFailedCreate = false;
try
{
  _ = failedCreateSimulation.CreateNpc(new SimulationVector(float.NaN, 0.0f));
}
catch (InvalidOperationException)
{
  rejectedFailedCreate = true;
}

if (!rejectedFailedCreate)
{
  throw new InvalidOperationException("NPC creation accepted a non-finite spawn position.");
}

NpcHandle retriedNpc = failedCreateSimulation.CreateNpc(new SimulationVector(30.0f, 0.0f));
if (retriedNpc.Value != 1)
{
  throw new InvalidOperationException(
    "Rejected NPC creation consumed an identity before commit.");
}

if (queuedSimulation.QueueNpcEventSpawns(
      new NpcEventSpawnTable([eventDefinition]),
      new SimulationVector(10.0f, 1.0f)) != 0)
{
  throw new InvalidOperationException(
    "NPC event spawn API bypassed the inactive invasion authority gate.");
}

queuedSimulation.QueueNpcSpawn(validSpawn with
{
  RequestedReplicationId = 4,
  Position = new SimulationVector(float.NaN, 0.0f)
});
queuedSimulation.Tick(new SimulationInputBatch());
if (queuedSimulation.NpcSpawnRejectionReasons.Count != 1 ||
    !queuedSimulation.NpcSpawnRejectionReasons[0].Contains("invalid", StringComparison.Ordinal))
{
  throw new InvalidOperationException("Rejected NPC spawn did not publish an auditable reason.");
}

NpcSystemPipeline pipeline = new();
pipeline.ValidateRegistration();
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
if (!queuedReplication.IsActive || queuedReplication.Revision != 1 ||
    queuedReplication.NpcType != ordinaryDefinition.NetId ||
    queuedReplication.DefinitionId != ordinaryDefinition.DefinitionId ||
    queuedReplication.MaximumHealth != ordinaryDefinition.MaximumHealth ||
    queuedReplication.BehaviorId != ordinaryDefinition.BehaviorId ||
    queuedReplication.Faction != ordinaryDefinition.Faction ||
    queuedReplication.Category != ordinaryDefinition.Category)
{
  throw new InvalidOperationException(
    "Queued NPC spawn did not commit a definition-backed active replication.");
}

queuedSimulation.QueueNpcSpawn(validSpawn with
{
  RequestedReplicationId = 6,
  HomelessDespawn = true
});
queuedSimulation.Tick(new SimulationInputBatch());
NpcStateSnapshot homelessState = queuedSimulation.CreateNpcStateSnapshots()
  .Single(snapshot => snapshot.Replication.ReplicationId == 6);
if (!homelessState.Lifecycle.HomelessDespawn)
{
  throw new InvalidOperationException(
    "NPC state snapshot did not retain the explicit homeless-despawn lifecycle flag.");
}
Console.WriteLine(
  "PASS: NPC homeless-despawn lifecycle flag has typed spawn and state-snapshot ownership");

NpcHomeComponent defaultHome = new(
  homeTileX: 0,
  homeTileY: 0,
  isHomeless: false,
  returnTimeoutTicks: 0);
if (defaultHome.LookForHomeTimeout != 0 || defaultHome.IsReadyToLookForHome)
{
  throw new InvalidOperationException(
    "NPC home-search timeout did not default to a non-ready zero state.");
}

bool rejectedNegativeHomeSearchTimeout = false;
try
{
  _ = new NpcHomeComponent(
    homeTileX: 0,
    homeTileY: 0,
    isHomeless: true,
    returnTimeoutTicks: 0,
    lookForHomeTimeout: -1);
}
catch (ArgumentOutOfRangeException)
{
  rejectedNegativeHomeSearchTimeout = true;
}

if (!rejectedNegativeHomeSearchTimeout)
{
  throw new InvalidOperationException(
    "NPC home-search timeout accepted a negative constructor value.");
}

NpcHomeComponent kickedOutHome = new(0, 0, false, 0);
kickedOutHome.MarkKickedOut();
if (!kickedOutHome.IsHomeless ||
    kickedOutHome.LookForHomeTimeout != NpcHomeComponent.KickOutLookForHomeTimeout ||
    kickedOutHome.IsReadyToLookForHome)
{
  throw new InvalidOperationException(
    "NPC kick-out state did not apply the source-backed home-search delay.");
}

NpcHomeComponent movedRoomHome = new(0, 0, false, 0, lookForHomeTimeout: 4);
movedRoomHome.MarkMovedRoom();
if (!movedRoomHome.IsHomeless || movedRoomHome.LookForHomeTimeout != 0 ||
    !movedRoomHome.IsReadyToLookForHome)
{
  throw new InvalidOperationException(
    "NPC move-room state did not produce a homeless zero-timeout boundary.");
}

NpcHomeComponent timeoutHome = new(0, 0, true, 0, lookForHomeTimeout: 2);
NpcHomeTimeoutSystem timeoutSystem = new();
timeoutSystem.Tick(ref timeoutHome);
if (timeoutHome.LookForHomeTimeout != 1 || timeoutHome.IsReadyToLookForHome)
{
  throw new InvalidOperationException(
    "NPC home-search timeout did not decrement a positive value exactly once.");
}

timeoutSystem.Tick(ref timeoutHome);
timeoutSystem.Tick(ref timeoutHome);
if (timeoutHome.LookForHomeTimeout != 0 || !timeoutHome.IsReadyToLookForHome)
{
  throw new InvalidOperationException(
    "NPC home-search timeout did not clamp at zero and publish readiness.");
}

timeoutHome.LookForHomeTimeout = -1;
bool rejectedNegativeRuntimeHomeSearchTimeout = false;
try
{
  timeoutSystem.Tick(ref timeoutHome);
}
catch (ArgumentOutOfRangeException)
{
  rejectedNegativeRuntimeHomeSearchTimeout = true;
}

if (!rejectedNegativeRuntimeHomeSearchTimeout)
{
  throw new InvalidOperationException(
    "NPC home-search timeout system accepted a negative runtime value.");
}

NpcHomeComponent housedHome = new(0, 0, false, 0, lookForHomeTimeout: 0);
if (housedHome.IsReadyToLookForHome)
{
  throw new InvalidOperationException(
    "NPC home-search readiness inferred homelessness from a zero timeout alone.");
}

using DomeSimulation homeTimeoutSimulation = new();
NpcHandle homeTimeoutNpc = homeTimeoutSimulation.CreateNpc(new SimulationVector(30.0f, 0.0f));
Entity homeTimeoutEntity = default;
QueryDescription npcReplicationQuery = new QueryDescription()
  .WithAll<SimulationComponents.NpcTagComponent, NpcReplicationComponent>();
homeTimeoutSimulation.World.Query(
  in npcReplicationQuery,
  (Entity entity, ref NpcReplicationComponent replication) =>
  {
    if (replication.ReplicationId == homeTimeoutNpc.Value)
    {
      homeTimeoutEntity = entity;
    }
  });
if (homeTimeoutEntity == default)
{
  throw new InvalidOperationException("NPC home-search verifier could not locate its entity.");
}

NpcHomeComponent simulationHome = new(0, 0, true, 0, lookForHomeTimeout: 2);
homeTimeoutSimulation.World.Add(homeTimeoutEntity, in simulationHome);
homeTimeoutSimulation.Tick(new SimulationInputBatch());
NpcHomeComponent advancedSimulationHome = homeTimeoutSimulation.World
  .Get<NpcHomeComponent>(homeTimeoutEntity);
if (advancedSimulationHome.LookForHomeTimeout != 1)
{
  throw new InvalidOperationException(
    "Authoritative NPC lifecycle did not tick an attached home-search timeout.");
}

NpcHandle noHomeNpc = homeTimeoutSimulation.CreateNpc(new SimulationVector(32.0f, 0.0f));
Entity noHomeEntity = default;
homeTimeoutSimulation.World.Query(
  in npcReplicationQuery,
  (Entity entity, ref NpcReplicationComponent replication) =>
  {
    if (replication.ReplicationId == noHomeNpc.Value)
    {
      noHomeEntity = entity;
    }
  });
homeTimeoutSimulation.Tick(new SimulationInputBatch());
if (noHomeEntity == default || homeTimeoutSimulation.World.Has<NpcHomeComponent>(noHomeEntity))
{
  throw new InvalidOperationException(
    "NPCs without a home component were assigned inferred home-search state.");
}
NpcStateSnapshot noHomeState = homeTimeoutSimulation.CreateNpcStateSnapshots()
  .Single(snapshot => snapshot.Replication.ReplicationId == noHomeNpc.Value);
if (noHomeState.HasHome || noHomeState.HasHomePublication)
{
  throw new InvalidOperationException(
    "NPC state snapshots inferred Home or publication state for an entity without Home.");
}

NpcHandle inactiveHomeNpc = homeTimeoutSimulation.CreateNpc(new SimulationVector(34.0f, 0.0f));
Entity inactiveHomeEntity = default;
homeTimeoutSimulation.World.Query(
  in npcReplicationQuery,
  (Entity entity, ref NpcReplicationComponent replication) =>
  {
    if (replication.ReplicationId == inactiveHomeNpc.Value)
    {
      inactiveHomeEntity = entity;
    }
  });
NpcHomeComponent inactiveHome = new(0, 0, true, 0, lookForHomeTimeout: 2);
homeTimeoutSimulation.World.Add(inactiveHomeEntity, in inactiveHome);
homeTimeoutSimulation.QueueNpcDespawn(new DespawnNpcCommand(
  inactiveHomeNpc,
  NpcDespawnReason.OutOfRange));
homeTimeoutSimulation.Tick(new SimulationInputBatch());
NpcHomeComponent unchangedInactiveHome = homeTimeoutSimulation.World
  .Get<NpcHomeComponent>(inactiveHomeEntity);
if (unchangedInactiveHome.LookForHomeTimeout != 2)
{
  throw new InvalidOperationException(
    "Inactive NPC home-search timeout advanced outside the authoritative active boundary.");
}

Console.WriteLine(
  "PASS: NPC home-search timeout owner, zero-ready boundary and active lifecycle tick");

NpcHomePublicationComponent initialHomePublication = new();
if (initialHomePublication.LastPublishedHomeTileX != -1 ||
    initialHomePublication.LastPublishedHomeTileY != -1 ||
    initialHomePublication.LastPublishedHomeless)
{
  throw new InvalidOperationException(
    "NPC home publication baseline did not preserve the legacy uninitialized tuple.");
}

NpcHomeComponent publicationHome = new(-1, -1, false, 0);
NpcHomePublicationSystem publicationSystem = new();
if (initialHomePublication.HasPendingPublication(publicationHome))
{
  throw new InvalidOperationException(
    "NPC home publication baseline reported a change for the legacy default tuple.");
}

if (!publicationSystem.ApplyHomeTileState(
      ref publicationHome,
      ref initialHomePublication,
      isHomeless: false,
      homeTileX: 12,
      homeTileY: 8) ||
    publicationHome.HomeTileX != 12 || publicationHome.HomeTileY != 8 ||
    initialHomePublication.HasPendingPublication(publicationHome))
{
  throw new InvalidOperationException(
    "NPC home publication update did not compare before writing and capture the new tuple.");
}

if (publicationSystem.ApplyHomeTileState(
      ref publicationHome,
      ref initialHomePublication,
      isHomeless: false,
      homeTileX: 12,
      homeTileY: 8))
{
  throw new InvalidOperationException(
    "NPC home publication update reported an unchanged home tuple as changed.");
}

if (!publicationSystem.ApplyHomeTileState(
      ref publicationHome,
      ref initialHomePublication,
      isHomeless: false,
      homeTileX: 12,
      homeTileY: 9) ||
    !publicationSystem.ApplyHomeTileState(
      ref publicationHome,
      ref initialHomePublication,
      isHomeless: true,
      homeTileX: 12,
      homeTileY: 9) ||
    !publicationHome.IsHomeless || publicationHome.HomeTileY != 9)
{
  throw new InvalidOperationException(
    "NPC home publication update did not distinguish coordinate and homeless tuples.");
}

publicationHome.HomeTileX = 13;
if (!initialHomePublication.HasPendingPublication(publicationHome))
{
  throw new InvalidOperationException(
    "NPC home publication baseline did not detect a changed home coordinate.");
}

publicationSystem.CapturePublishedState(ref initialHomePublication, publicationHome);
if (initialHomePublication.HasPendingPublication(publicationHome) ||
    initialHomePublication.LastPublishedHomeTileX != 13)
{
  throw new InvalidOperationException(
    "NPC home publication capture did not clear the pending baseline change.");
}

using DomeSimulation publicationSimulation = new();
NpcHandle publicationNpc = publicationSimulation.CreateNpc(new SimulationVector(36.0f, 0.0f));
Entity publicationEntity = default;
publicationSimulation.World.Query(
  in npcReplicationQuery,
  (Entity entity, ref NpcReplicationComponent replication) =>
  {
    if (replication.ReplicationId == publicationNpc.Value)
    {
      publicationEntity = entity;
    }
  });
NpcHomeComponent persistedHome = new(12, 8, true, 0);
NpcHomePublicationComponent persistedPublication = new(12, 8, false);
publicationSimulation.World.Add(publicationEntity, in persistedHome);
publicationSimulation.World.Add(publicationEntity, in persistedPublication);
WorldMetadata publicationMetadata = new(
  "npc-home-publication",
  new WorldSeed(17),
  4200,
  1200);
DomeSimulationSnapshot publicationSnapshot = publicationSimulation.CreatePersistenceSnapshot(
  publicationMetadata);
NpcStateSnapshot persistedState = publicationSnapshot.NpcStates
  .Single(snapshot => snapshot.Replication.ReplicationId == publicationNpc.Value);
if (!persistedState.HasHome || !persistedState.HasHomePublication ||
    persistedState.HomePublication.LastPublishedHomeTileX != 12 ||
    persistedState.HomePublication.LastPublishedHomeTileY != 8 ||
    persistedState.HomePublication.LastPublishedHomeless)
{
  throw new InvalidOperationException(
    "NPC home publication baseline was not retained in the authoritative state snapshot.");
}

using DomeSimulation restoredPublicationSimulation = new(publicationSnapshot);
NpcStateSnapshot restoredPublicationState = restoredPublicationSimulation
  .CreateNpcStateSnapshots()
  .Single(snapshot => snapshot.Replication.ReplicationId == publicationNpc.Value);
if (!restoredPublicationState.HasHomePublication ||
    restoredPublicationState.HomePublication.LastPublishedHomeTileX !=
      persistedState.HomePublication.LastPublishedHomeTileX ||
    restoredPublicationState.HomePublication.LastPublishedHomeTileY !=
      persistedState.HomePublication.LastPublishedHomeTileY ||
    restoredPublicationState.HomePublication.LastPublishedHomeless !=
      persistedState.HomePublication.LastPublishedHomeless)
{
  throw new InvalidOperationException(
    "NPC home publication baseline did not survive state snapshot restore.");
}

NpcStateSnapshot invalidPublicationState = persistedState with
{
  HasHome = false,
  Home = default
};
try
{
  _ = new DomeSimulation(new DomeSimulationSnapshot(
    publicationSnapshot.World,
    [invalidPublicationState.Replication],
    publicationSnapshot.WorldItems,
    publicationSnapshot.TickNumber,
    npcStates: [invalidPublicationState]));
  throw new InvalidOperationException(
    "NPC restore accepted a publication sidecar without an explicit Home component.");
}
catch (ArgumentException)
{
}

Console.WriteLine(
  "PASS: NPC home publication baseline owner and authoritative state round-trip");

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

SimulationEntityLimits singleNpcLimits = new(MaximumNpcs: 1);
using DomeSimulation inactiveSlotReuseSimulation = new(
  new WorldGrid(200, 150),
  new WorldSeed(1),
  singleNpcLimits);
NpcHandle inactiveSlot = inactiveSlotReuseSimulation.CreateNpc(
  new SimulationVector(80.0f, 0.0f));
inactiveSlotReuseSimulation.QueueNpcDespawn(new DespawnNpcCommand(
  inactiveSlot,
  NpcDespawnReason.OutOfRange));
inactiveSlotReuseSimulation.Tick(new SimulationInputBatch());
NpcReplicationSnapshot inactiveTombstone = inactiveSlotReuseSimulation
  .CreateNpcReplicationSnapshots()
  .Single(snapshot => snapshot.ReplicationId == inactiveSlot.Value);
NpcHandle reusedInactiveSlot = inactiveSlotReuseSimulation.CreateNpc(
  new SimulationVector(90.0f, 0.0f),
  definitionId: 614);
NpcReplicationSnapshot reusedInactiveReplication = inactiveSlotReuseSimulation
  .CreateNpcReplicationSnapshots()
  .Single(snapshot => snapshot.ReplicationId == inactiveSlot.Value);
if (reusedInactiveSlot != inactiveSlot ||
    !reusedInactiveReplication.IsActive ||
    reusedInactiveReplication.Revision <= inactiveTombstone.Revision ||
    reusedInactiveReplication.NpcType != 614 ||
    inactiveSlotReuseSimulation.NpcCount != 1)
{
  throw new InvalidOperationException(
    "NPC creation did not reuse an inactive slot with a new active revision.");
}

using DomeSimulation activeSlotReuseSimulation = new(
  new WorldGrid(200, 150),
  new WorldSeed(1),
  singleNpcLimits);
activeSlotReuseSimulation.QueueNpcSpawn(new SpawnNpcCommand(
  DefinitionId: 1,
  Position: new SimulationVector(100.0f, 0.0f),
  Source: NpcSpawnSource.Command,
  CanBeReplaced: true));
activeSlotReuseSimulation.Tick(new SimulationInputBatch());
NpcReplicationSnapshot replaceableReplication = activeSlotReuseSimulation
  .CreateNpcReplicationSnapshots()
  .Single();
activeSlotReuseSimulation.QueueNpcSpawn(new SpawnNpcCommand(
  DefinitionId: 614,
  Position: new SimulationVector(110.0f, 0.0f),
  Source: NpcSpawnSource.Command));
activeSlotReuseSimulation.Tick(new SimulationInputBatch());
NpcReplicationSnapshot activeSlotReplacement = activeSlotReuseSimulation
  .CreateNpcReplicationSnapshots()
  .Single();
if (activeSlotReplacement.ReplicationId != replaceableReplication.ReplicationId ||
    activeSlotReplacement.Revision <= replaceableReplication.Revision ||
    !activeSlotReplacement.IsActive ||
    activeSlotReplacement.NpcType != 614 ||
    activeSlotReuseSimulation.NpcCount != 1)
{
  throw new InvalidOperationException(
    "NPC spawn did not reuse the lowest active slot marked replaceable.");
}

using DomeSimulation protectedSlotSimulation = new(
  new WorldGrid(200, 150),
  new WorldSeed(1),
  singleNpcLimits);
protectedSlotSimulation.CreateNpc(new SimulationVector(120.0f, 0.0f));
protectedSlotSimulation.QueueNpcSpawn(new SpawnNpcCommand(
  DefinitionId: 614,
  Position: new SimulationVector(130.0f, 0.0f),
  Source: NpcSpawnSource.Command));
protectedSlotSimulation.Tick(new SimulationInputBatch());
if (protectedSlotSimulation.NpcCount != 1 ||
    protectedSlotSimulation.NpcSpawnRejectionReasons.Count != 1)
{
  throw new InvalidOperationException(
    "NPC spawn replaced a non-replaceable active slot or ignored capacity.");
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

using DomeSimulation hostileDamageSimulation = new();
NpcHandle hostileSource = hostileDamageSimulation.CreateNpc(
  new SimulationVector(20.0f, 0.0f),
  definitionId: 1);
hostileDamageSimulation.QueueNpcSpawn(new SpawnNpcCommand(
  DefinitionId: 1,
  Position: new SimulationVector(40.0f, 0.0f),
  Source: NpcSpawnSource.Command,
  RequestedReplicationId: 2,
  DoesNotTakeDamageFromHostiles: true));
hostileDamageSimulation.Tick(new SimulationInputBatch());
NpcHandle immuneTarget = new(2);
NpcReplicationSnapshot immuneBefore = hostileDamageSimulation
  .CreateNpcReplicationSnapshots()
  .Single(snapshot => snapshot.ReplicationId == immuneTarget.Value);
if (!hostileDamageSimulation.CreateNpcStateSnapshots()
      .Single(snapshot => snapshot.Replication.ReplicationId == immuneTarget.Value)
      .Behavior.DoesNotTakeDamageFromHostiles)
{
  throw new InvalidOperationException(
    "NPC spawn did not initialize the typed hostile-damage immunity state.");
}

hostileDamageSimulation.QueueHostileNpcDamage(hostileSource, immuneTarget, 10);
hostileDamageSimulation.Tick(new SimulationInputBatch());
NpcReplicationSnapshot immuneAfterHostile = hostileDamageSimulation
  .CreateNpcReplicationSnapshots()
  .Single(snapshot => snapshot.ReplicationId == immuneTarget.Value);
if (immuneAfterHostile.Health != immuneBefore.Health)
{
  throw new InvalidOperationException(
    "Hostile NPC damage bypassed the target hostile-damage immunity gate.");
}

hostileDamageSimulation.QueueNpcDamage(immuneTarget, 10);
hostileDamageSimulation.Tick(new SimulationInputBatch());
NpcReplicationSnapshot immuneAfterExternal = hostileDamageSimulation
  .CreateNpcReplicationSnapshots()
  .Single(snapshot => snapshot.ReplicationId == immuneTarget.Value);
if (immuneAfterExternal.Health >= immuneAfterHostile.Health)
{
  throw new InvalidOperationException(
    "External NPC damage was incorrectly blocked by hostile-damage immunity.");
}

WorldMetadata hostileDamageMetadata = new(
  "npc-hostile-damage",
  new WorldSeed(1),
  4200,
  1200);
DomeSimulationSnapshot hostileDamagePersistence = hostileDamageSimulation
  .CreatePersistenceSnapshot(hostileDamageMetadata);
NpcStateSnapshot persistedImmuneTarget = hostileDamagePersistence.NpcStates
  .Single(snapshot => snapshot.Replication.ReplicationId == immuneTarget.Value);
if (!persistedImmuneTarget.Behavior.DoesNotTakeDamageFromHostiles)
{
  throw new InvalidOperationException(
    "NPC persistence dropped the hostile-damage immunity state.");
}

using DomeSimulation restoredHostileDamage = new(hostileDamagePersistence);
if (!restoredHostileDamage.CreateNpcStateSnapshots()
      .Single(snapshot => snapshot.Replication.ReplicationId == immuneTarget.Value)
      .Behavior.DoesNotTakeDamageFromHostiles)
{
  throw new InvalidOperationException(
    "Restored NPC persistence did not retain hostile-damage immunity.");
}

using DomeSimulation factionDamageSimulation = new();
NpcHandle townSource = factionDamageSimulation.CreateNpc(
  new SimulationVector(20.0f, 0.0f),
  definitionId: DomeSimulation.FixtureNpcType);
NpcHandle factionTarget = factionDamageSimulation.CreateNpc(
  new SimulationVector(40.0f, 0.0f),
  definitionId: 1);
int factionTargetHealth = factionDamageSimulation.CreateNpcReplicationSnapshots()
  .Single(snapshot => snapshot.ReplicationId == factionTarget.Value)
  .Health;
factionDamageSimulation.QueueHostileNpcDamage(townSource, factionTarget, 10);
factionDamageSimulation.Tick(new SimulationInputBatch());
if (factionDamageSimulation.CreateNpcReplicationSnapshots()
      .Single(snapshot => snapshot.ReplicationId == factionTarget.Value)
      .Health != factionTargetHealth)
{
  throw new InvalidOperationException(
    "A non-hostile NPC source was admitted to the hostile damage path.");
}

using DomeSimulation staleSourceDamageSimulation = new();
NpcHandle staleSource = staleSourceDamageSimulation.CreateNpc(
  new SimulationVector(20.0f, 0.0f),
  definitionId: 1);
NpcHandle staleSourceTarget = staleSourceDamageSimulation.CreateNpc(
  new SimulationVector(40.0f, 0.0f),
  definitionId: 1);
int staleTargetHealth = staleSourceDamageSimulation.CreateNpcReplicationSnapshots()
  .Single(snapshot => snapshot.ReplicationId == staleSourceTarget.Value)
  .Health;
staleSourceDamageSimulation.QueueNpcDespawn(new DespawnNpcCommand(
  staleSource,
  NpcDespawnReason.OutOfRange));
staleSourceDamageSimulation.Tick(new SimulationInputBatch());
staleSourceDamageSimulation.QueueHostileNpcDamage(staleSource, staleSourceTarget, 10);
staleSourceDamageSimulation.Tick(new SimulationInputBatch());
if (staleSourceDamageSimulation.CreateNpcReplicationSnapshots()
      .Single(snapshot => snapshot.ReplicationId == staleSourceTarget.Value)
      .Health != staleTargetHealth)
{
  throw new InvalidOperationException(
    "An inactive NPC source was admitted after settlement-time identity revalidation.");
}

using DomeSimulation invalidKindDamageSimulation = new();
NpcHandle invalidKindSource = invalidKindDamageSimulation.CreateNpc(
  new SimulationVector(20.0f, 0.0f),
  definitionId: 1);
NpcHandle invalidKindTarget = invalidKindDamageSimulation.CreateNpc(
  new SimulationVector(40.0f, 0.0f),
  definitionId: 1);
int invalidKindTargetHealth = invalidKindDamageSimulation.CreateNpcReplicationSnapshots()
  .Single(snapshot => snapshot.ReplicationId == invalidKindTarget.Value)
  .Health;
invalidKindDamageSimulation.QueueNpcDamage(new DamageNpcCommand(
  invalidKindTarget,
  10,
  SourceKind: (NpcDamageSourceKind)99,
  SourceNpc: invalidKindSource));
invalidKindDamageSimulation.Tick(new SimulationInputBatch());
if (invalidKindDamageSimulation.CreateNpcReplicationSnapshots()
      .Single(snapshot => snapshot.ReplicationId == invalidKindTarget.Value)
      .Health != invalidKindTargetHealth)
{
  throw new InvalidOperationException(
    "An undefined NPC damage source kind was admitted at settlement.");
}

try
{
  invalidKindDamageSimulation.QueueHostileNpcDamage(
    new NpcHandle(999),
    invalidKindTarget,
    10);
  throw new InvalidOperationException("NPC hostile damage accepted a missing source handle.");
}
catch (ArgumentException)
{
}

Console.WriteLine(
  "PASS: NPC hostile-damage source identity, immunity gate, external damage and persistence");

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

using DomeSimulation invasionDeathSimulation = new();
if (!invasionDeathSimulation.TryQueueWorldInvasion(
      new WorldInvasionStartCommand(Type: 1, Size: 10, Sequence: 1)))
{
  throw new InvalidOperationException("NPC invasion-death fixture could not start an invasion.");
}

invasionDeathSimulation.Tick(new SimulationInputBatch());
NpcHandle invasionNpcA = invasionDeathSimulation.CreateNpc(
  new SimulationVector(30.0f, 0.0f),
  definitionId: 26);
NpcHandle invasionNpcB = invasionDeathSimulation.CreateNpc(
  new SimulationVector(40.0f, 0.0f),
  definitionId: 26);
invasionDeathSimulation.QueueNpcDamage(invasionNpcA, 100);
invasionDeathSimulation.QueueNpcDamage(invasionNpcB, 100);
invasionDeathSimulation.Tick(new SimulationInputBatch());
if (invasionDeathSimulation.CreateWorldProgressionSnapshot().InvasionSize != 10)
{
  throw new InvalidOperationException(
    "NPC invasion progress was committed before the queued death command settled.");
}

invasionDeathSimulation.Tick(new SimulationInputBatch());
if (invasionDeathSimulation.CreateWorldProgressionSnapshot().InvasionSize != 8)
{
  throw new InvalidOperationException(
    "NPC deaths did not commit one invasion-progress command per slain NPC.");
}

using DomeSimulation mismatchInvasionSimulation = new();
if (!mismatchInvasionSimulation.TryQueueWorldInvasion(
      new WorldInvasionStartCommand(Type: 2, Size: 10, Sequence: 1)))
{
  throw new InvalidOperationException("NPC mismatch invasion fixture could not start an invasion.");
}

mismatchInvasionSimulation.Tick(new SimulationInputBatch());
NpcHandle mismatchNpc = mismatchInvasionSimulation.CreateNpc(
  new SimulationVector(30.0f, 0.0f),
  definitionId: 26);
mismatchInvasionSimulation.QueueNpcDamage(mismatchNpc, 100);
mismatchInvasionSimulation.Tick(new SimulationInputBatch());
mismatchInvasionSimulation.Tick(new SimulationInputBatch());
if (mismatchInvasionSimulation.CreateWorldProgressionSnapshot().InvasionSize != 10)
{
  throw new InvalidOperationException(
    "NPC deaths from a mismatched invasion group changed invasion progress.");
}

using DomeSimulation noInvasionSimulation = new();
NpcHandle noInvasionNpc = noInvasionSimulation.CreateNpc(
  new SimulationVector(30.0f, 0.0f),
  definitionId: 26);
noInvasionSimulation.QueueNpcDamage(noInvasionNpc, 100);
noInvasionSimulation.Tick(new SimulationInputBatch());
noInvasionSimulation.Tick(new SimulationInputBatch());
if (noInvasionSimulation.CreateWorldProgressionSnapshot().InvasionSize != 0)
{
  throw new InvalidOperationException("NPC death created invasion progress without an active invasion.");
}

Console.WriteLine("PASS: NPC death pipeline queues invasion progress with unique sequences");

static void VerifyNpcSnapshotIdentityOverflow()
{
  WorldGrid grid = new(400, 300);
  WorldMetadata metadata = new("npc-id-restore", new WorldSeed(4), 400, 300);
  NpcReplicationSnapshot validNpc = new(
    ReplicationId: 1,
    NpcType: 1,
    Position: new SimulationVector(20.0f, 20.0f),
    Velocity: default,
    Health: 100,
    IsActive: true,
    Revision: 1,
    Section: grid.GetSectionCoordinates(20, 20),
    DefinitionId: 1,
    MaximumHealth: 100);
  DomeSimulationSnapshot validSnapshot = new(
    grid.CreateSnapshot(metadata),
    [validNpc],
    [],
    tickNumber: 0);
  using DomeSimulation restored = new(validSnapshot);
  if (restored.CreateNpcReplicationSnapshots().Count != 1)
  {
    throw new InvalidOperationException(
      "A valid NPC persistence snapshot did not restore its NPC set.");
  }

  DomeSimulationSnapshot overflowSnapshot = new(
    grid.CreateSnapshot(metadata),
    [validNpc with { ReplicationId = int.MaxValue }],
    [],
    tickNumber: 0);
  bool rejectedNpcIdOverflow = false;
  try
  {
    _ = new DomeSimulation(overflowSnapshot);
  }
  catch (ArgumentOutOfRangeException)
  {
    rejectedNpcIdOverflow = true;
  }

  if (!rejectedNpcIdOverflow)
  {
    throw new InvalidOperationException(
      "Persistence accepted an NPC ID that would overflow the next allocator.");
  }

  Console.WriteLine("PASS: NPC persistence rejects max IDs before allocator overflow");

  DomeSimulationSnapshot negativeRevisionSnapshot = new(
    grid.CreateSnapshot(metadata),
    [validNpc with { Revision = -1 }],
    [],
    tickNumber: 0);
  bool rejectedNegativeRevision = false;
  try
  {
    _ = new DomeSimulation(negativeRevisionSnapshot);
  }
  catch (ArgumentOutOfRangeException)
  {
    rejectedNegativeRevision = true;
  }

  if (!rejectedNegativeRevision)
  {
    throw new InvalidOperationException(
      "Persistence accepted an NPC revision below zero.");
  }

  Console.WriteLine("PASS: NPC persistence rejects negative revisions");

  DomeSimulationSnapshot revisionOverflowSnapshot = new(
    grid.CreateSnapshot(metadata),
    [validNpc with { Revision = long.MaxValue }],
    [],
    tickNumber: 0);
  bool rejectedRevisionOverflow = false;
  try
  {
    _ = new DomeSimulation(revisionOverflowSnapshot);
  }
  catch (ArgumentOutOfRangeException)
  {
    rejectedRevisionOverflow = true;
  }

  if (!rejectedRevisionOverflow)
  {
    throw new InvalidOperationException(
      "Persistence accepted a revision that cannot advance.");
  }

  Console.WriteLine("PASS: NPC persistence rejects max revision before overflow");
}

static void VerifyLegacyAiStyle2Type501Explosion()
{
  using World behaviorWorld = World.Create();
  Entity behaviorProjectile = behaviorWorld.Create(
    new SimulationComponents.TransformComponent(0.0f, 0.0f),
    new SimulationComponents.VelocityComponent(10.0f, 0.0f),
    new SimulationComponents.ProjectileBehaviorComponent(9, default));
  ProjectileBehaviorSystem behaviorSystem = ProjectileBehaviorSystem.CreateDefault();
  for (int tick = 1; tick < 18; tick++)
  {
    _ = behaviorSystem.TryAdvance(behaviorProjectile, behaviorWorld, tick, out _);
  }

  SimulationComponents.VelocityComponent beforeGravity =
    behaviorWorld.Get<SimulationComponents.VelocityComponent>(behaviorProjectile);
  if (beforeGravity.X != 10.0f || beforeGravity.Y != 0.0f ||
      !behaviorSystem.TryAdvance(behaviorProjectile, behaviorWorld, 18, out _))
  {
    throw new InvalidOperationException(
      "Type 501 did not preserve the source eighteen-tick gravity boundary.");
  }

  SimulationComponents.VelocityComponent afterGravity =
    behaviorWorld.Get<SimulationComponents.VelocityComponent>(behaviorProjectile);
  if (MathF.Abs(afterGravity.X - 9.95f) > 0.0001f ||
      MathF.Abs(afterGravity.Y - 0.2f) > 0.0001f)
  {
    throw new InvalidOperationException(
      "Type 501 did not apply the source gravity and drag values at tick eighteen.");
  }

  NpcRangedAttackCommitSystem rangedCommit = new();
  NpcRangedAttackCommand attack = new(
    new NpcHandle(1),
    Sequence: 1,
    ProjectileType: 501,
    Damage: 10,
    Position: new SimulationVector(20.0f, 20.0f),
    Velocity: new SimulationVector(0.0f, 0.0f));
  if (!rangedCommit.TryCommit(
        attack,
        ProjectileDefinitionRegistry.CreateDefault(),
        out NpcProjectileSpawnRequest request))
  {
    throw new InvalidOperationException(
      "NPC ranged commit rejected the source-backed type 501 projectile definition.");
  }

  using DomeSimulation simulation = new(new WorldGrid(400, 300));
  PlayerHandle target = simulation.CreatePlayer(new SimulationVector(23.0f, 20.0f));
  NpcHandle source = simulation.CreateNpc(new SimulationVector(20.0f, 20.0f));
  _ = simulation.CreateNpcProjectile(request with
  {
    SourceNpc = source,
    Definition = request.Definition with { LifetimeTicks = 1 }
  });
  simulation.Tick(new SimulationInputBatch());

  NpcProjectileReplicationSnapshot projectile = simulation
    .CreateNpcProjectileReplicationSnapshots()
    .Single();
  if (projectile.IsActive || projectile.TombstoneReason != ProjectileTombstoneReason.Expired ||
      simulation.CreatePlayerStateSnapshot(target).Health != 90)
  {
    throw new InvalidOperationException(
      "Type 501 expiry did not apply its authoritative on-despawn area damage.");
  }
}

static void VerifyLegacyAiStyle2Type240Explosion()
{
  using World behaviorWorld = World.Create();
  Entity behaviorProjectile = behaviorWorld.Create(
    new SimulationComponents.TransformComponent(0.0f, 0.0f),
    new SimulationComponents.VelocityComponent(10.0f, 0.0f),
    new SimulationComponents.ProjectileBehaviorComponent(10, default));
  ProjectileBehaviorSystem behaviorSystem = ProjectileBehaviorSystem.CreateDefault();
  for (int tick = 1; tick < 16; tick++)
  {
    _ = behaviorSystem.TryAdvance(behaviorProjectile, behaviorWorld, tick, out _);
  }

  SimulationComponents.VelocityComponent beforeGravity =
    behaviorWorld.Get<SimulationComponents.VelocityComponent>(behaviorProjectile);
  if (beforeGravity.X != 10.0f || beforeGravity.Y != 0.0f ||
      !behaviorSystem.TryAdvance(behaviorProjectile, behaviorWorld, 16, out _))
  {
    throw new InvalidOperationException(
      "Type 240 did not preserve the source sixteen-tick gravity boundary.");
  }

  SimulationComponents.VelocityComponent afterGravity =
    behaviorWorld.Get<SimulationComponents.VelocityComponent>(behaviorProjectile);
  if (MathF.Abs(afterGravity.X - 9.91f) > 0.0001f ||
      MathF.Abs(afterGravity.Y - 0.18f) > 0.0001f)
  {
    throw new InvalidOperationException(
      "Type 240 did not apply the source gravity and drag values at tick sixteen.");
  }

  NpcRangedAttackCommitSystem rangedCommit = new();
  NpcRangedAttackCommand attack = new(
    new NpcHandle(1),
    Sequence: 1,
    ProjectileType: 240,
    Damage: 10,
    Position: new SimulationVector(20.0f, 20.0f),
    Velocity: new SimulationVector(0.0f, 0.0f));
  if (!rangedCommit.TryCommit(
        attack,
        ProjectileDefinitionRegistry.CreateDefault(),
        out NpcProjectileSpawnRequest request))
  {
    throw new InvalidOperationException(
      "NPC ranged commit rejected the source-backed type 240 projectile definition.");
  }

  using DomeSimulation simulation = new(new WorldGrid(400, 300));
  PlayerHandle target = simulation.CreatePlayer(new SimulationVector(23.0f, 20.0f));
  NpcHandle source = simulation.CreateNpc(new SimulationVector(20.0f, 20.0f));
  _ = simulation.CreateNpcProjectile(request with
  {
    SourceNpc = source,
    Definition = request.Definition with { LifetimeTicks = 1 }
  });
  simulation.Tick(new SimulationInputBatch());

  NpcProjectileReplicationSnapshot projectile = simulation
    .CreateNpcProjectileReplicationSnapshots()
    .Single();
  if (projectile.IsActive || projectile.TombstoneReason != ProjectileTombstoneReason.Expired ||
      simulation.CreatePlayerStateSnapshot(target).Health != 90)
  {
    throw new InvalidOperationException(
      "Type 240 expiry did not apply its authoritative on-despawn area damage.");
  }
}

static void VerifyNpcCheckActivePixelRangePolicy()
{
  NpcActivityRangeDefinition sourceRange = NpcActivityRangeDefinition.Version1456;
  if (sourceRange.ActiveRangeX != 4032 || sourceRange.ActiveRangeY != 2520 ||
      sourceRange.ScreenWidth != 1920 || sourceRange.ScreenHeight != 1200)
  {
    throw new InvalidOperationException(
      "NPC CheckActive pixel range constants did not preserve the source values.");
  }

  NpcActivityRangeDefinition testRange = new(
    activeRangeX: 10,
    activeRangeY: 10,
    screenWidth: 100,
    screenHeight: 100);
  NpcPixelBounds npc = new(x: 50.0f, y: 50.0f, width: 10, height: 10);
  NpcActivityRangeDecision decision = NpcActivityRangePolicy.Evaluate(
    npc,
    testRange,
    [
      new NpcPlayerActivity(
        IsActive: true,
        new NpcPixelRectangle(50, 50, 2, 2)),
      new NpcPlayerActivity(
        IsActive: true,
        new NpcPixelRectangle(100, 50, 2, 2)),
      new NpcPlayerActivity(
        IsActive: false,
        new NpcPixelRectangle(55, 55, 2, 2)),
      new NpcPlayerActivity(
        IsActive: true,
        new NpcPixelRectangle(116, 50, 2, 2)),
      new NpcPlayerActivity(
        IsActive: true,
        new NpcPixelRectangle(65, 50, 2, 2))
    ]);
  if (!decision.HasActivePlayerInRange || !decision.ShouldRefreshInactivityTimer)
  {
    throw new InvalidOperationException(
      "NPC CheckActive pixel rectangles did not preserve active and screen range intersections.");
  }

  NpcActivityRangeDecision noIntersection = NpcActivityRangePolicy.Evaluate(
    npc,
    testRange,
    [new NpcPlayerActivity(true, new NpcPixelRectangle(116, 116, 2, 2))]);
  if (noIntersection.HasActivePlayerInRange || noIntersection.ShouldRefreshInactivityTimer)
  {
    throw new InvalidOperationException(
      "NPC CheckActive pixel range policy accepted a non-intersecting player.");
  }

  bool invalidBoundsRejected = false;
  try
  {
    _ = new NpcPixelBounds(float.NaN, 0.0f, 1, 1);
  }
  catch (ArgumentOutOfRangeException)
  {
    invalidBoundsRejected = true;
  }

  if (!invalidBoundsRejected)
  {
    throw new InvalidOperationException(
      "NPC CheckActive pixel bounds accepted a non-finite position.");
  }

  Console.WriteLine("PASS: NPC CheckActive pixel-range policy");
}

static void VerifyNpcCheckActiveSlotContributionPolicy()
{
  NpcActivitySlotContributionInput ordinaryInput = new(
    NpcType: 1,
    LifeMaximum: 100,
    ReleaseOwner: NpcActivitySlotContributionPolicy.UnownedReleaseOwner,
    NpcSlotCost: 1.0f,
    IsSlimeRainActive: false,
    IsNpcActive: true,
    IsPlayerInActiveRange: true);
  NpcActivitySlotContribution ordinary = NpcActivitySlotContributionPolicy.Evaluate(ordinaryInput);
  if (!ordinary.ShouldContribute || ordinary.SlotWeight != 1.0f)
  {
    throw new InvalidOperationException(
      "NPC CheckActive slot contribution did not preserve the ordinary source weight.");
  }

  NpcActivitySlotContribution slimeRain = NpcActivitySlotContributionPolicy.Evaluate(
    ordinaryInput with { IsSlimeRainActive = true });
  if (!slimeRain.ShouldContribute || MathF.Abs(slimeRain.SlotWeight - 0.65f) > 0.0001f)
  {
    throw new InvalidOperationException(
      "NPC CheckActive slot contribution did not apply the source Slime Rain multiplier.");
  }

  NpcActivitySlotContribution unregisteredSlimeRain = NpcActivitySlotContributionPolicy.Evaluate(
    ordinaryInput with { NpcType = 2, IsSlimeRainActive = true });
  if (!unregisteredSlimeRain.ShouldContribute || unregisteredSlimeRain.SlotWeight != 1.0f)
  {
    throw new InvalidOperationException(
      "NPC CheckActive slot contribution scaled an unregistered Slime Rain type.");
  }

  NpcActivitySlotContribution[] excluded =
  [
    NpcActivitySlotContributionPolicy.Evaluate(ordinaryInput with { NpcType = 25 }),
    NpcActivitySlotContributionPolicy.Evaluate(ordinaryInput with { NpcType = 30 }),
    NpcActivitySlotContributionPolicy.Evaluate(ordinaryInput with { NpcType = 33 })
  ];
  if (excluded.Any(contribution => contribution.ShouldContribute || contribution.SlotWeight != 0.0f))
  {
    throw new InvalidOperationException(
      "NPC CheckActive slot contribution accepted one of the source-excluded types.");
  }

  NpcActivitySlotContribution[] ineligible =
  [
    NpcActivitySlotContributionPolicy.Evaluate(ordinaryInput with { ReleaseOwner = 4 }),
    NpcActivitySlotContributionPolicy.Evaluate(ordinaryInput with { LifeMaximum = 0 }),
    NpcActivitySlotContributionPolicy.Evaluate(ordinaryInput with { IsNpcActive = false }),
    NpcActivitySlotContributionPolicy.Evaluate(ordinaryInput with { IsPlayerInActiveRange = false })
  ];
  if (ineligible.Any(contribution => contribution.ShouldContribute || contribution.SlotWeight != 0.0f))
  {
    throw new InvalidOperationException(
      "NPC CheckActive slot contribution ignored a source eligibility guard.");
  }

  bool invalidCostRejected = false;
  try
  {
    _ = NpcActivitySlotContributionPolicy.Evaluate(ordinaryInput with { NpcSlotCost = float.NaN });
  }
  catch (ArgumentOutOfRangeException)
  {
    invalidCostRejected = true;
  }

  bool invalidTypeRejected = false;
  try
  {
    _ = NpcActivitySlotContributionPolicy.Evaluate(
      ordinaryInput with { NpcType = LegacySlimeRainNpcRegistry.NpcTypeCount });
  }
  catch (ArgumentOutOfRangeException)
  {
    invalidTypeRejected = true;
  }

  if (!invalidCostRejected || !invalidTypeRejected)
  {
    throw new InvalidOperationException(
      "NPC CheckActive slot contribution accepted invalid numeric or type input.");
  }

  Console.WriteLine("PASS: NPC CheckActive slot-contribution policy");
}

static void VerifyNpcInvasionBossCapPolicy()
{
  NpcInvasionBossSlotAccount[] accounts =
  [
    new NpcInvasionBossSlotAccount(315, IsActive: true, NpcSlotCost: 3.0f),
    new NpcInvasionBossSlotAccount(325, IsActive: true, NpcSlotCost: 2.0f),
    new NpcInvasionBossSlotAccount(1, IsActive: true, NpcSlotCost: 100.0f),
    new NpcInvasionBossSlotAccount(327, IsActive: false, NpcSlotCost: 100.0f)
  ];
  NpcInvasionBossCapDecision belowCap = NpcInvasionBossCapPolicy.Evaluate(
    accounts,
    activePlayerCount: 2);
  if (belowCap.HasReachedCap || belowCap.ActiveBossSlotCost != 5.0f ||
      belowCap.PerPlayerBossSlotLimit != 13 || belowCap.GlobalBossSlotLimit != 26)
  {
    throw new InvalidOperationException(
      "NPC invasion-boss cap did not preserve the source weighted threshold or type filter.");
  }

  NpcInvasionBossCapDecision atCap = NpcInvasionBossCapPolicy.Evaluate(
    [new NpcInvasionBossSlotAccount(315, IsActive: true, NpcSlotCost: 26.0f)],
    activePlayerCount: 2);
  if (!atCap.HasReachedCap)
  {
    throw new InvalidOperationException(
      "NPC invasion-boss cap did not reject a weighted total at the source threshold.");
  }

  if (!LegacyNpcInvasionBossRegistry.IsInvasionBoss(346) ||
      LegacyNpcInvasionBossRegistry.IsInvasionBoss(328 + 1) ||
      LegacyNpcInvasionBossRegistry.InvasionBossNpcTypeCount != 7)
  {
    throw new InvalidOperationException(
      "NPC invasion-boss registry did not preserve the exact source type set.");
  }

  bool invalidTypeRejected = false;
  try
  {
    _ = NpcInvasionBossCapPolicy.Evaluate(
      [new NpcInvasionBossSlotAccount(-1, IsActive: true, NpcSlotCost: 1.0f)],
      activePlayerCount: 1);
  }
  catch (ArgumentOutOfRangeException)
  {
    invalidTypeRejected = true;
  }

  if (!invalidTypeRejected)
  {
    throw new InvalidOperationException(
      "NPC invasion-boss cap accepted an out-of-range NPC type.");
  }

  SpawnNpcCommand invasionSpawn = new(
    DefinitionId: 1,
    Position: new SimulationVector(8.0f, 8.0f),
    Source: NpcSpawnSource.Natural,
    RequestedReplicationId: 901);
  NpcSpawnEligibilitySystem eligibility = new();
  if (eligibility.Evaluate(new NpcSpawnSnapshot(
        [new NpcSpawnCandidate(invasionSpawn, IsOccupied: false, IsProtectedSlot: false,
          IsInvasionCandidate: true)],
        activeNpcCount: 0,
        maximumNpcCount: 2,
        protectedSlotCount: 0,
        existingReplicationIds: new HashSet<int>(),
        invasionState: new NpcInvasionSpawnState(
          InvasionType: 2,
          InvasionSize: 40,
          InvasionDelayTicks: 0,
          ReachedInvasionBossCap: true))).Count != 0)
  {
    throw new InvalidOperationException(
      "NPC invasion spawn eligibility ignored the typed boss-cap gate.");
  }

  Console.WriteLine("PASS: NPC invasion-boss weighted cap policy");
}

static void VerifyNpcCheckActiveTimerRefreshPolicy()
{
  if (NpcCheckActiveTimerRefreshPolicy.Version1456ActiveTime != 750)
  {
    throw new InvalidOperationException(
      "NPC CheckActive timer-refresh policy changed the source active-time constant.");
  }

  NpcCheckActiveTimerRefreshInput refreshInput = new(
    IsNpcActive: true,
    HasScreenRangePlayer: true,
    DoesNotDespawnToInactivityAndCountsNpcSlots: false,
    ActiveTime: NpcCheckActiveTimerRefreshPolicy.Version1456ActiveTime,
    TimeLeft: 10,
    DespawnEncouraged: true);
  NpcCheckActiveTimerRefreshDecision refreshed =
    NpcCheckActiveTimerRefreshPolicy.Evaluate(refreshInput);
  if (!refreshed.ShouldRefresh || refreshed.TimeLeft != 750 || refreshed.DespawnEncouraged)
  {
    throw new InvalidOperationException(
      "NPC CheckActive screen-range hit did not reset timeLeft and clear despawnEncouraged.");
  }

  if (refreshInput.TimeLeft != 10 || !refreshInput.DespawnEncouraged)
  {
    throw new InvalidOperationException(
      "NPC CheckActive timer-refresh policy mutated its caller-owned input.");
  }

  NpcCheckActiveTimerRefreshInput noPlayerInput = refreshInput with
  {
    HasScreenRangePlayer = false,
    TimeLeft = 123
  };
  NpcCheckActiveTimerRefreshDecision noPlayer =
    NpcCheckActiveTimerRefreshPolicy.Evaluate(noPlayerInput);
  if (noPlayer.ShouldRefresh || noPlayer.TimeLeft != 123 || !noPlayer.DespawnEncouraged)
  {
    throw new InvalidOperationException(
      "NPC CheckActive timer-refresh policy changed state without a screen-range player.");
  }

  NpcCheckActiveTimerRefreshDecision inactive =
    NpcCheckActiveTimerRefreshPolicy.Evaluate(refreshInput with
    {
      IsNpcActive = false,
      TimeLeft = 321
    });
  if (inactive.ShouldRefresh || inactive.TimeLeft != 321 || !inactive.DespawnEncouraged)
  {
    throw new InvalidOperationException(
      "NPC CheckActive timer-refresh policy refreshed an inactive NPC.");
  }

  NpcCheckActiveTimerRefreshDecision slotCounting =
    NpcCheckActiveTimerRefreshPolicy.Evaluate(refreshInput with
    {
      DoesNotDespawnToInactivityAndCountsNpcSlots = true,
      TimeLeft = 222
    });
  if (slotCounting.ShouldRefresh || slotCounting.TimeLeft != 222 ||
      !slotCounting.DespawnEncouraged)
  {
    throw new InvalidOperationException(
      "NPC CheckActive type-668 slot-counting branch incorrectly refreshed its timer.");
  }

  NpcCheckActiveTimerRefreshDecision equalThreshold =
    NpcCheckActiveTimerRefreshPolicy.Evaluate(refreshInput with { TimeLeft = 750 });
  if (!equalThreshold.ShouldRefresh || equalThreshold.TimeLeft != 750 ||
      equalThreshold.DespawnEncouraged)
  {
    throw new InvalidOperationException(
      "NPC CheckActive timer refresh did not preserve the equal active-time threshold.");
  }

  foreach (int invalidActiveTime in new[] { 0, -1 })
  {
    try
    {
      _ = NpcCheckActiveTimerRefreshPolicy.Evaluate(refreshInput with
      {
        ActiveTime = invalidActiveTime
      });
      throw new InvalidOperationException(
        "NPC CheckActive timer-refresh policy accepted a non-positive active time.");
    }
    catch (ArgumentOutOfRangeException)
    {
    }
  }

  Console.WriteLine("PASS: NPC CheckActive screen-range timer-refresh policy");
}

static void VerifyNpcCheckActiveDeactivationPolicy()
{
  NpcCheckActiveDeactivationInput deactivateInput = new(
    IsNpcActive: true,
    HasKeepAlive: false,
    DoesNotDespawnToInactivityAndCountsNpcSlots: false,
    TimeLeft: 10,
    Life: 45);
  NpcCheckActiveDeactivationDecision deactivated =
    NpcCheckActiveDeactivationPolicy.Evaluate(deactivateInput);
  if (!deactivated.WasProcessed || deactivated.IsActive || deactivated.TimeLeft != 9 ||
      deactivated.Life != 0 || !deactivated.ShouldSkipNextSpawnCycle)
  {
    throw new InvalidOperationException(
      "NPC CheckActive timeout branch did not preserve the source deactivation transition.");
  }

  if (deactivateInput.TimeLeft != 10 || deactivateInput.Life != 45)
  {
    throw new InvalidOperationException(
      "NPC CheckActive deactivation policy mutated its caller-owned input.");
  }

  NpcCheckActiveDeactivationDecision keptAlive =
    NpcCheckActiveDeactivationPolicy.Evaluate(deactivateInput with { HasKeepAlive = true });
  if (!keptAlive.WasProcessed || !keptAlive.IsActive || keptAlive.TimeLeft != 9 ||
      keptAlive.Life != 45 || keptAlive.ShouldSkipNextSpawnCycle)
  {
    throw new InvalidOperationException(
      "NPC CheckActive kept an NPC alive with the wrong timer or spawn-cycle decision.");
  }

  NpcCheckActiveDeactivationDecision timerExpired =
    NpcCheckActiveDeactivationPolicy.Evaluate(
      deactivateInput with { HasKeepAlive = true, TimeLeft = 1 });
  if (!timerExpired.WasProcessed || timerExpired.IsActive || timerExpired.TimeLeft != 0 ||
      timerExpired.Life != 0 || !timerExpired.ShouldSkipNextSpawnCycle)
  {
    throw new InvalidOperationException(
      "NPC CheckActive did not deactivate when the kept-alive timer reached zero.");
  }

  NpcCheckActiveDeactivationDecision slotCounting =
    NpcCheckActiveDeactivationPolicy.Evaluate(
      deactivateInput with
      {
        HasKeepAlive = false,
        DoesNotDespawnToInactivityAndCountsNpcSlots = true,
        TimeLeft = 2,
        Life = 17
      });
  if (slotCounting.WasProcessed || !slotCounting.IsActive || slotCounting.TimeLeft != 2 ||
      slotCounting.Life != 17 || slotCounting.ShouldSkipNextSpawnCycle)
  {
    throw new InvalidOperationException(
      "NPC CheckActive type-668 slot-counting branch did not bypass deactivation.");
  }

  NpcCheckActiveDeactivationDecision inactive =
    NpcCheckActiveDeactivationPolicy.Evaluate(
      deactivateInput with { IsNpcActive = false, TimeLeft = 4, Life = 8 });
  if (inactive.WasProcessed || inactive.IsActive || inactive.TimeLeft != 4 ||
      inactive.Life != 8 || inactive.ShouldSkipNextSpawnCycle)
  {
    throw new InvalidOperationException(
      "NPC CheckActive deactivation policy revived or mutated an inactive NPC.");
  }

  foreach (NpcCheckActiveDeactivationInput invalidInput in new[]
  {
    deactivateInput with { TimeLeft = -1 },
    deactivateInput with { Life = -1 }
  })
  {
    try
    {
      _ = NpcCheckActiveDeactivationPolicy.Evaluate(invalidInput);
      throw new InvalidOperationException(
        "NPC CheckActive deactivation policy accepted a negative lifecycle value.");
    }
    catch (ArgumentOutOfRangeException)
    {
    }
  }

  NpcSpawnCycleStateComponent spawnCycle = new();
  if (spawnCycle.TryConsumeSkipNextSpawnCycle())
  {
    throw new InvalidOperationException("NPC spawn-cycle gate consumed an unmarked cycle.");
  }

  spawnCycle.MarkSkipNextSpawnCycle();
  if (!spawnCycle.TryConsumeSkipNextSpawnCycle() ||
      spawnCycle.TryConsumeSkipNextSpawnCycle())
  {
    throw new InvalidOperationException(
      "NPC spawn-cycle gate did not consume the source noSpawnCycle marker exactly once.");
  }

  Console.WriteLine("PASS: NPC CheckActive timeout deactivation and spawn-cycle gate");
}

static void VerifyNpcCheckDeadQualificationPolicy()
{
  NpcCheckDeadQualificationInput eligibleInput = new(
    IsNpcActive: true,
    IsRootSegment: true,
    Life: 0);
  NpcCheckDeadQualificationDecision eligible =
    NpcCheckDeadQualificationPolicy.Evaluate(eligibleInput);
  if (!eligible.ShouldProcess ||
      eligible.RejectionReason != NpcCheckDeadQualificationRejectionReason.None)
  {
    throw new InvalidOperationException(
      "NPC checkDead qualification did not admit an active root with non-positive life.");
  }

  NpcCheckDeadQualificationDecision negativeLife =
    NpcCheckDeadQualificationPolicy.Evaluate(eligibleInput with { Life = -1 });
  if (!negativeLife.ShouldProcess ||
      negativeLife.RejectionReason != NpcCheckDeadQualificationRejectionReason.None)
  {
    throw new InvalidOperationException(
      "NPC checkDead qualification rejected a non-positive life value.");
  }

  NpcCheckDeadQualificationDecision inactive =
    NpcCheckDeadQualificationPolicy.Evaluate(eligibleInput with { IsNpcActive = false });
  if (inactive.ShouldProcess ||
      inactive.RejectionReason != NpcCheckDeadQualificationRejectionReason.Inactive)
  {
    throw new InvalidOperationException(
      "NPC checkDead qualification did not reject an inactive NPC first.");
  }

  NpcCheckDeadQualificationDecision nonRoot =
    NpcCheckDeadQualificationPolicy.Evaluate(eligibleInput with { IsRootSegment = false });
  if (nonRoot.ShouldProcess ||
      nonRoot.RejectionReason != NpcCheckDeadQualificationRejectionReason.NonRootSegment)
  {
    throw new InvalidOperationException(
      "NPC checkDead qualification did not reject a non-root worm segment.");
  }

  NpcCheckDeadQualificationDecision positiveLife =
    NpcCheckDeadQualificationPolicy.Evaluate(eligibleInput with { Life = 1 });
  if (positiveLife.ShouldProcess ||
      positiveLife.RejectionReason != NpcCheckDeadQualificationRejectionReason.PositiveLife)
  {
    throw new InvalidOperationException(
      "NPC checkDead qualification admitted an NPC with positive life.");
  }

  NpcCheckDeadQualificationDecision guardOrder =
    NpcCheckDeadQualificationPolicy.Evaluate(
      eligibleInput with
      {
        IsNpcActive = false,
        IsRootSegment = false,
        Life = 1
      });
  if (guardOrder.ShouldProcess ||
      guardOrder.RejectionReason != NpcCheckDeadQualificationRejectionReason.Inactive)
  {
    throw new InvalidOperationException(
      "NPC checkDead qualification changed the source guard ordering.");
  }

  if (!eligibleInput.IsNpcActive || !eligibleInput.IsRootSegment || eligibleInput.Life != 0)
  {
    throw new InvalidOperationException(
      "NPC checkDead qualification mutated its caller-owned input.");
  }

  Console.WriteLine("PASS: NPC checkDead entry qualification policy");
}

static void VerifyNpcCheckDeadSpecialTransitionPolicy()
{
  NpcCheckDeadSpecialTransitionInput transformInput = new(
    Npc: new NpcHandle(42),
    NpcType: 397,
    Life: 0,
    MaximumHealth: 250,
    Ai0: 0.0f,
    Ai1: 4.0f,
    Ai2: 5.0f,
    Ai3: 7.0f,
    Center: new SimulationVector(120.75f, 240.9f));
  NpcCheckDeadSpecialTransitionDecision transformed =
    NpcCheckDeadSpecialTransitionPolicy.Evaluate(transformInput);
  if (!transformed.ShouldReturnFromCheckDead ||
      transformed.Kind != NpcCheckDeadSpecialTransitionKind.Type396Or397 ||
      transformed.State.Life != 250 || transformed.State.Ai0 != -2.0f ||
      transformed.State.Ai1 != 4.0f || transformed.State.Ai2 != 5.0f ||
      transformed.State.Ai3 != 7.0f || !transformed.State.DontTakeDamage ||
      transformed.State.DontTakeDamageFromHostiles || !transformed.State.NetUpdate ||
      transformed.SpawnIntent is not { } spawnIntent ||
      spawnIntent.Parent != transformInput.Npc || spawnIntent.DefinitionId != 400 ||
      spawnIntent.Position != new SimulationVector(120.0f, 240.0f) ||
      spawnIntent.ChildAi3 != 7.0f ||
      !spawnIntent.ChildNetUpdate)
  {
    throw new InvalidOperationException(
      "NPC checkDead 396/397 transition did not preserve the typed transform/spawn contract.");
  }

  foreach (int npcType in new[] { 396, 397 })
  {
    NpcCheckDeadSpecialTransitionDecision terminal =
      NpcCheckDeadSpecialTransitionPolicy.Evaluate(
        transformInput with { NpcType = npcType, Ai0 = -2.0f });
    if (!terminal.ShouldReturnFromCheckDead || terminal.Kind !=
        NpcCheckDeadSpecialTransitionKind.Type396Or397 || terminal.SpawnIntent is not null ||
        terminal.State.Life != 0 || terminal.State.Ai0 != -2.0f)
    {
      throw new InvalidOperationException(
        "NPC checkDead 396/397 terminal state incorrectly repeated its transform.");
    }
  }

  NpcCheckDeadSpecialTransitionDecision type398 =
    NpcCheckDeadSpecialTransitionPolicy.Evaluate(
      transformInput with { NpcType = 398, Ai0 = 0.0f });
  if (!type398.ShouldReturnFromCheckDead ||
      type398.Kind != NpcCheckDeadSpecialTransitionKind.Type398 ||
      type398.State.Life != 250 || type398.State.Ai0 != 2.0f ||
      !type398.State.DontTakeDamage || !type398.State.NetUpdate ||
      type398.SpawnIntent is not null)
  {
    throw new InvalidOperationException(
      "NPC checkDead type 398 transition did not preserve its state reset.");
  }

  NpcCheckDeadSpecialTransitionDecision type398Terminal =
    NpcCheckDeadSpecialTransitionPolicy.Evaluate(
      transformInput with { NpcType = 398, Ai0 = 2.0f });
  if (type398Terminal.ShouldReturnFromCheckDead || type398Terminal.Kind !=
      NpcCheckDeadSpecialTransitionKind.None || type398Terminal.SpawnIntent is not null ||
      type398Terminal.State.Life != 0 || type398Terminal.State.Ai0 != 2.0f)
  {
    throw new InvalidOperationException(
      "NPC checkDead type 398 terminal state incorrectly repeated its transition.");
  }

  foreach (int npcType in new[] { 517, 422, 507, 493 })
  {
    NpcCheckDeadSpecialTransitionDecision family =
      NpcCheckDeadSpecialTransitionPolicy.Evaluate(
        transformInput with { NpcType = npcType, Ai1 = 9.0f, Ai2 = 0.0f });
    if (!family.ShouldReturnFromCheckDead ||
        family.Kind != NpcCheckDeadSpecialTransitionKind.Type517Family ||
        family.State.Life != 250 || family.State.Ai1 != 0.0f ||
        family.State.Ai2 != 1.0f || !family.State.DontTakeDamage ||
        !family.State.NetUpdate || family.SpawnIntent is not null)
    {
      throw new InvalidOperationException(
      "NPC checkDead 517/422/507/493 transition did not preserve its state reset.");
    }
  }

  NpcCheckDeadSpecialTransitionDecision familyTerminal =
    NpcCheckDeadSpecialTransitionPolicy.Evaluate(
      transformInput with { NpcType = 517, Ai2 = 1.0f });
  if (familyTerminal.ShouldReturnFromCheckDead || familyTerminal.Kind !=
      NpcCheckDeadSpecialTransitionKind.None || familyTerminal.SpawnIntent is not null ||
      familyTerminal.State.Life != 0 || familyTerminal.State.Ai2 != 1.0f)
  {
    throw new InvalidOperationException(
      "NPC checkDead 517-family terminal state incorrectly repeated its transition.");
  }

  NpcCheckDeadSpecialTransitionDecision type548 =
    NpcCheckDeadSpecialTransitionPolicy.Evaluate(
      transformInput with
      {
        NpcType = 548,
        Ai0 = 6.0f,
        Ai1 = 0.0f,
        DontTakeDamage = true
      });
  if (!type548.ShouldReturnFromCheckDead ||
      type548.Kind != NpcCheckDeadSpecialTransitionKind.Type548 ||
      type548.State.Life != 250 || type548.State.Ai0 != 0.0f ||
      type548.State.Ai1 != 1.0f || !type548.State.DontTakeDamage ||
      !type548.State.DontTakeDamageFromHostiles || !type548.State.NetUpdate ||
      type548.SpawnIntent is not null)
  {
    throw new InvalidOperationException(
      "NPC checkDead type 548 transition did not preserve its hostiles-immunity reset.");
  }

  NpcCheckDeadSpecialTransitionDecision type548Terminal =
    NpcCheckDeadSpecialTransitionPolicy.Evaluate(
      transformInput with { NpcType = 548, Ai1 = 1.0f });
  if (type548Terminal.ShouldReturnFromCheckDead || type548Terminal.Kind !=
      NpcCheckDeadSpecialTransitionKind.None || type548Terminal.SpawnIntent is not null ||
      type548Terminal.State.Life != 0 || type548Terminal.State.Ai0 != 0.0f ||
      type548Terminal.State.Ai1 != 1.0f)
  {
    throw new InvalidOperationException(
      "NPC checkDead type 548 terminal state incorrectly repeated its transition.");
  }

  NpcCheckDeadSpecialTransitionDecision unchanged =
    NpcCheckDeadSpecialTransitionPolicy.Evaluate(
      transformInput with { NpcType = 1, NetUpdate = true });
  if (unchanged.ShouldReturnFromCheckDead || unchanged.Kind !=
      NpcCheckDeadSpecialTransitionKind.None || unchanged.State.Life != 0 ||
      unchanged.State.Ai0 != 0.0f || !unchanged.State.NetUpdate ||
      unchanged.SpawnIntent is not null)
  {
    throw new InvalidOperationException(
      "NPC checkDead special-transition policy changed an unrelated NPC type.");
  }

  foreach (NpcCheckDeadSpecialTransitionInput invalidInput in new[]
  {
    transformInput with { Npc = default },
    transformInput with { NpcType = -1 },
    transformInput with { NpcType = 697 },
    transformInput with { Life = 1 },
    transformInput with { MaximumHealth = 0 },
    transformInput with { Ai0 = float.NaN },
    transformInput with { Ai1 = float.PositiveInfinity },
    transformInput with { Ai2 = float.NegativeInfinity },
    transformInput with { Ai3 = float.NaN },
    transformInput with { Center = new SimulationVector(float.NaN, 0.0f) }
  })
  {
    try
    {
      _ = NpcCheckDeadSpecialTransitionPolicy.Evaluate(invalidInput);
      throw new InvalidOperationException(
        "NPC checkDead special-transition policy accepted invalid typed input.");
    }
    catch (ArgumentOutOfRangeException)
    {
    }
  }

  if (transformInput.Life != 0 || transformInput.Ai0 != 0.0f ||
      transformInput.Ai1 != 4.0f || transformInput.Ai2 != 5.0f ||
      transformInput.Ai3 != 7.0f)
  {
    throw new InvalidOperationException(
      "NPC checkDead special-transition policy mutated caller-owned input.");
  }

  Console.WriteLine("PASS: NPC checkDead special transform/spawn policy");
}

static void VerifyNpcCheckDeadInvasionProgressCommandPolicy()
{
  NpcCheckDeadInvasionProgressDecision decision =
    NpcCheckDeadInvasionProgressPolicy.Evaluate(new NpcCheckDeadInvasionProgressInput(
      NpcType: 26,
      InvasionType: 1,
      InvasionSize: 8,
      InvasionSizeStart: 20));
  if (!NpcCheckDeadInvasionProgressCommandPolicy.TryCreate(
        decision,
        sequence: 42,
        out WorldInvasionProgressCommand command) ||
      command != new WorldInvasionProgressCommand(1, 42))
  {
    throw new InvalidOperationException(
      "NPC checkDead invasion progress command bridge changed the source amount contract.");
  }

  if (NpcCheckDeadInvasionProgressCommandPolicy.TryCreate(
        decision with { Applies = false },
        sequence: 42,
        out _) ||
      NpcCheckDeadInvasionProgressCommandPolicy.TryCreate(decision, sequence: -1, out _))
  {
    throw new InvalidOperationException(
      "NPC checkDead invasion progress command bridge accepted an invalid decision or sequence.");
  }

  Console.WriteLine("PASS: NPC checkDead invasion progress command bridge");
}

static void VerifyLegacyNpcInvasionGroupRegistry()
{
  int[][] expectedGroups =
  [
    [26, 27, 28, 29, 111, 471, 472],
    [143, 144, 145],
    [212, 213, 214, 215, 216, 252, 491, 492, 662],
    [381, 382, 383, 385, 386, 387, 388, 389, 390, 391, 394, 395, 520]
  ];
  if (LegacyNpcInvasionGroupRegistry.InvasionGroupTypeCount != 32)
  {
    throw new InvalidOperationException(
      "NPC invasion-group registry changed the source grouped type count.");
  }

  for (int groupIndex = 0; groupIndex < expectedGroups.Length; groupIndex++)
  {
    int expectedGroup = groupIndex + 1;
    for (int typeIndex = 0; typeIndex < expectedGroups[groupIndex].Length; typeIndex++)
    {
      if (LegacyNpcInvasionGroupRegistry.GetInvasionGroup(expectedGroups[groupIndex][typeIndex]) !=
          expectedGroup)
      {
        throw new InvalidOperationException(
          "NPC invasion-group registry changed a source group mapping.");
      }
    }
  }

  if (LegacyNpcInvasionGroupRegistry.GetInvasionGroup(338) != 0 ||
      LegacyNpcInvasionGroupRegistry.GetInvasionGroup(305) != 0 ||
      LegacyNpcInvasionGroupRegistry.GetInvasionGroup(547) != 0)
  {
    throw new InvalidOperationException(
      "NPC invasion-group registry widened into non-invasion source categories.");
  }

  bool rejectedType = false;
  try
  {
    _ = LegacyNpcInvasionGroupRegistry.GetInvasionGroup(697);
  }
  catch (ArgumentOutOfRangeException)
  {
    rejectedType = true;
  }

  if (!rejectedType)
  {
    throw new InvalidOperationException(
      "NPC invasion-group registry accepted an out-of-range type.");
  }

  Console.WriteLine("PASS: NPC source-backed invasion-group registry");
}

static void VerifyNpcTombstoneProjectileTypePolicy()
{
  if (NpcTombstoneProjectileTypePolicy.Resolve(17, 0) != 527 ||
      NpcTombstoneProjectileTypePolicy.Resolve(441, 4) != 531 ||
      NpcTombstoneProjectileTypePolicy.Resolve(18, 0) != 43 ||
      NpcTombstoneProjectileTypePolicy.Resolve(18, 1) != 201 ||
      NpcTombstoneProjectileTypePolicy.Resolve(18, 5) != 205)
  {
    throw new InvalidOperationException(
      "NPC town tombstone projectile type policy changed the source random mapping.");
  }

  bool rejectedVariant = false;
  try
  {
    _ = NpcTombstoneProjectileTypePolicy.Resolve(18, 6);
  }
  catch (ArgumentOutOfRangeException)
  {
    rejectedVariant = true;
  }

  if (!rejectedVariant)
  {
    throw new InvalidOperationException(
      "NPC town tombstone projectile type policy accepted an invalid random sample.");
  }

  Console.WriteLine("PASS: NPC town tombstone projectile type policy");
}

static void VerifyNpcCheckDeadSpawnCyclePolicy()
{
  NpcCheckDeadSpawnCycleDecision qualifiedDecision =
    NpcCheckDeadSpawnCyclePolicy.Evaluate(new NpcCheckDeadSpawnCycleInput(
      IsQualifiedDeath: true,
      SkipNextSpawnCycleAlreadyMarked: false));
  if (!qualifiedDecision.ShouldMarkSkipNextSpawnCycle || qualifiedDecision.WasProcessed)
  {
    throw new InvalidOperationException(
      "NPC checkDead did not produce the source noSpawnCycle mark intent.");
  }

  NpcCheckDeadSpawnCycleDecision repeatedDecision =
    NpcCheckDeadSpawnCyclePolicy.Evaluate(new NpcCheckDeadSpawnCycleInput(
      IsQualifiedDeath: true,
      SkipNextSpawnCycleAlreadyMarked: true));
  if (!repeatedDecision.ShouldMarkSkipNextSpawnCycle || !repeatedDecision.WasProcessed)
  {
    throw new InvalidOperationException(
      "NPC checkDead noSpawnCycle intent did not preserve idempotent mark semantics.");
  }

  NpcCheckDeadSpawnCycleDecision unqualifiedDecision =
    NpcCheckDeadSpawnCyclePolicy.Evaluate(new NpcCheckDeadSpawnCycleInput(
      IsQualifiedDeath: false,
      SkipNextSpawnCycleAlreadyMarked: false));
  if (unqualifiedDecision.ShouldMarkSkipNextSpawnCycle || unqualifiedDecision.WasProcessed)
  {
    throw new InvalidOperationException(
      "NPC checkDead noSpawnCycle intent ran before death qualification.");
  }

  Console.WriteLine("PASS: NPC checkDead noSpawnCycle intent policy");
}

static void VerifyNpcCheckDeadGoodWorldProjectilePolicy()
{
  NpcCheckDeadGoodWorldProjectileDecision decision =
    NpcCheckDeadGoodWorldProjectilePolicy.Evaluate(
      new NpcCheckDeadGoodWorldProjectileInput(
        IsGoodWorld: true,
        NpcType: 631,
        Center: new SimulationVector(-2.5f, 4.0f),
        ProjectileOwner: -1));
  if (!decision.ShouldSpawn || decision.ProjectileType != 99 ||
      decision.Position != new SimulationVector(-2.5f, 4.0f) ||
      decision.Velocity != new SimulationVector(0.0f, 0.0f) ||
      decision.Damage != 70 || decision.Knockback != 10.0f || decision.ProjectileOwner != -1)
  {
    throw new InvalidOperationException(
      "NPC checkDead Good World projectile intent changed its source constants.");
  }

  NpcCheckDeadGoodWorldProjectileDecision ordinaryWorld =
    NpcCheckDeadGoodWorldProjectilePolicy.Evaluate(
      new NpcCheckDeadGoodWorldProjectileInput(
        IsGoodWorld: false,
        NpcType: 631,
        Center: new SimulationVector(1.0f, 2.0f),
        ProjectileOwner: 3));
  NpcCheckDeadGoodWorldProjectileDecision ordinaryType =
    NpcCheckDeadGoodWorldProjectilePolicy.Evaluate(
      new NpcCheckDeadGoodWorldProjectileInput(
        IsGoodWorld: true,
        NpcType: 630,
        Center: new SimulationVector(1.0f, 2.0f),
        ProjectileOwner: 3));
  if (ordinaryWorld.ShouldSpawn || ordinaryType.ShouldSpawn)
  {
    throw new InvalidOperationException(
      "NPC checkDead Good World projectile intent widened beyond type 631 and the world gate.");
  }

  bool rejectedPosition = false;
  try
  {
    _ = NpcCheckDeadGoodWorldProjectilePolicy.Evaluate(
      new NpcCheckDeadGoodWorldProjectileInput(
        IsGoodWorld: true,
        NpcType: 631,
        Center: new SimulationVector(float.NaN, 0.0f),
        ProjectileOwner: 0));
  }
  catch (ArgumentOutOfRangeException)
  {
    rejectedPosition = true;
  }

  if (!rejectedPosition)
  {
    throw new InvalidOperationException(
      "NPC checkDead Good World projectile intent accepted a non-finite position.");
  }

  Console.WriteLine("PASS: NPC checkDead Good World projectile intent policy");
}

static void VerifyNpcCheckDeadInvasionProgressPolicy()
{
  NpcCheckDeadInvasionProgressInput goblin = new(
    NpcType: 26,
    InvasionType: 1,
    InvasionSize: 7,
    InvasionSizeStart: 20);
  NpcCheckDeadInvasionProgressDecision goblinDecision =
    NpcCheckDeadInvasionProgressPolicy.Evaluate(goblin);
  if (!goblinDecision.Applies || goblinDecision.InvasionGroup != 1 ||
      goblinDecision.Points != 1 || goblinDecision.RemainingSize != 6 ||
      goblinDecision.ProgressStart != 14)
  {
    throw new InvalidOperationException(
      "NPC checkDead invasion progress did not decrement the source default point value.");
  }

  NpcCheckDeadInvasionProgressDecision bossDecision =
    NpcCheckDeadInvasionProgressPolicy.Evaluate(goblin with
    {
      NpcType = 216,
      InvasionType = 3,
      InvasionSize = 2,
      InvasionSizeStart = 10
    });
  if (!bossDecision.Applies || bossDecision.InvasionGroup != 3 ||
      bossDecision.Points != 5 || bossDecision.RemainingSize != 0 ||
      bossDecision.ProgressStart != 10)
  {
    throw new InvalidOperationException(
      "NPC checkDead invasion progress did not preserve the source boss point mapping.");
  }

  NpcCheckDeadInvasionProgressDecision zeroPointDecision =
    NpcCheckDeadInvasionProgressPolicy.Evaluate(goblin with
    {
      NpcType = 472,
      InvasionType = 1,
      InvasionSize = 7,
      InvasionSizeStart = 20
    });
  if (zeroPointDecision.Applies || zeroPointDecision.Points != 0 ||
      zeroPointDecision.RemainingSize != 7)
  {
    throw new InvalidOperationException(
      "NPC checkDead invasion progress changed the source zero-point branch.");
  }

  NpcCheckDeadInvasionProgressDecision wrongGroupDecision =
    NpcCheckDeadInvasionProgressPolicy.Evaluate(goblin with { InvasionType = 2 });
  if (wrongGroupDecision.Applies || wrongGroupDecision.RemainingSize != 7)
  {
    throw new InvalidOperationException(
      "NPC checkDead invasion progress applied to a mismatched invasion group.");
  }

  NpcCheckDeadInvasionProgressDecision inactiveDecision =
    NpcCheckDeadInvasionProgressPolicy.Evaluate(goblin with { InvasionSize = 0 });
  if (inactiveDecision.Applies || inactiveDecision.RemainingSize != 0)
  {
    throw new InvalidOperationException(
      "NPC checkDead invasion progress accepted an exhausted invasion.");
  }

  bool rejectedInvalidType = false;
  try
  {
    _ = NpcCheckDeadInvasionProgressPolicy.Evaluate(goblin with { NpcType = -1 });
  }
  catch (ArgumentOutOfRangeException)
  {
    rejectedInvalidType = true;
  }

  if (!rejectedInvalidType)
  {
    throw new InvalidOperationException(
      "NPC checkDead invasion progress accepted an out-of-range NPC type.");
  }

  Console.WriteLine("PASS: NPC checkDead invasion progress policy");
}

static void VerifyNpcCheckActiveWormSegmentPolicy()
{
  NpcCheckActiveWormSegmentInput input = new(
    Trigger: new NpcHandle(10),
    TriggerAiStyle: 6,
    TriggerNextSegment: 11.9f,
    Segments:
    [
      new NpcCheckActiveWormSegmentState(
        new NpcHandle(11),
        AiStyle: 6,
        NextSegment: 12.8f,
        IsActive: true),
      new NpcCheckActiveWormSegmentState(
        new NpcHandle(12),
        AiStyle: 6,
        NextSegment: 13.0f,
        IsActive: false),
      new NpcCheckActiveWormSegmentState(
        new NpcHandle(13),
        AiStyle: 6,
        NextSegment: 10.0f,
        IsActive: true)
    ]);
  NpcCheckActiveWormSegmentDecision decision =
    NpcCheckActiveWormSegmentPolicy.Evaluate(input);
  if (!decision.WasProcessed || decision.DespawnCommands.Count != 1 ||
      decision.DespawnCommands[0] !=
        new DespawnNpcCommand(new NpcHandle(11), NpcDespawnReason.SegmentRootRemoved))
  {
    throw new InvalidOperationException(
      "NPC CheckActive worm-segment policy did not stop at the first inactive child.");
  }

  if (!input.Segments[0].IsActive || input.Segments[0].Handle != new NpcHandle(11))
  {
    throw new InvalidOperationException(
      "NPC CheckActive worm-segment policy mutated caller-owned segment input.");
  }

  NpcCheckActiveWormSegmentDecision nonWormTrigger =
    NpcCheckActiveWormSegmentPolicy.Evaluate(input with { TriggerAiStyle = 5 });
  if (nonWormTrigger.WasProcessed || nonWormTrigger.DespawnCommands.Count != 0)
  {
    throw new InvalidOperationException(
      "NPC CheckActive worm-segment policy accepted a non-worm trigger.");
  }

  NpcCheckActiveWormSegmentDecision selfLink =
    NpcCheckActiveWormSegmentPolicy.Evaluate(input with { TriggerNextSegment = 10.9f });
  if (!selfLink.WasProcessed || selfLink.DespawnCommands.Count != 0)
  {
    throw new InvalidOperationException(
      "NPC CheckActive worm-segment policy did not stop at a self link.");
  }

  NpcCheckActiveWormSegmentDecision truncatedLink =
    NpcCheckActiveWormSegmentPolicy.Evaluate(input with
    {
      TriggerNextSegment = 11.9f,
      Segments =
      [
        input.Segments[0] with { NextSegment = 12.9f },
        input.Segments[1] with { IsActive = true, AiStyle = 5 }
      ]
    });
  if (truncatedLink.DespawnCommands.Count != 1 ||
      truncatedLink.DespawnCommands[0].Npc != new NpcHandle(11))
  {
    throw new InvalidOperationException(
      "NPC CheckActive worm-segment policy did not preserve source integer projection.");
  }

  NpcCheckActiveWormSegmentDecision outOfRangeLink =
    NpcCheckActiveWormSegmentPolicy.Evaluate(input with { TriggerNextSegment = 200.0f });
  if (outOfRangeLink.DespawnCommands.Count != 0)
  {
    throw new InvalidOperationException(
      "NPC CheckActive worm-segment policy traversed the maxNPCs sentinel.");
  }

  NpcCheckActiveWormSegmentDecision cycle =
    NpcCheckActiveWormSegmentPolicy.Evaluate(input with
    {
      TriggerNextSegment = 11.0f,
      Segments =
      [
        input.Segments[0] with { NextSegment = 12.0f },
        input.Segments[1] with { IsActive = true, NextSegment = 11.0f }
      ]
    });
  if (cycle.DespawnCommands.Count != 2 ||
      cycle.DespawnCommands[0].Npc != new NpcHandle(11) ||
      cycle.DespawnCommands[1].Npc != new NpcHandle(12))
  {
    throw new InvalidOperationException(
      "NPC CheckActive worm-segment policy did not terminate a repeated child cycle.");
  }

  NpcCheckActiveWormSegmentDecision terminalLink =
    NpcCheckActiveWormSegmentPolicy.Evaluate(input with
    {
      Segments = [input.Segments[0] with { NextSegment = float.NaN }]
    });
  if (terminalLink.DespawnCommands.Count != 1 ||
      terminalLink.DespawnCommands[0].Npc != new NpcHandle(11))
  {
    throw new InvalidOperationException(
      "NPC CheckActive worm-segment policy did not emit before a terminal child link.");
  }

  foreach (NpcCheckActiveWormSegmentInput invalidInput in new[]
  {
    input with { Trigger = default },
    input with { TriggerNextSegment = float.NaN },
    input with
    {
      Segments =
      [input.Segments[0] with { Handle = default }]
    },
    input with { MaxNpcCount = NpcCheckActiveWormSegmentPolicy.Version1456MaxNpcCount + 1 }
  })
  {
    try
    {
      _ = NpcCheckActiveWormSegmentPolicy.Evaluate(invalidInput);
      throw new InvalidOperationException(
        "NPC CheckActive worm-segment policy accepted invalid identity or AI state.");
    }
    catch (ArgumentOutOfRangeException)
    {
    }
  }

  Console.WriteLine("PASS: NPC CheckActive worm-segment follow-up policy");
}

static void VerifyNpcCheckActiveKeepAlivePolicy()
{
  int[] expectedStaticTypes =
  [
    7,
    10,
    13,
    35,
    36,
    39,
    87,
    127,
    128,
    129,
    130,
    131,
    392,
    393,
    394,
    491,
    492
  ];
  if (LegacyNpcCheckActiveKeepAliveRegistry.NpcTypeCount != 697 ||
      LegacyNpcCheckActiveKeepAliveRegistry.StaticKeepAliveNpcTypeCount !=
        expectedStaticTypes.Length)
  {
    throw new InvalidOperationException(
      "NPC CheckActive keep-alive registry changed the source domain or exact type count.");
  }

  for (int index = 0; index < expectedStaticTypes.Length; index++)
  {
    if (!LegacyNpcCheckActiveKeepAliveRegistry.IsStaticKeepAliveType(expectedStaticTypes[index]))
    {
      throw new InvalidOperationException(
        $"NPC type {expectedStaticTypes[index]} lost its CheckActive keep-alive rule.");
    }
  }

  if (LegacyNpcCheckActiveKeepAliveRegistry.IsStaticKeepAliveType(6) ||
      LegacyNpcCheckActiveKeepAliveRegistry.IsStaticKeepAliveType(8))
  {
    throw new InvalidOperationException(
      "NPC CheckActive keep-alive registry widened across adjacent non-source types.");
  }

  NpcCheckActiveKeepAliveInput staticInput = new(
    NpcType: 7,
    IsActive: true,
    HasActivePlayer: true,
    IsBoss: false,
    Ai0: 0.0f,
    Ai2: 0.0f,
    IsDayTime: true);
  NpcCheckActiveKeepAliveDecision staticDecision =
    NpcCheckActiveKeepAlivePolicy.Evaluate(staticInput);
  if (!staticDecision.ShouldKeepActive || staticDecision.ShouldRefreshInactivityTimer)
  {
    throw new InvalidOperationException(
      "NPC CheckActive static keep-alive type changed its keep/refresh behavior.");
  }

  NpcCheckActiveKeepAliveDecision ordinaryDecision =
    NpcCheckActiveKeepAlivePolicy.Evaluate(staticInput with { NpcType = 6 });
  if (ordinaryDecision.ShouldKeepActive || ordinaryDecision.ShouldRefreshInactivityTimer)
  {
    throw new InvalidOperationException(
      "NPC CheckActive keep-alive policy accepted a neighboring ordinary type.");
  }

  NpcCheckActiveKeepAliveDecision bossDecision =
    NpcCheckActiveKeepAlivePolicy.Evaluate(staticInput with { NpcType = 6, IsBoss = true });
  if (!bossDecision.ShouldKeepActive || bossDecision.ShouldRefreshInactivityTimer)
  {
    throw new InvalidOperationException(
      "NPC CheckActive boss keep-alive policy did not require an active player.");
  }

  NpcCheckActiveKeepAliveDecision type399Refresh =
    NpcCheckActiveKeepAlivePolicy.Evaluate(staticInput with { NpcType = 399, Ai0 = 1.0f });
  if (!type399Refresh.ShouldKeepActive || !type399Refresh.ShouldRefreshInactivityTimer)
  {
    throw new InvalidOperationException(
      "NPC type 399 did not refresh its inactivity timer for ai[0] == 1.");
  }

  NpcCheckActiveKeepAliveDecision type399KeepOnly =
    NpcCheckActiveKeepAlivePolicy.Evaluate(staticInput with { NpcType = 399, Ai0 = 0.0f });
  if (!type399KeepOnly.ShouldKeepActive || type399KeepOnly.ShouldRefreshInactivityTimer)
  {
    throw new InvalidOperationException(
      "NPC type 399 incorrectly refreshed its timer for an unrelated ai[0] value.");
  }

  NpcCheckActiveKeepAliveDecision nightTypeRefresh =
    NpcCheckActiveKeepAlivePolicy.Evaluate(
      staticInput with { NpcType = 583, IsDayTime = false, Ai2 = 0.0f });
  if (!nightTypeRefresh.ShouldKeepActive || !nightTypeRefresh.ShouldRefreshInactivityTimer)
  {
    throw new InvalidOperationException(
      "NPC type 583 did not keep active and refresh at night with ai[2] == 0.");
  }

  for (int npcType = 583; npcType <= 585; npcType++)
  {
    NpcCheckActiveKeepAliveDecision nightType =
      NpcCheckActiveKeepAlivePolicy.Evaluate(
        staticInput with { NpcType = npcType, IsDayTime = false, Ai2 = 0.0f });
    if (!nightType.ShouldKeepActive || !nightType.ShouldRefreshInactivityTimer)
    {
      throw new InvalidOperationException(
        $"NPC type {npcType} did not preserve the source night keep-alive rule.");
    }
  }

  NpcCheckActiveKeepAliveDecision dayTypeDecision =
    NpcCheckActiveKeepAlivePolicy.Evaluate(
      staticInput with { NpcType = 583, IsDayTime = true, Ai2 = 0.0f });
  NpcCheckActiveKeepAliveDecision nightAiTypeDecision =
    NpcCheckActiveKeepAlivePolicy.Evaluate(
      staticInput with { NpcType = 583, IsDayTime = false, Ai2 = 1.0f });
  if (dayTypeDecision.ShouldKeepActive || dayTypeDecision.ShouldRefreshInactivityTimer ||
      nightAiTypeDecision.ShouldKeepActive || nightAiTypeDecision.ShouldRefreshInactivityTimer)
  {
    throw new InvalidOperationException(
      "NPC types 583..585 widened their night-only ai[2] == 0 rule.");
  }

  NpcCheckActiveKeepAliveDecision type690Decision =
    NpcCheckActiveKeepAlivePolicy.Evaluate(staticInput with { NpcType = 690 });
  if (type690Decision.ShouldKeepActive || type690Decision.ShouldRefreshInactivityTimer)
  {
    throw new InvalidOperationException(
      "NPC type 690 was incorrectly included in the active-player keep-alive owner.");
  }

  NpcCheckActiveKeepAliveDecision noPlayerBoss =
    NpcCheckActiveKeepAlivePolicy.Evaluate(
      staticInput with { IsBoss = true, HasActivePlayer = false });
  NpcCheckActiveKeepAliveDecision noPlayerStatic =
    NpcCheckActiveKeepAlivePolicy.Evaluate(staticInput with { HasActivePlayer = false });
  NpcCheckActiveKeepAliveDecision noPlayerType399 =
    NpcCheckActiveKeepAlivePolicy.Evaluate(
      staticInput with { NpcType = 399, HasActivePlayer = false, Ai0 = 1.0f });
  if (noPlayerBoss.ShouldKeepActive || noPlayerStatic.ShouldKeepActive ||
      noPlayerType399.ShouldKeepActive || noPlayerType399.ShouldRefreshInactivityTimer)
  {
    throw new InvalidOperationException(
      "NPC CheckActive keep-alive branches ran without an active player.");
  }

  NpcCheckActiveKeepAliveDecision inactiveNpc =
    NpcCheckActiveKeepAlivePolicy.Evaluate(staticInput with { IsActive = false, IsBoss = true });
  if (inactiveNpc.ShouldKeepActive || inactiveNpc.ShouldRefreshInactivityTimer)
  {
    throw new InvalidOperationException(
      "NPC CheckActive keep-alive policy revived an inactive NPC.");
  }

  NpcCheckActiveKeepAliveDecision overlappingBranch =
    NpcCheckActiveKeepAlivePolicy.Evaluate(
      staticInput with { NpcType = 399, IsBoss = true, Ai0 = 2.0f });
  if (!overlappingBranch.ShouldKeepActive || !overlappingBranch.ShouldRefreshInactivityTimer)
  {
    throw new InvalidOperationException(
      "NPC CheckActive overlapping boss/type-399 branches lost timer refresh semantics.");
  }

  bool negativeTypeRejected = false;
  try
  {
    _ = NpcCheckActiveKeepAlivePolicy.Evaluate(staticInput with { NpcType = -1 });
  }
  catch (ArgumentOutOfRangeException)
  {
    negativeTypeRejected = true;
  }

  bool upperTypeRejected = false;
  try
  {
    _ = NpcCheckActiveKeepAlivePolicy.Evaluate(
      staticInput with { NpcType = LegacyNpcCheckActiveKeepAliveRegistry.NpcTypeCount });
  }
  catch (ArgumentOutOfRangeException)
  {
    upperTypeRejected = true;
  }

  bool nanRejected = false;
  try
  {
    _ = NpcCheckActiveKeepAlivePolicy.Evaluate(staticInput with { Ai0 = float.NaN });
  }
  catch (ArgumentOutOfRangeException)
  {
    nanRejected = true;
  }

  bool infinityRejected = false;
  try
  {
    _ = NpcCheckActiveKeepAlivePolicy.Evaluate(staticInput with { Ai2 = float.PositiveInfinity });
  }
  catch (ArgumentOutOfRangeException)
  {
    infinityRejected = true;
  }

  if (!negativeTypeRejected || !upperTypeRejected || !nanRejected || !infinityRejected)
  {
    throw new InvalidOperationException(
      "NPC CheckActive keep-alive policy accepted invalid type or non-finite AI input.");
  }

  Console.WriteLine("PASS: NPC CheckActive active-player keep-alive policy");
}

static void VerifyLegacyNpcInactivityRegistry()
{
  int[] expectedUnconditionalTypes =
  [
    8, 9, 11, 12, 14, 15, 36, 40, 41, 88, 89, 90, 91, 92, 96, 97, 99, 100,
    113, 114, 115, 118, 119, 128, 129, 130, 131, 134, 135, 136, 246, 247, 248,
    249, 263, 267, 328, 379, 380, 392, 393, 394, 396, 397, 398, 400, 422, 437,
    438, 439, 440, 488, 492, 493, 507, 517, 548, 549, 551, 564, 565
  ];
  if (LegacyNpcInactivityRegistry.StaticNeverDespawnTypeCount != expectedUnconditionalTypes.Length)
  {
    throw new InvalidOperationException(
      "NPC inactivity registry changed the source unconditional type count.");
  }

  HashSet<int> noActiveCompanions = new();
  for (int index = 0; index < expectedUnconditionalTypes.Length; index++)
  {
    int npcType = expectedUnconditionalTypes[index];
    if (!LegacyNpcInactivityRegistry.DoesNotDespawnToInactivity(npcType, noActiveCompanions))
    {
      throw new InvalidOperationException(
        $"NPC type {npcType} lost its source inactivity-preservation rule.");
    }
  }

  if (!LegacyNpcInactivityRegistry.DoesNotDespawnToInactivity(139, new HashSet<int> { 134 }) ||
      LegacyNpcInactivityRegistry.DoesNotDespawnToInactivity(139, new HashSet<int> { 135 }))
  {
    throw new InvalidOperationException(
      "NPC type 139 did not preserve its type-134 companion gate.");
  }

  for (int npcType = 552; npcType <= 578; npcType++)
  {
    bool isConditionalType = npcType <= 563 || npcType >= 566;
    bool shouldPreserveWithCompanion = isConditionalType || npcType is 564 or 565;
    bool preservesWithCompanion = LegacyNpcInactivityRegistry.DoesNotDespawnToInactivity(
      npcType,
      new HashSet<int> { 548 });
    bool preservesWithoutCompanion = LegacyNpcInactivityRegistry.DoesNotDespawnToInactivity(
      npcType,
      new HashSet<int> { 547 });
    if (preservesWithCompanion != shouldPreserveWithCompanion ||
        preservesWithoutCompanion != (npcType is 564 or 565))
    {
      throw new InvalidOperationException(
        $"NPC type {npcType} did not preserve its source inactivity branch.");
    }
  }

  if (LegacyNpcInactivityRegistry.DoesNotDespawnToInactivity(139, noActiveCompanions) ||
      LegacyNpcInactivityRegistry.DoesNotDespawnToInactivity(552, noActiveCompanions) ||
      LegacyNpcInactivityRegistry.DoesNotDespawnToInactivity(579, noActiveCompanions))
  {
    throw new InvalidOperationException(
      "NPC inactivity policy did not fail closed for conditional or invalid types.");
  }

  bool negativeTypeRejected = false;
  try
  {
    _ = LegacyNpcInactivityRegistry.DoesNotDespawnToInactivity(-1, noActiveCompanions);
  }
  catch (ArgumentOutOfRangeException)
  {
    negativeTypeRejected = true;
  }

  bool upperTypeRejected = false;
  try
  {
    _ = LegacyNpcInactivityRegistry.DoesNotDespawnToInactivity(697, noActiveCompanions);
  }
  catch (ArgumentOutOfRangeException)
  {
    upperTypeRejected = true;
  }

  if (!negativeTypeRejected || !upperTypeRejected)
  {
    throw new InvalidOperationException(
      "NPC inactivity registry did not reject an out-of-range net ID.");
  }

  if (!LegacyNpcInactivityRegistry.DoesNotDespawnToInactivityAndCountsNpcSlots(668) ||
      LegacyNpcInactivityRegistry.DoesNotDespawnToInactivityAndCountsNpcSlots(667) ||
      LegacyNpcInactivityRegistry.DoesNotDespawnToInactivityAndCountsNpcSlots(669))
  {
    throw new InvalidOperationException(
      "NPC inactivity slot-counting exception changed from the source type-668 rule.");
  }

  bool slotTypeRejected = false;
  try
  {
    _ = LegacyNpcInactivityRegistry.DoesNotDespawnToInactivityAndCountsNpcSlots(-1);
  }
  catch (ArgumentOutOfRangeException)
  {
    slotTypeRejected = true;
  }

  if (!slotTypeRejected)
  {
    throw new InvalidOperationException(
      "NPC inactivity slot-counting registry did not reject an out-of-range net ID.");
  }

  bool nullContextRejected = false;
  try
  {
    _ = LegacyNpcInactivityRegistry.DoesNotDespawnToInactivity(8, null!);
  }
  catch (ArgumentNullException)
  {
    nullContextRejected = true;
  }

  if (!nullContextRejected)
  {
    throw new InvalidOperationException(
      "NPC inactivity policy accepted a null active-type context.");
  }

  NpcLifecycleSystem lifecycleSystem = new();
  NpcLifecycleComponent protectedLifecycle = new(isActive: true, timeLeft: 2);
  NpcLifecycleResult protectedResult = lifecycleSystem.Advance(
    ref protectedLifecycle,
    currentHealth: 100,
    doesNotDespawnToInactivity: true);
  if (protectedResult.BecameInactive || protectedResult.IsDead ||
      !protectedLifecycle.IsActive || protectedLifecycle.TimeLeft != 2)
  {
    throw new InvalidOperationException(
      "NPC inactivity-preservation gate decremented or deactivated a protected NPC.");
  }

  NpcLifecycleComponent lethalProtectedLifecycle = new(isActive: true, timeLeft: 2);
  NpcLifecycleResult lethalProtectedResult = lifecycleSystem.Advance(
    ref lethalProtectedLifecycle,
    currentHealth: 0,
    doesNotDespawnToInactivity: true);
  if (!lethalProtectedResult.BecameInactive || !lethalProtectedResult.IsDead ||
      lethalProtectedLifecycle.IsActive ||
      lethalProtectedLifecycle.DespawnReason != NpcDespawnReason.Killed)
  {
    throw new InvalidOperationException(
      "NPC inactivity-preservation gate changed lethal lifecycle ordering.");
  }

  NpcLifecycleComponent immortalProtectedLifecycle = new(isActive: true, timeLeft: 0);
  NpcLifecycleResult immortalProtectedResult = lifecycleSystem.Advance(
    ref immortalProtectedLifecycle,
    currentHealth: 0,
    isImmortal: true,
    doesNotDespawnToInactivity: true);
  if (immortalProtectedResult.BecameInactive || immortalProtectedResult.IsDead ||
      !immortalProtectedLifecycle.IsActive)
  {
    throw new InvalidOperationException(
      "NPC inactivity-preservation gate changed immortal lifecycle ordering.");
  }

  Console.WriteLine("PASS: NPC source-backed inactivity registry and lifecycle gate");
}

static void VerifyLegacyNpcTownRegistry()
{
  int[] expectedTownNpcTypes =
  [
    17, 18, 19, 20, 22, 37, 38, 54, 107, 108, 124, 142, 160, 178, 207, 208,
    209, 227, 228, 229, 353, 368, 369, 441, 550, 588, 633, 637, 638, 656, 663,
    670, 678, 679, 680, 681, 682, 683, 684
  ];
  if (LegacyNpcTownRegistry.StaticTownNpcTypeCount != expectedTownNpcTypes.Length)
  {
    throw new InvalidOperationException(
      "NPC town registry changed the source static town-NPC type count.");
  }

  HashSet<int> expectedTownNpcTypeSet = new(expectedTownNpcTypes);
  for (int npcType = 0; npcType < LegacyNpcTownRegistry.NpcTypeCount; npcType++)
  {
    bool expected = expectedTownNpcTypeSet.Contains(npcType);
    if (LegacyNpcTownRegistry.IsTownNpc(npcType) != expected)
    {
      throw new InvalidOperationException(
        $"NPC type {npcType} changed the source townNPC classification.");
    }
  }

  if (LegacyNpcTownRegistry.IsTownNpc(453) || LegacyNpcTownRegistry.IsTownNpc(690))
  {
    throw new InvalidOperationException(
      "NPC town registry conflated isLikeATownNPC or type-690 AI behavior with townNPC.");
  }

  bool negativeTypeRejected = false;
  try
  {
    _ = LegacyNpcTownRegistry.IsTownNpc(-1);
  }
  catch (ArgumentOutOfRangeException)
  {
    negativeTypeRejected = true;
  }

  bool upperTypeRejected = false;
  try
  {
    _ = LegacyNpcTownRegistry.IsTownNpc(LegacyNpcTownRegistry.NpcTypeCount);
  }
  catch (ArgumentOutOfRangeException)
  {
    upperTypeRejected = true;
  }

  if (!negativeTypeRejected || !upperTypeRejected)
  {
    throw new InvalidOperationException(
      "NPC town registry did not reject an out-of-range net ID.");
  }

  NpcLifecycleSystem lifecycleSystem = new();
  NpcLifecycleComponent townLifecycle = new(isActive: true, timeLeft: 2);
  NpcLifecycleResult townResult = lifecycleSystem.Advance(
    ref townLifecycle,
    currentHealth: 100,
    isTownNpc: true);
  if (townResult.BecameInactive || townResult.IsDead || !townLifecycle.IsActive ||
      townLifecycle.TimeLeft != 2)
  {
    throw new InvalidOperationException(
      "NPC town-inactivity gate decremented or deactivated a positive-health town NPC.");
  }

  NpcLifecycleComponent lethalTownLifecycle = new(isActive: true, timeLeft: 2);
  NpcLifecycleResult lethalTownResult = lifecycleSystem.Advance(
    ref lethalTownLifecycle,
    currentHealth: 0,
    isTownNpc: true);
  if (!lethalTownResult.BecameInactive || !lethalTownResult.IsDead ||
      lethalTownLifecycle.IsActive ||
      lethalTownLifecycle.DespawnReason != NpcDespawnReason.Killed)
  {
    throw new InvalidOperationException(
      "NPC town-inactivity gate changed lethal lifecycle ordering.");
  }

  NpcLifecycleComponent immortalTownLifecycle = new(isActive: true, timeLeft: 0);
  NpcLifecycleResult immortalTownResult = lifecycleSystem.Advance(
    ref immortalTownLifecycle,
    currentHealth: 0,
    isImmortal: true,
    isTownNpc: true);
  if (immortalTownResult.BecameInactive || immortalTownResult.IsDead ||
      !immortalTownLifecycle.IsActive)
  {
    throw new InvalidOperationException(
      "NPC town-inactivity gate changed immortal lifecycle ordering.");
  }

  Console.WriteLine("PASS: NPC source-backed townNPC registry and lifecycle gate");
}

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
