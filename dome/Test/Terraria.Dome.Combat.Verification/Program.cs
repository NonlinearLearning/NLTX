using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Arch.Core;
using Terraria.Dome.Simulation;
using Terraria.Dome.Simulation.Combat.Components;
using Terraria.Dome.Simulation.Combat.Events;
using Terraria.Dome.Simulation.Combat.Systems;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.Components;
using Terraria.Dome.Simulation.Items;
using Terraria.Dome.Simulation.Npc.Definitions;
using Terraria.Dome.Simulation.Npc.Commands;
using Terraria.Dome.Simulation.Npc.Components;
using Terraria.Dome.Simulation.Npc.Systems;
using Terraria.Dome.Simulation.Projectile.Behaviors;
using Terraria.Dome.Simulation.Projectile.Definitions;
using Terraria.Dome.Simulation.Projectile;
using Terraria.Dome.Simulation.Projectile.Systems;
using Terraria.Dome.Simulation.Player.Components;
using Terraria.Dome.Simulation.Player.Commands;
using Terraria.Dome.Simulation.StatusEffects.Definitions;
using Terraria.Dome.Simulation.StatusEffects.Snapshots;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Simulation.WorldObjects.Placement;
using Terraria.Dome.Combat.Verification.Fixtures;

using EntityEcs.Components;
bool meleeBuffOnly = args.Any(
  argument => StringComparer.Ordinal.Equals(argument, "--melee-buff-only"));
bool lightPetOnly = args.Any(
  argument => StringComparer.Ordinal.Equals(argument, "--light-pet-only"));
bool persistentBuffOnly = args.Any(
  argument => StringComparer.Ordinal.Equals(argument, "--persistent-buff-only"));
bool debuffOnly = args.Any(
  argument => StringComparer.Ordinal.Equals(argument, "--debuff-only"));
bool vanityPetOnly = args.Any(
  argument => StringComparer.Ordinal.Equals(argument, "--vanity-pet-only"));
bool type656Only = args.Any(
  argument => StringComparer.Ordinal.Equals(argument, "--type656-only"));
bool type657Only = args.Any(
  argument => StringComparer.Ordinal.Equals(argument, "--type657-only"));
bool type658Only = args.Any(
  argument => StringComparer.Ordinal.Equals(argument, "--type658-only"));
bool type658StateOnly = args.Any(
  argument => StringComparer.Ordinal.Equals(argument, "--type658-state-only"));
bool type658ChildOnly = args.Any(
  argument => StringComparer.Ordinal.Equals(argument, "--type658-child-only"));
bool localAiHitOnly = args.Any(
  argument => StringComparer.Ordinal.Equals(argument, "--localai-hit-only"));
bool type607Only = args.Any(
  argument => StringComparer.Ordinal.Equals(argument, "--type607-only"));
bool networkUpdateOnly = args.Any(
  argument => StringComparer.Ordinal.Equals(argument, "--network-update-only"));
bool hitDamageDecayOnly = args.Any(
  argument => StringComparer.Ordinal.Equals(argument, "--hit-damage-decay-only"));
bool immunityScopeOnly = args.Any(
  argument => StringComparer.Ordinal.Equals(argument, "--immunity-scope-only"));
bool npcSlotResetOnly = args.Any(
  argument => StringComparer.Ordinal.Equals(argument, "--npc-slot-reset-only"));
bool ownerAttackCooldownCopyOnly = args.Any(
  argument => StringComparer.Ordinal.Equals(argument, "--owner-attack-cooldown-copy-only"));
bool type1091HostileScalingOnly = args.Any(
  argument => StringComparer.Ordinal.Equals(argument, "--type1091-hostile-scaling-only"));
bool type1091DamageEligibilityOnly = args.Any(
  argument => StringComparer.Ordinal.Equals(argument, "--type1091-damage-eligibility-only"));
if (networkUpdateOnly)
{
  VerifyProjectileNetworkUpdatePolicy();
  VerifyProjectilePrimaryUpdateCadence();
  VerifyProjectileTombstoneNetworkReset();
  Console.WriteLine(
    "SUMMARY: projectile network-update focused verification completed; remaining Combat checks " +
    "were intentionally not run");
  Environment.Exit(0);
}
if (type607Only)
{
  VerifyProjectileType607FriendlyTransition();
  Console.WriteLine(
    "SUMMARY: type-607 friendly transition focused verification completed; remaining Combat checks " +
    "were intentionally not run");
  Environment.Exit(0);
}
if (hitDamageDecayOnly)
{
  VerifyProjectileAcceptedHitDamageDecay();
  Console.WriteLine(
    "SUMMARY: accepted-hit damage decay focused verification completed; remaining Combat checks " +
    "were intentionally not run");
  Environment.Exit(0);
}
if (immunityScopeOnly)
{
  VerifyProjectileLocalNpcImmunityDoesNotApplyToPlayers();
  VerifyProjectileStaticNpcImmunityDoesNotApplyToPlayers();
  Console.WriteLine(
    "SUMMARY: projectile NPC immunity scope focused verification completed; remaining Combat checks " +
    "were intentionally not run");
  Environment.Exit(0);
}
if (npcSlotResetOnly)
{
  VerifyProjectileNpcSlotReset();
  VerifyProjectileNpcSlotResetOnReuse();
  Console.WriteLine(
    "SUMMARY: projectile NPC slot reset focused verification completed; remaining Combat checks " +
    "were intentionally not run");
  Environment.Exit(0);
}
if (ownerAttackCooldownCopyOnly)
{
  VerifyProjectileCopiesOwnerAttackCooldownOnSpawn();
  Console.WriteLine(
    "SUMMARY: projectile owner attack cooldown copy focused verification completed; remaining " +
    "Combat checks were intentionally not run");
  Environment.Exit(0);
}
if (type1091HostileScalingOnly)
{
  VerifyProjectileType1091HostileDamageScaling();
  Console.WriteLine(
    "SUMMARY: type-1091 hostile damage scaling focused verification completed; remaining " +
    "Combat checks were intentionally not run");
  Environment.Exit(0);
}
if (type1091DamageEligibilityOnly)
{
  VerifyProjectileType1091DamageEligibility();
  Console.WriteLine(
    "SUMMARY: type-1091 damage eligibility focused verification completed; remaining Combat " +
    "checks were intentionally not run");
  Environment.Exit(0);
}
if (localAiHitOnly)
{
  VerifyProjectileType656LocalAiHitCounter();
  VerifyProjectileType697LocalAiHitState();
  Console.WriteLine(
    "SUMMARY: localAI accepted-hit focused verification completed; remaining Combat checks were " +
    "intentionally not run");
  Environment.Exit(0);
}

if (type656Only)
{
  VerifyProjectileType656LocalAiHitCounter();
  Console.WriteLine(
    "SUMMARY: type-656 localAI focused verification completed; remaining Combat checks were " +
    "intentionally not run");
  Environment.Exit(0);
}

if (type657Only)
{
  VerifyProjectileType657Definition();
  VerifyProjectileType657Tick();
  Console.WriteLine(
    "SUMMARY: type-657 definition focused verification completed; remaining Combat checks were " +
    "intentionally not run");
  Environment.Exit(0);
}

if (type658Only)
{
  VerifyProjectileType658Definition();
  Console.WriteLine(
    "SUMMARY: type-658 definition focused verification completed; remaining Combat checks were " +
    "intentionally not run");
  Environment.Exit(0);
}

if (type658StateOnly)
{
  VerifyProjectileType658StateTick();
  VerifyProjectileType658RuntimeState();
  Console.WriteLine(
    "SUMMARY: type-658 state focused verification completed; remaining Combat checks were " +
    "intentionally not run");
  Environment.Exit(0);
}

if (type658ChildOnly)
{
  VerifyProjectileType658ChildSpawn();
  Console.WriteLine(
    "SUMMARY: type-658 child-spawn focused verification completed; remaining Combat checks were " +
    "intentionally not run");
  Environment.Exit(0);
}

if (lightPetOnly)
{
  VerifyLegacyLightPetBuffRegistry();
  Console.WriteLine(
    "SUMMARY: light pet buff focused verification completed; remaining Combat checks were " +
    "intentionally not run");
  Environment.Exit(0);
}

if (persistentBuffOnly)
{
  VerifyLegacyPersistentBuffRegistry();
  Console.WriteLine(
    "SUMMARY: persistent buff focused verification completed; remaining Combat checks were " +
    "intentionally not run");
  Environment.Exit(0);
}

if (debuffOnly)
{
  try
  {
    VerifyLegacyDebuffRegistry();
    Console.WriteLine(
      "SUMMARY: debuff focused verification completed; remaining Combat checks were intentionally not run");
    Environment.Exit(0);
  }
  catch (InvalidOperationException exception)
  {
    Console.Error.WriteLine(exception.Message);
    Environment.Exit(1);
  }
}

if (vanityPetOnly)
{
  try
  {
    VerifyLegacyVanityPetBuffRegistry();
    Console.WriteLine(
      "SUMMARY: vanity pet buff focused verification completed; remaining Combat checks were " +
      "intentionally not run");
    Environment.Exit(0);
  }
  catch (InvalidOperationException exception)
  {
    Console.Error.WriteLine(exception.Message);
    Environment.Exit(1);
  }
}

VerifyLegacyMeleeBuffRegistry();
VerifyLegacyLightPetBuffRegistry();
VerifyLegacyPersistentBuffRegistry();
VerifyLegacyDebuffRegistry();
VerifyLegacyVanityPetBuffRegistry();
if (meleeBuffOnly)
{
  Console.WriteLine(
    "SUMMARY: melee buff focused verification completed; remaining Combat checks were " +
    "intentionally not run");
  Environment.Exit(0);
}

using DomeSimulation vitalsSimulation = new(new WorldGrid(400, 300));
PlayerHandle vitalsPlayer = vitalsSimulation.CreatePlayer(new SimulationVector(2.0f, 0.0f));
PlayerStateSnapshot initialVitals = vitalsSimulation.CreatePlayerStateSnapshot(vitalsPlayer);
if (initialVitals.Mana != 20 || initialVitals.MaximumMana != 20)
{
  throw new InvalidOperationException(
    "Default player mana was not represented in the authoritative snapshot.");
}

vitalsSimulation.QueuePlayerDamage(vitalsPlayer, 60);
vitalsSimulation.QueuePlayerDamage(vitalsPlayer, 60);
vitalsSimulation.Tick(new SimulationInputBatch());
PlayerStateSnapshot deadVitals = vitalsSimulation.CreatePlayerStateSnapshot(vitalsPlayer);
if (deadVitals.Health != 0 || deadVitals.IsActive ||
    vitalsSimulation.CreatePlayerDiedEvents().Count != 1 ||
    vitalsSimulation.CreatePlayerDamagedEvents().Count != 2)
{
  throw new InvalidOperationException(
    "Queued player damage did not produce ordered damage and death facts.");
}

Console.WriteLine("PASS: player vitals and damage events are bounded and ordered");

VerifyBoundedVitalRegeneration();
VerifyManaRegenerationAccumulatorBoundary();
VerifyStableReplicationAndCombatLifecycle();
VerifyNpcContactDamage();
VerifyPlayerDamageDifficultyModes();
VerifyProjectileSweepPreventsNpcTunneling();
VerifyProjectileTileCollision();
VerifyProjectileBounce();
VerifyProjectileSlopeImpactAxis();
VerifyProjectileLiquidPolicy();
VerifyProjectileIgnoreWaterPolicy();
VerifyType5SourceBackedTickAndWaterDefaults();
VerifyProjectileMaxUpdatesProjection();
VerifyDeterministicNpcLoot();
VerifyForgedProjectileOwnerRejected();
VerifyProjectileOwnedBySomeonePolicy();
VerifyProjectileOwnerHitCheck();
VerifyProjectileReflectionEligibilityPolicy();
VerifyNpcProjectileReflectionEligibility();
VerifyType4SourceBackedDefaults();
VerifyType2SourceBackedDefaults();
VerifyType6SourceBackedDefaults();
VerifyProjectileDamageClassDefaults();
VerifyType3LegacyAiStyleMetadata();
VerifyType249SourceBackedMetadata();
VerifyHostileProjectileLegacyAiMetadata();
VerifyType599SourceBackedMetadata();
VerifyType909SourceBackedMetadata();
VerifyType520SourceBackedMetadata();
VerifyType521And522SourceBackedMetadata();
VerifyType162SourceBackedMetadata();
VerifyType471501504SourceBackedMetadata();
VerifyType21166330SourceBackedMetadata();
VerifyType954979SourceBackedMetadata();
VerifyType3041012370371936SourceBackedMetadata();
VerifyProjectileType656LocalAiHitCounter();
VerifyProjectileType607FriendlyTransition();
VerifyType6970621SourceBackedMetadata();
VerifyType240SourceBackedMetadata();
VerifyType589SourceBackedMetadata();
VerifyType124SourceBackedAiMetadata();
VerifyArrowMetadata();
VerifyType3And6SourceBackedDamageClasses();
VerifyType300SourceBackedDamageClass();
VerifyColdDamageMetadata();
VerifyProjectileDefinitionMetadataProjection();
VerifyNpcProjectileDefinitionMetadataProjection();
VerifyProjectileDamageClassEventPropagation();
VerifyProjectileHostileDamageScaling();
VerifyProjectileType1091DamageEligibility();
VerifyProjectileVelocityInputsRejected();
VerifyProjectileLifetimeBoundary();
VerifyProjectileExtraUpdatesBoundary();
VerifyProjectileTombstoneReasons();
VerifyProjectileIdentityAllocator();
VerifyProjectileTombstoneRetention();
VerifyProjectileIdentityCursorRestore();
VerifyProjectileLocalImmunity();
VerifyProjectilePlayerImmunity();
VerifyProjectileStaticNpcImmunity();
VerifyStaticNpcImmunitySingleHitPolicy();
VerifyProjectileOwnerMeleeHitCooldown();
VerifyProjectileSectionCoordinates();
VerifyPlayerAttackCooldownRule();
VerifyPaladinShieldDefensePolicy();
VerifyPlayerLuckAndMiscCounterPolicies();
VerifyProjectileStaticImmunityDefinitionBoundary();
VerifyProjectileStopsDamageAfterPenetration();
VerifyHostileProjectileEligibility();
VerifyProjectileOwnerEligibilityContract();
VerifyProjectileOwnerMinionAttackTarget();
VerifyProjectileOwnerMinionAttackTargetResolution();
VerifyProjectileRestrikeDelayLifecycle();
VerifyProjectileNetworkUpdatePolicy();
VerifyProjectilePrimaryNetworkUpdateState();
VerifyProjectileSoundDelayPolicy();
VerifyProjectileTileCollisionState();
VerifyProjectileTileCollisionBehaviorOverride();
VerifyProjectileMinionSpawnSource();
VerifyProjectileBehaviorOverflowBoundary();
VerifyProjectileBehaviorReplicationState();
VerifyLegacyAiStyle2Behavior();
VerifyLegacyAiStyle2Type21Definition();
VerifyLegacyAiStyle2Type330Definition();
VerifyLegacyAiStyle2Type589Definition();
VerifyLegacyAiStyle2Type1012Definition();
VerifyLegacyAiStyle2Type304Lifecycle();
VerifyLegacyAiStyle2Type166Behavior();
VerifyLegacyAiStyle2Type48Definition();
VerifyLegacyAiStyle2Type599Definition();
VerifyLegacyAiStyle2Type520Definition();
VerifyLegacyAiStyle2Type471Definition();
VerifyLegacyAiStyle2Type162FriendlyAreaDamage();
VerifyLegacyAiStyle2StatusEffectFamily();
VerifyLegacyAiStyle2HitStatusFamily();
VerifyLegacyAiStyle2HitStatusDefinitions(954, 24);
VerifyLegacyAiStyle2HitStatusDefinitions(979, 44);
VerifyProjectileTileConversionOnDespawn(69, 2, 2);
VerifyProjectileTileConversionOnDespawn(70, 1, 1);
  VerifyProjectileTileConversionOnDespawn(621, 4, 477);
VerifyType281ReleaseContract();
VerifyType281TerminalNpcRelease();
VerifyLegacyAiStyle49Type281Behavior();
VerifyLegacyAiStyle29Type521ChildSpawn();
VerifyLegacyAiStyle2RandomFrameBehavior();
VerifyLegacyAiStyle2DelayedBehavior();
VerifyLegacyAiStyle2DelayedFamilyDefinitions();
VerifyLegacyAiStyle2ImmediateGravityBehavior();
VerifyLegacyAiStyle2FiveTickGravityBehavior();
VerifyLegacyAiStyle2SixtyTickGravityBehavior();
VerifyLegacyAiStyle190OwnerAnchor();
VerifyLegacyAiStyle17Type43Behavior();
VerifyProjectileWorldObjectPlacementContract();
VerifyProjectileWorldObjectPlacementActorValidation();
VerifyWorldObjectPlacementRegistrySemantics();
VerifyProjectileWipableTurretPolicy();
VerifyProjectileWipableTurretSimulationQuery();
VerifyProjectileUuidLifecycle();
VerifyProjectilePenetrationBoundary();
VerifyForgedProjectilePenetrationRejected();
VerifyProjectileTargetEligibility();
VerifyNpcTrapImmunityEligibility();
ProjectileBehaviorFixtures.VerifyLinear();
ProjectileBehaviorFixtures.VerifyGravity();
using World invalidProjectileWorld = World.Create();
ProjectileBehaviorSystem behaviorSystemBoundary = ProjectileBehaviorSystem.CreateDefault();
if (behaviorSystemBoundary.TryAdvance(default, invalidProjectileWorld, 1, out int invalidBehaviorId) ||
    invalidBehaviorId != 0 ||
    new ProjectileLifetimeSystem().Advance(default, invalidProjectileWorld))
{
  throw new InvalidOperationException("Projectile systems accepted an absent entity identity.");
}

try
{
  _ = new ProjectileReplicationSystem().Project(
    default,
    invalidProjectileWorld,
    replicationId: 1,
    revision: 1,
    default);
  throw new InvalidOperationException("Projectile replication accepted an absent entity identity.");
}
catch (ArgumentException)
{
}

try
{
  _ = new ProjectileReplicationSystem().Project(
    default,
    invalidProjectileWorld,
    replicationId: 0,
    revision: -1,
    default);
  throw new InvalidOperationException("Projectile replication accepted invalid snapshot identity.");
}
catch (ArgumentOutOfRangeException)
{
}

ProjectileDefinition directDefinition = new(
  ProjectileType: 1,
  BehaviorId: 1,
  Damage: 10,
  LifetimeTicks: 20,
  Collider: new ColliderComponent(1.0f, 1.0f),
  Friendly: true,
  Hostile: false,
  MaximumPenetration: 1);
try
{
  _ = new ProjectileDefinitionRegistry([
    directDefinition with { LifetimeTicks = 0 }]);
  throw new InvalidOperationException("Projectile registry accepted an invalid lifetime definition.");
}
catch (ArgumentOutOfRangeException)
{
}

ProjectileDefinitionRegistry defaultProjectileDefinitions =
  ProjectileDefinitionRegistry.CreateDefault();
if (defaultProjectileDefinitions.OrderedDefinitions.Count < 33 ||
    defaultProjectileDefinitions.OrderedDefinitions[0].ProjectileType != 1 ||
      defaultProjectileDefinitions.OrderedDefinitions[4].ProjectileType != 5 ||
      defaultProjectileDefinitions.OrderedDefinitions[5].ProjectileType != 6 ||
      defaultProjectileDefinitions.OrderedDefinitions[6].ProjectileType != 69 ||
      defaultProjectileDefinitions.OrderedDefinitions[7].ProjectileType != 70 ||
      defaultProjectileDefinitions.OrderedDefinitions[8].ProjectileType != 621 ||
      defaultProjectileDefinitions.OrderedDefinitions[9].ProjectileType != 249 ||
      defaultProjectileDefinitions.OrderedDefinitions[10].ProjectileType != 347 ||
      defaultProjectileDefinitions.OrderedDefinitions[11].ProjectileType != 300 ||
      defaultProjectileDefinitions.OrderedDefinitions[12].ProjectileType != 48 ||
      defaultProjectileDefinitions.OrderedDefinitions[13].ProjectileType != 599 ||
      defaultProjectileDefinitions.OrderedDefinitions[14].ProjectileType != 909 ||
      defaultProjectileDefinitions.OrderedDefinitions[15].ProjectileType != 520 ||
      defaultProjectileDefinitions.OrderedDefinitions[16].ProjectileType != 471 ||
      defaultProjectileDefinitions.OrderedDefinitions[17].ProjectileType != 501 ||
      defaultProjectileDefinitions.OrderedDefinitions[18].ProjectileType != 504 ||
      defaultProjectileDefinitions.OrderedDefinitions[19].ProjectileType != 240 ||
      defaultProjectileDefinitions.OrderedDefinitions[20].ProjectileType != 521 ||
      defaultProjectileDefinitions.OrderedDefinitions[21].ProjectileType != 522 ||
      defaultProjectileDefinitions.OrderedDefinitions[22].ProjectileType != 162 ||
      defaultProjectileDefinitions.OrderedDefinitions[23].ProjectileType != 281 ||
      defaultProjectileDefinitions.OrderedDefinitions[24].ProjectileType != 21 ||
      defaultProjectileDefinitions.OrderedDefinitions[25].ProjectileType != 330 ||
      defaultProjectileDefinitions.OrderedDefinitions[26].ProjectileType != 589 ||
      defaultProjectileDefinitions.OrderedDefinitions[27].ProjectileType != 1012 ||
      defaultProjectileDefinitions.OrderedDefinitions[28].ProjectileType != 304 ||
      defaultProjectileDefinitions.OrderedDefinitions[29].ProjectileType != 166 ||
      defaultProjectileDefinitions.OrderedDefinitions[30].ProjectileType != 370 ||
      defaultProjectileDefinitions.OrderedDefinitions[31].ProjectileType != 371 ||
      defaultProjectileDefinitions.OrderedDefinitions[32].ProjectileType != 936)
{
  throw new InvalidOperationException(
    "Projectile registry did not preserve deterministic registration order.");
}

if (!defaultProjectileDefinitions.TryGet(3, out ProjectileDefinition legacyAiStyle2Definition) ||
    legacyAiStyle2Definition.BehaviorId != 3 || !legacyAiStyle2Definition.Friendly ||
    legacyAiStyle2Definition.Hostile || legacyAiStyle2Definition.MaximumPenetration != 4 ||
    legacyAiStyle2Definition.Collider.Width != 1.375f ||
    legacyAiStyle2Definition.Collider.Height != 1.375f)
{
  throw new InvalidOperationException(
    "Default type 3 projectile definition did not match the legacy aiStyle 2 source contract.");
}

if (!defaultProjectileDefinitions.TryGet(48, out ProjectileDefinition type48AiStyle2Definition) ||
    type48AiStyle2Definition.BehaviorId != 3 || !type48AiStyle2Definition.Friendly ||
    type48AiStyle2Definition.Hostile || type48AiStyle2Definition.MaximumPenetration != 2 ||
    type48AiStyle2Definition.Collider.Width != 0.75f ||
    type48AiStyle2Definition.Collider.Height != 0.75f ||
    type48AiStyle2Definition.LegacyAiStyle != 2 ||
    type48AiStyle2Definition.DamageClass != ProjectileDamageClass.Ranged)
{
  throw new InvalidOperationException(
    "Default type 48 projectile definition did not match the generic aiStyle 2 source contract.");
}

if (!defaultProjectileDefinitions.TryGet(599, out ProjectileDefinition type599AiStyle2Definition) ||
    type599AiStyle2Definition.BehaviorId != 3 || !type599AiStyle2Definition.Friendly ||
    type599AiStyle2Definition.Hostile || type599AiStyle2Definition.MaximumPenetration != 6 ||
    type599AiStyle2Definition.Collider.Width != 1.375f ||
    type599AiStyle2Definition.Collider.Height != 1.375f)
{
  throw new InvalidOperationException(
    "Default type 599 projectile definition did not match the generic aiStyle 2 source contract.");
}

if (!defaultProjectileDefinitions.TryGet(520, out ProjectileDefinition type520AiStyle2Definition) ||
    type520AiStyle2Definition.BehaviorId != 3 || !type520AiStyle2Definition.Friendly ||
    type520AiStyle2Definition.Hostile || type520AiStyle2Definition.MaximumPenetration != 3 ||
    type520AiStyle2Definition.Collider.Width != 1.375f ||
    type520AiStyle2Definition.Collider.Height != 1.375f)
{
  throw new InvalidOperationException(
    "Default type 520 projectile definition did not match the generic aiStyle 2 source contract.");
}

if (!defaultProjectileDefinitions.TryGet(909, out ProjectileDefinition type909AiStyle2Definition) ||
    type909AiStyle2Definition.BehaviorId != 8 || type909AiStyle2Definition.Friendly ||
    !type909AiStyle2Definition.Hostile || type909AiStyle2Definition.MaximumPenetration != 1 ||
    type909AiStyle2Definition.Collider.Width != 0.75f ||
    type909AiStyle2Definition.Collider.Height != 0.75f ||
    type909AiStyle2Definition.PlayerDamagePolicy != PlayerDamagePolicy.HostileNonPvp)
{
  throw new InvalidOperationException(
    "Default type 909 projectile definition did not match the random-frame aiStyle 2 source contract.");
}

if (!defaultProjectileDefinitions.TryGet(69, out ProjectileDefinition delayedAiStyle2Definition) ||
    delayedAiStyle2Definition.BehaviorId != 4 || !delayedAiStyle2Definition.Friendly ||
    delayedAiStyle2Definition.Hostile || delayedAiStyle2Definition.MaximumPenetration != 1 ||
    delayedAiStyle2Definition.Collider.Width != 0.875f ||
    delayedAiStyle2Definition.Collider.Height != 0.875f)
{
  throw new InvalidOperationException(
    "Default type 69 projectile definition did not match the delayed aiStyle 2 source contract.");
}

if (!defaultProjectileDefinitions.TryGet(249, out ProjectileDefinition immediateAiStyle2Definition) ||
    immediateAiStyle2Definition.BehaviorId != 5 || !immediateAiStyle2Definition.Friendly ||
    immediateAiStyle2Definition.Hostile || immediateAiStyle2Definition.MaximumPenetration != 1 ||
    immediateAiStyle2Definition.Collider.Width != 0.75f ||
    immediateAiStyle2Definition.Collider.Height != 0.75f)
{
  throw new InvalidOperationException(
    "Default type 249 projectile definition did not match the immediate aiStyle 2 source contract.");
}

if (!defaultProjectileDefinitions.TryGet(347, out ProjectileDefinition fiveTickAiStyle2Definition) ||
    fiveTickAiStyle2Definition.BehaviorId != 6 || fiveTickAiStyle2Definition.Friendly ||
    !fiveTickAiStyle2Definition.Hostile || fiveTickAiStyle2Definition.MaximumPenetration != -1 ||
    fiveTickAiStyle2Definition.Collider.Width != 0.375f ||
    fiveTickAiStyle2Definition.Collider.Height != 0.375f ||
    fiveTickAiStyle2Definition.PlayerDamagePolicy != PlayerDamagePolicy.HostileNonPvp)
{
  throw new InvalidOperationException(
    "Default type 347 projectile definition did not match the five-tick aiStyle 2 source contract.");
}

if (!defaultProjectileDefinitions.TryGet(300, out ProjectileDefinition sixtyTickAiStyle2Definition) ||
    sixtyTickAiStyle2Definition.BehaviorId != 7 || sixtyTickAiStyle2Definition.Friendly ||
    !sixtyTickAiStyle2Definition.Hostile || sixtyTickAiStyle2Definition.MaximumPenetration != -1 ||
    sixtyTickAiStyle2Definition.Collider.Width != 2.375f ||
    sixtyTickAiStyle2Definition.Collider.Height != 2.375f ||
    sixtyTickAiStyle2Definition.CollidesWithTiles)
{
  throw new InvalidOperationException(
    "Default type 300 projectile definition did not match the sixty-tick aiStyle 2 source contract.");
}

try
{
  ((IDictionary<int, ProjectileDefinition>)defaultProjectileDefinitions.Definitions)[1] =
    directDefinition;
  throw new InvalidOperationException("Projectile registry exposed a mutable definitions map.");
}
catch (NotSupportedException)
{
}

ProjectileSpawnSystem directSpawn = new();
try
{
  _ = directSpawn.Spawn(
    invalidProjectileWorld,
    new SpawnProjectileCommand(default, 0.0f, 0.0f, 1, 10, 20),
    directDefinition,
    identity: 1);
  throw new InvalidOperationException("Direct projectile spawn accepted an invalid owner.");
}
catch (ArgumentException)
{
}

try
{
  _ = directSpawn.Spawn(
    invalidProjectileWorld,
    new SpawnProjectileCommand(new PlayerHandle(1), 0.0f, 0.0f, 1, 10, 20),
    directDefinition,
    identity: int.MaxValue);
  throw new InvalidOperationException(
    "Direct projectile spawn accepted a replication identity that cannot advance.");
}
catch (ArgumentException)
{
}
Console.WriteLine("PASS: authoritative combat records, contact damage, collision, cooldown and loot");

static void VerifyBoundedVitalRegeneration()
{
  ProjectileDefinitionRegistry baseDefinitions = ProjectileDefinitionRegistry.CreateDefault();
  ProjectileDefinitionRegistry definitions = new(baseDefinitions.OrderedDefinitions.Select(
    definition => definition.ProjectileType == 281
      ? definition with { LifetimeTicks = 1 }
      : definition));
  using DomeSimulation simulation = new(new WorldGrid(400, 300), definitions);
  PlayerHandle player = simulation.CreatePlayer(new SimulationVector(2.0f, 0.0f));
  simulation.QueuePlayerDamage(player, 10);
  simulation.Tick(new SimulationInputBatch());
  simulation.Tick(new SimulationInputBatch());
  simulation.Tick(new SimulationInputBatch());
  simulation.Tick(new SimulationInputBatch());
  for (int index = 0; index < 4; index++)
  {
    simulation.Tick(new SimulationInputBatch());
  }

  PlayerStateSnapshot state = simulation.CreatePlayerStateSnapshot(player);
  if (state.Health != 91 || state.Health > state.MaximumHealth)
  {
    throw new InvalidOperationException(
      "Player health did not regenerate once after the authoritative delay.");
  }
}

static void VerifyManaRegenerationAccumulatorBoundary()
{
  using World world = World.Create();
  Entity player = world.Create(
    new HealthComponent(100, 100),
    new HealthRegenerationComponent(),
    new ManaComponent(0, 20));
  ref ManaComponent mana = ref world.Get<ManaComponent>(player);
  mana.RegenerationAccumulator = int.MaxValue;

  new PlayerVitalRegenSystem().Apply(world, [player]);
  mana = world.Get<ManaComponent>(player);
  if (mana.Current != 1 || mana.RegenerationAccumulator != 0)
  {
    throw new InvalidOperationException(
      "Mana regeneration did not clamp an exhausted accumulator before incrementing.");
  }
}

static void VerifyForgedProjectileOwnerRejected()
{
  using DomeSimulation simulation = new(new WorldGrid(400, 300));
  simulation.QueueProjectileSpawn(new SpawnProjectileCommand(
    new PlayerHandle(999),
    10.0f,
    0.0f,
    1,
    10,
    20));
  simulation.Tick(new SimulationInputBatch());
  if (simulation.CreateProjectileReplicationSnapshots().Count != 0)
  {
    throw new InvalidOperationException("Projectile spawn accepted an unknown owner.");
  }

  PlayerHandle player = simulation.CreatePlayer(new SimulationVector(10.0f, 0.0f));
  simulation.QueuePlayerDamage(player, 200);
  simulation.Tick(new SimulationInputBatch());
  simulation.QueueProjectileSpawn(new SpawnProjectileCommand(
    player,
    10.0f,
    0.0f,
    1,
    10,
    20));
  simulation.Tick(new SimulationInputBatch());
  if (simulation.CreateProjectileReplicationSnapshots().Count != 0)
  {
    throw new InvalidOperationException("Projectile spawn accepted an inactive owner.");
  }

  PlayerHandle activePlayer = simulation.CreatePlayer(new SimulationVector(10.0f, 0.0f));
  simulation.QueueProjectileSpawn(new SpawnProjectileCommand(
    activePlayer,
    float.NaN,
    0.0f,
    1,
    10,
    20));
  simulation.Tick(new SimulationInputBatch());
  if (simulation.CreateProjectileReplicationSnapshots().Count != 0)
  {
    throw new InvalidOperationException("Projectile spawn accepted a non-finite position.");
  }

  simulation.QueueProjectileSpawn(new SpawnProjectileCommand(
    activePlayer,
    10.0f,
    0.0f,
    1,
    10,
    -1,
    MaximumPenetration: 0));
  simulation.Tick(new SimulationInputBatch());
  IReadOnlyList<ProjectileReplicationSnapshot> definitionControlledSnapshots =
    simulation.CreateProjectileReplicationSnapshots();
  if (definitionControlledSnapshots.Count != 1 ||
      definitionControlledSnapshots[0].RemainingLifetime != 1200)
  {
    throw new InvalidOperationException(
      "Projectile spawn did not replace command lifetime with the authoritative definition value.");
  }

  simulation.QueueProjectileSpawn(new SpawnProjectileCommand(
    activePlayer,
    10.0f,
    0.0f,
    1,
    10,
    20,
    MaximumPenetration: -1));
  simulation.Tick(new SimulationInputBatch());
  if (simulation.CreateProjectileReplicationSnapshots().Count != 2)
  {
    throw new InvalidOperationException(
      "Projectile spawn did not accept compatibility input without using it as authority.");
  }

  simulation.QueueProjectileSpawn(new SpawnProjectileCommand(
    activePlayer,
    10.0f,
    0.0f,
    1,
    999,
    20,
    AuthoritativeDamage: 999,
    AuthoritativeKnockback: 999.0f));
  simulation.Tick(new SimulationInputBatch());
  IReadOnlyList<ProjectileReplicationSnapshot> forgedDamageSnapshots =
    simulation.CreateProjectileReplicationSnapshots();
  if (forgedDamageSnapshots.Count != 3 ||
      forgedDamageSnapshots.Any(snapshot => snapshot.Damage != 10 ||
        snapshot.DefinitionKnockback != 0.0f))
  {
    throw new InvalidOperationException(
      "Projectile spawn did not replace forged combat values with authoritative definition values.");
  }
}

static void VerifyProjectileVelocityInputsRejected()
{
  using DomeSimulation simulation = new(new WorldGrid(400, 300));
  PlayerHandle player = simulation.CreatePlayer(new SimulationVector(10.0f, 0.0f));

  simulation.QueueProjectileSpawn(new SpawnProjectileCommand(
    player,
    10.0f,
    0.0f,
    1,
    10,
    20,
    InitialVelocityY: float.NaN));
  simulation.Tick(new SimulationInputBatch());
  if (simulation.CreateProjectileReplicationSnapshots().Count != 0)
  {
    throw new InvalidOperationException(
      "Projectile spawn accepted a non-finite initial vertical velocity.");
  }

  simulation.QueueProjectileSpawn(new SpawnProjectileCommand(
    player,
    10.0f,
    0.0f,
    1,
    10,
    20,
    ProjectileSpeed: float.PositiveInfinity));
  simulation.Tick(new SimulationInputBatch());
  if (simulation.CreateProjectileReplicationSnapshots().Count != 0)
  {
    throw new InvalidOperationException("Projectile spawn accepted a non-finite speed.");
  }
}

static void VerifyProjectileTargetEligibility()
{
  ProjectileTargetEligibilitySystem system = new();
  ProjectileDefinitionComponent friendly = new(
    1,
    1,
    10,
    30,
    new ColliderComponent(0.5f, 0.5f),
    Friendly: true,
    Hostile: false);
  ProjectileDefinitionComponent hostile = friendly with
  {
    Friendly = false,
    Hostile = true
  };
  if (!system.CanDamageNpc(friendly) || system.CanDamageNpc(hostile))
  {
    throw new InvalidOperationException(
      "Projectile NPC eligibility did not enforce the friendly/hostile source guard.");
  }

  NpcDefinitionComponent npcDefinition = new(1, 1, NpcFaction.Hostile, NpcCategory.Enemy);
  NpcAuthorityComponent npcAuthority = new(3, false, false);
  NpcBehaviorStateComponent npcBehavior = new(
    NpcBehaviorId.OrdinaryChase,
    new NpcChaseState(1.0f, 0.0f),
    new NpcTownHomeState(default, true, 0));
  NpcLifecycleComponent npcLifecycle = new(true, 750);
  HealthComponent npcHealth = new(20, 20);
  NpcBehaviorStateComponent unchaseableBehavior = npcBehavior;
  unchaseableBehavior.IsChaseable = false;
  NpcBehaviorStateComponent immuneBehavior = npcBehavior;
  immuneBehavior.DoesNotTakeDamage = true;
  if (!system.CanTargetNpc(
        friendly,
        npcDefinition,
        npcAuthority,
        npcBehavior,
        npcLifecycle,
        npcHealth) ||
      system.CanTargetNpc(
        hostile,
        npcDefinition,
        npcAuthority,
        npcBehavior,
        npcLifecycle,
        npcHealth) ||
      system.CanTargetNpc(
        friendly,
        npcDefinition,
        npcAuthority,
        unchaseableBehavior,
        npcLifecycle,
        npcHealth) ||
      !system.CanTargetNpc(
        friendly,
        npcDefinition,
        npcAuthority,
        immuneBehavior,
        npcLifecycle,
        npcHealth,
        ignoreDoesNotTakeDamage: true))
  {
    throw new InvalidOperationException(
      "Projectile NPC targeting did not consume typed NPC chaseability state.");
  }
}

static void VerifyNpcTrapImmunityEligibility()
{
  NpcDefinition defaultDefinition = new(
    DefinitionId: 661,
    NetId: 661,
    MaximumHealth: 5,
    Defense: 0,
    ColliderWidth: 1.0f,
    ColliderHeight: 1.0f,
    BehaviorId: NpcBehaviorId.OrdinaryChase,
    LootTableId: 0);
  if (defaultDefinition.IsTrapImmune)
  {
    throw new InvalidOperationException(
      "The source-default NPC definition unexpectedly enabled trap immunity.");
  }

  NpcDefinition trapImmuneDefinition = new(
    DefinitionId: 662,
    NetId: 662,
    MaximumHealth: 500,
    Defense: 22,
    ColliderWidth: 18.0f,
    ColliderHeight: 40.0f,
    BehaviorId: NpcBehaviorId.OrdinaryChase,
    LootTableId: 0,
    AiStyle: 122,
    IsTrapImmune: true);
  using World spawnWorld = World.Create();
  NpcSpawnCommitSystem spawnSystem = new();
  if (!spawnSystem.TryCommit(
        spawnWorld,
        new NpcDefinitionRegistry([trapImmuneDefinition]),
        new SpawnNpcCommand(
          trapImmuneDefinition.DefinitionId,
          new SimulationVector(1.0f, 1.0f),
          NpcSpawnSource.Command),
        replicationId: 1,
        out NpcSpawnCommitResult spawnResult,
        out string failureReason))
  {
    throw new InvalidOperationException(
      $"Trap-immune NPC spawn was rejected: {failureReason}");
  }

  NpcAuthorityComponent trapImmuneAuthority =
    spawnWorld.Get<NpcAuthorityComponent>(spawnResult.Entity);
  if (!trapImmuneAuthority.IsTrapImmune)
  {
    throw new InvalidOperationException(
      "The NPC spawn commit did not preserve the explicit trap-immunity capability.");
  }
  NpcDefinitionComponent npcDefinition = new(
    trapImmuneDefinition.DefinitionId,
    trapImmuneDefinition.NetId,
    trapImmuneDefinition.Faction,
    trapImmuneDefinition.Category);
  NpcBehaviorStateComponent npcBehavior = new(
    NpcBehaviorId.OrdinaryChase,
    new NpcChaseState(1.0f, 0.0f),
    new NpcTownHomeState(default, true, 0));
  NpcLifecycleComponent npcLifecycle = new(true, 750);
  HealthComponent npcHealth = new(500, 500);
  ProjectileDefinitionComponent trapProjectile = new(
    ProjectileType: 1,
    BehaviorId: 1,
    DefaultDamage: 10,
    DefaultLifetimeTicks: 20,
    Collider: new ColliderComponent(1.0f, 1.0f),
    Friendly: true,
    Hostile: false,
    IsTrap: true);
  ProjectileDefinitionComponent ordinaryProjectile = trapProjectile with { IsTrap = false };
  ProjectileTargetEligibilitySystem system = new();
  if (system.CanTargetNpc(
        trapProjectile,
        npcDefinition,
        trapImmuneAuthority,
        npcBehavior,
        npcLifecycle,
        npcHealth) ||
      !system.CanTargetNpc(
        ordinaryProjectile,
        npcDefinition,
        trapImmuneAuthority,
        npcBehavior,
        npcLifecycle,
        npcHealth))
  {
    throw new InvalidOperationException(
      "Trap immunity did not reject only trap projectiles for the typed NPC owner.");
  }

  NpcAuthorityComponent ordinaryAuthority = new(
    trapImmuneDefinition.AiStyle,
    trapImmuneDefinition.IsImmortal,
    trapImmuneDefinition.AlwaysReplicate,
    trapImmuneDefinition.TakenDamageMultiplier,
    trapImmuneDefinition.NpcSlotCost,
    isTrapImmune: false);
  if (!system.CanTargetNpc(
        trapProjectile,
        npcDefinition,
        ordinaryAuthority,
        npcBehavior,
        npcLifecycle,
        npcHealth))
  {
    throw new InvalidOperationException(
      "Trap immunity incorrectly rejected a trap projectile for a normal NPC.");
  }

  Console.WriteLine("PASS: NPC trap immunity owner gates trap projectile eligibility");
}

static void VerifyProjectileLifetimeBoundary()
{
  using (World forgedLifetimeWorld = World.Create())
  {
    Entity forgedLifetime = forgedLifetimeWorld.Create(new ProjectileLifetimeComponent
    {
      RemainingTicks = int.MinValue
    });
    if (!new ProjectileLifetimeSystem().Advance(forgedLifetime, forgedLifetimeWorld) ||
        forgedLifetimeWorld.Get<ProjectileLifetimeComponent>(forgedLifetime).RemainingTicks != 0)
    {
      throw new InvalidOperationException(
        "Projectile lifetime overflow was not clamped to the expired state.");
    }
  }

  try
  {
    _ = new ProjectileLifetimeComponent(0);
    throw new InvalidOperationException("Projectile lifetime component accepted zero ticks.");
  }
  catch (ArgumentOutOfRangeException)
  {
  }

  try
  {
    _ = new ProjectileLifetimeComponent(-1);
    throw new InvalidOperationException("Projectile lifetime component accepted negative ticks.");
  }
  catch (ArgumentOutOfRangeException)
  {
  }

  ProjectileDefinitionRegistry lifetimeDefinitions = new(
    ProjectileDefinitionRegistry.CreateDefault().OrderedDefinitions.Select(definition =>
      definition.ProjectileType == 1 ? definition with { LifetimeTicks = 30 } : definition));
  using DomeSimulation simulation = new(new WorldGrid(400, 300), lifetimeDefinitions);
  PlayerHandle player = simulation.CreatePlayer(new SimulationVector(10.0f, 0.0f));
  simulation.QueueProjectileSpawn(new SpawnProjectileCommand(
    player,
    10.0f,
    0.0f,
    1,
    10,
    1));
  for (int tick = 0; tick <= 30; tick++)
  {
    simulation.Tick(new SimulationInputBatch());
  }

  ProjectileReplicationSnapshot projectile = simulation
    .CreateProjectileReplicationSnapshots().Single();
  if (projectile.IsActive || projectile.RemainingLifetime > 0 || projectile.Revision <= 0)
  {
    throw new InvalidOperationException(
      "A projectile did not expire at its authoritative definition lifetime boundary.");
  }
}

static void VerifyProjectileBehaviorOverflowBoundary()
{
  using World world = World.Create();
  Entity linearEntity = world.Create(
    new LocationComponent(float.MaxValue, 0.0f),
    new VelocityComponent(float.MaxValue, 0.0f),
    new ProjectileBehaviorComponent(1, default));
  ProjectileBehaviorSystem linear = ProjectileBehaviorSystem.CreateDefault();
  if (linear.TryAdvance(linearEntity, world, 1, out _) ||
      world.Get<LocationComponent>(linearEntity).X != float.MaxValue)
  {
    throw new InvalidOperationException("Linear projectile behavior committed a non-finite position.");
  }

  Entity gravityEntity = world.Create(
    new LocationComponent(0.0f, float.MaxValue),
    new VelocityComponent(0.0f, float.MaxValue),
    new ProjectileBehaviorComponent(2, default));
  if (linear.TryAdvance(gravityEntity, world, 1, out _) ||
      world.Get<LocationComponent>(gravityEntity).Y != float.MaxValue)
  {
    throw new InvalidOperationException("Gravity projectile behavior committed a non-finite position.");
  }
}

static void VerifyProjectileBehaviorReplicationState()
{
  ProjectileBehaviorReplicationState linear = ProjectileBehaviorStateProjection.Project(
    new ProjectileBehaviorComponent(1, new ProjectileBehaviorState(0.0f, 0.0f, 7, 0)));
  if (linear.Ai0 != 7.0f || linear.Ai1 != 0.0f || linear.Ai2 != 0.0f)
  {
    throw new InvalidOperationException("Linear projectile behavior state did not map to Ai0.");
  }

  ProjectileBehaviorReplicationState gravity = ProjectileBehaviorStateProjection.Project(
    new ProjectileBehaviorComponent(2, new ProjectileBehaviorState(-1.25f, 0.0f, 8, 0)));
  if (gravity.Ai0 != -1.25f || gravity.Ai1 != 8.0f || gravity.Ai2 != 0.0f)
  {
    throw new InvalidOperationException(
      "Gravity projectile behavior state did not map to typed AI fields.");
  }

  try
  {
    _ = ProjectileBehaviorStateProjection.Project(
      new ProjectileBehaviorComponent(99, default));
    throw new InvalidOperationException("Unknown projectile behavior state was projected.");
  }
  catch (InvalidDataException)
  {
  }
}

static void VerifyLegacyAiStyle2Behavior()
{
  using World world = World.Create();
  Entity projectile = world.Create(
    new LocationComponent(0.0f, 0.0f),
    new VelocityComponent(10.0f, 0.0f),
    new ProjectileBehaviorComponent(3, default));
  ProjectileBehaviorSystem behaviorSystem = ProjectileBehaviorSystem.CreateDefault();
  for (int tick = 1; tick <= 19; tick++)
  {
    if (!behaviorSystem.TryAdvance(projectile, world, tick, out int behaviorId) || behaviorId != 3)
    {
      throw new InvalidOperationException("Legacy aiStyle 2 behavior was not registered.");
    }
  }

  VelocityComponent delayedVelocity = world.Get<VelocityComponent>(projectile);
  if (delayedVelocity.X != 10.0f || delayedVelocity.Y != 0.0f)
  {
    throw new InvalidOperationException(
      "Legacy aiStyle 2 behavior applied gravity before the source delay elapsed.");
  }

  if (!behaviorSystem.TryAdvance(projectile, world, 20, out _))
  {
    throw new InvalidOperationException("Legacy aiStyle 2 behavior rejected the delay boundary.");
  }

  VelocityComponent acceleratedVelocity = world.Get<VelocityComponent>(projectile);
  ProjectileBehaviorComponent acceleratedBehavior =
    world.Get<ProjectileBehaviorComponent>(projectile);
  if (MathF.Abs(acceleratedVelocity.X - 9.7f) > 0.0001f ||
      MathF.Abs(acceleratedVelocity.Y - 0.4f) > 0.0001f ||
      acceleratedBehavior.State.Primary != 20.0f || acceleratedBehavior.State.Phase != 20)
  {
    throw new InvalidOperationException(
      "Legacy aiStyle 2 behavior did not apply source drag, gravity, and timer state.");
  }

  world.Set(projectile, new VelocityComponent(0.0f, 31.9f));
  world.Set(projectile, new ProjectileBehaviorComponent(
    3,
    new ProjectileBehaviorState(19.0f, 0.0f, 19, 0)));
  if (!behaviorSystem.TryAdvance(projectile, world, 20, out _) ||
      world.Get<VelocityComponent>(projectile).Y != 32.0f)
  {
    throw new InvalidOperationException("Legacy aiStyle 2 behavior did not cap vertical velocity.");
  }

  ProjectileBehaviorReplicationState replicated = ProjectileBehaviorStateProjection.Project(
    world.Get<ProjectileBehaviorComponent>(projectile));
  if (replicated.Ai0 != 20.0f || replicated.Ai1 != 20.0f || replicated.Ai2 != 0.0f)
  {
    throw new InvalidOperationException(
      "Legacy aiStyle 2 timer state did not map to typed AI fields.");
  }
}

static void VerifyLegacyAiStyle2Type21Definition()
{
  ProjectileDefinitionRegistry baseDefinitions = ProjectileDefinitionRegistry.CreateDefault();
  ProjectileDefinitionRegistry definitions = new(baseDefinitions.OrderedDefinitions.Select(
    definition => definition.ProjectileType == 370
      ? definition with { LifetimeTicks = 1 }
      : definition));
  if (!definitions.TryGet(21, out ProjectileDefinition definition) ||
      definition.BehaviorId != LegacyAiStyle2ProjectileBehavior.Id || !definition.Friendly ||
      definition.Hostile || definition.MaximumPenetration != 1 ||
      definition.Collider.Width != 1.0f || definition.Collider.Height != 1.0f ||
      definition.LifetimeTicks != 3600)
  {
    throw new InvalidOperationException(
      "Default registry did not expose the source-backed type 21 generic aiStyle 2 definition.");
  }

  using DomeSimulation simulation = new(new WorldGrid(400, 300));
  PlayerHandle owner = simulation.CreatePlayer(new SimulationVector(0.0f, 0.0f));
  simulation.QueueProjectileSpawn(new SpawnProjectileCommand(
    owner,
    10.0f,
    0.0f,
    1,
    10,
    20,
    ProjectileType: 21,
    ProjectileSpeed: 10.0f));
  for (int tick = 1; tick <= 21; tick++)
  {
    simulation.Tick(new SimulationInputBatch());
  }

  ProjectileReplicationSnapshot snapshot = simulation
    .CreateProjectileReplicationSnapshots()
    .Single();
  if (snapshot.ProjectileType != 21 ||
      MathF.Abs(snapshot.Velocity.X - 9.7f) > 0.0001f ||
      MathF.Abs(snapshot.Velocity.Y - 0.4f) > 0.0001f ||
      snapshot.Ai0 != 20.0f || snapshot.Ai1 != 21.0f || snapshot.Ai2 != 0.0f)
  {
    throw new InvalidOperationException(
      "Type 21 did not use the generic source-backed aiStyle 2 motion and projection.");
  }
}

static void VerifyLegacyAiStyle2Type330Definition()
{
  ProjectileDefinitionRegistry definitions = ProjectileDefinitionRegistry.CreateDefault();
  if (!definitions.TryGet(330, out ProjectileDefinition definition) ||
      definition.BehaviorId != LegacyAiStyle2ProjectileBehavior.Id || !definition.Friendly ||
      definition.Hostile || definition.MaximumPenetration != 6 ||
      definition.Collider.Width != 1.375f || definition.Collider.Height != 1.375f ||
      definition.LifetimeTicks != 3600)
  {
    throw new InvalidOperationException(
      "Default registry did not expose the source-backed type 330 definition.");
  }

  using DomeSimulation simulation = new(new WorldGrid(400, 300));
  PlayerHandle owner = simulation.CreatePlayer(new SimulationVector(10.0f, 0.0f));
  simulation.QueueProjectileSpawn(new SpawnProjectileCommand(
    owner,
    10.0f,
    0.0f,
    1,
    10,
    20,
    ProjectileType: 330,
    ProjectileSpeed: 10.0f));
  for (int tick = 1; tick <= 21; tick++)
  {
    simulation.Tick(new SimulationInputBatch());
  }

  ProjectileReplicationSnapshot snapshot = simulation
    .CreateProjectileReplicationSnapshots()
    .Single();
  if (snapshot.ProjectileType != 330 ||
      MathF.Abs(snapshot.Velocity.X - 9.7f) > 0.0001f ||
      MathF.Abs(snapshot.Velocity.Y - 0.4f) > 0.0001f ||
      snapshot.Ai0 != 20.0f || snapshot.Ai1 != 21.0f || snapshot.Ai2 != 0.0f)
  {
    throw new InvalidOperationException(
      "Type 330 did not use the generic source-backed aiStyle 2 motion and projection.");
  }
}

static void VerifyLegacyAiStyle2Type589Definition()
{
  ProjectileDefinitionRegistry definitions = ProjectileDefinitionRegistry.CreateDefault();
  if (!definitions.TryGet(589, out ProjectileDefinition definition) ||
      definition.BehaviorId != LegacyAiStyle2ProjectileBehavior.Id || !definition.Friendly ||
      definition.Hostile || definition.MaximumPenetration != 1 ||
      definition.Collider.Width != 0.625f || definition.Collider.Height != 0.625f ||
      definition.LifetimeTicks != 3600)
  {
    throw new InvalidOperationException(
      "Default registry did not expose the source-backed type 589 definition.");
  }

  using DomeSimulation simulation = new(new WorldGrid(400, 300));
  PlayerHandle owner = simulation.CreatePlayer(new SimulationVector(10.0f, 0.0f));
  simulation.QueueProjectileSpawn(new SpawnProjectileCommand(
    owner,
    10.0f,
    0.0f,
    1,
    10,
    20,
    ProjectileType: 589,
    ProjectileSpeed: 10.0f));
  for (int tick = 1; tick <= 21; tick++)
  {
    simulation.Tick(new SimulationInputBatch());
  }

  ProjectileReplicationSnapshot snapshot = simulation
    .CreateProjectileReplicationSnapshots()
    .Single();
  if (snapshot.ProjectileType != 589 ||
      MathF.Abs(snapshot.Velocity.X - 9.7f) > 0.0001f ||
      MathF.Abs(snapshot.Velocity.Y - 0.4f) > 0.0001f ||
      snapshot.Ai0 != 20.0f || snapshot.Ai1 != 21.0f || snapshot.Ai2 != 0.0f)
  {
    throw new InvalidOperationException(
      "Type 589 did not use the generic source-backed aiStyle 2 motion and projection.");
  }
}

static void VerifyLegacyAiStyle2Type1012Definition()
{
  ProjectileDefinitionRegistry definitions = ProjectileDefinitionRegistry.CreateDefault();
  if (!definitions.TryGet(1012, out ProjectileDefinition definition) ||
      definition.BehaviorId != LegacyAiStyle2ProjectileBehavior.Id || !definition.Friendly ||
      definition.Hostile || definition.MaximumPenetration != 1 ||
      definition.Collider.Width != 1.125f || definition.Collider.Height != 1.125f ||
      definition.LifetimeTicks != 3600)
  {
    throw new InvalidOperationException(
      "Default registry did not expose the source-backed type 1012 generic aiStyle 2 definition.");
  }

  using DomeSimulation simulation = new(new WorldGrid(400, 300));
  PlayerHandle player = simulation.CreatePlayer(new SimulationVector(10.0f, 0.0f));
  simulation.QueueProjectileSpawn(new SpawnProjectileCommand(
    player,
    10.0f,
    0.0f,
    1,
    10,
    20,
    ProjectileType: 1012,
    ProjectileSpeed: 10.0f));
  for (int tick = 1; tick <= 21; tick++)
  {
    simulation.Tick(new SimulationInputBatch());
  }

  ProjectileReplicationSnapshot type1012 = simulation
    .CreateProjectileReplicationSnapshots()
    .Single();
  if (type1012.ProjectileType != 1012 ||
      MathF.Abs(type1012.Velocity.X - 9.7f) > 0.0001f ||
      MathF.Abs(type1012.Velocity.Y - 0.4f) > 0.0001f || type1012.Ai0 != 20.0f ||
      type1012.Ai1 != 21.0f || type1012.Ai2 != 0.0f)
  {
    throw new InvalidOperationException(
      "Default type 1012 projectile did not use the generic aiStyle 2 source behavior.");
  }
}

static void VerifyLegacyAiStyle2Type304Lifecycle()
{
  ProjectileDefinitionRegistry definitions = ProjectileDefinitionRegistry.CreateDefault();
  if (!definitions.TryGet(304, out ProjectileDefinition definition) ||
      definition.BehaviorId != LegacyAiStyle2Type304ProjectileBehavior.Id ||
      !definition.Friendly ||
      definition.Hostile || definition.MaximumPenetration != 1 ||
      definition.Collider.Width != 1.875f || definition.Collider.Height != 1.875f ||
      definition.LifetimeTicks != 3600)
  {
    throw new InvalidOperationException(
      "Default registry did not expose the source-backed type 304 lifecycle definition.");
  }

  using World world = World.Create();
  Entity projectile = world.Create(
    new LocationComponent(0.0f, 0.0f),
    new VelocityComponent(10.0f, 0.0f),
    new ProjectileBehaviorComponent(LegacyAiStyle2Type304ProjectileBehavior.Id, default));
  ProjectileBehaviorSystem behaviorSystem = ProjectileBehaviorSystem.CreateDefault();
  ProjectileBehaviorEffectSystem effects = new();
  Entity cappedProjectile = world.Create(
    new LocationComponent(0.0f, 0.0f),
    new VelocityComponent(0.0f, 35.0f),
    new ProjectileBehaviorComponent(LegacyAiStyle2Type304ProjectileBehavior.Id, default));
  if (!behaviorSystem.TryAdvance(cappedProjectile, world, 1, out _) ||
      world.Get<VelocityComponent>(cappedProjectile).Y != 32.0f)
  {
    throw new InvalidOperationException("Type 304 did not retain the shared aiStyle 2 cap.");
  }

  ProjectileDamageComponent damage = new(100);
  ProjectileDefinitionComponent runtimeDefinition = new(
    304,
    LegacyAiStyle2Type304ProjectileBehavior.Id,
    100,
    3600,
    new ColliderComponent(1.875f, 1.875f),
    Friendly: true,
    Hostile: false,
    Knockback: 10.0f);
  ProjectileLifetimeComponent lifetime = new(3600);
  for (int tick = 1; tick <= 29; tick++)
  {
    if (!behaviorSystem.TryAdvance(projectile, world, tick, out int behaviorId) ||
        behaviorId != LegacyAiStyle2Type304ProjectileBehavior.Id)
    {
      throw new InvalidOperationException("Type 304 behavior was not registered.");
    }

    effects.Apply(
      ref damage,
      ref runtimeDefinition,
      ref lifetime,
      world.Get<ProjectileBehaviorComponent>(projectile));
  }

  if (damage.Amount != 100 || runtimeDefinition.Knockback != 10.0f ||
      lifetime.RemainingTicks != 3600 ||
      world.Get<ProjectileBehaviorComponent>(projectile).State.LocalAi0 != 29.0f ||
      !behaviorSystem.TryAdvance(projectile, world, 30, out _))
  {
    throw new InvalidOperationException(
      "Type 304 decayed combat state before its source boundary.");
  }

  effects.Apply(
    ref damage,
    ref runtimeDefinition,
    ref lifetime,
    world.Get<ProjectileBehaviorComponent>(projectile));
  if (world.Get<ProjectileBehaviorComponent>(projectile).State.LocalAi0 != 30.0f)
  {
    throw new InvalidOperationException("Type 304 localAI[0] did not advance with its behavior tick.");
  }
  if (damage.Amount != 90 || runtimeDefinition.Knockback != 9.0f ||
      lifetime.RemainingTicks != 3600)
  {
    throw new InvalidOperationException(
      "Type 304 did not apply source damage and knockback decay.");
  }

  for (int tick = 31; tick <= 55; tick++)
  {
    if (!behaviorSystem.TryAdvance(projectile, world, tick, out _))
    {
      throw new InvalidOperationException("Type 304 behavior rejected a valid lifecycle tick.");
    }

    effects.Apply(
      ref damage,
      ref runtimeDefinition,
      ref lifetime,
      world.Get<ProjectileBehaviorComponent>(projectile));
  }

  ProjectileBehaviorReplicationState state = ProjectileBehaviorStateProjection.Project(
    world.Get<ProjectileBehaviorComponent>(projectile));
  if (damage.Amount != 2 || runtimeDefinition.Knockback != 0.0f ||
      lifetime.RemainingTicks != 0 ||
      state.Ai0 != 55.0f || state.Ai1 != 55.0f || state.Ai2 != 0.0f)
  {
    throw new InvalidOperationException("Type 304 did not complete its source lifecycle state.");
  }

  using DomeSimulation simulation = new(new WorldGrid(400, 300));
  PlayerHandle player = simulation.CreatePlayer(new SimulationVector(10.0f, 0.0f));
  simulation.QueueProjectileSpawn(new SpawnProjectileCommand(
    player,
    10.0f,
    0.0f,
    1,
    10,
    20,
    ProjectileType: 304,
    ProjectileSpeed: 4.0f));
  for (int tick = 1; tick <= 56; tick++)
  {
    simulation.Tick(new SimulationInputBatch());
  }

  ProjectileReplicationSnapshot tombstone = simulation
    .CreateProjectileReplicationSnapshots()
    .Single();
  if (tombstone.IsActive || tombstone.TombstoneReason != ProjectileTombstoneReason.Expired ||
      tombstone.RemainingLifetime != 0)
  {
    throw new InvalidOperationException(
      $"Type 304 did not commit its terminal expired tombstone: active={tombstone.IsActive}, " +
      $"reason={tombstone.TombstoneReason}, lifetime={tombstone.RemainingLifetime}, " +
      $"ai0={tombstone.Ai0}, revision={tombstone.Revision}.");
  }
}

static void VerifyLegacyAiStyle2Type166Behavior()
{
  ProjectileDefinitionRegistry definitions = ProjectileDefinitionRegistry.CreateDefault();
  if (!definitions.TryGet(166, out ProjectileDefinition definition) ||
      definition.BehaviorId != LegacyAiStyle2Type166ProjectileBehavior.Id ||
      !definition.Friendly || definition.Hostile || definition.MaximumPenetration != 1 ||
      definition.Collider.Width != 0.875f || definition.Collider.Height != 0.875f ||
      definition.LifetimeTicks != 3600)
  {
    throw new InvalidOperationException(
      "Default registry did not expose the source-backed type 166 definition.");
  }

  using World world = World.Create();
  Entity projectile = world.Create(
    new LocationComponent(0.0f, 0.0f),
    new VelocityComponent(10.0f, 0.0f),
    new ProjectileBehaviorComponent(LegacyAiStyle2Type166ProjectileBehavior.Id, default));
  ProjectileBehaviorSystem behaviorSystem = ProjectileBehaviorSystem.CreateDefault();
  for (int tick = 1; tick < 20; tick++)
  {
    if (!behaviorSystem.TryAdvance(projectile, world, tick, out int behaviorId) ||
        behaviorId != LegacyAiStyle2Type166ProjectileBehavior.Id)
    {
      throw new InvalidOperationException("Type 166 behavior was not registered.");
    }
  }

  VelocityComponent beforeGravity = world.Get<VelocityComponent>(projectile);
  if (beforeGravity.X != 10.0f || beforeGravity.Y != 0.0f ||
      !behaviorSystem.TryAdvance(projectile, world, 20, out _) ||
      MathF.Abs(world.Get<VelocityComponent>(projectile).X - 9.8f) > 0.0001f ||
      MathF.Abs(world.Get<VelocityComponent>(projectile).Y - 0.3f) > 0.0001f)
  {
    throw new InvalidOperationException(
      "Type 166 did not preserve its source timer, drag, and gravity boundary.");
  }

  ProjectileBehaviorReplicationState state = ProjectileBehaviorStateProjection.Project(
    world.Get<ProjectileBehaviorComponent>(projectile));
  if (state.Ai0 != 20.0f || state.Ai1 != 20.0f || state.Ai2 != 0.0f)
  {
    throw new InvalidOperationException("Type 166 timer state did not project to typed AI fields.");
  }

  world.Set(projectile, new VelocityComponent(0.0f, 31.9f));
  world.Set(projectile, new ProjectileBehaviorComponent(
    LegacyAiStyle2Type166ProjectileBehavior.Id,
    new ProjectileBehaviorState(19.0f, 0.0f, 19, 0)));
  if (!behaviorSystem.TryAdvance(projectile, world, 20, out _) ||
      world.Get<VelocityComponent>(projectile).Y != 32.0f)
  {
    throw new InvalidOperationException("Type 166 did not retain the shared aiStyle 2 cap.");
  }
}

static void VerifyLegacyAiStyle2Type48Definition()
{
  using DomeSimulation simulation = new(new WorldGrid(400, 300));
  PlayerHandle player = simulation.CreatePlayer(new SimulationVector(10.0f, 0.0f));
  simulation.QueueProjectileSpawn(new SpawnProjectileCommand(
    player,
    10.0f,
    0.0f,
    1,
    10,
    20,
    ProjectileType: 48,
    ProjectileSpeed: 10.0f));
  for (int tick = 1; tick <= 21; tick++)
  {
    simulation.Tick(new SimulationInputBatch());
  }

  IReadOnlyList<ProjectileReplicationSnapshot> snapshots =
    simulation.CreateProjectileReplicationSnapshots();
  if (snapshots.Count != 1)
  {
    throw new InvalidOperationException(
      "Default type 48 projectile definition did not spawn through the authoritative registry.");
  }

  ProjectileReplicationSnapshot type48 = snapshots[0];
  if (type48.ProjectileType != 48 || MathF.Abs(type48.Velocity.X - 9.7f) > 0.0001f ||
      MathF.Abs(type48.Velocity.Y - 0.4f) > 0.0001f ||
      type48.Ai0 != 20.0f || type48.Ai1 != 21.0f || type48.Ai2 != 0.0f)
  {
    throw new InvalidOperationException(
      $"Default type 48 projectile did not use the generic aiStyle 2 source behavior: " +
      $"type={type48.ProjectileType}, vx={type48.Velocity.X}, vy={type48.Velocity.Y}, " +
      $"ai0={type48.Ai0}, ai1={type48.Ai1}, ai2={type48.Ai2}.");
  }
}

static void VerifyLegacyAiStyle2Type599Definition()
{
  using DomeSimulation simulation = new(new WorldGrid(400, 300));
  PlayerHandle player = simulation.CreatePlayer(new SimulationVector(10.0f, 0.0f));
  simulation.QueueProjectileSpawn(new SpawnProjectileCommand(
    player,
    10.0f,
    0.0f,
    1,
    10,
    20,
    ProjectileType: 599,
    ProjectileSpeed: 10.0f));
  for (int tick = 1; tick <= 21; tick++)
  {
    simulation.Tick(new SimulationInputBatch());
  }

  IReadOnlyList<ProjectileReplicationSnapshot> snapshots =
    simulation.CreateProjectileReplicationSnapshots();
  if (snapshots.Count != 1)
  {
    throw new InvalidOperationException(
      "Default type 599 projectile definition did not spawn through the authoritative registry.");
  }

  ProjectileReplicationSnapshot type599 = snapshots[0];
  if (type599.ProjectileType != 599 ||
      MathF.Abs(type599.Velocity.X - 9.7f) > 0.0001f ||
      MathF.Abs(type599.Velocity.Y - 0.4f) > 0.0001f ||
      type599.Ai0 != 20.0f || type599.Ai1 != 21.0f || type599.Ai2 != 0.0f)
  {
    throw new InvalidOperationException(
      "Default type 599 projectile did not use the generic aiStyle 2 source behavior.");
  }
}

static void VerifyLegacyAiStyle2Type520Definition()
{
  using DomeSimulation simulation = new(new WorldGrid(400, 300));
  PlayerHandle player = simulation.CreatePlayer(new SimulationVector(10.0f, 0.0f));
  simulation.QueueProjectileSpawn(new SpawnProjectileCommand(
    player,
    10.0f,
    0.0f,
    1,
    10,
    20,
    ProjectileType: 520,
    ProjectileSpeed: 10.0f));
  for (int tick = 1; tick <= 21; tick++)
  {
    simulation.Tick(new SimulationInputBatch());
  }

  IReadOnlyList<ProjectileReplicationSnapshot> snapshots =
    simulation.CreateProjectileReplicationSnapshots();
  if (snapshots.Count != 1)
  {
    throw new InvalidOperationException(
      "Default type 520 projectile definition did not spawn through the authoritative registry.");
  }

  ProjectileReplicationSnapshot type520 = snapshots[0];
  if (type520.ProjectileType != 520 ||
      MathF.Abs(type520.Velocity.X - 9.7f) > 0.0001f ||
      MathF.Abs(type520.Velocity.Y - 0.4f) > 0.0001f ||
      type520.Ai0 != 20.0f || type520.Ai1 != 21.0f || type520.Ai2 != 0.0f)
  {
    throw new InvalidOperationException(
      "Default type 520 projectile did not use the generic aiStyle 2 source behavior.");
  }
}

static void VerifyLegacyAiStyle2Type471Definition()
{
  ProjectileDefinitionRegistry definitions = ProjectileDefinitionRegistry.CreateDefault();
  if (!definitions.TryGet(471, out ProjectileDefinition definition) ||
      definition.BehaviorId != LegacyAiStyle2ProjectileBehavior.Id || definition.Friendly ||
      !definition.Hostile || definition.MaximumPenetration != 1 ||
      definition.Collider.Width != 1.0f || definition.Collider.Height != 1.0f ||
      definition.LifetimeTicks != 3600 ||
      definition.PlayerDamagePolicy != PlayerDamagePolicy.HostileNonPvp)
  {
    throw new InvalidOperationException(
      "Default registry did not expose the source-backed hostile type 471 definition.");
  }

  using DomeSimulation simulation = new(new WorldGrid(400, 300));
  PlayerHandle player = simulation.CreatePlayer(new SimulationVector(10.0f, 0.0f));
  simulation.QueueProjectileSpawn(new SpawnProjectileCommand(
    player,
    10.0f,
    0.0f,
    1,
    10,
    20,
    ProjectileType: 471,
    ProjectileSpeed: 10.0f));
  for (int tick = 1; tick <= 21; tick++)
  {
    simulation.Tick(new SimulationInputBatch());
  }

  ProjectileReplicationSnapshot type471 = simulation.CreateProjectileReplicationSnapshots().Single();
  if (type471.ProjectileType != 471 || MathF.Abs(type471.Velocity.X - 9.7f) > 0.0001f ||
      MathF.Abs(type471.Velocity.Y - 0.4f) > 0.0001f || type471.Ai0 != 20.0f ||
      type471.Ai1 != 21.0f || type471.Ai2 != 0.0f)
  {
    throw new InvalidOperationException(
      "Default type 471 projectile did not use the generic aiStyle 2 source behavior.");
  }
}

static void VerifyLegacyAiStyle2Type162FriendlyAreaDamage()
{
  ProjectileDefinitionRegistry definitions = ProjectileDefinitionRegistry.CreateDefault();
  if (!definitions.TryGet(162, out ProjectileDefinition defaultDefinition) ||
      defaultDefinition.BehaviorId != 13 || !defaultDefinition.Friendly ||
      defaultDefinition.Hostile || defaultDefinition.MaximumPenetration != 4 ||
      defaultDefinition.Collider.Width != 1.0f || defaultDefinition.Collider.Height != 1.0f ||
      defaultDefinition.OnDespawnAreaDamage.Width != 4.0f ||
      defaultDefinition.OnDespawnAreaDamage.Height != 4.0f)
  {
    throw new InvalidOperationException(
      "Default registry did not expose the source-backed type 162 area-damage definition.");
  }

  using World behaviorWorld = World.Create();
  Entity behaviorProjectile = behaviorWorld.Create(
    new LocationComponent(0.0f, 0.0f),
    new VelocityComponent(10.0f, 0.0f),
    new ProjectileBehaviorComponent(13, default));
  ProjectileBehaviorSystem behaviorSystem = ProjectileBehaviorSystem.CreateDefault();
  for (int tick = 1; tick < 18; tick++)
  {
    if (!behaviorSystem.TryAdvance(behaviorProjectile, behaviorWorld, tick, out int behaviorId) ||
        behaviorId != 13)
    {
      throw new InvalidOperationException("Type 162 behavior was not registered.");
    }
  }

  VelocityComponent beforeGravity = behaviorWorld.Get<VelocityComponent>(behaviorProjectile);
  if (beforeGravity.X != 10.0f || beforeGravity.Y != 0.0f ||
      !behaviorSystem.TryAdvance(behaviorProjectile, behaviorWorld, 18, out _) ||
      MathF.Abs(behaviorWorld.Get<VelocityComponent>(behaviorProjectile).X - 9.9f) > 0.0001f ||
      MathF.Abs(behaviorWorld.Get<VelocityComponent>(behaviorProjectile).Y - 0.28f) > 0.0001f)
  {
    throw new InvalidOperationException(
      "Type 162 did not preserve its source timer, drag, and gravity boundary.");
  }

  behaviorWorld.Set(behaviorProjectile, new VelocityComponent(0.0f, 31.9f));
  behaviorWorld.Set(behaviorProjectile, new ProjectileBehaviorComponent(
    LegacyAiStyle2Type162ProjectileBehavior.Id,
    new ProjectileBehaviorState(17.0f, 0.0f, 17, 0)));
  if (!behaviorSystem.TryAdvance(behaviorProjectile, behaviorWorld, 18, out _) ||
      behaviorWorld.Get<VelocityComponent>(behaviorProjectile).Y != 32.0f)
  {
    throw new InvalidOperationException("Type 162 did not retain the shared aiStyle 2 cap.");
  }

  ProjectileDefinitionRegistry fixtureDefinitions = new(definitions.OrderedDefinitions.Select(
    definition => definition.ProjectileType == 162
      ? definition with { Damage = 20, LifetimeTicks = 1 }
      : definition));
  using DomeSimulation simulation = new(new WorldGrid(400, 300), fixtureDefinitions);
  PlayerHandle owner = simulation.CreatePlayer(new SimulationVector(0.0f, 0.0f));
  NpcHandle inArea = simulation.CreateNpc(new SimulationVector(14.0f, 0.0f), definitionId: 2);
  NpcHandle outOfArea = simulation.CreateNpc(new SimulationVector(20.0f, 0.0f), definitionId: 2);
  simulation.QueueProjectileSpawn(new SpawnProjectileCommand(
    owner,
    8.0f,
    0.0f,
    1,
    20,
    1,
    ProjectileType: 162,
    ProjectileSpeed: 4.0f));
  for (int tick = 0; tick < 4; tick++)
  {
    simulation.Tick(new SimulationInputBatch());
  }

  ProjectileReplicationSnapshot expired = simulation
    .CreateProjectileReplicationSnapshots()
    .Single();
  if (expired.IsActive || expired.TombstoneReason != ProjectileTombstoneReason.Expired ||
      simulation.CreateSnapshot().FindNpc(inArea).Health != 80 ||
      simulation.CreateSnapshot().FindNpc(outOfArea).Health != 100 ||
      simulation.CreatePlayerStateSnapshot(owner).Health != 100)
  {
    throw new InvalidOperationException(
      "Type 162 expiry did not apply friendly area damage only to in-area NPC targets.");
  }
}

static void VerifyLegacyAiStyle2StatusEffectFamily()
{
  ProjectileDefinitionRegistry baseDefinitions = ProjectileDefinitionRegistry.CreateDefault();
  ProjectileDefinitionRegistry definitions = new(baseDefinitions.OrderedDefinitions.Select(
    definition => definition.ProjectileType == 370
      ? definition with { LifetimeTicks = 1 }
      : definition));
  using DomeSimulation simulation = new(new WorldGrid(400, 300), definitions);
  PlayerHandle owner = simulation.CreatePlayer(new SimulationVector(0.0f, 0.0f));
  PlayerHandle playerInRange = simulation.CreatePlayer(new SimulationVector(3.0f, 0.0f));
  PlayerHandle playerAtBoundary = simulation.CreatePlayer(new SimulationVector(5.1f, 0.0f));
  NpcHandle npcInRange = simulation.CreateNpc(new SimulationVector(3.0f, 0.0f));
  NpcHandle npcAtBoundary = simulation.CreateNpc(new SimulationVector(5.1f, 0.0f));
  simulation.QueueProjectileSpawn(new SpawnProjectileCommand(
    owner,
    0.0f,
    0.0f,
    0,
    0,
    1,
    ProjectileType: 370,
    ProjectileSpeed: 1.0f));
  simulation.Tick(new SimulationInputBatch());
  simulation.Tick(new SimulationInputBatch());

  IReadOnlyList<StatusEffectSnapshot> statuses = simulation.CreateStatusEffectSnapshots();
  IReadOnlyList<PlayerStatusEffectStateSnapshot> playerStates =
    simulation.CreatePlayerStatusEffectStateSnapshots();
  PlayerStatusEffectStateSnapshot playerState = playerStates.Single(state =>
    state.PlayerId == playerInRange.Value);
  if (playerState.Effects.Count != 1 || playerState.Effects[0].Type != 119 ||
      playerState.Effects[0].RemainingTicks != 1800)
  {
    throw new InvalidOperationException(
      "Player status effects did not project through the authoritative aggregate snapshot.");
  }

  if (!HasStatus(statuses, StatusEffectTargetKind.Player, playerInRange.Value, 119, 1800) ||
      !HasStatus(statuses, StatusEffectTargetKind.Npc, npcInRange.Value, 119, 1800) ||
      HasStatus(statuses, StatusEffectTargetKind.Player, playerAtBoundary.Value, 119, 1800) ||
      HasStatus(statuses, StatusEffectTargetKind.Npc, npcAtBoundary.Value, 119, 1800))
  {
    throw new InvalidOperationException(
      "Type 370 did not commit its source-backed status effect to the eligible target set.");
  }
}

static void VerifyLegacyAiStyle2HitStatusFamily()
{
  ProjectileDefinitionRegistry definitions = ProjectileDefinitionRegistry.CreateDefault();
  if (!definitions.TryGet(504, out ProjectileDefinition definition) ||
      definition.BehaviorId != 18 || !definition.Friendly || definition.Hostile ||
      definition.MaximumPenetration != 2 || definition.Collider.Width != 0.625f ||
      definition.Collider.Height != 0.625f || definition.LifetimeTicks != 3600)
  {
    throw new InvalidOperationException(
      "Default registry did not expose the source-backed type 504 hit-status definition.");
  }

  if (!definition.OnHitStatusEffect.IsEnabled ||
      definition.OnHitStatusEffect.Type != 323 ||
      definition.OnHitStatusEffect.MinimumDurationTicks != 60 ||
      definition.OnHitStatusEffect.MaximumDurationTicks != 239)
  {
    throw new InvalidOperationException(
      "Type 504 did not expose its accepted-hit status-effect definition.");
  }

  using DomeSimulation hitSimulation = new(new WorldGrid(400, 300));
  PlayerHandle hitOwner = hitSimulation.CreatePlayer(new SimulationVector(0.0f, 0.0f));
  NpcHandle target = hitSimulation.CreateNpc(new SimulationVector(0.0f, 0.0f), definitionId: 488);
  hitSimulation.QueueProjectileSpawn(new SpawnProjectileCommand(
    hitOwner,
    0.0f,
    0.0f,
    0,
    10,
    20,
    ProjectileType: 504,
    ProjectileSpeed: 0.0f));
  hitSimulation.Tick(new SimulationInputBatch());
  hitSimulation.Tick(new SimulationInputBatch());

  IReadOnlyList<StatusEffectSnapshot> hitStatuses = hitSimulation.CreateStatusEffectSnapshots();
  StatusEffectSnapshot? hitStatus = hitStatuses.FirstOrDefault(status =>
    status.TargetKind == StatusEffectTargetKind.Npc &&
    status.TargetId == target.Value && status.Type == 323);
  if (hitStatus is not StatusEffectSnapshot acceptedHitStatus ||
      acceptedHitStatus.RemainingTicks < 59 || acceptedHitStatus.RemainingTicks > 239)
  {
    throw new InvalidOperationException(
      "Accepted type 504 hit did not commit its typed status effect to the NPC target.");
  }

  using DomeSimulation simulation = new(new WorldGrid(400, 300));
  PlayerHandle owner = simulation.CreatePlayer(new SimulationVector(0.0f, 0.0f));
  simulation.QueueProjectileSpawn(new SpawnProjectileCommand(
    owner,
    10.0f,
    0.0f,
    1,
    10,
    20,
    ProjectileType: 504,
    ProjectileSpeed: 0.0f));
  for (int tick = 1; tick <= 21; tick++)
  {
    simulation.Tick(new SimulationInputBatch());
  }

  ProjectileReplicationSnapshot accelerating = simulation
    .CreateProjectileReplicationSnapshots().Single();
  if (!accelerating.IsActive || accelerating.Ai0 != 20.0f || accelerating.Ai1 != 21.0f ||
      MathF.Abs(accelerating.Velocity.X - 3.96f) > 0.0001f ||
      MathF.Abs(accelerating.Velocity.Y - 0.1f) > 0.0001f)
  {
    throw new InvalidOperationException(
      "Type 504 did not apply its source tick-20 motion and replicated timer state.");
  }

  for (int tick = 22; tick <= 61; tick++)
  {
    simulation.Tick(new SimulationInputBatch());
  }

  ProjectileReplicationSnapshot expired = simulation.CreateProjectileReplicationSnapshots().Single();
  if (expired.IsActive || expired.TombstoneReason != ProjectileTombstoneReason.Expired ||
      expired.Ai0 != 60.0f || expired.Ai1 != 61.0f)
  {
    throw new InvalidOperationException(
      "Type 504 did not commit its source terminal lifetime through the tombstone path.");
  }
}

static void VerifyLegacyAiStyle2HitStatusDefinitions(int projectileType, ushort statusType)
{
  ProjectileDefinitionRegistry definitions = ProjectileDefinitionRegistry.CreateDefault();
  if (!definitions.TryGet(projectileType, out ProjectileDefinition definition) ||
      definition.BehaviorId != 18 || !definition.Friendly || definition.Hostile ||
      definition.MaximumPenetration != 2 || definition.OnHitStatusEffect.Type != statusType ||
      definition.OnHitStatusEffect.MinimumDurationTicks != 60 ||
      definition.OnHitStatusEffect.MaximumDurationTicks != 239)
  {
    throw new InvalidOperationException(
      $"Default type {projectileType} did not expose its source-backed hit-status definition.");
  }

  using DomeSimulation simulation = new(new WorldGrid(400, 300));
  PlayerHandle owner = simulation.CreatePlayer(new SimulationVector(0.0f, 0.0f));
  NpcHandle target = simulation.CreateNpc(new SimulationVector(0.0f, 0.0f), definitionId: 488);
  simulation.QueueProjectileSpawn(new SpawnProjectileCommand(
    owner,
    0.0f,
    0.0f,
    0,
    10,
    20,
    ProjectileType: projectileType,
    ProjectileSpeed: 0.0f));
  simulation.Tick(new SimulationInputBatch());
  simulation.Tick(new SimulationInputBatch());
  simulation.Tick(new SimulationInputBatch());

  StatusEffectSnapshot? status = simulation.CreateStatusEffectSnapshots().FirstOrDefault(
    candidate => candidate.TargetKind == StatusEffectTargetKind.Npc &&
      candidate.TargetId == target.Value && candidate.Type == statusType);
  if (status is not StatusEffectSnapshot accepted ||
      accepted.RemainingTicks < 59 || accepted.RemainingTicks > 239)
  {
    throw new InvalidOperationException(
      $"Accepted type {projectileType} hit did not commit status {statusType}.");
  }

}

static void VerifyProjectileTileConversionOnDespawn(
  int projectileType,
  ushort conversionType,
  ushort expectedTileType)
{
  ProjectileDefinitionRegistry baseDefinitions = ProjectileDefinitionRegistry.CreateDefault();
  ProjectileDefinitionRegistry definitions = new(baseDefinitions.OrderedDefinitions.Select(
    definition => definition.ProjectileType == projectileType
      ? definition with { LifetimeTicks = 1 }
      : definition));
  using DomeSimulation simulation = new(new WorldGrid(400, 300), definitions);
  PlayerHandle owner = simulation.CreatePlayer(new SimulationVector(0.0f, 0.0f));
  simulation.QueueProjectileSpawn(new SpawnProjectileCommand(
    owner,
    0.0f,
    0.0f,
    1,
    10,
    20,
    ProjectileType: projectileType,
    ProjectileSpeed: 0.0f));
  simulation.Tick(new SimulationInputBatch());
  simulation.Tick(new SimulationInputBatch());

  bool converted = false;
  for (int x = 0; x < simulation.WorldGrid.Width; x++)
  {
    WorldTile tile = simulation.WorldGrid.GetTile(x, 0);
    if (tile.IsActive && tile.Type == expectedTileType)
    {
      converted = true;
      break;
    }
  }
  if (!converted)
  {
    throw new InvalidOperationException(
      $"Type {projectileType} despawn did not commit conversion {conversionType}.");
  }
}

static void VerifyType281ReleaseContract()
{
  SpawnNpcCommand command = new(
    614,
    new SimulationVector(1.0f, 2.0f),
    NpcSpawnSource.Command,
    ReleaseOwner: 7,
    ReleaseVariant: 3);
  if (command.DefinitionId != 614 || command.ReleaseOwner != 7 || command.ReleaseVariant != 3)
  {
    throw new InvalidOperationException("Type 281 release command did not preserve owner variant.");
  }
}

static void VerifyType281TerminalNpcRelease()
{
  ProjectileDefinitionRegistry defaults = ProjectileDefinitionRegistry.CreateDefault();
  ProjectileDefinitionRegistry definitions = new(defaults.OrderedDefinitions.Select(definition =>
    definition.ProjectileType == 281 ? definition with { LifetimeTicks = 1 } : definition));
  using DomeSimulation simulation = new(new WorldGrid(400, 300), definitions);
  PlayerHandle owner = simulation.CreatePlayer(new SimulationVector(10.0f, 10.0f));
  simulation.QueueProjectileSpawn(new SpawnProjectileCommand(
    owner, 10.0f, 10.0f, 1, 10, 20, ProjectileType: 281, ProjectileSpeed: 1.0f));
  simulation.Tick(new SimulationInputBatch());
  simulation.Tick(new SimulationInputBatch());
  simulation.Tick(new SimulationInputBatch());
  NpcReplicationSnapshot? release = simulation.CreateNpcReplicationSnapshots().FirstOrDefault(
    snapshot => snapshot.DefinitionId == 614);
  if (release is not NpcReplicationSnapshot releasedNpc ||
      releasedNpc.ReleaseOwner != owner.Value || !releasedNpc.SpawnedFromStatue ||
      releasedNpc.DifficultyScale != 1.0f || releasedNpc.MaximumHealth != 100 ||
      releasedNpc.Health != 100)
  {
    throw new InvalidOperationException(
      $"Type 281 terminal release did not preserve owner in NPC 614 replication: " +
      $"count={simulation.CreateNpcReplicationSnapshots().Count}, " +
      string.Join(';', simulation.CreateNpcReplicationSnapshots().Select(snapshot =>
        $"{snapshot.DefinitionId}:{snapshot.ReleaseOwner}:{snapshot.SpawnedFromStatue}:expected={owner.Value}")));
  }
}

static bool HasStatus(
  IReadOnlyList<StatusEffectSnapshot> statuses,
  StatusEffectTargetKind targetKind,
  int targetId,
  ushort type,
  int durationTicks)
{
  return statuses.Any(status =>
    status.TargetKind == targetKind && status.TargetId == targetId &&
    status.Type == type && status.RemainingTicks == durationTicks);
}

static void VerifyLegacyAiStyle49Type281Behavior()
{
  ProjectileDefinitionRegistry definitions = ProjectileDefinitionRegistry.CreateDefault();
  if (!definitions.TryGet(281, out ProjectileDefinition definition) ||
      definition.BehaviorId != 14 || !definition.Friendly || definition.Hostile ||
      definition.LifetimeTicks != 600 || definition.MaximumPenetration != -1 ||
      definition.Collider.Width != 1.25f || definition.Collider.Height != 1.25f ||
      definition.LegacyAiStyle != 49 ||
      !definition.ReflectsFromTiles || definition.BounceVelocityMultiplier != 0.5f ||
      definition.MinimumBounceSpeed != 2.0f)
  {
    throw new InvalidOperationException(
      "Default registry did not expose the source-backed type 281 definition.");
  }

  using World behaviorWorld = World.Create();
  Entity behaviorProjectile = behaviorWorld.Create(
    new LocationComponent(0.0f, 0.0f),
    new VelocityComponent(10.0f, 15.8f),
    new ProjectileBehaviorComponent(14, default));
  ProjectileBehaviorSystem behaviorSystem = ProjectileBehaviorSystem.CreateDefault();
  for (int tick = 1; tick < 18; tick++)
  {
    if (!behaviorSystem.TryAdvance(behaviorProjectile, behaviorWorld, tick, out int behaviorId) ||
        behaviorId != 14)
    {
      throw new InvalidOperationException("Type 281 behavior was not registered.");
    }
  }

  VelocityComponent beforeGravity = behaviorWorld.Get<VelocityComponent>(behaviorProjectile);
  if (beforeGravity.X != 10.0f || beforeGravity.Y != 15.8f ||
      !behaviorSystem.TryAdvance(behaviorProjectile, behaviorWorld, 18, out _) ||
      MathF.Abs(behaviorWorld.Get<VelocityComponent>(behaviorProjectile).X - 9.9f) > 0.0001f ||
      MathF.Abs(behaviorWorld.Get<VelocityComponent>(behaviorProjectile).Y - 15.9f) > 0.0001f)
  {
    throw new InvalidOperationException(
      "Type 281 did not preserve its source timer, drag, gravity, and velocity cap.");
  }

  WorldGrid world = new(400, 300);
  _ = world.TrySetTile(14, 0, new WorldTile(IsActive: true, Type: 1));
  using DomeSimulation bounceSimulation = new(world);
  PlayerHandle owner = bounceSimulation.CreatePlayer(new SimulationVector(10.0f, 0.0f));
  bounceSimulation.QueueProjectileSpawn(new SpawnProjectileCommand(
    owner,
    10.0f,
    0.0f,
    1,
    10,
    20,
    ProjectileType: 281,
    ProjectileSpeed: 4.0f));
  bounceSimulation.Tick(new SimulationInputBatch());
  bounceSimulation.Tick(new SimulationInputBatch());
  ProjectileReplicationSnapshot bounced = bounceSimulation
    .CreateProjectileReplicationSnapshots()
    .Single();
  if (!bounced.IsActive || bounced.Velocity.X >= 0.0f ||
      MathF.Abs(bounced.Velocity.X + 2.0f) > 0.0001f || bounced.Ai0 != 1.0f)
  {
    throw new InvalidOperationException(
      "Type 281 did not apply the source tile reflection multiplier.");
  }

  using DomeSimulation slowSimulation = new(world);
  PlayerHandle slowOwner = slowSimulation.CreatePlayer(new SimulationVector(10.0f, 0.0f));
  slowSimulation.QueueProjectileSpawn(new SpawnProjectileCommand(
    slowOwner,
    13.0f,
    0.0f,
    1,
    10,
    20,
    ProjectileType: 281,
    ProjectileSpeed: 1.0f));
  slowSimulation.Tick(new SimulationInputBatch());
  slowSimulation.Tick(new SimulationInputBatch());
  ProjectileReplicationSnapshot slowImpact = slowSimulation
    .CreateProjectileReplicationSnapshots()
    .Single();
  if (slowImpact.IsActive || slowImpact.TombstoneReason != ProjectileTombstoneReason.TileHit)
  {
    throw new InvalidOperationException(
      "Type 281 did not terminate when its source bounce-speed threshold was not met.");
  }
}

static void VerifyLegacyAiStyle29Type521ChildSpawn()
{
  ProjectileDefinitionRegistry definitions = ProjectileDefinitionRegistry.CreateDefault();
  if (!definitions.TryGet(521, out ProjectileDefinition parentDefinition) ||
      parentDefinition.BehaviorId != 11 || !parentDefinition.Friendly ||
      parentDefinition.Collider.Width != 0.875f || parentDefinition.Collider.Height != 0.875f ||
      !definitions.TryGet(522, out ProjectileDefinition childDefinition) ||
      childDefinition.BehaviorId != 12 || !childDefinition.Friendly ||
      childDefinition.Collider.Width != 0.5f || childDefinition.Collider.Height != 0.5f)
  {
    throw new InvalidOperationException(
      "Default registry did not expose the source-backed 521 and 522 definitions.");
  }

  WorldGrid world = new(400, 300);
  _ = world.TrySetTile(14, 0, new WorldTile(IsActive: true, Type: 1));
  using DomeSimulation itemSimulation = new(new WorldGrid(400, 300));
  PlayerHandle itemOwner = itemSimulation.CreatePlayer(new SimulationVector(10.0f, 0.0f));
  itemSimulation.GetInventory(itemOwner).SetSlot(0, new ItemStack(8, 1));
  itemSimulation.Tick(new SimulationInputBatch(new PlayerInput(
    itemOwner,
    MoveLeft: false,
    MoveRight: false,
    Jump: false,
    Fire: false,
    UseItem: true)));
  itemSimulation.Tick(new SimulationInputBatch());
  ProjectileReplicationSnapshot itemProjectile = itemSimulation
    .CreateProjectileReplicationSnapshots().Single();
  if (itemProjectile.ProjectileType != 2 || itemProjectile.Damage != 14)
  {
    throw new InvalidOperationException(
      "A validated item use did not preserve its server-authoritative projectile damage.");
  }

  ProjectileDefinitionRegistry parentDefinitions = new(definitions.OrderedDefinitions.Select(
    definition => definition.ProjectileType == 521
      ? definition with { Damage = 40, Knockback = 5.0f }
      : definition));
  using DomeSimulation simulation = new(world, parentDefinitions);
  PlayerHandle owner = simulation.CreatePlayer(new SimulationVector(10.0f, 0.0f));
  simulation.QueueProjectileSpawn(new SpawnProjectileCommand(
    owner,
    10.0f,
    0.75f,
    1,
    40,
    20,
    ProjectileType: 521,
    ProjectileSpeed: 4.0f,
    BannerIdToRespondTo: 7));
  simulation.Tick(new SimulationInputBatch());
  simulation.Tick(new SimulationInputBatch());
  simulation.Tick(new SimulationInputBatch());

  ProjectileReplicationSnapshot[] snapshots = simulation
    .CreateProjectileReplicationSnapshots()
    .ToArray();
  ProjectileReplicationSnapshot parent = snapshots.Single(
    snapshot => snapshot.ProjectileType == 521);
  ProjectileReplicationSnapshot[] children = snapshots.Where(
    snapshot => snapshot.ProjectileType == 522).ToArray();
  if (parent.IsActive || parent.TombstoneReason != ProjectileTombstoneReason.TileHit ||
      children.Length < 3 || children.Length > 5 ||
      children.Any(snapshot => !snapshot.IsActive || snapshot.Owner != owner || snapshot.Damage != 32 ||
        MathF.Abs(snapshot.DefinitionKnockback - 4.0f) > 0.0001f ||
        snapshot.Identity == parent.Identity || snapshot.Identity <= 0 ||
        snapshot.Banner != 7 ||
        !float.IsFinite(snapshot.Velocity.X) || !float.IsFinite(snapshot.Velocity.Y) ||
        MathF.Sqrt(snapshot.Velocity.X * snapshot.Velocity.X +
          snapshot.Velocity.Y * snapshot.Velocity.Y) < 7.0f ||
        MathF.Sqrt(snapshot.Velocity.X * snapshot.Velocity.X +
          snapshot.Velocity.Y * snapshot.Velocity.Y) > 10.0f ||
        MathF.Abs(snapshot.Position.X - (parent.Position.X + 0.4375f)) > 0.0001f ||
        MathF.Abs(snapshot.Position.Y - (parent.Position.Y + 0.4375f)) > 0.0001f) ||
      children.Select(snapshot => snapshot.Identity).Distinct().Count() != children.Length)
  {
    throw new InvalidOperationException(
      "Type 521 tombstone did not commit one valid authoritative type-522 child set.");
  }

  using DomeSimulation bounceSimulation = new(world);
  PlayerHandle bounceOwner = bounceSimulation.CreatePlayer(new SimulationVector(10.0f, 0.0f));
  bounceSimulation.QueueProjectileSpawn(new SpawnProjectileCommand(
    bounceOwner,
    10.0f,
    0.75f,
    1,
    10,
    20,
    ProjectileType: 522,
    ProjectileSpeed: 4.0f));
  bounceSimulation.Tick(new SimulationInputBatch());
  bounceSimulation.Tick(new SimulationInputBatch());
  ProjectileReplicationSnapshot bounced = bounceSimulation
    .CreateProjectileReplicationSnapshots()
    .Single();
  if (!bounced.IsActive || bounced.Velocity.X >= 0.0f ||
      MathF.Abs(MathF.Abs(bounced.Velocity.Y) - 0.2f) > 0.0001f ||
      bounced.Ai1 != 1.0f)
  {
    throw new InvalidOperationException(
      "Type 522 did not apply source motion and reflect from a solid tile.");
  }

  using DomeSimulation expirySimulation = new(new WorldGrid(400, 300));
  PlayerHandle expiryOwner = expirySimulation.CreatePlayer(new SimulationVector(10.0f, 0.0f));
  expirySimulation.QueueProjectileSpawn(new SpawnProjectileCommand(
    expiryOwner,
    10.0f,
    0.0f,
    1,
    10,
    20,
    ProjectileType: 522,
    ProjectileSpeed: 4.0f));
  for (int tick = 0; tick < 42; tick++)
  {
    expirySimulation.Tick(new SimulationInputBatch());
  }

  ProjectileReplicationSnapshot expiredChild = expirySimulation
    .CreateProjectileReplicationSnapshots().Single();
  if (expiredChild.IsActive || expiredChild.TombstoneReason != ProjectileTombstoneReason.Expired ||
      expiredChild.Ai1 != 41.0f)
  {
    throw new InvalidOperationException(
      "Type 522 did not retain its source timer state through ordinary expiry.");
  }
}

static void VerifyLegacyAiStyle2RandomFrameBehavior()
{
  using World world = World.Create();
  Entity projectile = world.Create(
    new LocationComponent(0.0f, 0.0f),
    new VelocityComponent(10.0f, 0.0f),
    new ProjectileBehaviorComponent(8, new ProjectileBehaviorState(0.0f, 4.0f, 0, 0)));
  ProjectileBehaviorSystem behaviorSystem = ProjectileBehaviorSystem.CreateDefault();
  for (int tick = 1; tick <= 37; tick++)
  {
    if (!behaviorSystem.TryAdvance(projectile, world, tick, out int behaviorId) || behaviorId != 8)
    {
      throw new InvalidOperationException(
        "Legacy random-frame aiStyle 2 behavior was not registered.");
    }
  }

  VelocityComponent delayedVelocity = world.Get<VelocityComponent>(projectile);
  if (delayedVelocity.X != 10.0f || delayedVelocity.Y != 0.0f)
  {
    throw new InvalidOperationException(
      "Legacy random-frame aiStyle 2 behavior applied gravity before the source delay elapsed.");
  }

  if (!behaviorSystem.TryAdvance(projectile, world, 38, out _))
  {
    throw new InvalidOperationException(
      "Legacy random-frame aiStyle 2 behavior rejected the source delay boundary.");
  }

  ProjectileBehaviorComponent accelerated = world.Get<ProjectileBehaviorComponent>(projectile);
  VelocityComponent acceleratedVelocity = world.Get<VelocityComponent>(projectile);
  if (MathF.Abs(acceleratedVelocity.X - 9.7f) > 0.0001f ||
      MathF.Abs(acceleratedVelocity.Y - 0.4f) > 0.0001f ||
      accelerated.State.Primary != 38.0f || accelerated.State.Secondary != 4.0f ||
      accelerated.State.Phase != 38)
  {
    throw new InvalidOperationException(
      "Legacy random-frame aiStyle 2 behavior did not retain timer and frame state.");
  }

  ProjectileBehaviorReplicationState replicated = ProjectileBehaviorStateProjection.Project(
    accelerated);
  if (replicated.Ai0 != 38.0f || replicated.Ai1 != 4.0f || replicated.Ai2 != 0.0f)
  {
    throw new InvalidOperationException(
      "Legacy random-frame aiStyle 2 state did not map to legacy AI fields.");
  }

  using DomeSimulation simulation = new(new WorldGrid(400, 300));
  PlayerHandle owner = simulation.CreatePlayer(new SimulationVector(10.0f, 0.0f));
  simulation.QueueProjectileSpawn(new SpawnProjectileCommand(
    owner,
    10.0f,
    0.0f,
    1,
    10,
    20,
    ProjectileType: 909,
    ProjectileSpeed: 10.0f));
  simulation.QueueProjectileSpawn(new SpawnProjectileCommand(
    owner,
    30.0f,
    0.0f,
    1,
    10,
    20,
    ProjectileType: 909,
    ProjectileSpeed: 10.0f));
  simulation.Tick(new SimulationInputBatch());
  simulation.Tick(new SimulationInputBatch());
  ProjectileReplicationSnapshot[] spawned = simulation.CreateProjectileReplicationSnapshots()
    .OrderBy(snapshot => snapshot.ReplicationId)
    .ToArray();
  if (spawned.Length != 2 || spawned[0].Ai0 != 1.0f || spawned[0].Ai1 != 1.0f ||
      spawned[1].Ai0 != 1.0f || spawned[1].Ai1 != 2.0f)
  {
    throw new InvalidOperationException(
      "Type 909 did not derive stable random-frame variants from server identity.");
  }
}

static void VerifyLegacyAiStyle2DelayedBehavior()
{
  using World world = World.Create();
  Entity projectile = world.Create(
    new LocationComponent(0.0f, 0.0f),
    new VelocityComponent(10.0f, 0.0f),
    new ProjectileBehaviorComponent(4, default));
  ProjectileBehaviorSystem behaviorSystem = ProjectileBehaviorSystem.CreateDefault();
  for (int tick = 1; tick <= 9; tick++)
  {
    if (!behaviorSystem.TryAdvance(projectile, world, tick, out int behaviorId) || behaviorId != 4)
    {
      throw new InvalidOperationException("Legacy delayed aiStyle 2 behavior was not registered.");
    }
  }

  VelocityComponent delayedVelocity = world.Get<VelocityComponent>(projectile);
  if (delayedVelocity.X != 10.0f || delayedVelocity.Y != 0.0f)
  {
    throw new InvalidOperationException(
      "Legacy delayed aiStyle 2 behavior applied gravity before the source delay elapsed.");
  }

  if (!behaviorSystem.TryAdvance(projectile, world, 10, out _))
  {
    throw new InvalidOperationException(
      "Legacy delayed aiStyle 2 behavior rejected the delay boundary.");
  }

  VelocityComponent acceleratedVelocity = world.Get<VelocityComponent>(projectile);
  ProjectileBehaviorComponent acceleratedBehavior =
    world.Get<ProjectileBehaviorComponent>(projectile);
  if (MathF.Abs(acceleratedVelocity.X - 9.9f) > 0.0001f ||
      MathF.Abs(acceleratedVelocity.Y - 0.25f) > 0.0001f ||
      acceleratedBehavior.State.Primary != 10.0f || acceleratedBehavior.State.Phase != 10)
  {
    throw new InvalidOperationException(
      "Legacy delayed aiStyle 2 behavior did not apply source drag, gravity, and timer state.");
  }

  world.Set(projectile, new VelocityComponent(0.0f, 31.9f));
  world.Set(projectile, new ProjectileBehaviorComponent(
    4,
    new ProjectileBehaviorState(9.0f, 0.0f, 9, 0)));
  if (!behaviorSystem.TryAdvance(projectile, world, 10, out _) ||
      world.Get<VelocityComponent>(projectile).Y != 32.0f)
  {
    throw new InvalidOperationException(
      "Legacy delayed aiStyle 2 behavior did not retain the global vertical velocity cap.");
  }

  ProjectileBehaviorReplicationState replicated = ProjectileBehaviorStateProjection.Project(
    acceleratedBehavior);
  if (replicated.Ai0 != 10.0f || replicated.Ai1 != 10.0f || replicated.Ai2 != 0.0f)
  {
    throw new InvalidOperationException(
      "Legacy delayed aiStyle 2 timer state did not map to typed AI fields.");
  }
}

static void VerifyLegacyAiStyle2DelayedFamilyDefinitions()
{
  VerifyLegacyAiStyle2DelayedFamilyDefinition(70);
  VerifyLegacyAiStyle2DelayedFamilyDefinition(621);
}

static void VerifyLegacyAiStyle2DelayedFamilyDefinition(int projectileType)
{
  ProjectileDefinitionRegistry definitions = ProjectileDefinitionRegistry.CreateDefault();
  if (!definitions.TryGet(projectileType, out ProjectileDefinition definition) ||
      definition.BehaviorId != 4 || !definition.Friendly || definition.Hostile ||
      definition.MaximumPenetration != 1 || definition.Collider.Width != 0.875f ||
      definition.Collider.Height != 0.875f || definition.LifetimeTicks != 3600)
  {
    throw new InvalidOperationException(
      $"Default type {projectileType} projectile definition did not match the delayed source contract.");
  }

  using DomeSimulation simulation = new(new WorldGrid(400, 300));
  PlayerHandle player = simulation.CreatePlayer(new SimulationVector(10.0f, 0.0f));
  simulation.QueueProjectileSpawn(new SpawnProjectileCommand(
    player,
    10.0f,
    0.0f,
    1,
    10,
    20,
    ProjectileType: projectileType,
    ProjectileSpeed: 10.0f));
  for (int tick = 1; tick <= 11; tick++)
  {
    simulation.Tick(new SimulationInputBatch());
  }

  ProjectileReplicationSnapshot snapshot = simulation.CreateProjectileReplicationSnapshots().Single();
  if (snapshot.ProjectileType != projectileType ||
      MathF.Abs(snapshot.Velocity.X - 9.9f) > 0.0001f ||
      MathF.Abs(snapshot.Velocity.Y - 0.25f) > 0.0001f ||
      snapshot.Ai0 != 10.0f || snapshot.Ai1 != 11.0f || snapshot.Ai2 != 0.0f)
  {
    throw new InvalidOperationException(
      $"Default type {projectileType} did not use the delayed aiStyle 2 source behavior.");
  }
}

static void VerifyLegacyAiStyle2ImmediateGravityBehavior()
{
  using World world = World.Create();
  Entity projectile = world.Create(
    new LocationComponent(0.0f, 0.0f),
    new VelocityComponent(10.0f, 0.0f),
    new ProjectileBehaviorComponent(5, default));
  ProjectileBehaviorSystem behaviorSystem = ProjectileBehaviorSystem.CreateDefault();
  if (!behaviorSystem.TryAdvance(projectile, world, 1, out int behaviorId) || behaviorId != 5)
  {
    throw new InvalidOperationException("Legacy immediate aiStyle 2 behavior was not registered.");
  }

  VelocityComponent velocity = world.Get<VelocityComponent>(projectile);
  ProjectileBehaviorComponent behavior = world.Get<ProjectileBehaviorComponent>(projectile);
  if (MathF.Abs(velocity.X - 10.0f) > 0.0001f ||
      MathF.Abs(velocity.Y - 0.25f) > 0.0001f ||
      behavior.State.Primary != 1.0f || behavior.State.Phase != 1)
  {
    throw new InvalidOperationException(
      "Legacy immediate aiStyle 2 behavior did not apply source gravity and timer state.");
  }

  world.Set(projectile, new VelocityComponent(0.0f, 31.9f));
  world.Set(projectile, new ProjectileBehaviorComponent(
    5,
    new ProjectileBehaviorState(1.0f, 0.0f, 1, 0)));
  if (!behaviorSystem.TryAdvance(projectile, world, 2, out _) ||
      world.Get<VelocityComponent>(projectile).Y != 32.0f)
  {
    throw new InvalidOperationException(
      "Legacy immediate aiStyle 2 behavior did not retain the global vertical velocity cap.");
  }

  ProjectileBehaviorReplicationState replicated = ProjectileBehaviorStateProjection.Project(behavior);
  if (replicated.Ai0 != 1.0f || replicated.Ai1 != 1.0f || replicated.Ai2 != 0.0f)
  {
    throw new InvalidOperationException(
      "Legacy immediate aiStyle 2 timer state did not map to typed AI fields.");
  }
}

static void VerifyLegacyAiStyle2FiveTickGravityBehavior()
{
  using World world = World.Create();
  Entity projectile = world.Create(
    new LocationComponent(0.0f, 0.0f),
    new VelocityComponent(10.0f, 0.0f),
    new ProjectileBehaviorComponent(6, default));
  ProjectileBehaviorSystem behaviorSystem = ProjectileBehaviorSystem.CreateDefault();
  for (int tick = 1; tick <= 4; tick++)
  {
    if (!behaviorSystem.TryAdvance(projectile, world, tick, out int behaviorId) || behaviorId != 6)
    {
      throw new InvalidOperationException("Legacy five-tick aiStyle 2 behavior was not registered.");
    }
  }

  VelocityComponent delayedVelocity = world.Get<VelocityComponent>(projectile);
  if (delayedVelocity.X != 10.0f || delayedVelocity.Y != 0.0f)
  {
    throw new InvalidOperationException(
      "Legacy five-tick aiStyle 2 behavior applied gravity before the source delay elapsed.");
  }

  if (!behaviorSystem.TryAdvance(projectile, world, 5, out _))
  {
    throw new InvalidOperationException(
      "Legacy five-tick aiStyle 2 behavior rejected the source delay boundary.");
  }

  VelocityComponent acceleratedVelocity = world.Get<VelocityComponent>(projectile);
  ProjectileBehaviorComponent acceleratedBehavior =
    world.Get<ProjectileBehaviorComponent>(projectile);
  if (MathF.Abs(acceleratedVelocity.X - 10.0f) > 0.0001f ||
      MathF.Abs(acceleratedVelocity.Y - 0.25f) > 0.0001f ||
      acceleratedBehavior.State.Primary != 5.0f || acceleratedBehavior.State.Phase != 5)
  {
    throw new InvalidOperationException(
      "Legacy five-tick aiStyle 2 behavior did not apply source gravity and timer state.");
  }

  world.Set(projectile, new VelocityComponent(0.0f, 31.9f));
  world.Set(projectile, new ProjectileBehaviorComponent(
    6,
    new ProjectileBehaviorState(4.0f, 0.0f, 4, 0)));
  if (!behaviorSystem.TryAdvance(projectile, world, 5, out _) ||
      world.Get<VelocityComponent>(projectile).Y != 32.0f)
  {
    throw new InvalidOperationException(
      "Legacy five-tick aiStyle 2 behavior did not retain the global vertical velocity cap.");
  }

  ProjectileBehaviorReplicationState replicated = ProjectileBehaviorStateProjection.Project(
    acceleratedBehavior);
  if (replicated.Ai0 != 5.0f || replicated.Ai1 != 5.0f || replicated.Ai2 != 0.0f)
  {
    throw new InvalidOperationException(
      "Legacy five-tick aiStyle 2 timer state did not map to typed AI fields.");
  }
}

static void VerifyLegacyAiStyle2SixtyTickGravityBehavior()
{
  using World world = World.Create();
  Entity projectile = world.Create(
    new LocationComponent(0.0f, 0.0f),
    new VelocityComponent(10.0f, 0.0f),
    new ProjectileBehaviorComponent(7, default));
  ProjectileBehaviorSystem behaviorSystem = ProjectileBehaviorSystem.CreateDefault();
  for (int tick = 1; tick <= 59; tick++)
  {
    if (!behaviorSystem.TryAdvance(projectile, world, tick, out int behaviorId) || behaviorId != 7)
    {
      throw new InvalidOperationException(
        "Legacy sixty-tick aiStyle 2 behavior was not registered.");
    }
  }

  VelocityComponent delayedVelocity = world.Get<VelocityComponent>(projectile);
  if (delayedVelocity.X != 10.0f || delayedVelocity.Y != 0.0f)
  {
    throw new InvalidOperationException(
      "Legacy sixty-tick aiStyle 2 behavior applied gravity before the source delay elapsed.");
  }

  if (!behaviorSystem.TryAdvance(projectile, world, 60, out _))
  {
    throw new InvalidOperationException(
      "Legacy sixty-tick aiStyle 2 behavior rejected the source delay boundary.");
  }

  VelocityComponent acceleratedVelocity = world.Get<VelocityComponent>(projectile);
  ProjectileBehaviorComponent acceleratedBehavior =
    world.Get<ProjectileBehaviorComponent>(projectile);
  if (MathF.Abs(acceleratedVelocity.X - 9.9f) > 0.0001f ||
      MathF.Abs(acceleratedVelocity.Y - 0.2f) > 0.0001f ||
      acceleratedBehavior.State.Primary != 60.0f || acceleratedBehavior.State.Phase != 60)
  {
    throw new InvalidOperationException(
      "Legacy sixty-tick aiStyle 2 behavior did not apply source drag, gravity, and timer state.");
  }

  world.Set(projectile, new VelocityComponent(0.0f, 31.9f));
  world.Set(projectile, new ProjectileBehaviorComponent(
    7,
    new ProjectileBehaviorState(59.0f, 0.0f, 59, 0)));
  if (!behaviorSystem.TryAdvance(projectile, world, 60, out _) ||
      world.Get<VelocityComponent>(projectile).Y != 32.0f)
  {
    throw new InvalidOperationException(
      "Legacy sixty-tick aiStyle 2 behavior did not retain the global vertical velocity cap.");
  }

  ProjectileBehaviorReplicationState replicated = ProjectileBehaviorStateProjection.Project(
    acceleratedBehavior);
  if (replicated.Ai0 != 60.0f || replicated.Ai1 != 60.0f || replicated.Ai2 != 0.0f)
  {
    throw new InvalidOperationException(
      "Legacy sixty-tick aiStyle 2 timer state did not map to typed AI fields.");
  }

  WorldGrid tileWorld = new(400, 300);
  _ = tileWorld.TrySetTile(14, 0, new WorldTile(IsActive: true, Type: 1));
  using DomeSimulation simulation = new(tileWorld);
  PlayerHandle owner = simulation.CreatePlayer(new SimulationVector(10.0f, 0.0f));
  simulation.QueueProjectileSpawn(new SpawnProjectileCommand(
    owner,
    10.0f,
    0.0f,
    1,
    10,
    20,
    ProjectileType: 300));
  simulation.Tick(new SimulationInputBatch());
  ProjectileReplicationSnapshot tilePassing = simulation
    .CreateProjectileReplicationSnapshots().Single();
  if (!tilePassing.IsActive || tilePassing.TombstoneReason != ProjectileTombstoneReason.None)
  {
    throw new InvalidOperationException(
      "Type 300 projectile did not preserve its source tileCollide false contract.");
  }
}

static void VerifyProjectileUuidLifecycle()
{
  using DomeSimulation simulation = new(new WorldGrid(400, 300));
  PlayerHandle player = simulation.CreatePlayer(new SimulationVector(10.0f, 0.0f));
  simulation.QueueProjectileSpawn(new SpawnProjectileCommand(
    player,
    10.0f,
    0.0f,
    1,
    10,
    20));
  simulation.Tick(new SimulationInputBatch());
  Guid? initialUuid = simulation.CreateProjectileReplicationSnapshots().Single().ProjectileUuid;
  if (!initialUuid.HasValue || initialUuid.Value == Guid.Empty)
  {
    throw new InvalidOperationException(
      "Authoritative player projectile did not allocate a UUID.");
  }

  simulation.Tick(new SimulationInputBatch());
  Guid? refreshedUuid = simulation.CreateProjectileReplicationSnapshots().Single().ProjectileUuid;
  if (refreshedUuid != initialUuid)
  {
    throw new InvalidOperationException(
      "Authoritative player projectile did not preserve its UUID across refresh.");
  }
}

static void VerifyProjectileTombstoneReasons()
{
  ProjectileDefinitionRegistry expiryDefinitions = new(
    ProjectileDefinitionRegistry.CreateDefault().OrderedDefinitions.Select(definition =>
      definition.ProjectileType == 1 ? definition with { LifetimeTicks = 30 } : definition));
  using DomeSimulation expirySimulation = new(new WorldGrid(400, 300), expiryDefinitions);
  PlayerHandle expiryPlayer = expirySimulation.CreatePlayer(new SimulationVector(10.0f, 0.0f));
  expirySimulation.QueueProjectileSpawn(new SpawnProjectileCommand(
    expiryPlayer,
    10.0f,
    0.0f,
    1,
    10,
    1));
  for (int tick = 0; tick <= 30; tick++)
  {
    expirySimulation.Tick(new SimulationInputBatch());
  }
  ProjectileReplicationSnapshot expired = expirySimulation
    .CreateProjectileReplicationSnapshots().Single();
  if (expired.IsActive || expired.TombstoneReason != ProjectileTombstoneReason.Expired)
  {
    throw new InvalidOperationException("Expired projectile did not retain its tombstone reason.");
  }

  WorldGrid tileWorld = new(400, 300);
  _ = tileWorld.TrySetTile(14, 0, new WorldTile(IsActive: true, Type: 1));
  using DomeSimulation tileSimulation = new(tileWorld);
  PlayerHandle tilePlayer = tileSimulation.CreatePlayer(new SimulationVector(10.0f, 0.0f));
  tileSimulation.QueueProjectileSpawn(new SpawnProjectileCommand(
    tilePlayer,
    10.0f,
    0.0f,
    1,
    10,
    20));
  tileSimulation.Tick(new SimulationInputBatch());
  tileSimulation.Tick(new SimulationInputBatch());
  ProjectileReplicationSnapshot tileHit = tileSimulation
    .CreateProjectileReplicationSnapshots().Single();
  if (tileHit.IsActive || tileHit.TombstoneReason != ProjectileTombstoneReason.TileHit)
  {
    throw new InvalidOperationException("Tile-hit projectile did not retain its tombstone reason.");
  }

  using DomeSimulation penetrationSimulation = new(new WorldGrid(400, 300));
  PlayerHandle penetrationPlayer = penetrationSimulation.CreatePlayer(
    new SimulationVector(10.0f, 0.0f));
  _ = penetrationSimulation.CreateNpc(new SimulationVector(14.0f, 0.0f));
  penetrationSimulation.QueueProjectileSpawn(new SpawnProjectileCommand(
    penetrationPlayer,
    10.0f,
    0.0f,
    1,
    10,
    20,
    MaximumPenetration: -1));
  penetrationSimulation.Tick(new SimulationInputBatch());
  penetrationSimulation.Tick(new SimulationInputBatch());
  ProjectileReplicationSnapshot penetrated = penetrationSimulation
    .CreateProjectileReplicationSnapshots().Single();
  if (penetrated.IsActive ||
      penetrated.TombstoneReason != ProjectileTombstoneReason.Penetrated ||
      penetrated.HitCount != 1 || penetrated.MaximumPenetration != 1)
  {
    throw new InvalidOperationException(
      $"Projectile penetration did not preserve authoritative capacity: " +
      $"reason={penetrated.TombstoneReason}, hits={penetrated.HitCount}, " +
      $"maximum={penetrated.MaximumPenetration}.");
  }
}

static void VerifyProjectileIdentityAllocator()
{
  ProjectileIdentityAllocator allocator = new();
  if (allocator.Allocate() != 1 || allocator.Allocate() != 2 ||
      allocator.NextIdentity != 3)
  {
    throw new InvalidOperationException(
      "Projectile identity allocator did not preserve monotonic server ownership.");
  }

  ProjectileIdentityAllocator exhausted = new(int.MaxValue - 1);
  if (exhausted.Allocate() != int.MaxValue - 1)
  {
    throw new InvalidOperationException("Projectile identity allocator rejected its final valid ID.");
  }

  try
  {
    _ = exhausted.Allocate();
    throw new InvalidOperationException("Projectile identity allocator reused an exhausted ID.");
  }
  catch (InvalidOperationException)
  {
  }
}

static void VerifyProjectileTombstoneRetention()
{
  ProjectileReplicationSnapshot snapshot = new(
    1,
    1,
    new PlayerHandle(1),
    default,
    default,
    10,
    0,
    IsActive: false,
    Revision: 2,
    default,
    TombstoneReason: ProjectileTombstoneReason.TileHit,
    TombstoneRetainedUntilTick: 30);
  if (!ProjectileTombstonePolicy.IsRetained(snapshot, 29) ||
      ProjectileTombstonePolicy.IsRetained(snapshot, 30))
  {
    throw new InvalidOperationException(
      "Projectile tombstone retention did not expire at its exclusive boundary.");
  }

  if (ProjectileTombstonePolicy.CalculateRetentionUntil(long.MaxValue) != long.MaxValue)
  {
    throw new InvalidOperationException("Projectile tombstone retention overflow was not clamped.");
  }
}

static void VerifyProjectileIdentityCursorRestore()
{
  WorldGrid world = new(400, 300);
  WorldMetadata metadata = new(
    "projectile-identity",
    new WorldSeed(7),
    400,
    300,
    spawnX: 10,
    spawnY: 10);
  DomeSimulationSnapshot snapshot = new(
    world.CreateSnapshot(metadata),
    [],
    [],
    0,
    nextProjectileIdentity: 17);
  using DomeSimulation simulation = new(snapshot);
  PlayerHandle player = simulation.CreatePlayer(new SimulationVector(10.0f, 10.0f));
  simulation.QueueProjectileSpawn(new SpawnProjectileCommand(
    player,
    10.0f,
    10.0f,
    1,
    10,
    10));
  simulation.Tick(new SimulationInputBatch());
  if (simulation.CreateProjectileReplicationSnapshots().Single().ReplicationId != 17)
  {
    throw new InvalidOperationException(
      "Projectile identity cursor restarted instead of continuing after snapshot restore.");
  }
}

static void VerifyProjectileLocalImmunity()
{
  using World world = World.Create();
  Entity firstProjectile = world.Create(new ProjectilePenetrationComponent(1));
  Entity secondProjectile = world.Create(new ProjectilePenetrationComponent(1));
  Entity target = world.Create(new NpcTagComponent());
  HitImmunityComponent immunity = new();
  ProjectileDamageSystem system = new();
  IReadOnlyList<DamageRequestedEvent> accepted = system.Resolve(
    world,
    [
      new DamageRequestedEvent(firstProjectile, target, 10, 1, 7),
      new DamageRequestedEvent(secondProjectile, target, 10, 2, 7)
    ],
    immunity);
  if (accepted.Count != 2)
  {
    throw new InvalidOperationException(
      "Projectile-local immunity incorrectly blocked a different projectile identity.");
  }

  if (system.Resolve(
        world,
        [new DamageRequestedEvent(firstProjectile, target, 10, 1, 7)],
        immunity).Count != 0)
  {
    throw new InvalidOperationException(
      "Projectile-local immunity did not block a repeated hit from the same projectile.");
  }

  Entity noLocalImmunityProjectile = world.Create(
    new ProjectileDefinitionComponent(1001, 1, 10, 30, new ColliderComponent(1, 1), true, false),
    new ProjectilePenetrationComponent(-1));
  HitImmunityComponent definitionImmunity = new();
  if (system.Resolve(world,
      [new DamageRequestedEvent(noLocalImmunityProjectile, target, 10, 3, 7)], definitionImmunity).Count != 1 ||
      system.Resolve(world,
      [new DamageRequestedEvent(noLocalImmunityProjectile, target, 10, 3, 7)], definitionImmunity).Count != 1)
  {
    throw new InvalidOperationException(
      "Projectile definition without local NPC immunity incorrectly applied local immunity.");
  }

  Entity cooldownProjectile = world.Create(
    new ProjectileDefinitionComponent(
      1002,
      1,
      10,
      30,
      new ColliderComponent(1, 1),
      true,
      false,
      UsesLocalNpcImmunity: true,
      LocalNpcHitCooldownTicks: 3),
    new ProjectilePenetrationComponent(-1));
  HitImmunityComponent cooldownImmunity = new();
  if (system.Resolve(world,
      [new DamageRequestedEvent(cooldownProjectile, target, 10, 4, 7)], cooldownImmunity).Count != 1 ||
      !cooldownImmunity.IsProjectileImmune(4, 7))
  {
    throw new InvalidOperationException("Projectile local NPC cooldown was not applied.");
  }

  new HitImmunitySystem().Tick(cooldownImmunity);
  new HitImmunitySystem().Tick(cooldownImmunity);
  if (!cooldownImmunity.IsProjectileImmune(4, 7))
  {
    throw new InvalidOperationException("Projectile local NPC cooldown expired too early.");
  }

  Entity permanentCooldownProjectile = world.Create(
    new ProjectileDefinitionComponent(
      1003,
      1,
      10,
      30,
      new ColliderComponent(1, 1),
      true,
      false,
      UsesLocalNpcImmunity: true,
      LocalNpcHitCooldownTicks: -1),
    new ProjectilePenetrationComponent(-1));
  HitImmunityComponent permanentImmunity = new();
  DamageRequestedEvent permanentHit = new(permanentCooldownProjectile, target, 10, 5, 7);
  if (system.Resolve(world, [permanentHit], permanentImmunity).Count != 1 ||
      !permanentImmunity.IsProjectileImmune(5, 7))
  {
    throw new InvalidOperationException("Permanent projectile local NPC immunity was not applied.");
  }

  for (int index = 0; index < 10; index++)
  {
    new HitImmunitySystem().Tick(permanentImmunity);
  }

  if (system.Resolve(world, [permanentHit], permanentImmunity).Count != 0)
  {
    throw new InvalidOperationException("Permanent projectile local NPC immunity unexpectedly expired.");
  }

  Entity defaultCooldownProjectile = world.Create(
    new ProjectileDefinitionComponent(
      1004,
      1,
      10,
      30,
      new ColliderComponent(1, 1),
      true,
      false,
      UsesLocalNpcImmunity: true,
      LocalNpcHitCooldownTicks: -2),
    new ProjectilePenetrationComponent(-1));
  HitImmunityComponent defaultImmunity = new();
  DamageRequestedEvent defaultHit = new(defaultCooldownProjectile, target, 10, 6, 7);
  if (system.Resolve(world, [defaultHit], defaultImmunity).Count != 1 ||
      defaultImmunity.IsProjectileImmune(6, 7) ||
      system.Resolve(world, [defaultHit], defaultImmunity).Count != 1)
  {
    throw new InvalidOperationException(
      "The -2 local NPC cooldown sentinel incorrectly created projectile-local immunity.");
  }

  ProjectileDefinition sentinelDefinition = new(
    1005,
    1,
    10,
    30,
    new ColliderComponent(1, 1),
    true,
    false,
    -1,
    UsesLocalNpcImmunity: true,
    LocalNpcHitCooldownTicks: -2);
  _ = new ProjectileDefinitionRegistry([sentinelDefinition]);
  _ = new ProjectileDefinitionRegistry([
    sentinelDefinition with { LocalNpcHitCooldownTicks = -1 }
  ]);

  try
  {
    _ = new ProjectileDefinitionRegistry([
      sentinelDefinition with { LocalNpcHitCooldownTicks = -3 }
    ]);
    throw new InvalidOperationException("Projectile definitions accepted an unsupported local immunity value.");
  }
  catch (ArgumentOutOfRangeException)
  {
  }

  Entity fallbackTarget = world.Create(new NpcTagComponent());
  ProjectileDefinitionComponent fallbackDefinition = new(
    1006,
    1,
    10,
    30,
    new ColliderComponent(1, 1),
    true,
    false,
    UsesLocalNpcImmunity: true,
    LocalNpcHitCooldownTicks: -2);
  Entity fallbackFirstProjectile = world.Create(
    fallbackDefinition,
    new ProjectileOwnerComponent(new PlayerHandle(7)),
    new ProjectilePenetrationComponent(-1));
  Entity fallbackSecondProjectile = world.Create(
    fallbackDefinition,
    new ProjectileOwnerComponent(new PlayerHandle(7)),
    new ProjectilePenetrationComponent(-1));
  HitImmunityComponent fallbackImmunity = new();
  DamageRequestedEvent fallbackFirstHit = new(fallbackFirstProjectile, fallbackTarget, 10, 7, 8);
  DamageRequestedEvent fallbackSecondHit = new(fallbackSecondProjectile, fallbackTarget, 10, 8, 8);
  if (system.Resolve(world, [fallbackFirstHit], fallbackImmunity).Count != 1 ||
      !fallbackImmunity.IsOwnerNpcImmune(7, 8) ||
      system.Resolve(world, [fallbackSecondHit], fallbackImmunity).Count != 0)
  {
    throw new InvalidOperationException(
      "The -2 local NPC cooldown fallback did not protect the owner-target pair.");
  }

  for (int index = 0; index < 10; index++)
  {
    new HitImmunitySystem().Tick(fallbackImmunity);
  }

  Entity fallbackThirdProjectile = world.Create(
    fallbackDefinition,
    new ProjectileOwnerComponent(new PlayerHandle(7)),
    new ProjectilePenetrationComponent(-1));
  DamageRequestedEvent fallbackThirdHit = new(fallbackThirdProjectile, fallbackTarget, 10, 9, 8);
  if (system.Resolve(world, [fallbackThirdHit], fallbackImmunity).Count != 1)
  {
    throw new InvalidOperationException("The -2 local NPC cooldown fallback did not expire at 10 ticks.");
  }

  Entity singleHitTarget = world.Create(new NpcTagComponent());
  Entity singleHitProjectile = world.Create(
    fallbackDefinition,
    new ProjectileOwnerComponent(new PlayerHandle(7)),
    new ProjectilePenetrationComponent(1));
  HitImmunityComponent singleHitImmunity = new();
  if (system.Resolve(
        world,
        [new DamageRequestedEvent(singleHitProjectile, singleHitTarget, 10, 10, 11)],
        singleHitImmunity).Count != 1 ||
      singleHitImmunity.IsOwnerNpcImmune(7, 11))
  {
    throw new InvalidOperationException(
      "The -2 local NPC cooldown fallback incorrectly affected a single-hit projectile.");
  }
}

static void VerifyProjectileLocalNpcImmunityDoesNotApplyToPlayers()
{
  using World world = World.Create();
  Entity projectile = world.Create(
    new ProjectileDefinitionComponent(
      1200,
      1,
      10,
      60,
      new ColliderComponent(1, 1),
      true,
      false,
      UsesLocalNpcImmunity: true,
      LocalNpcHitCooldownTicks: -1),
    new ProjectilePenetrationComponent(-1));
  Entity player = world.Create(new PlayerTagComponent());
  HitImmunityComponent immunity = new();
  immunity.SetProjectile(77, 9, 10);
  ProjectileDamageSystem system = new();
  DamageRequestedEvent hit = new(projectile, player, 10, 77, 9);

  if (system.Resolve(world, [hit], immunity).Count != 1)
  {
    throw new InvalidOperationException(
      "A local-NPC-immunity projectile could not hit a player in the scope fixture.");
  }

  for (int tick = 0; tick < 40; tick++)
  {
    new HitImmunitySystem().Tick(immunity);
  }

  if (system.Resolve(world, [hit], immunity).Count != 1)
  {
    throw new InvalidOperationException(
      "Projectile local NPC immunity leaked into player hit eligibility after player immunity " +
      "expired.");
  }
}

static void VerifyProjectileStaticNpcImmunityDoesNotApplyToPlayers()
{
  using World world = World.Create();
  Entity projectile = world.Create(
    new ProjectileDefinitionComponent(
      1201,
      1,
      10,
      60,
      new ColliderComponent(1, 1),
      true,
      false,
      UsesStaticNpcImmunity: true,
      StaticNpcHitCooldownTicks: 10),
    new ProjectilePenetrationComponent(-1));
  Entity player = world.Create(new PlayerTagComponent());
  HitImmunityComponent immunity = new();
  immunity.SetStaticNpc(1201, 88, 10);
  ProjectileDamageSystem system = new();
  DamageRequestedEvent hit = new(projectile, player, 10, 78, 88);

  if (system.Resolve(world, [hit], immunity).Count != 1)
  {
    throw new InvalidOperationException(
      "Static NPC immunity leaked into player hit eligibility.");
  }
}

static void VerifyProjectileNpcSlotReset()
{
  const int npcSlot = 17;
  const int otherNpcSlot = 18;
  HitImmunityComponent immunity = new();
  immunity.SetProjectile(100, npcSlot, 8);
  immunity.SetProjectilePermanent(101, npcSlot);
  immunity.SetStaticNpc(200, npcSlot, 8);
  immunity.SetPlayerProjectile(102, npcSlot, 40);
  immunity.Set(103, 8);
  immunity.SetOwnerNpc(7, npcSlot, 8);
  immunity.SetOwnerMeleeNpc(8, npcSlot, 8);
  immunity.SetProjectile(104, otherNpcSlot, 8);
  immunity.SetProjectilePermanent(105, otherNpcSlot);
  immunity.SetStaticNpc(201, otherNpcSlot, 8);

  new HitImmunitySystem().ResetNpcSlotData(immunity, npcSlot);

  if (immunity.IsProjectileImmune(100, npcSlot) ||
      immunity.IsProjectileImmune(101, npcSlot) ||
      immunity.IsStaticNpcImmune(200, npcSlot))
  {
    throw new InvalidOperationException(
      "NPC slot reset retained local or static Projectile immunity for the reused slot.");
  }

  if (!immunity.IsPlayerProjectileImmune(102, npcSlot) ||
      !immunity.IsImmune(103) ||
      !immunity.IsOwnerNpcImmune(7, npcSlot) ||
      !immunity.IsOwnerMeleeNpcImmune(8, npcSlot))
  {
    throw new InvalidOperationException(
      "NPC slot reset cleared player, NPC, or owner immunity outside Projectile local state.");
  }

  if (!immunity.IsProjectileImmune(104, otherNpcSlot) ||
      !immunity.IsProjectileImmune(105, otherNpcSlot) ||
      !immunity.IsStaticNpcImmune(201, otherNpcSlot))
  {
    throw new InvalidOperationException(
      "NPC slot reset changed immunity state belonging to another NPC slot.");
  }
}

static void VerifyProjectileNpcSlotResetOnReuse()
{
  ProjectileDefinitionRegistry defaults = ProjectileDefinitionRegistry.CreateDefault();
  ProjectileDefinitionRegistry definitions = new(defaults.OrderedDefinitions.Select(
    definition => definition.ProjectileType == 645
      ? definition with
      {
        Damage = 10,
        UsesStaticNpcImmunity = true,
        StaticNpcHitCooldownTicks = 10
      }
      : definition));
  using DomeSimulation simulation = new(new WorldGrid(400, 300), definitions);
  PlayerHandle owner = simulation.CreatePlayer(new SimulationVector(10.0f, 0.0f));
  NpcHandle original = simulation.CreateNpc(new SimulationVector(10.0f, 0.0f), 488);
  simulation.QueueProjectileSpawn(new SpawnProjectileCommand(
    owner,
    10.0f,
    0.0f,
    Facing: 1,
    Damage: 10,
    LifetimeTicks: 60,
    ProjectileType: 645,
    ProjectileSpeed: 0.0f,
    UseZeroVelocity: true));
  simulation.Tick(new SimulationInputBatch());
  simulation.Tick(new SimulationInputBatch());
  simulation.Tick(new SimulationInputBatch());
  ProjectileReplicationSnapshot initialProjectile =
    simulation.CreateProjectileReplicationSnapshots().Single();
  NpcReplicationSnapshot initialNpc = simulation.CreateNpcReplicationSnapshots().Single();
  if (initialProjectile.HitCount != 1)
  {
    throw new InvalidOperationException(
      $"The local-immunity slot-reuse fixture did not record its initial NPC hit: " +
      $"hits={initialProjectile.HitCount}, active={initialProjectile.IsActive}, " +
      $"projectilePosition={initialProjectile.Position}, npcActive={initialNpc.IsActive}, " +
      $"npcPosition={initialNpc.Position}, npcHealth={initialNpc.Health}.");
  }

  simulation.QueueNpcDespawn(new DespawnNpcCommand(original, NpcDespawnReason.OutOfRange));
  simulation.Tick(new SimulationInputBatch());
  NpcHandle reused = simulation.CreateNpc(new SimulationVector(10.0f, 0.0f));
  if (reused != original)
  {
    throw new InvalidOperationException("NPC slot reuse did not select the inactive slot.");
  }

  simulation.Tick(new SimulationInputBatch());
  simulation.Tick(new SimulationInputBatch());
  ProjectileReplicationSnapshot reusedProjectile =
    simulation.CreateProjectileReplicationSnapshots().Single();
  NpcReplicationSnapshot reusedNpc = simulation.CreateNpcReplicationSnapshots().Single();
  if (reusedProjectile.HitCount != 2)
  {
    throw new InvalidOperationException(
      $"NPC slot reuse retained Projectile local/static immunity from the previous occupant: " +
      $"hits={reusedProjectile.HitCount}, active={reusedProjectile.IsActive}, " +
      $"projectilePosition={reusedProjectile.Position}, npcActive={reusedNpc.IsActive}, " +
      $"npcPosition={reusedNpc.Position}, npcHealth={reusedNpc.Health}.");
  }
}

static void VerifyProjectileCopiesOwnerAttackCooldownOnSpawn()
{
  int[] expectedTypes =
  [
    1059, 1060, 1061, 1062, 1063, 1064, 1065, 1066, 1067, 1068,
    1069, 1070, 1071, 1072, 1074, 1075, 1076, 1101, 1102
  ];
  if (LegacyProjectileAttackCooldownCopyRegistry.CopyTypes.Count != expectedTypes.Length)
  {
    throw new InvalidOperationException(
      "Source-backed owner attack cooldown copy registry changed its type count.");
  }

  foreach (int projectileType in expectedTypes)
  {
    if (!LegacyProjectileAttackCooldownCopyRegistry.IsEnabled(projectileType))
    {
      throw new InvalidOperationException(
        $"Source-backed owner attack cooldown copy registry omitted type {projectileType}.");
    }
  }

  if (LegacyProjectileAttackCooldownCopyRegistry.IsEnabled(1058) ||
      LegacyProjectileAttackCooldownCopyRegistry.IsEnabled(1073) ||
      LegacyProjectileAttackCooldownCopyRegistry.IsEnabled(0))
  {
    throw new InvalidOperationException(
      "Owner attack cooldown copy registry accepted a type outside the source set.");
  }

  ProjectileDefinition definition = new(
    1059,
    1,
    10,
    60,
    new ColliderComponent(1.0f, 1.0f),
    true,
    false,
    -1,
    UsesLocalNpcImmunity: true,
    LocalNpcHitCooldownTicks: 10);
  ProjectileDefinitionRegistry definitions = new([definition]);
  if (!definitions.TryGet(1059, out definition) ||
      !definition.CopiesOwnerAttackCooldownToLocalImmunityOnSpawn)
  {
    throw new InvalidOperationException(
      "The source-backed owner attack cooldown capability was not applied to type 1059.");
  }

  using World world = World.Create();
  HitImmunityComponent immunity = new();
  immunity.SetOwnerMeleeNpc(7, 17, 6);
  immunity.SetOwnerMeleeNpc(7, 18, 0);
  immunity.SetOwnerMeleeNpc(8, 17, 9);
  immunity.SetOwnerMeleeNpc(8, 19, 9);
  SpawnProjectileCommand command = new(
    new PlayerHandle(7),
    0.0f,
    0.0f,
    Facing: 1,
    Damage: 10,
    LifetimeTicks: 60,
    ProjectileType: 1059,
    ProjectileSpeed: 0.0f,
    UseZeroVelocity: true);
  Entity projectile = new ProjectileSpawnSystem().Spawn(
    world,
    command,
    definition,
    identity: 12,
    projectileHitImmunity: immunity);
  ProjectileDefinitionComponent runtimeDefinition =
    world.Get<ProjectileDefinitionComponent>(projectile);
  ProjectileReplicationSnapshot snapshot = new ProjectileReplicationSystem().Project(
    projectile,
    world,
    replicationId: 12,
    revision: 1,
    section: new WorldSectionCoordinates(0, 0));
  if (!runtimeDefinition.CopiesOwnerAttackCooldownToLocalImmunityOnSpawn ||
      !snapshot.CopiesOwnerAttackCooldownToLocalImmunityOnSpawn ||
      !immunity.IsNpcProjectileImmune(12, 17) ||
      immunity.IsNpcProjectileImmune(12, 18) ||
      immunity.IsNpcProjectileImmune(12, 19) ||
      immunity.IsNpcProjectileImmune(13, 17))
  {
    throw new InvalidOperationException(
      "Projectile spawn did not copy only the owning player's positive melee cooldowns.");
  }

  Entity target = world.Create(new NpcTagComponent());
  DamageRequestedEvent blockedHit = new(projectile, target, 10, 12, 17);
  ProjectileDamageSystem damage = new();
  if (damage.Resolve(world, [blockedHit], immunity).Count != 0)
  {
    throw new InvalidOperationException(
      "Copied owner melee cooldown did not block the matching local-NPC hit.");
  }

  for (int index = 0; index < 6; index++)
  {
    new HitImmunitySystem().Tick(immunity);
  }

  if (damage.Resolve(world, [blockedHit], immunity).Count != 1)
  {
    throw new InvalidOperationException(
      "Copied owner melee cooldown did not expire with the copied tick budget.");
  }

  ProjectileDefinition withoutCopy = new(
    1058,
    1,
    10,
    60,
    new ColliderComponent(1.0f, 1.0f),
    true,
    false,
    -1,
    UsesLocalNpcImmunity: true,
    LocalNpcHitCooldownTicks: 10);
  immunity.SetOwnerMeleeNpc(7, 17, 4);
  Entity unmarkedProjectile = new ProjectileSpawnSystem().Spawn(
    world,
    command with { ProjectileType = 1058 },
    withoutCopy,
    identity: 13,
    projectileHitImmunity: immunity);
  if (world.Get<ProjectileDefinitionComponent>(unmarkedProjectile)
        .CopiesOwnerAttackCooldownToLocalImmunityOnSpawn ||
      immunity.IsNpcProjectileImmune(13, 17))
  {
    throw new InvalidOperationException(
      "Projectile without the capability copied owner melee cooldown state.");
  }
}

static void VerifyProjectilePlayerImmunity()
{
  using World world = World.Create();
  Entity projectile = world.Create(new ProjectilePenetrationComponent(-1));
  Entity player = world.Create(new PlayerTagComponent());
  HitImmunityComponent immunity = new();
  ProjectileDamageSystem system = new();
  DamageRequestedEvent hit = new(projectile, player, 10, 12, 34);

  if (system.Resolve(world, [hit], immunity).Count != 1 ||
      !immunity.IsProjectileImmune(12, 34))
  {
    throw new InvalidOperationException(
      "A projectile hit against a player did not create playerImmune state.");
  }

  if (system.Resolve(world, [hit], immunity).Count != 0)
  {
    throw new InvalidOperationException(
      "playerImmune did not reject a repeated hit from the same projectile.");
  }

  for (int tick = 0; tick < 39; tick++)
  {
    new HitImmunitySystem().Tick(immunity);
  }

  if (!immunity.IsProjectileImmune(12, 34))
  {
    throw new InvalidOperationException("playerImmune expired before its source-backed duration.");
  }

  new HitImmunitySystem().Tick(immunity);
  if (immunity.IsProjectileImmune(12, 34) ||
      system.Resolve(world, [hit], immunity).Count != 1)
  {
    throw new InvalidOperationException(
      "playerImmune did not expire and allow a later hit at 40 ticks.");
  }
}

static void VerifyProjectileExtraUpdatesBoundary()
{
  ProjectileDefinitionRegistry defaults = ProjectileDefinitionRegistry.CreateDefault();
  ProjectileDefinitionRegistry definitions = new(defaults.OrderedDefinitions.Select(definition =>
    definition.ProjectileType == 1 ? definition with { ExtraUpdates = 1, LifetimeTicks = 4 } : definition));
  using DomeSimulation simulation = new(new WorldGrid(400, 300), definitions);
  PlayerHandle owner = simulation.CreatePlayer(new SimulationVector(10.0f, 10.0f));
  simulation.QueueProjectileSpawn(new SpawnProjectileCommand(
    owner, 10.0f, 10.0f, 1, 0, 0, ProjectileType: 1, ProjectileSpeed: 1.0f));
  simulation.Tick(new SimulationInputBatch());
  simulation.Tick(new SimulationInputBatch());
  ProjectileReplicationSnapshot projectile = simulation.CreateProjectileReplicationSnapshots().Single();
  if (projectile.Position.X != 12.0f || projectile.RemainingLifetime != 2 ||
      projectile.UpdateCount != 2)
  {
    throw new InvalidOperationException(
      $"extraUpdates did not execute two deterministic substeps: x={projectile.Position.X}, " +
      $"timeLeft={projectile.RemainingLifetime}, updates={projectile.UpdateCount}");
  }
}

static void VerifyProjectileStaticNpcImmunity()
{
  using World world = World.Create();
  Entity firstProjectile = world.Create(
    new ProjectileDefinitionComponent(281, 14, 10, 600, new ColliderComponent(1, 1), true, false,
      UsesStaticNpcImmunity: true, StaticNpcHitCooldownTicks: 10),
    new ProjectilePenetrationComponent(-1));
  Entity secondProjectile = world.Create(
    new ProjectileDefinitionComponent(281, 14, 10, 600, new ColliderComponent(1, 1), true, false,
      UsesStaticNpcImmunity: true, StaticNpcHitCooldownTicks: 10),
    new ProjectilePenetrationComponent(-1));
  Entity target = world.Create(new NpcTagComponent());
  HitImmunityComponent immunity = new();
  ProjectileDamageSystem system = new();
  if (system.Resolve(world,
      [new DamageRequestedEvent(firstProjectile, target, 10, 1, 7)], immunity).Count != 1 ||
      system.Resolve(world,
      [new DamageRequestedEvent(secondProjectile, target, 10, 2, 7)], immunity).Count != 0)
  {
    throw new InvalidOperationException(
      "Type 281 static NPC immunity did not block a second projectile of the same type.");
  }

  for (int index = 0; index < 10; index++)
  {
    new HitImmunitySystem().Tick(immunity);
  }

  if (system.Resolve(world,
      [new DamageRequestedEvent(secondProjectile, target, 10, 2, 7)], immunity).Count != 1)
  {
    throw new InvalidOperationException(
      "Type 281 static NPC immunity did not expire after its ten-tick cooldown.");
  }
}

static void VerifyProjectileOwnerMeleeHitCooldown()
{
  if (!ProjectileAttackCooldownPolicy.CaresForAttackCooldown(
        usesOwnerMeleeHitCooldown: true,
        isNpcProjectile: false,
        isTrap: false,
        ownerValue: 254) ||
      ProjectileAttackCooldownPolicy.CaresForAttackCooldown(
        usesOwnerMeleeHitCooldown: true,
        isNpcProjectile: false,
        isTrap: false,
        ownerValue: 255) ||
      ProjectileAttackCooldownPolicy.CaresForAttackCooldown(
        usesOwnerMeleeHitCooldown: true,
        isNpcProjectile: false,
        isTrap: true,
        ownerValue: 1) ||
      ProjectileAttackCooldownPolicy.CaresForAttackCooldown(
        usesOwnerMeleeHitCooldown: false,
        isNpcProjectile: false,
        isTrap: false,
        ownerValue: 1))
  {
    throw new InvalidOperationException(
      "CareForAttackCD policy did not preserve owner, trap, and capability boundaries.");
  }

  using World world = World.Create();
  ProjectileDefinitionComponent definition = new(
    972,
    1,
    10,
    30,
    new ColliderComponent(1.0f, 1.0f),
    true,
    false,
    -1,
    UsesOwnerMeleeHitCooldown: true);
  PlayerHandle owner = new(1);
  Entity target = world.Create(new NpcTagComponent());
  Entity firstProjectile = world.Create(
    definition,
    new ProjectileOwnerComponent(owner),
    new ProjectilePenetrationComponent(-1));
  Entity secondProjectile = world.Create(
    definition,
    new ProjectileOwnerComponent(owner),
    new ProjectilePenetrationComponent(-1));
  HitImmunityComponent immunity = new();
  ProjectileDamageSystem damage = new();
  IReadOnlyList<DamageRequestedEvent> accepted = damage.Resolve(
    world,
    [
      new DamageRequestedEvent(firstProjectile, target, 10, 1, 17),
      new DamageRequestedEvent(secondProjectile, target, 10, 2, 17)
    ],
    immunity,
    _ => 3);
  if (accepted.Count != 1 || accepted[0].Projectile != firstProjectile ||
      !immunity.IsOwnerMeleeNpcImmune(owner.Value, 17))
  {
    throw new InvalidOperationException(
      "Owner melee cooldown did not reject the second projectile hit on the same NPC.");
  }

  HitImmunitySystem immunitySystem = new();
  immunitySystem.Tick(immunity);
  immunitySystem.Tick(immunity);
  immunitySystem.Tick(immunity);
  Entity thirdProjectile = world.Create(
    definition,
    new ProjectileOwnerComponent(owner),
    new ProjectilePenetrationComponent(-1));
  IReadOnlyList<DamageRequestedEvent> afterCooldown = damage.Resolve(
    world,
    [new DamageRequestedEvent(thirdProjectile, target, 10, 3, 17)],
    immunity,
    _ => 3);
  if (afterCooldown.Count != 1)
  {
    throw new InvalidOperationException("Owner melee cooldown did not expire after its animation ticks.");
  }

  Entity trapProjectile = world.Create(
    definition,
    new ProjectileOwnerComponent(owner),
    new ProjectileTrapComponent(),
    new ProjectilePenetrationComponent(-1));
  Entity trapTarget = world.Create(new NpcTagComponent());
  if (damage.Resolve(
        world,
        [new DamageRequestedEvent(trapProjectile, trapTarget, 10, 4, 18)],
        immunity,
        _ => 3).Count != 1 ||
      immunity.IsOwnerMeleeNpcImmune(owner.Value, 18))
  {
    throw new InvalidOperationException(
      "CareForAttackCD incorrectly applied owner melee cooldown to a trap projectile.");
  }

  Entity reservedOwnerProjectile = world.Create(
    definition,
    new ProjectileOwnerComponent(new PlayerHandle(255)),
    new ProjectilePenetrationComponent(-1));
  Entity reservedOwnerTarget = world.Create(new NpcTagComponent());
  if (damage.Resolve(
        world,
        [new DamageRequestedEvent(reservedOwnerProjectile, reservedOwnerTarget, 10, 5, 19)],
        immunity,
        _ => 3).Count != 1 ||
      immunity.IsOwnerMeleeNpcImmune(255, 19))
  {
    throw new InvalidOperationException(
      "CareForAttackCD incorrectly applied owner melee cooldown to reserved owner slot 255.");
  }
}

static void VerifyPlayerAttackCooldownRule()
{
  PlayerControlStateComponent state = new() { FireCooldownTicks = 4 };
  state.ApplyAttackCooldown(10);
  if (state.FireCooldownTicks != 10)
  {
    throw new InvalidOperationException(
      "Attack cooldown did not extend to the requested frame count.");
  }

  state.ApplyAttackCooldown(3);
  state.ApplyAttackCooldown(10);
  if (state.FireCooldownTicks != 10)
  {
    throw new InvalidOperationException(
      "Attack cooldown was lowered or changed by an idempotent application.");
  }

  bool rejectedNegative = false;
  try
  {
    state.ApplyAttackCooldown(-1);
  }
  catch (ArgumentOutOfRangeException)
  {
    rejectedNegative = true;
  }

  if (!rejectedNegative)
  {
    throw new InvalidOperationException(
      "Attack cooldown accepted a negative frame count.");
  }

  using DomeSimulation simulation = new(new WorldGrid(400, 300));
  PlayerHandle player = simulation.CreatePlayer(new SimulationVector(2.0f, 0.0f));
  simulation.Tick(new SimulationInputBatch(new PlayerInput(
    player,
    MoveLeft: false,
    MoveRight: false,
    Jump: false,
    Fire: true)));
  PlayerStateSnapshot snapshot = simulation.CreatePlayerStateSnapshot(player);
  if (snapshot.FireCooldownTicks != 10)
  {
    throw new InvalidOperationException(
      "Player fire commit did not project the component attack cooldown.");
  }

  Console.WriteLine("PASS: player attack cooldown is monotonic and bounded");
}

static void VerifyPaladinShieldDefensePolicy()
{
  if (!PaladinShieldDefensePolicy.CanDefend(
        isActive: true,
        isDead: false,
        hasPaladinShield: true,
        team: 2,
        otherPlayerTeam: 2,
        life: 26,
        maximumLife: 100) ||
      PaladinShieldDefensePolicy.CanDefend(true, false, true, 0, 0, 100, 100) ||
      PaladinShieldDefensePolicy.CanDefend(true, false, true, 2, 3, 100, 100) ||
      PaladinShieldDefensePolicy.CanDefend(true, false, true, 2, 2, 25, 100) ||
      PaladinShieldDefensePolicy.CanDefend(false, false, true, 2, 2, 100, 100) ||
      PaladinShieldDefensePolicy.CanDefend(true, true, true, 2, 2, 100, 100) ||
      PaladinShieldDefensePolicy.CanDefend(true, false, false, 2, 2, 100, 100) ||
      PaladinShieldDefensePolicy.CanDefend(true, false, true, 2, 2, 101, 100))
  {
    throw new InvalidOperationException(
      "Paladin shield defense policy did not enforce active, team and life thresholds.");
  }

  Console.WriteLine("PASS: Paladin shield defense policy enforces source-backed thresholds");
}

static void VerifyPlayerLuckAndMiscCounterPolicies()
{
  PlayerLuckInputs inputs = new(
    LadyBugLuckTimeLeft: PlayerLuckCalculationPolicy.LadyBugGoodLuckTime / 2,
    TorchLuck: 0.5f,
    LuckPotionLevel: 2,
    KiteLuckLevel: 3,
    UsedGalaxyPearl: true,
    LanternsUp: true,
    HasGardenGnomeNearby: true,
    Stinky: true,
    EquipmentBasedLuckBonus: 0.1f,
    CoinLuck: 25.0f,
    BrokenMirrorBadLuck: true);
  float luck = PlayerLuckCalculationPolicy.Calculate(inputs);
  if (MathF.Abs(luck - 0.73f) > 0.0001f)
  {
    throw new InvalidOperationException("Player luck equation did not match source-backed factors.");
  }

  PlayerLuckInputs advanced = PlayerLuckCalculationPolicy.AdvanceFactors(inputs, dayRate: 1);
  if (advanced.LadyBugLuckTimeLeft != inputs.LadyBugLuckTimeLeft - 1 ||
      advanced.CoinLuck >= inputs.CoinLuck)
  {
    throw new InvalidOperationException("Player luck factors did not advance deterministically.");
  }

  PlayerLuckInputs expired = inputs with { LadyBugLuckTimeLeft = 1, CoinLuck = 0.25f };
  expired = PlayerLuckCalculationPolicy.AdvanceFactors(expired, dayRate: 2);
  if (expired.LadyBugLuckTimeLeft != 0 || expired.CoinLuck != 0.0f)
  {
    throw new InvalidOperationException("Player luck factor expiry did not clamp at zero.");
  }

  if (PlayerMiscCounterPolicy.Advance(298) != 299 ||
      PlayerMiscCounterPolicy.Advance(299) != 0)
  {
    throw new InvalidOperationException("Player misc counter did not wrap at 300 ticks.");
  }

  try
  {
    _ = PlayerMiscCounterPolicy.Advance(300);
    throw new InvalidOperationException("Player misc counter accepted an out-of-range value.");
  }
  catch (ArgumentOutOfRangeException)
  {
  }

  try
  {
    _ = PlayerLuckCalculationPolicy.Calculate(inputs with { CoinLuck = float.NaN });
    throw new InvalidOperationException("Player luck accepted a non-finite coin factor.");
  }
  catch (ArgumentOutOfRangeException)
  {
  }

  try
  {
    _ = PlayerLuckCalculationPolicy.AdvanceFactors(inputs, dayRate: -1);
    throw new InvalidOperationException("Player luck accepted a negative day rate.");
  }
  catch (ArgumentOutOfRangeException)
  {
  }

  Console.WriteLine("PASS: player luck factors and misc counter follow source-backed ordering");
}

static void VerifyProjectileStaticImmunityDefinitionBoundary()
{
  ProjectileDefinition defaults = ProjectileDefinitionRegistry.CreateDefault().OrderedDefinitions
    .Single(definition => definition.ProjectileType == 281);
  ProjectileDefinition unconfigured = defaults with
  {
    UsesStaticNpcImmunity = false,
    StaticNpcHitCooldownTicks = -1
  };
  _ = new ProjectileDefinitionRegistry([unconfigured]);

  try
  {
    _ = new ProjectileDefinitionRegistry([
      defaults with { UsesStaticNpcImmunity = true, StaticNpcHitCooldownTicks = 0 }]);
    throw new InvalidOperationException(
      "Projectile definitions accepted static NPC immunity without a cooldown.");
  }
  catch (ArgumentOutOfRangeException)
  {
  }

  try
  {
    _ = new ProjectileDefinitionRegistry([
      defaults with { UsesStaticNpcImmunity = true, StaticNpcHitCooldownTicks = -1 }]);
    throw new InvalidOperationException(
      "Projectile definitions accepted enabled static NPC immunity without a cooldown.");
  }
  catch (ArgumentOutOfRangeException)
  {
  }
}

static void VerifyProjectileOwnedBySomeonePolicy()
{
  if (!ProjectileOwnershipPolicy.OwnedBySomeone(new ProjectileOwnerComponent(new PlayerHandle(1))) ||
      ProjectileOwnershipPolicy.OwnedBySomeone(new ProjectileOwnerComponent(default)) ||
      !ProjectileOwnershipPolicy.OwnedBySomeone(
        new ProjectileOwnerComponent(new PlayerHandle(254)),
        isNpcProjectile: false,
        isTrap: false) ||
      ProjectileOwnershipPolicy.OwnedBySomeone(
        new ProjectileOwnerComponent(new PlayerHandle(1)),
        isNpcProjectile: true,
        isTrap: false) ||
      ProjectileOwnershipPolicy.OwnedBySomeone(
        new ProjectileOwnerComponent(new PlayerHandle(1)),
        isNpcProjectile: false,
        isTrap: true))
  {
    throw new InvalidOperationException(
      "Projectile OwnedBySomeone policy did not preserve owner, NPC, and trap boundaries.");
  }
}

static void VerifyProjectileSectionCoordinates()
{
  if (ProjectileSectionCoordinatePolicy.Resolve(
        new SimulationVector(199.99f, 149.99f),
        400,
        300) != new WorldSectionCoordinates(0, 0) ||
      ProjectileSectionCoordinatePolicy.Resolve(
        new SimulationVector(200.0f, 150.0f),
        400,
        300) != new WorldSectionCoordinates(1, 1) ||
      ProjectileSectionCoordinatePolicy.Resolve(
        new SimulationVector(-10.0f, -1.0f),
        400,
        300) != new WorldSectionCoordinates(0, 0) ||
      ProjectileSectionCoordinatePolicy.Resolve(
        new SimulationVector(999.0f, 999.0f),
        400,
        300) != new WorldSectionCoordinates(1, 1))
  {
    throw new InvalidOperationException(
      "Projectile section coordinates did not clamp and partition positions deterministically.");
  }

  try
  {
    _ = ProjectileSectionCoordinatePolicy.Resolve(
      new SimulationVector(float.NaN, 0.0f),
      400,
      300);
    throw new InvalidOperationException(
      "Projectile section coordinates accepted a non-finite position.");
  }
  catch (ArgumentOutOfRangeException)
  {
  }

  Console.WriteLine("PASS: projectile NetSectionCoordinates policy clamps finite positions");
}

static void VerifyProjectileOwnerHitCheck()
{
  WorldGrid world = new(400, 300);
  ProjectileOwnerHitCheckSystem system = new();
  LocationComponent owner = new(1.0f, 1.0f);
  ColliderComponent collider = new(1.0f, 1.0f);
  LocationComponent target = new(5.0f, 1.0f);
  if (system.CanHit(world, owner, collider, 1, 1.0f, target, collider, 3.0f) ||
      !system.CanHit(world, owner, collider, 1, 1.0f, target, collider, 5.0f))
  {
    throw new InvalidOperationException("Projectile owner hit distance gate is inconsistent.");
  }

  _ = world.TrySetTile(3, 1, new WorldTile(IsActive: true, Type: 1));
  _ = world.TrySetTile(3, 0, new WorldTile(IsActive: true, Type: 1));
  _ = world.TrySetTile(3, 2, new WorldTile(IsActive: true, Type: 1));
  if (system.CanHit(world, owner, collider, 1, 1.0f, target, collider, 5.0f))
  {
    throw new InvalidOperationException("Projectile owner hit check did not reject an occluded target.");
  }

  WorldGrid colliderWorld = new(400, 300);
  _ = colliderWorld.TrySetTile(4, 3, new WorldTile(IsActive: true, Type: 1));
  ProjectileCollisionSystem collision = new();
  LocationComponent pathStart = new(1.0f, 1.0f);
  LocationComponent pathEnd = new(5.0f, 1.0f);
  ColliderComponent smallCollider = new(1.0f, 1.0f);
  ColliderComponent largeCollider = new(3.0f, 3.0f);
  bool fixedColliderHit = collision.PathHitsSolidTile(
    colliderWorld,
    pathStart,
    pathEnd,
    smallCollider);
  bool interpolatedColliderHit = collision.PathHitsSolidTile(
    colliderWorld,
    pathStart,
    smallCollider,
    pathEnd,
    largeCollider);
  if (fixedColliderHit || !interpolatedColliderHit)
  {
    throw new InvalidOperationException(
      $"Owner hit collider sweep did not incorporate target collider interpolation: " +
      $"fixed={fixedColliderHit}, interpolated={interpolatedColliderHit}.");
  }
}

static void VerifyProjectileReflectionEligibilityPolicy()
{
  ProjectileDefinitionComponent eligible = new(
    1, 1, 10, 30, new ColliderComponent(1, 1), true, false);
  ProjectileDefinitionComponent hostile = eligible with { Hostile = true };
  ProjectileDefinitionComponent zeroDamage = eligible with { DefaultDamage = 0 };
  if (!ProjectileReflectionEligibilityPolicy.CanBeReflected(eligible, true) ||
      ProjectileReflectionEligibilityPolicy.CanBeReflected(eligible, false) ||
      ProjectileReflectionEligibilityPolicy.CanBeReflected(hostile, true) ||
      ProjectileReflectionEligibilityPolicy.CanBeReflected(zeroDamage, true))
  {
    throw new InvalidOperationException("Projectile reflection eligibility policy is inconsistent.");
  }
}

static void VerifyNpcProjectileReflectionEligibility()
{
  NpcBehaviorStateComponent defaultBehavior = new(
    NpcBehaviorId.OrdinaryChase,
    new NpcChaseState(1.0f, 0.0f),
    new NpcTownHomeState(default, true, 0));
  if (defaultBehavior.ReflectsProjectiles)
  {
    throw new InvalidOperationException(
      "The source-default NPC behavior unexpectedly enabled projectile reflection.");
  }

  NpcBehaviorStateComponent reflectingBehavior = new(
    NpcBehaviorId.OrdinaryChase,
    new NpcChaseState(1.0f, 0.0f),
    new NpcTownHomeState(default, true, 0),
    reflectsProjectiles: true);
  ProjectileDefinitionComponent eligibleByType = new(
    ProjectileType: 728,
    BehaviorId: 0,
    DefaultDamage: 1,
    DefaultLifetimeTicks: 30,
    Collider: new ColliderComponent(1.0f, 1.0f),
    Friendly: true,
    Hostile: false,
    LegacyAiStyle: 0);
  ProjectileDefinitionComponent eligibleByAiStyle = eligibleByType with
  {
    ProjectileType = 1000,
    LegacyAiStyle = 2
  };
  ProjectileDefinitionComponent unsupported = eligibleByType with
  {
    ProjectileType = 1000,
    LegacyAiStyle = 0
  };
  ProjectileDefinitionComponent hostile = eligibleByType with { Hostile = true };

  if (!ProjectileReflectionEligibilityPolicy.CanBeReflectedByNpc(
        eligibleByType,
        runtimeDamage: 10,
        reflectingBehavior,
        isActive: true) ||
      !ProjectileReflectionEligibilityPolicy.CanBeReflectedByNpc(
        eligibleByAiStyle,
        runtimeDamage: 10,
        reflectingBehavior,
        isActive: true) ||
      ProjectileReflectionEligibilityPolicy.CanBeReflectedByNpc(
        eligibleByType,
        runtimeDamage: 10,
        defaultBehavior,
        isActive: true) ||
      ProjectileReflectionEligibilityPolicy.CanBeReflectedByNpc(
        eligibleByType,
        runtimeDamage: 10,
        reflectingBehavior,
        isActive: false) ||
      ProjectileReflectionEligibilityPolicy.CanBeReflectedByNpc(
        hostile,
        runtimeDamage: 10,
        reflectingBehavior,
        isActive: true) ||
      ProjectileReflectionEligibilityPolicy.CanBeReflectedByNpc(
        eligibleByType,
        runtimeDamage: 0,
        reflectingBehavior,
        isActive: true) ||
      ProjectileReflectionEligibilityPolicy.CanBeReflectedByNpc(
        unsupported,
        runtimeDamage: 10,
        reflectingBehavior,
        isActive: true))
  {
    throw new InvalidOperationException(
      "NPC projectile reflection eligibility did not preserve the source-backed gates.");
  }

  Console.WriteLine(
    "PASS: NPC reflects-projectiles behavior owner and projectile eligibility policy");
}

static void VerifyType4SourceBackedDefaults()
{
  ProjectileDefinitionRegistry definitions = ProjectileDefinitionRegistry.CreateDefault();
  if (!definitions.TryGet(4, out ProjectileDefinition definition) ||
      definition.LifetimeTicks != 1200 || definition.MaximumPenetration != 5 ||
      definition.ExtraUpdates != 0 || !definition.Friendly || definition.Hostile)
  {
    throw new InvalidOperationException("Type 4 defaults do not match the source-backed contract.");
  }
}

static void VerifyType2SourceBackedDefaults()
{
  ProjectileDefinitionRegistry definitions = ProjectileDefinitionRegistry.CreateDefault();
  if (!definitions.TryGet(2, out ProjectileDefinition definition) ||
      definition.LifetimeTicks != 1200 || definition.BehaviorId != 2 ||
      !definition.Friendly || definition.Hostile || definition.MaximumPenetration != 1)
  {
    throw new InvalidOperationException("Type 2 defaults do not match the source-backed contract.");
  }
}

static void VerifyType6SourceBackedDefaults()
{
  ProjectileDefinitionRegistry definitions = ProjectileDefinitionRegistry.CreateDefault();
  if (!definitions.TryGet(6, out ProjectileDefinition definition) ||
      definition.BehaviorId != 1 || definition.MaximumPenetration != -1 ||
      !definition.Friendly || definition.Hostile ||
      definition.PlayerDamagePolicy != PlayerDamagePolicy.None || definition.LegacyAiStyle != 3)
  {
    throw new InvalidOperationException("Type 6 defaults do not match the source-backed contract.");
  }
}

static void VerifyProjectileDamageClassDefaults()
{
  ProjectileDefinitionRegistry definitions = ProjectileDefinitionRegistry.CreateDefault();
  foreach (int projectileType in new[] { 1, 2, 4 })
  {
    if (!definitions.TryGet(projectileType, out ProjectileDefinition definition) ||
        definition.DamageClass != ProjectileDamageClass.Ranged)
    {
      throw new InvalidOperationException(
        $"Projectile type {projectileType} did not retain its source-backed ranged class.");
    }
  }
}

static void VerifyType3LegacyAiStyleMetadata()
{
  ProjectileDefinitionRegistry definitions = ProjectileDefinitionRegistry.CreateDefault();
  if (!definitions.TryGet(3, out ProjectileDefinition definition) ||
      definition.BehaviorId != 3 || definition.LegacyAiStyle != 2 ||
      !definition.Friendly || definition.Hostile || definition.MaximumPenetration != 4)
  {
    throw new InvalidOperationException(
      "Type 3 did not preserve its source aiStyle separately from typed behavior.");
  }
}

static void VerifyType249SourceBackedMetadata()
{
  ProjectileDefinitionRegistry definitions = ProjectileDefinitionRegistry.CreateDefault();
  if (!definitions.TryGet(249, out ProjectileDefinition definition) ||
      definition.Collider.Width != 0.75f || definition.Collider.Height != 0.75f ||
      definition.LegacyAiStyle != 2 || definition.IgnoreWater || !definition.Friendly ||
      definition.Hostile || definition.DamageClass != ProjectileDamageClass.Ranged)
  {
    throw new InvalidOperationException("Type 249 metadata does not match the source-backed contract.");
  }
}

static void VerifyHostileProjectileLegacyAiMetadata()
{
  ProjectileDefinitionRegistry definitions = ProjectileDefinitionRegistry.CreateDefault();
  if (!definitions.TryGet(347, out ProjectileDefinition type347) ||
      type347.LegacyAiStyle != 2 || type347.Collider.Width != 0.375f ||
      type347.Collider.Height != 0.375f || !type347.Hostile || type347.Friendly ||
      !definitions.TryGet(300, out ProjectileDefinition type300) ||
      type300.LegacyAiStyle != 2 || !type300.IgnoreWater || type300.CollidesWithTiles)
  {
    throw new InvalidOperationException(
      "Hostile projectile legacy AI metadata does not match source-backed definitions.");
  }
}

static void VerifyType599SourceBackedMetadata()
{
  ProjectileDefinitionRegistry definitions = ProjectileDefinitionRegistry.CreateDefault();
  if (!definitions.TryGet(599, out ProjectileDefinition definition) ||
      definition.LegacyAiStyle != 2 || definition.DamageClass != ProjectileDamageClass.Ranged ||
      definition.MaximumPenetration != 6 || !definition.Friendly || definition.Hostile)
  {
    throw new InvalidOperationException("Type 599 metadata does not match the source-backed contract.");
  }
}

static void VerifyType909SourceBackedMetadata()
{
  ProjectileDefinitionRegistry definitions = ProjectileDefinitionRegistry.CreateDefault();
  if (!definitions.TryGet(909, out ProjectileDefinition definition) ||
      definition.LegacyAiStyle != 2 || definition.Collider.Width != 0.75f ||
      definition.Collider.Height != 0.75f || !definition.Hostile || definition.Friendly ||
      definition.MaximumPenetration != 1 ||
      definition.PlayerDamagePolicy != PlayerDamagePolicy.HostileNonPvp)
  {
    throw new InvalidOperationException("Type 909 metadata does not match the source-backed contract.");
  }
}

static void VerifyType520SourceBackedMetadata()
{
  ProjectileDefinitionRegistry definitions = ProjectileDefinitionRegistry.CreateDefault();
  if (!definitions.TryGet(520, out ProjectileDefinition definition) ||
      definition.LegacyAiStyle != 2 || definition.DamageClass != ProjectileDamageClass.Ranged ||
      definition.MaximumPenetration != 3 || !definition.Friendly || definition.Hostile)
  {
    throw new InvalidOperationException("Type 520 metadata does not match the source-backed contract.");
  }
}

static void VerifyType521And522SourceBackedMetadata()
{
  ProjectileDefinitionRegistry definitions = ProjectileDefinitionRegistry.CreateDefault();
  if (!definitions.TryGet(521, out ProjectileDefinition parent) ||
      parent.LegacyAiStyle != 29 || parent.DamageClass != ProjectileDamageClass.Magic ||
      parent.ExtraUpdates != 1 || !parent.Friendly || parent.Hostile ||
      !parent.ChildSpawn.IsEnabled || parent.ChildSpawn.ProjectileType != 522)
  {
    throw new InvalidOperationException("Type 521 metadata does not match the source-backed contract.");
  }

  if (!definitions.TryGet(522, out ProjectileDefinition child) ||
      child.LegacyAiStyle != 29 || child.DamageClass != ProjectileDamageClass.Magic ||
      !child.Friendly || child.Hostile)
  {
    throw new InvalidOperationException("Type 522 metadata does not match the source-backed contract.");
  }
}

static void VerifyType162SourceBackedMetadata()
{
  ProjectileDefinitionRegistry definitions = ProjectileDefinitionRegistry.CreateDefault();
  if (!definitions.TryGet(162, out ProjectileDefinition definition) ||
      definition.LegacyAiStyle != 2 || definition.DamageClass != ProjectileDamageClass.Ranged ||
      definition.LifetimeTicks != 3600 ||
      definition.MaximumPenetration != 4 || !definition.Friendly || definition.Hostile)
  {
    throw new InvalidOperationException("Type 162 metadata does not match the source-backed contract.");
  }
}

static void VerifyType471501504SourceBackedMetadata()
{
  ProjectileDefinitionRegistry definitions = ProjectileDefinitionRegistry.CreateDefault();
  if (!definitions.TryGet(471, out ProjectileDefinition hostileRanged) ||
      hostileRanged.LegacyAiStyle != 2 ||
      hostileRanged.DamageClass != ProjectileDamageClass.Ranged ||
      hostileRanged.Collider.Width != 1.0f || hostileRanged.Collider.Height != 1.0f ||
      !hostileRanged.Hostile || hostileRanged.Friendly)
  {
    throw new InvalidOperationException("Type 471 metadata does not match the source-backed contract.");
  }

  if (!definitions.TryGet(501, out ProjectileDefinition areaDamage) ||
      areaDamage.LegacyAiStyle != 2 || areaDamage.DamageClass != ProjectileDamageClass.Ranged ||
      areaDamage.Collider.Width != 0.875f || areaDamage.Collider.Height != 0.875f ||
      !areaDamage.Hostile || areaDamage.Friendly || !areaDamage.OnDespawnAreaDamage.IsEnabled)
  {
    throw new InvalidOperationException("Type 501 metadata does not match the source-backed contract.");
  }

  if (!definitions.TryGet(504, out ProjectileDefinition melee) ||
      melee.LegacyAiStyle != 2 || melee.DamageClass != ProjectileDamageClass.Melee ||
      melee.Collider.Width != 0.625f || melee.Collider.Height != 0.625f ||
      !melee.Friendly || melee.Hostile || !melee.OnHitStatusEffect.IsEnabled)
  {
    throw new InvalidOperationException("Type 504 metadata does not match the source-backed contract.");
  }
}

static void VerifyType21166330SourceBackedMetadata()
{
  ProjectileDefinitionRegistry definitions = ProjectileDefinitionRegistry.CreateDefault();
  if (!definitions.TryGet(21, out ProjectileDefinition type21) ||
      type21.LegacyAiStyle != 2 || type21.DamageClass != ProjectileDamageClass.Ranged ||
      type21.Collider.Width != 1.0f || type21.Collider.Height != 1.0f ||
      !type21.Friendly || type21.Hostile)
  {
    throw new InvalidOperationException("Type 21 metadata does not match the source-backed contract.");
  }

  if (!definitions.TryGet(166, out ProjectileDefinition type166) ||
      type166.LegacyAiStyle != 2 || type166.DamageClass != ProjectileDamageClass.Ranged ||
      type166.Collider.Width != 0.875f || type166.Collider.Height != 0.875f ||
      !type166.Friendly || type166.Hostile)
  {
    throw new InvalidOperationException("Type 166 metadata does not match the source-backed contract.");
  }

  if (!definitions.TryGet(330, out ProjectileDefinition type330) ||
      type330.LegacyAiStyle != 2 || type330.DamageClass != ProjectileDamageClass.Ranged ||
      type330.Collider.Width != 1.375f || type330.Collider.Height != 1.375f ||
      type330.MaximumPenetration != 6 || !type330.Friendly || type330.Hostile)
  {
    throw new InvalidOperationException("Type 330 metadata does not match the source-backed contract.");
  }
}

static void VerifyType954979SourceBackedMetadata()
{
  ProjectileDefinitionRegistry definitions = ProjectileDefinitionRegistry.CreateDefault();
  if (!definitions.TryGet(954, out ProjectileDefinition type954) ||
      type954.LegacyAiStyle != 2 || type954.DamageClass != ProjectileDamageClass.Magic ||
      type954.Collider.Width != 0.625f || type954.Collider.Height != 0.625f ||
      type954.MaximumPenetration != 2 || !type954.Friendly || type954.Hostile ||
      !type954.OnHitStatusEffect.IsEnabled)
  {
    throw new InvalidOperationException("Type 954 metadata does not match the source-backed contract.");
  }

  if (!definitions.TryGet(979, out ProjectileDefinition type979) ||
      type979.LegacyAiStyle != 2 || type979.DamageClass != ProjectileDamageClass.Magic ||
      type979.Collider.Width != 0.625f || type979.Collider.Height != 0.625f ||
      type979.MaximumPenetration != 2 || !type979.Friendly || type979.Hostile ||
      !type979.OnHitStatusEffect.IsEnabled)
  {
    throw new InvalidOperationException("Type 979 metadata does not match the source-backed contract.");
  }
}

static void VerifyType3041012370371936SourceBackedMetadata()
{
  ProjectileDefinitionRegistry definitions = ProjectileDefinitionRegistry.CreateDefault();
  if (!definitions.TryGet(304, out ProjectileDefinition type304) ||
      type304.LegacyAiStyle != 2 || type304.Collider.Width != 1.875f ||
      type304.Collider.Height != 1.875f || !type304.Friendly || type304.Hostile)
  {
    throw new InvalidOperationException("Type 304 metadata does not match the source-backed contract.");
  }

  if (!definitions.TryGet(1012, out ProjectileDefinition type1012) ||
      type1012.LegacyAiStyle != 2 || type1012.DamageClass != ProjectileDamageClass.Melee ||
      type1012.Collider.Width != 1.125f || type1012.Collider.Height != 1.125f ||
      !type1012.Friendly || type1012.Hostile)
  {
    throw new InvalidOperationException("Type 1012 metadata does not match the source-backed contract.");
  }

  if (!definitions.TryGet(370, out ProjectileDefinition type370) ||
      type370.LegacyAiStyle != 2 || !type370.Friendly || type370.Hostile ||
      !type370.OnDespawnStatusEffect.IsEnabled ||
      !definitions.TryGet(371, out ProjectileDefinition type371) ||
      type371.LegacyAiStyle != 2 || !type371.Friendly || type371.Hostile ||
      !type371.OnDespawnStatusEffect.IsEnabled ||
      !definitions.TryGet(936, out ProjectileDefinition type936) ||
      type936.LegacyAiStyle != 2 || !type936.Friendly || type936.Hostile ||
      !type936.OnDespawnStatusEffect.IsEnabled)
  {
    throw new InvalidOperationException("Type 370/371/936 metadata does not match the source-backed contract.");
  }
}

static void VerifyType6970621SourceBackedMetadata()
{
  ProjectileDefinitionRegistry definitions = ProjectileDefinitionRegistry.CreateDefault();
  foreach (int projectileType in new[] { 69, 70, 621 })
  {
    if (!definitions.TryGet(projectileType, out ProjectileDefinition definition) ||
        definition.LegacyAiStyle != 2 || definition.Collider.Width != 0.875f ||
        definition.Collider.Height != 0.875f || !definition.Friendly || definition.Hostile ||
        definition.MaximumPenetration != 1)
    {
      throw new InvalidOperationException(
        $"Type {projectileType} metadata does not match the source-backed contract: " +
        $"ai={definition.LegacyAiStyle}, size={definition.Collider.Width}x{definition.Collider.Height}, " +
        $"friendly={definition.Friendly}, hostile={definition.Hostile}, " +
        $"penetration={definition.MaximumPenetration}.");
    }
  }
}

static void VerifyType240SourceBackedMetadata()
{
  ProjectileDefinitionRegistry definitions = ProjectileDefinitionRegistry.CreateDefault();
  if (!definitions.TryGet(240, out ProjectileDefinition definition) ||
      definition.LegacyAiStyle != 2 || definition.Collider.Width != 1.0f ||
      definition.Collider.Height != 1.0f || !definition.Hostile || definition.Friendly ||
      definition.MaximumPenetration != -1 || !definition.OnDespawnAreaDamage.IsEnabled)
  {
    throw new InvalidOperationException("Type 240 metadata does not match the source-backed contract.");
  }
}

static void VerifyType589SourceBackedMetadata()
{
  ProjectileDefinitionRegistry definitions = ProjectileDefinitionRegistry.CreateDefault();
  if (!definitions.TryGet(589, out ProjectileDefinition definition) ||
      definition.LegacyAiStyle != 2 || definition.Collider.Width != 0.625f ||
      definition.Collider.Height != 0.625f || !definition.Friendly || definition.Hostile ||
      definition.MaximumPenetration != 1 || definition.DamageClass != ProjectileDamageClass.Generic)
  {
    throw new InvalidOperationException("Type 589 metadata does not match the source-backed contract.");
  }
}

static void VerifyType124SourceBackedAiMetadata()
{
  ProjectileDefinitionRegistry definitions = ProjectileDefinitionRegistry.CreateDefault();
  if (!definitions.TryGet(1, out ProjectileDefinition type1) || type1.LegacyAiStyle != 1 ||
      type1.DamageClass != ProjectileDamageClass.Ranged || !type1.Friendly || type1.Hostile)
  {
    throw new InvalidOperationException("Type 1 AI metadata does not match the source-backed contract.");
  }

  if (!definitions.TryGet(2, out ProjectileDefinition type2) || type2.LegacyAiStyle != 1 ||
      type2.DamageClass != ProjectileDamageClass.Ranged || !type2.Friendly || type2.Hostile)
  {
    throw new InvalidOperationException("Type 2 AI metadata does not match the source-backed contract.");
  }

  if (!definitions.TryGet(4, out ProjectileDefinition type4) || type4.LegacyAiStyle != 1 ||
      type4.DamageClass != ProjectileDamageClass.Ranged || !type4.Friendly || type4.Hostile ||
      type4.MaximumPenetration != 5)
  {
    throw new InvalidOperationException("Type 4 AI metadata does not match the source-backed contract.");
  }
}

static void VerifyArrowMetadata()
{
  ProjectileDefinitionRegistry definitions = ProjectileDefinitionRegistry.CreateDefault();
  foreach (int projectileType in new[] { 1, 2, 4, 5 })
  {
    if (!definitions.TryGet(projectileType, out ProjectileDefinition definition) ||
        !definition.IsArrow)
    {
      throw new InvalidOperationException(
        $"Type {projectileType} arrow metadata does not match the source-backed contract.");
    }
  }

  if (definitions.Definitions[3].IsArrow)
  {
    throw new InvalidOperationException("Type 3 incorrectly inherited arrow metadata.");
  }
}

static void VerifyProjectileWipableTurretPolicy()
{
  int[] persistingTypes = [663, 665, 667, 677, 678, 679, 688, 689, 690, 691, 692, 693];
  foreach (int projectileType in persistingTypes)
  {
    if (!LegacySentryPersistenceRegistry.ShouldPersist(projectileType, eventActive: true) ||
        LegacySentryPersistenceRegistry.ShouldPersist(projectileType, eventActive: false))
    {
      throw new InvalidOperationException(
        $"Sentry persistence type {projectileType} did not preserve the source event gate.");
    }

    if (ProjectileTurretPersistencePolicy.CanWipe(
          projectileType,
          new PlayerHandle(1),
          new PlayerHandle(1),
          isSentry: true,
          eventActive: true))
    {
      throw new InvalidOperationException(
        $"Active event incorrectly allowed wipe for sentry type {projectileType}.");
    }
  }

  bool ordinarySentryWipable = ProjectileTurretPersistencePolicy.CanWipe(
        projectileType: 14,
        new PlayerHandle(1),
        new PlayerHandle(1),
        isSentry: true,
        eventActive: true);
  bool eventSentryWipable = ProjectileTurretPersistencePolicy.CanWipe(
        projectileType: 663,
        new PlayerHandle(1),
        new PlayerHandle(1),
        isSentry: true,
        eventActive: false);
  bool otherOwnerWipable = ProjectileTurretPersistencePolicy.CanWipe(
        projectileType: 14,
        new PlayerHandle(1),
        new PlayerHandle(2),
        isSentry: true,
        eventActive: true);
  bool nonSentryWipable = ProjectileTurretPersistencePolicy.CanWipe(
        projectileType: 14,
        new PlayerHandle(1),
        new PlayerHandle(1),
        isSentry: false,
        eventActive: true);
  bool invalidTypeWipable = ProjectileTurretPersistencePolicy.CanWipe(
        projectileType: -1,
        new PlayerHandle(1),
        new PlayerHandle(1),
        isSentry: true,
        eventActive: false);
  if (!ordinarySentryWipable || !eventSentryWipable || otherOwnerWipable ||
      nonSentryWipable || invalidTypeWipable)
  {
    throw new InvalidOperationException(
      $"WipableTurret policy gates were incorrect: ordinary={ordinarySentryWipable}, " +
      $"event={eventSentryWipable}, otherOwner={otherOwnerWipable}, " +
      $"nonSentry={nonSentryWipable}, invalidType={invalidTypeWipable}.");
  }

  Console.WriteLine("PASS: source-backed WipableTurret persistence policy is typed and fail-closed");
}

static void VerifyProjectileWipableTurretSimulationQuery()
{
  ProjectileDefinitionRegistry definitions = new([
    new ProjectileDefinition(
      663,
      1,
      10,
      1200,
      new ColliderComponent(0.5f, 0.5f),
      true,
      false,
      1,
      IsSentry: true),
    new ProjectileDefinition(
      14,
      1,
      10,
      1200,
      new ColliderComponent(0.5f, 0.5f),
      true,
      false,
      1)]);
  using DomeSimulation simulation = new(new WorldGrid(400, 300), definitions);
  PlayerHandle owner = simulation.CreatePlayer(new SimulationVector(5.0f, 5.0f));
  PlayerHandle otherPlayer = simulation.CreatePlayer(new SimulationVector(15.0f, 15.0f));
  simulation.QueueProjectileSpawn(new SpawnProjectileCommand(
    owner,
    5.0f,
    5.0f,
    1,
    10,
    1200,
    ProjectileType: 663));
  simulation.QueueProjectileSpawn(new SpawnProjectileCommand(
    owner,
    15.0f,
    15.0f,
    1,
    10,
    1200,
    ProjectileType: 14));
  simulation.Tick(new SimulationInputBatch());

  ProjectileReplicationSnapshot[] snapshots = simulation.CreateProjectileReplicationSnapshots()
    .Where(snapshot => snapshot.IsActive)
    .ToArray();
  ProjectileReplicationSnapshot persistent = snapshots.Single(
    snapshot => snapshot.ProjectileType == 663);
  ProjectileReplicationSnapshot ordinary = snapshots.Single(
    snapshot => snapshot.ProjectileType == 14);
  ProjectileReplicationSnapshot[] beforeQueries = snapshots.ToArray();
  if (simulation.CanWipeProjectileTurret(persistent.ReplicationId, owner, eventActive: true) ||
      !simulation.CanWipeProjectileTurret(persistent.ReplicationId, owner, eventActive: false) ||
      simulation.CanWipeProjectileTurret(
        persistent.ReplicationId,
        otherPlayer,
        eventActive: false) ||
      simulation.CanWipeProjectileTurret(ordinary.ReplicationId, owner, eventActive: false) ||
      simulation.CanWipeProjectileTurret(0, owner, eventActive: false) ||
      !beforeQueries.SequenceEqual(
        simulation.CreateProjectileReplicationSnapshots().Where(snapshot => snapshot.IsActive)))
  {
    throw new InvalidOperationException(
      "Authoritative WipableTurret query did not preserve event, owner, sentry, or identity gates.");
  }

  Console.WriteLine(
    "PASS: authoritative WipableTurret query is read-only and fail-closed for active projectiles");
}

static void VerifyType3And6SourceBackedDamageClasses()
{
  ProjectileDefinitionRegistry definitions = ProjectileDefinitionRegistry.CreateDefault();
  if (!definitions.TryGet(3, out ProjectileDefinition type3) ||
      type3.LegacyAiStyle != 2 || type3.DamageClass != ProjectileDamageClass.Ranged ||
      !type3.Friendly || type3.Hostile || type3.MaximumPenetration != 4)
  {
    throw new InvalidOperationException("Type 3 damage class does not match the source-backed contract.");
  }

  if (!definitions.TryGet(6, out ProjectileDefinition type6) ||
      type6.LegacyAiStyle != 3 || type6.DamageClass != ProjectileDamageClass.Melee ||
      !type6.Friendly || type6.Hostile || type6.MaximumPenetration != -1)
  {
    throw new InvalidOperationException("Type 6 damage class does not match the source-backed contract.");
  }
}

static void VerifyType300SourceBackedDamageClass()
{
  ProjectileDefinitionRegistry definitions = ProjectileDefinitionRegistry.CreateDefault();
  if (!definitions.TryGet(300, out ProjectileDefinition definition) ||
      definition.LegacyAiStyle != 2 || definition.DamageClass != ProjectileDamageClass.Magic ||
      definition.Collider.Width != 2.375f || definition.Collider.Height != 2.375f ||
      !definition.Hostile || definition.Friendly || definition.MaximumPenetration != -1 ||
      definition.CollidesWithTiles || !definition.IgnoreWater)
  {
    throw new InvalidOperationException("Type 300 metadata does not match the source-backed contract.");
  }
}

static void VerifyColdDamageMetadata()
{
  ProjectileDefinitionRegistry definitions = ProjectileDefinitionRegistry.CreateDefault();
  if (!definitions.TryGet(520, out ProjectileDefinition type520) || !type520.IsColdDamage ||
      !definitions.TryGet(979, out ProjectileDefinition type979) || !type979.IsColdDamage ||
      !definitions.TryGet(3, out ProjectileDefinition nonColdType) || nonColdType.IsColdDamage ||
      definitions.TryGet(599, out ProjectileDefinition nonCold) && nonCold.IsColdDamage)
  {
    throw new InvalidOperationException("Cold-damage metadata does not match the source-backed contract.");
  }
}

static void VerifyProjectileDefinitionMetadataProjection()
{
  using World world = World.Create();
  ProjectileDefinitionRegistry definitions = ProjectileDefinitionRegistry.CreateDefault();
  ProjectileDefinition definition = definitions.Definitions[520] with
  {
    MaximumPenetration = 7,
    ArmorPenetration = 7,
    NoEnchantments = true,
    NoEnchantmentVisuals = true,
    OriginatedFromActivableTile = true,
    NoDropItem = true,
    UsesLocalNpcImmunity = true,
    LocalNpcHitCooldownTicks = 3,
    IsNetworkImportant = true,
    Scale = 1.5f,
    OwnerHitCheck = true,
    OwnerHitCheckDistance = 300.0f,
    IsSentry = true,
    IsMinion = true,
    MinionSlots = 1.0f,
    MinionPosition = 2,
    IsTrap = true,
    IsBobber = true,
    IsCounterweight = true,
    DecidesManualFallThrough = true,
    ManualDirectionChange = true,
    UsesOwnerMeleeHitCooldown = true,
    HostileDamageScaling = ProjectileHostileDamageScaling.Lightning,
    ReflectsFromTiles = true,
    CorrectSlopeCollision = false,
    MaximumBounces = 2,
    BounceVelocityMultiplier = 0.5f,
    MinimumBounceSpeed = 1.25f,
    LiquidPolicy = ProjectileLiquidPolicy.Destroy,
    ChildSpawn = new ProjectileChildSpawn(522, 3, 5, 7.0f, 10.0f, 0.8f, 0.8f),
    OnHitStatusEffect = new ProjectileOnHitStatusEffect(24, 10, 20, 1, 2),
    OnDespawnStatusEffect = new ProjectileOnDespawnStatusEffect(44, 30, 4.0f),
    OnDespawnAreaDamage = new ProjectileOnDespawnAreaDamage(6.0f, 8.0f)
  };
  Entity projectile = new ProjectileSpawnSystem().Spawn(
    world,
    new SpawnProjectileCommand(new PlayerHandle(1), 0.0f, 0.0f, 1, 10, 20, ProjectileType: 520),
    definition,
    1);
  world.Set(projectile, new ProjectileSoundDelayComponent(-1));
  ProjectileReplicationSnapshot snapshot = new ProjectileReplicationSystem().Project(
    projectile,
    world,
    1,
    1,
    default);
  if (snapshot.DamageClass != ProjectileDamageClass.Ranged || !snapshot.IsColdDamage ||
      snapshot.LegacyAiStyle != 2 || snapshot.IsArrow || snapshot.ArmorPenetration != 7 ||
      snapshot.MaximumPenetration != 7 ||
      !snapshot.NoEnchantments || !snapshot.NoEnchantmentVisuals ||
      !snapshot.OriginatedFromActivableTile || !snapshot.NoDropItem ||
      !snapshot.UsesLocalNpcImmunity || snapshot.LocalNpcHitCooldownTicks != 3 ||
      !snapshot.IsNetworkImportant || snapshot.Scale != 1.5f ||
      !snapshot.OwnerHitCheck || snapshot.OwnerHitCheckDistance != 300.0f ||
      !snapshot.IsSentry || !world.Has<ProjectileSentryComponent>(projectile) ||
      !snapshot.IsMinion || snapshot.MinionSlots != 1.0f ||
      snapshot.MinionPosition != 2 || !world.Has<ProjectileMinionComponent>(projectile) ||
      !snapshot.IsTrap || !world.Has<ProjectileTrapComponent>(projectile) ||
      !snapshot.IsBobber || !world.Has<ProjectileBobberComponent>(projectile) ||
      !snapshot.IsCounterweight || !world.Has<ProjectileCounterweightComponent>(projectile) ||
      !snapshot.DecidesManualFallThrough || snapshot.ShouldFallThrough ||
      !world.Has<ProjectileFallThroughComponent>(projectile) ||
      !snapshot.ManualDirectionChange || snapshot.Direction != 1 ||
      !snapshot.UsesOwnerMeleeHitCooldown ||
      snapshot.HostileDamageScaling != ProjectileHostileDamageScaling.Lightning ||
      !snapshot.CollidesWithTiles || snapshot.IgnoreWater ||
      !snapshot.ReflectsFromTiles || snapshot.CorrectSlopeCollision ||
      snapshot.MaximumBounces != 2 || snapshot.BounceVelocityMultiplier != 0.5f ||
      snapshot.MinimumBounceSpeed != 1.25f ||
      snapshot.LiquidPolicy != ProjectileLiquidPolicy.Destroy ||
      snapshot.ChildSpawn != definition.ChildSpawn ||
      snapshot.OnHitStatusEffect != definition.OnHitStatusEffect ||
      snapshot.OnDespawnStatusEffect != definition.OnDespawnStatusEffect ||
      snapshot.OnDespawnAreaDamage != definition.OnDespawnAreaDamage ||
      snapshot.BehaviorId != definition.BehaviorId || !snapshot.Friendly || snapshot.Hostile ||
      snapshot.PlayerDamagePolicy != definition.PlayerDamagePolicy || snapshot.SoundDelay != -1)
  {
    throw new InvalidOperationException("Projectile metadata was not projected into the live snapshot.");
  }

  ColliderComponent collider = world.Get<ColliderComponent>(projectile);
  if (collider.Width != definition.Collider.Width * definition.Scale ||
      collider.Height != definition.Collider.Height * definition.Scale)
  {
    throw new InvalidOperationException("Projectile scale was not applied to the authoritative collider.");
  }

  ProjectileDefinition arrowDefinition = definitions.Definitions[5];
  Entity arrowProjectile = new ProjectileSpawnSystem().Spawn(
    world,
    new SpawnProjectileCommand(new PlayerHandle(1), 0.0f, 0.0f, 1, 10, 20, ProjectileType: 5),
    arrowDefinition,
    2);
  ProjectileReplicationSnapshot arrowSnapshot = new ProjectileReplicationSystem().Project(
    arrowProjectile,
    world,
    2,
    1,
    default);
  if (!arrowSnapshot.IsArrow)
  {
    throw new InvalidOperationException("Arrow metadata was not projected into the live snapshot.");
  }

  ref VelocityComponent arrowVelocity = ref world.Get<VelocityComponent>(arrowProjectile);
  arrowVelocity.X = -4.0f;
  ref ProjectileDirectionComponent arrowDirection =
    ref world.Get<ProjectileDirectionComponent>(arrowProjectile);
  ProjectileDirectionSystem.Update(
    ref arrowDirection,
    arrowVelocity,
    world.Get<ProjectileDefinitionComponent>(arrowProjectile),
    world.Get<ProjectileBehaviorComponent>(arrowProjectile));
  ProjectileReplicationSnapshot directionSnapshot = new ProjectileReplicationSystem().Project(
    arrowProjectile,
    world,
    2,
    2,
    default);
  if (directionSnapshot.Direction != -1 || directionSnapshot.ManualDirectionChange)
  {
    throw new InvalidOperationException("Automatic projectile direction was not projected.");
  }

  ref VelocityComponent manualVelocity = ref world.Get<VelocityComponent>(projectile);
  manualVelocity.X = -4.0f;
  ref ProjectileDirectionComponent manualDirection =
    ref world.Get<ProjectileDirectionComponent>(projectile);
  ProjectileDirectionSystem.Update(
    ref manualDirection,
    manualVelocity,
    world.Get<ProjectileDefinitionComponent>(projectile),
    world.Get<ProjectileBehaviorComponent>(projectile));
  ProjectileReplicationSnapshot manualDirectionSnapshot = new ProjectileReplicationSystem().Project(
    projectile,
    world,
    1,
    3,
    default);
  if (manualDirectionSnapshot.Direction != 1)
  {
    throw new InvalidOperationException("Manual projectile direction was overwritten by velocity.");
  }

  WorldGrid platformWorld = new(WorldGrid.SectionWidth, WorldGrid.SectionHeight);
  _ = platformWorld.TrySetTile(5, 5, new WorldTile(IsActive: true, Type: 14));
  ProjectileCollisionSystem collision = new();
  if (!collision.HitsSolidTile(
        platformWorld,
        new LocationComponent(5.0f, 5.0f),
        new ColliderComponent(1.0f, 1.0f)) ||
      collision.HitsSolidTile(
        platformWorld,
        new LocationComponent(5.0f, 5.0f),
        new ColliderComponent(1.0f, 1.0f),
        fallThrough: true))
  {
    throw new InvalidOperationException(
      "Manual projectile fall-through did not distinguish platform collision.");
  }

  ref ProjectileFallThroughComponent fallThrough =
    ref world.Get<ProjectileFallThroughComponent>(projectile);
  fallThrough.ShouldFallThrough = true;
  ProjectileReplicationSnapshot fallThroughSnapshot = new ProjectileReplicationSystem().Project(
    projectile,
    world,
    1,
    2,
    default);
  if (!fallThroughSnapshot.ShouldFallThrough)
  {
    throw new InvalidOperationException("Projectile fall-through state was not projected.");
  }
}

static void VerifyNpcProjectileDefinitionMetadataProjection()
{
  using DomeSimulation simulation = new(new WorldGrid(400, 300));
  NpcHandle owner = simulation.CreateNpc(new SimulationVector(2.0f, 2.0f));
  ProjectileDefinition definition = ProjectileDefinitionRegistry.CreateDefault().Definitions[300] with
  {
    MaximumPenetration = -1,
    HostileDamageScaling = ProjectileHostileDamageScaling.Lightning,
    ReflectsFromTiles = true,
    CorrectSlopeCollision = false,
    MaximumBounces = 2,
    BounceVelocityMultiplier = 0.5f,
    MinimumBounceSpeed = 1.25f,
    LiquidPolicy = ProjectileLiquidPolicy.Pass,
    ChildSpawn = new ProjectileChildSpawn(521, 1, 1, 2.0f, 2.0f, 1.0f, 1.0f),
    OnHitStatusEffect = new ProjectileOnHitStatusEffect(24, 5, 5, 1, 1),
    OnDespawnStatusEffect = new ProjectileOnDespawnStatusEffect(44, 15, 2.0f),
    OnDespawnAreaDamage = new ProjectileOnDespawnAreaDamage(0.0f, 0.0f)
  };
  NpcProjectileReplicationSnapshot live = simulation.CreateNpcProjectile(
    new NpcProjectileSpawnRequest(
      owner,
      Sequence: 0,
      definition,
      Damage: 10,
      Position: new SimulationVector(4.0f, 2.0f),
      Velocity: new SimulationVector(1.0f, 0.0f),
      BannerIdToRespondTo: 9));
  if (live.DamageClass != ProjectileDamageClass.Magic || live.IsColdDamage ||
      live.LegacyAiStyle != 2 || live.MaximumPenetration != -1 || !live.IsActive ||
      live.Direction != 1 || live.ManualDirectionChange || live.BannerIdToRespondTo != 9 ||
      live.HostileDamageScaling != ProjectileHostileDamageScaling.Lightning ||
      live.CollidesWithTiles || !live.IgnoreWater || !live.ReflectsFromTiles ||
      live.CorrectSlopeCollision || live.MaximumBounces != 2 ||
      live.BounceVelocityMultiplier != 0.5f || live.MinimumBounceSpeed != 1.25f ||
      live.LiquidPolicy != ProjectileLiquidPolicy.Pass || live.ChildSpawn != definition.ChildSpawn ||
      live.OnHitStatusEffect != definition.OnHitStatusEffect ||
      live.OnDespawnStatusEffect != definition.OnDespawnStatusEffect ||
      live.OnDespawnAreaDamage != definition.OnDespawnAreaDamage ||
      live.BehaviorId != definition.BehaviorId || live.Friendly || !live.Hostile ||
      live.PlayerDamagePolicy != definition.PlayerDamagePolicy || live.TileCollisionEnabled)
  {
    throw new InvalidOperationException("NPC projectile metadata was not projected into the live snapshot.");
  }

  Entity projectile = default;
  simulation.World.Query(
    new QueryDescription().WithAll<NpcProjectileOwnerComponent, NpcProjectileNetworkIdentityComponent>(),
    (Entity entity) => projectile = entity);
  if (projectile == default)
  {
    throw new InvalidOperationException("NPC projectile entity was not materialized for tombstone projection.");
  }

  NpcProjectileReplicationSnapshot tombstone = new NpcProjectileReplicationSystem().ProjectTombstone(
    projectile,
    simulation.World,
    live.ReplicationId,
    revision: 2,
    currentTick: 1,
    default,
    ProjectileTombstoneReason.Administrative);
  if (tombstone.DamageClass != ProjectileDamageClass.Magic || tombstone.IsColdDamage ||
      tombstone.LegacyAiStyle != 2 || tombstone.MaximumPenetration != -1 || tombstone.IsActive ||
      tombstone.BannerIdToRespondTo != 9 || tombstone.TileCollisionEnabled)
  {
    throw new InvalidOperationException("NPC projectile metadata was not projected into the tombstone snapshot.");
  }
}

static void VerifyProjectileDamageClassEventPropagation()
{
  using World world = World.Create();
  Entity projectile = world.Create(
    new ProjectileDefinitionComponent(
      520,
      3,
      10,
      30,
      new ColliderComponent(1.0f, 1.0f),
      true,
      false,
      DamageClass: ProjectileDamageClass.Ranged,
      IsColdDamage: true,
      ArmorPenetration: 7),
    new ProjectilePenetrationComponent(1));
  Entity target = world.Create(new NpcTagComponent());
  DamageRequestedEvent request = new(
    projectile,
    target,
    10,
    1,
    1,
    ProjectileDamageClass.Ranged,
      IsColdDamage: true,
      ArmorPenetration: 7,
      BonusCritChance: 12,
      BonusTagDamage: 9,
      TagEffectType: 44);
  if (request.DamageClass != ProjectileDamageClass.Ranged || !request.IsColdDamage ||
      request.ArmorPenetration != 7 || request.BonusCritChance != 12 ||
      request.BonusTagDamage != 9 || request.TagEffectType != 44)
  {
    throw new InvalidOperationException("Projectile damage class was not carried by the damage event.");
  }

  DamagePlayerCommand playerCommand = new(
    new PlayerHandle(2),
    request.Amount,
    request.DamageClass,
    request.IsColdDamage);
  if (playerCommand.DamageClass != ProjectileDamageClass.Ranged || !playerCommand.IsColdDamage)
  {
    throw new InvalidOperationException("Projectile damage metadata was lost at command submission.");
  }
}

static void VerifyProjectileHostileDamageScaling()
{
  ProjectileHostileDamageScalingSystem system = new();
  ProjectileDefinitionComponent hostile = new(
    1002,
    1,
    10,
    30,
    new ColliderComponent(1.0f, 1.0f),
    Friendly: false,
    Hostile: true,
    PlayerDamagePolicy: PlayerDamagePolicy.HostileNonPvp);
  if (system.ScalePlayerDamage(10, hostile, new WorldRuleState()) != 10 ||
      system.ScalePlayerDamage(10, hostile, new WorldRuleState(isExpertMode: true)) != 20 ||
      system.ScalePlayerDamage(10, hostile,
        new WorldRuleState(isExpertMode: true, isMasterMode: true)) != 30 ||
      system.ScalePlayerDamage(10, hostile,
        new WorldRuleState(gameMode: WorldGameMode.Journey)) != 5)
  {
    throw new InvalidOperationException(
      "Hostile projectile scaling did not match the source-backed difficulty curve.");
  }

  ProjectileDefinitionComponent lightning = hostile with
  {
    HostileDamageScaling = ProjectileHostileDamageScaling.Lightning
  };
  if (system.ScalePlayerDamage(100, lightning, new WorldRuleState()) != 8 ||
      system.ScalePlayerDamage(100, lightning,
        new WorldRuleState(isExpertMode: true, isMasterMode: true)) != 24 ||
      system.ScalePlayerDamage(10, lightning,
        new WorldRuleState(gameMode: WorldGameMode.Journey)) != 1)
  {
    throw new InvalidOperationException(
      "Lightning hostile projectile scaling did not match the source-backed curve.");
  }
}

static void VerifyProjectileType1091HostileDamageScaling()
{
  if (LegacyProjectileHostileDamageScalingRegistry.LightningScalingTypes.Count != 1 ||
      !LegacyProjectileHostileDamageScalingRegistry.UsesLightningScaling(1091) ||
      LegacyProjectileHostileDamageScalingRegistry.UsesLightningScaling(1090) ||
      LegacyProjectileHostileDamageScalingRegistry.UsesLightningScaling(-1))
  {
    throw new InvalidOperationException(
      "The source-backed type-1091 hostile scaling registry admitted an incorrect type.");
  }

  ProjectileDefinition sourceDefinition = new(
    ProjectileType: 1091,
    BehaviorId: 1,
    Damage: 0,
    LifetimeTicks: 60,
    Collider: new ColliderComponent(1.0f, 1.0f),
    Friendly: true,
    Hostile: true,
    MaximumPenetration: -1,
    PlayerDamagePolicy: PlayerDamagePolicy.HostileNonPvp);
  ProjectileDefinitionRegistry registry = new([sourceDefinition]);
  ProjectileDefinition normalized = registry.Definitions[1091];
  if (normalized.HostileDamageScaling != ProjectileHostileDamageScaling.Lightning)
  {
    throw new InvalidOperationException(
      "ProjectileDefinitionRegistry did not normalize type-1091 to Lightning scaling.");
  }

  ProjectileDefinition ordinaryDefinition = sourceDefinition with { ProjectileType = 1090 };
  ProjectileDefinition ordinary = new ProjectileDefinitionRegistry([ordinaryDefinition])
    .Definitions[1090];
  if (ordinary.HostileDamageScaling != ProjectileHostileDamageScaling.Default)
  {
    throw new InvalidOperationException(
      "Hostile damage scaling registry leaked the type-1091 capability to another type.");
  }

  ProjectileDefinitionComponent component = new(
    normalized.ProjectileType,
    normalized.BehaviorId,
    normalized.Damage,
    normalized.LifetimeTicks,
    normalized.Collider,
    normalized.Friendly,
    normalized.Hostile,
    normalized.MaximumPenetration,
    PlayerDamagePolicy: normalized.PlayerDamagePolicy,
    HostileDamageScaling: normalized.HostileDamageScaling);
  ProjectileHostileDamageScalingSystem scaling = new();
  if (scaling.ScalePlayerDamage(100, component, new WorldRuleState()) != 8 ||
      scaling.ScalePlayerDamage(
        100,
        component,
        new WorldRuleState(isExpertMode: true, isMasterMode: true)) != 24 ||
      scaling.ScalePlayerDamage(100, component,
        new WorldRuleState(gameMode: WorldGameMode.Journey)) != 4)
  {
    throw new InvalidOperationException(
      "Type-1091 Lightning hostile damage scaling did not preserve the source curve.");
  }

  Console.WriteLine(
    "PASS: source-backed type-1091 hostile damage scaling normalizes and applies Lightning curve");
}

static void VerifyProjectileType1091DamageEligibility()
{
  ProjectileDefinitionComponent lightning = new(
    1091,
    1,
    10,
    60,
    new ColliderComponent(1.0f, 1.0f),
    Friendly: true,
    Hostile: true,
    PlayerDamagePolicy: PlayerDamagePolicy.HostileNonPvp);
  ProjectileBehaviorComponent beforeLightning = new(
    1,
    new ProjectileBehaviorState(0.0f, 0.0f, 0, 0, LocalAi0: 0.0f));
  ProjectileBehaviorComponent activeLightning = new(
    1,
    new ProjectileBehaviorState(0.0f, 0.0f, 0, 0, LocalAi0: 1.0f));
  ProjectileBehaviorComponent malformedLightning = new(
    1,
    new ProjectileBehaviorState(0.0f, 0.0f, 0, 0, LocalAi0: float.NaN));

  if (ProjectileDamageEligibilityPolicy.CanDealDamage(lightning, beforeLightning) ||
      !ProjectileDamageEligibilityPolicy.CanDealDamage(lightning, activeLightning) ||
      ProjectileDamageEligibilityPolicy.CanDealDamage(lightning, malformedLightning))
  {
    throw new InvalidOperationException(
      "Type-1091 damage eligibility did not preserve the source localAI[0] gate.");
  }

  ProjectileDefinitionComponent ordinary = lightning with { ProjectileType = 1090 };
  if (!ProjectileDamageEligibilityPolicy.CanDealDamage(ordinary, beforeLightning))
  {
    throw new InvalidOperationException(
      "Type-1091 damage eligibility leaked into an ordinary projectile type.");
  }

  using World world = World.Create();
  Entity coldProjectile = world.Create(
    lightning,
    beforeLightning,
    new ProjectilePenetrationComponent(-1),
    new ProjectileDamageComponent(10));
  Entity activeProjectile = world.Create(
    lightning,
    activeLightning,
    new ProjectilePenetrationComponent(-1),
    new ProjectileDamageComponent(10));
  Entity missingBehaviorProjectile = world.Create(
    lightning,
    new ProjectilePenetrationComponent(-1),
    new ProjectileDamageComponent(10));
  Entity target = world.Create(new NpcTagComponent());
  ProjectileDamageSystem system = new();
  HitImmunityComponent coldImmunity = new();
  HitImmunityComponent missingBehaviorImmunity = new();
  if (system.Resolve(
        world,
        [new DamageRequestedEvent(coldProjectile, target, 10, 1, 1)],
        coldImmunity).Count != 0 ||
      system.Resolve(
        world,
        [new DamageRequestedEvent(missingBehaviorProjectile, target, 10, 3, 1)],
        missingBehaviorImmunity).Count != 0 ||
      system.Resolve(
        world,
        [new DamageRequestedEvent(activeProjectile, target, 10, 2, 1)],
        new HitImmunityComponent()).Count != 1)
  {
    throw new InvalidOperationException(
      "Projectile damage resolution ignored the type-1091 localAI[0] eligibility gate.");
  }

  Console.WriteLine(
    "PASS: source-backed type-1091 localAI[0] damage eligibility gates resolution");
}

static void VerifyProjectileStopsDamageAfterPenetration()
{
  using World world = World.Create();
  Entity projectile = world.Create(
    new ProjectileDefinitionComponent(
      1001,
      1,
      10,
      30,
      new ColliderComponent(1, 1),
      true,
      false,
      StopsDealingDamageAfterPenetrateHits: true),
    new ProjectilePenetrationComponent(1),
    new ProjectileDamageComponent(10));
  Entity target = world.Create(new NpcTagComponent());
  IReadOnlyList<DamageRequestedEvent> accepted = new ProjectileDamageSystem().Resolve(
    world,
    [new DamageRequestedEvent(projectile, target, 10, 1, 1)],
    new HitImmunityComponent());
  ProjectilePenetrationComponent penetration = world.Get<ProjectilePenetrationComponent>(projectile);
  ProjectileDamageComponent damage = world.Get<ProjectileDamageComponent>(projectile);
  if (accepted.Count != 1 || penetration.RemainingPenetration != -1 || damage.Amount != 0)
  {
    throw new InvalidOperationException(
      "Projectile did not stop dealing damage after its configured penetration hit.");
  }
}

static void VerifyStaticNpcImmunitySingleHitPolicy()
{
  using World world = World.Create();
  Entity target = world.Create(new NpcTagComponent());
  Entity noCooldownProjectile = world.Create(
    new ProjectileDefinitionComponent(167, 34, 10, 45, new ColliderComponent(1, 1), true, false,
      UsesStaticNpcImmunity: true, StaticNpcHitCooldownTicks: 1,
      AppliesImmunityTimeOnSingleHits: false),
    new ProjectilePenetrationComponent(1));
  Entity appliesCooldownProjectile = world.Create(
    new ProjectileDefinitionComponent(167, 34, 10, 45, new ColliderComponent(1, 1), true, false,
      UsesStaticNpcImmunity: true, StaticNpcHitCooldownTicks: 1,
      AppliesImmunityTimeOnSingleHits: true),
    new ProjectilePenetrationComponent(1));
  ProjectileDamageSystem system = new();
  HitImmunityComponent immunity = new();
  _ = system.Resolve(world,
    [new DamageRequestedEvent(noCooldownProjectile, target, 10, 1, 7)], immunity);
  if (immunity.IsStaticNpcImmune(167, 7))
  {
    throw new InvalidOperationException(
      "Single-hit static immunity was applied without the source-backed opt-in.");
  }

  _ = system.Resolve(world,
    [new DamageRequestedEvent(appliesCooldownProjectile, target, 10, 2, 7)], immunity);
  if (!immunity.IsStaticNpcImmune(167, 7))
  {
    throw new InvalidOperationException(
      "Single-hit static immunity did not apply with the source-backed opt-in.");
  }
}

static void VerifyHostileProjectileEligibility()
{
  ProjectileDefinitionRegistry hostileDefinitions = new([
    new ProjectileDefinition(
      ProjectileType: 7,
      BehaviorId: 1,
      Damage: 10,
      LifetimeTicks: 20,
      Collider: new ColliderComponent(0.5f, 0.5f),
      Friendly: false,
      Hostile: true,
      MaximumPenetration: 1,
      PlayerDamagePolicy: PlayerDamagePolicy.HostileNonPvp)]);
  using DomeSimulation simulation = new(new WorldGrid(400, 300), hostileDefinitions);
  PlayerHandle owner = simulation.CreatePlayer(new SimulationVector(0.0f, 0.0f));
  PlayerHandle target = simulation.CreatePlayer(new SimulationVector(10.0f, 0.0f));
  simulation.QueueProjectileSpawn(new SpawnProjectileCommand(
    owner,
    10.0f,
    0.0f,
    1,
    10,
    20,
    ProjectileType: 7,
    ProjectileSpeed: 0.0f));
  simulation.Tick(new SimulationInputBatch());
  simulation.Tick(new SimulationInputBatch());
  if (simulation.CreatePlayerStateSnapshot(target).Health != 90)
  {
    throw new InvalidOperationException(
      "A hostile projectile did not apply authoritative damage to another player.");
  }

  if (simulation.CreatePlayerStateSnapshot(owner).Health != 100)
  {
    throw new InvalidOperationException("A hostile projectile damaged its own owner.");
  }
}

static void VerifyProjectilePenetrationBoundary()
{
  try
  {
    _ = new ProjectilePenetrationComponent(0);
    throw new InvalidOperationException("Projectile penetration component accepted zero capacity.");
  }
  catch (ArgumentOutOfRangeException)
  {
  }

  try
  {
    _ = new ProjectilePenetrationComponent(-2);
    throw new InvalidOperationException(
      "Projectile penetration component accepted an invalid negative capacity.");
  }
  catch (ArgumentOutOfRangeException)
  {
  }

  if (new ProjectilePenetrationComponent(-1).RemainingPenetration != -1)
  {
    throw new InvalidOperationException("Projectile infinite penetration sentinel was not preserved.");
  }

  using DomeSimulation simulation = new(new WorldGrid(400, 300));
  PlayerHandle player = simulation.CreatePlayer(new SimulationVector(10.0f, 0.0f));
  _ = simulation.CreateNpc(new SimulationVector(14.0f, 0.0f));
  simulation.QueueProjectileSpawn(new SpawnProjectileCommand(
    player,
    10.0f,
    0.0f,
    1,
    10,
    20,
    MaximumPenetration: 1));

  simulation.Tick(new SimulationInputBatch());
  simulation.Tick(new SimulationInputBatch());
  ProjectileReplicationSnapshot projectile = simulation
    .CreateProjectileReplicationSnapshots()
    .Single();
  if (simulation.CreateNpcReplicationSnapshots().Single().Health != 90 || projectile.IsActive)
  {
    throw new InvalidOperationException(
      "A one-penetration projectile did not despawn after its first authoritative hit.");
  }
}

static void VerifyForgedProjectilePenetrationRejected()
{
  using World world = World.Create();
  Entity projectile = world.Create(new ProjectilePenetrationComponent(-1)
  {
    RemainingPenetration = int.MinValue
  });
  Entity target = world.Create();
  IReadOnlyList<DamageRequestedEvent> accepted = new ProjectileDamageSystem().Resolve(
    world,
    [new DamageRequestedEvent(projectile, target, 10, 1, 1)],
    new HitImmunityComponent());
  if (accepted.Count != 0)
  {
    throw new InvalidOperationException(
      "Projectile damage accepted a forged penetration value below the infinite sentinel.");
  }
}

static void VerifyStableReplicationAndCombatLifecycle()
{
  using DomeSimulation simulation = new(new WorldGrid(400, 300));
  PlayerHandle player = simulation.CreatePlayer(new SimulationVector(10.0f, 0.0f));
  NpcHandle npc = simulation.CreateNpc(new SimulationVector(16.0f, 0.0f));

  simulation.Tick(new SimulationInputBatch(new PlayerInput(
    player,
    MoveLeft: false,
    MoveRight: false,
    Jump: false,
    Fire: true)));
  ProjectileReplicationSnapshot projectile = simulation.CreateProjectileReplicationSnapshots().Single();
  NpcReplicationSnapshot initialNpc = simulation.CreateNpcReplicationSnapshots().Single();
  if (projectile.ReplicationId <= 0 || projectile.ProjectileType != 1 ||
      projectile.Owner != player || projectile.Revision <= 0 ||
      initialNpc.ReplicationId <= 0 || initialNpc.NpcType != 1 || !initialNpc.IsActive)
  {
    throw new InvalidOperationException("Combat entities did not receive stable authority records.");
  }

  simulation.Tick(new SimulationInputBatch(new PlayerInput(
    player,
    MoveLeft: false,
    MoveRight: false,
    Jump: false,
    Fire: true)));
  NpcReplicationSnapshot damagedNpc = simulation.CreateNpcReplicationSnapshots().Single();
  ProjectileReplicationSnapshot despawnedProjectile = simulation
    .CreateProjectileReplicationSnapshots().Single();
  if (damagedNpc.ReplicationId != initialNpc.ReplicationId ||
      damagedNpc.Health != 90 || damagedNpc.Revision <= initialNpc.Revision ||
      despawnedProjectile.ReplicationId != projectile.ReplicationId ||
      despawnedProjectile.IsActive || despawnedProjectile.Revision <= projectile.Revision)
  {
    throw new InvalidOperationException("Projectile impact did not commit a stable NPC damage revision.");
  }

  if (simulation.CreateSnapshot().Projectiles.Count != 0)
  {
    throw new InvalidOperationException("A fire cooldown did not prevent immediate duplicate projectiles.");
  }
}

static void VerifyProjectileSweepPreventsNpcTunneling()
{
  using DomeSimulation simulation = new(new WorldGrid(400, 300));
  PlayerHandle player = simulation.CreatePlayer(new SimulationVector(10.0f, 0.0f));
  NpcHandle npc = simulation.CreateNpc(new SimulationVector(18.0f, 0.0f));

  simulation.Tick(new SimulationInputBatch(new PlayerInput(
    player,
    MoveLeft: false,
    MoveRight: false,
    Jump: false,
    Fire: true)));
  simulation.Tick(new SimulationInputBatch());
  simulation.Tick(new SimulationInputBatch());
  if (simulation.CreateSnapshot().FindNpc(npc).Health != 90)
  {
    throw new InvalidOperationException("A high-speed projectile tunneled through an NPC.");
  }
}

static void VerifyNpcContactDamage()
{
  using DomeSimulation simulation = new(new WorldGrid(400, 300));
  PlayerHandle player = simulation.CreatePlayer(new SimulationVector(10.0f, 0.0f));
  _ = simulation.CreateNpc(new SimulationVector(10.0f, 0.0f));

  simulation.Tick(new SimulationInputBatch());
  PlayerStateSnapshot state = simulation.CreatePlayerStateSnapshot(player);
  if (state.Health != 75 || simulation.CreatePlayerDamagedEvents().Count != 1)
  {
    throw new InvalidOperationException(
      "An overlapping hostile NPC did not produce one authoritative contact-damage event.");
  }

  NpcContactEffectSystem contactSystem = new();
  PlayerContactCandidate playerCandidate = new(
    player,
    new SimulationVector(10.0f, 0.0f),
    new ColliderComponent(1.0f, 2.0f),
    IsActive: true,
    CooldownTicks: 0);
  NpcContactCandidate townCandidate = new(
    new NpcHandle(1),
    new SimulationVector(10.0f, 0.0f),
    new ColliderComponent(1.0f, 2.0f),
    NpcFaction.Town,
    IsActive: true);
  NpcContactCandidate neutralCandidate = townCandidate with { Faction = NpcFaction.Neutral };
  if (contactSystem.ProduceDamageCommands(
        [townCandidate, neutralCandidate],
        [playerCandidate],
        25).Count != 0)
  {
    throw new InvalidOperationException(
      "Non-hostile NPC factions produced player contact-damage commands.");
  }
}

static void VerifyPlayerDamageDifficultyModes()
{
  int normalHealth = ResolveEquippedPlayerDamage(new WorldRuleState());
  int expertHealth = ResolveEquippedPlayerDamage(new WorldRuleState(isExpertMode: true));
  int masterHealth = ResolveEquippedPlayerDamage(
    new WorldRuleState(isExpertMode: true, isMasterMode: true));
  if (normalHealth != 83 || expertHealth != 84 || masterHealth != 85)
  {
    throw new InvalidOperationException(
      "Player damage commit did not use the world difficulty mitigation contract.");
  }
}

static int ResolveEquippedPlayerDamage(WorldRuleState worldRules)
{
  WorldMetadata metadata = new(
    "damage-mode",
    new WorldSeed(91),
    400,
    300,
    spawnX: 200,
    spawnY: 75);
  using DomeSimulation source = new(new WorldGrid(400, 300));
  DomeSimulationSnapshot snapshot = source.CreatePersistenceSnapshot(metadata, worldRules);
  using DomeSimulation simulation = new(snapshot);
  PlayerHandle player = simulation.CreatePlayer(new SimulationVector(10.0f, 0.0f));
  simulation.GetInventory(player).SetSlot(0, new ItemStack(4, 1));
  simulation.QueueEquipItem(player, 0, isVanity: false);
  simulation.Tick(new SimulationInputBatch());
  simulation.QueuePlayerDamage(player, 20);
  simulation.Tick(new SimulationInputBatch());
  return simulation.CreatePlayerStateSnapshot(player).Health;
}

static void VerifyProjectileTileCollision()
{
  WorldGrid world = new(400, 300);
  _ = world.TrySetTile(14, 0, new WorldTile(IsActive: true, Type: 1));
  ProjectileDefinitionRegistry definitions = new(
    ProjectileDefinitionRegistry.CreateDefault().OrderedDefinitions.Select(definition =>
      definition.ProjectileType == 5 ? definition with { IgnoreWater = false } : definition));
  using DomeSimulation simulation = new(world, definitions);
  PlayerHandle player = simulation.CreatePlayer(new SimulationVector(10.0f, 0.0f));
  _ = simulation.CreateNpc(new SimulationVector(20.0f, 0.0f));

  simulation.Tick(new SimulationInputBatch(new PlayerInput(
    player,
    MoveLeft: false,
    MoveRight: false,
    Jump: false,
    Fire: true)));
  simulation.Tick(new SimulationInputBatch());
  ProjectileReplicationSnapshot despawnedProjectile = simulation
    .CreateProjectileReplicationSnapshots().Single();
  if (despawnedProjectile.IsActive ||
      simulation.CreateNpcReplicationSnapshots().Single().Health != 100)
  {
    throw new InvalidOperationException("Projectile did not stop at the authoritative tile before NPC hit.");
  }
}

static void VerifyProjectileBounce()
{
  try
  {
    _ = new ProjectileBounceComponent(-1);
    throw new InvalidOperationException(
      "Projectile bounce component accepted a negative remaining-bounce count.");
  }
  catch (ArgumentOutOfRangeException)
  {
  }

  WorldGrid world = new(400, 300);
  _ = world.TrySetTile(14, 0, new WorldTile(IsActive: true, Type: 1));
  using DomeSimulation simulation = new(world);
  PlayerHandle player = simulation.CreatePlayer(new SimulationVector(10.0f, 0.0f));
  simulation.QueueProjectileSpawn(new SpawnProjectileCommand(
    player,
    10.0f,
    0.0f,
    1,
    10,
    20,
    ProjectileType: 4));
  simulation.Tick(new SimulationInputBatch());
  simulation.Tick(new SimulationInputBatch());
  simulation.Tick(new SimulationInputBatch());
  ProjectileReplicationSnapshot bounced = simulation
    .CreateProjectileReplicationSnapshots().Single();
  if (!bounced.IsActive || bounced.Velocity.X >= 0.0f ||
      bounced.TombstoneReason != ProjectileTombstoneReason.None || !bounced.Reflected)
  {
    throw new InvalidOperationException(
      "A configured projectile did not reflect velocity at a solid tile.");
  }

  simulation.Tick(new SimulationInputBatch());
  ProjectileReplicationSnapshot afterBounce = simulation
    .CreateProjectileReplicationSnapshots().Single();
  if (!afterBounce.IsActive || afterBounce.Position.X >= bounced.Position.X)
  {
    throw new InvalidOperationException(
      "A bounced projectile did not continue from the reflected trajectory.");
  }
}

static void VerifyProjectileLiquidPolicy()
{
  WorldGrid world = new(400, 300);
  _ = world.TrySetLiquid(18, 0, byte.MaxValue, type: 0);
  ProjectileDefinitionRegistry definitions = new(
    ProjectileDefinitionRegistry.CreateDefault().OrderedDefinitions.Select(definition =>
      definition.ProjectileType == 5 ? definition with { IgnoreWater = false } : definition));
  using DomeSimulation simulation = new(world, definitions);
  PlayerHandle player = simulation.CreatePlayer(new SimulationVector(10.0f, 0.0f));
  simulation.QueueProjectileSpawn(new SpawnProjectileCommand(
    player,
    10.0f,
    0.0f,
    1,
    10,
    20,
    ProjectileType: 5));
  simulation.Tick(new SimulationInputBatch());
  simulation.Tick(new SimulationInputBatch());
  ProjectileReplicationSnapshot liquidHit = simulation
    .CreateProjectileReplicationSnapshots().Single();
  if (liquidHit.IsActive || liquidHit.TombstoneReason != ProjectileTombstoneReason.LiquidHit)
  {
    throw new InvalidOperationException(
      "A destroy-on-liquid projectile did not produce a liquid tombstone.");
  }
}

static void VerifyProjectileIgnoreWaterPolicy()
{
  WorldGrid world = new(400, 300);
  _ = world.TrySetLiquid(14, 0, byte.MaxValue, type: 0);
  ProjectileDefinitionRegistry defaults = ProjectileDefinitionRegistry.CreateDefault();
  ProjectileDefinitionRegistry definitions = new(defaults.OrderedDefinitions.Select(definition =>
    definition.ProjectileType == 5 ? definition with { IgnoreWater = true } : definition));
  using DomeSimulation simulation = new(world, definitions);
  PlayerHandle player = simulation.CreatePlayer(new SimulationVector(10.0f, 0.0f));
  simulation.QueueProjectileSpawn(new SpawnProjectileCommand(
    player, 10.0f, 0.0f, 1, 10, 20, ProjectileType: 5));
  simulation.Tick(new SimulationInputBatch());
  simulation.Tick(new SimulationInputBatch());
  ProjectileReplicationSnapshot snapshot = simulation.CreateProjectileReplicationSnapshots().Single();
  if (!snapshot.IsActive || snapshot.TombstoneReason != ProjectileTombstoneReason.None)
  {
    throw new InvalidOperationException("IgnoreWater did not suppress liquid destruction.");
  }
}

static void VerifyLegacyAiStyle190OwnerAnchor()
{
  ProjectileDefinitionRegistry definitions = ProjectileDefinitionRegistry.CreateDefault();
  foreach ((int projectileType, int penetration) in new[]
  {
    (972, 2),
    (982, 3),
    (983, 6),
    (984, 3),
    (997, 3),
    (1043, 2)
  })
  {
    if (!definitions.TryGet(projectileType, out ProjectileDefinition definition) ||
        definition.BehaviorId != LegacyAiStyle190ProjectileBehavior.Id ||
        definition.LegacyAiStyle != 190 || definition.MaximumPenetration != penetration ||
        definition.CollidesWithTiles || !definition.IgnoreWater ||
        definition.DamageClass != ProjectileDamageClass.Melee ||
        !definition.UsesLocalNpcImmunity || definition.LocalNpcHitCooldownTicks != -1 ||
        !definition.OwnerHitCheck || definition.OwnerHitCheckDistance != 300.0f ||
        !definition.UsesOwnerMeleeHitCooldown)
    {
      throw new InvalidOperationException(
        $"Type {projectileType} does not match the source-backed aiStyle 190 contract.");
    }
  }

  using DomeSimulation simulation = new(new WorldGrid(400, 300), definitions);
  PlayerHandle owner = simulation.CreatePlayer(new SimulationVector(10.0f, 10.0f));
  simulation.QueueProjectileSpawn(new SpawnProjectileCommand(
    owner,
    0.0f,
    0.0f,
    1,
    10,
    20,
    ProjectileType: 972,
    ProjectileSpeed: 4.0f,
    Ai0: 1.0f,
    Ai1: 3.0f,
    Ai2: 1.25f));
  simulation.Tick(new SimulationInputBatch());
  simulation.Tick(new SimulationInputBatch());
  ProjectileReplicationSnapshot active = simulation.CreateProjectileReplicationSnapshots().Single();
  QueryDescription ownerQuery = new QueryDescription()
    .WithAll<Terraria.Dome.Simulation.Player.Components.PlayerIdentityComponent>();
  Entity ownerEntity = default;
  simulation.World.Query(
    in ownerQuery,
    (Entity entity,
      ref Terraria.Dome.Simulation.Player.Components.PlayerIdentityComponent identity) =>
    {
      if (identity.Player == owner)
      {
        ownerEntity = entity;
      }
    });
  if (ownerEntity == default)
  {
    throw new InvalidOperationException("The aiStyle 190 owner entity was not materialized.");
  }

  LocationComponent ownerTransform = simulation.World.Get<LocationComponent>(ownerEntity);
  if (!active.IsActive || active.Position.X != ownerTransform.X - 4.0f ||
      active.Position.Y != ownerTransform.Y || active.Ai0 != 1.0f ||
      active.Ai1 != 3.0f || active.Ai2 != 1.25f)
  {
    throw new InvalidOperationException(
      "The aiStyle 190 projectile did not retain its owner-anchor or AI inputs.");
  }

  simulation.Tick(new SimulationInputBatch());
  simulation.Tick(new SimulationInputBatch());
  ProjectileReplicationSnapshot expired = simulation.CreateProjectileReplicationSnapshots().Single();
  if (expired.IsActive || expired.TombstoneReason != ProjectileTombstoneReason.Expired)
  {
    throw new InvalidOperationException("The aiStyle 190 projectile did not expire at Ai1.");
  }
}

static void VerifyLegacyAiStyle17Type43Behavior()
{
  ProjectileDefinitionRegistry definitions = ProjectileDefinitionRegistry.CreateDefault();
  if (!definitions.TryGet(43, out ProjectileDefinition definition) ||
      definition.BehaviorId != LegacyAiStyle17ProjectileBehavior.Id ||
      definition.LegacyAiStyle != 17 || definition.MaximumPenetration != -1 ||
      definition.Knockback != 12.0f)
  {
    throw new InvalidOperationException("Type 43 did not retain its source-backed aiStyle 17 defaults.");
  }

  using DomeSimulation simulation = new(new WorldGrid(400, 300), definitions);
  PlayerHandle owner = simulation.CreatePlayer(new SimulationVector(10.0f, 10.0f));
  simulation.QueueProjectileSpawn(new SpawnProjectileCommand(
    owner, 10.0f, 10.0f, 1, 0, 20, ProjectileType: 43, ProjectileSpeed: 4.0f,
    MiscText: "source sign"));
  simulation.Tick(new SimulationInputBatch());
  simulation.Tick(new SimulationInputBatch());
  ProjectileReplicationSnapshot snapshot = simulation.CreateProjectileReplicationSnapshots().Single();
  Entity projectile = default;
  simulation.World.Query(
    new QueryDescription().WithAll<ProjectileMiscTextComponent>(),
    (Entity entity) => projectile = entity);
  if (!snapshot.IsActive || snapshot.Position.X != 13.92f || snapshot.Position.Y != 10.2f ||
      snapshot.Velocity.X != 3.92f || snapshot.Velocity.Y != 0.2f || projectile == default ||
      simulation.World.Get<ProjectileMiscTextComponent>(projectile).Value != "source sign")
  {
    throw new InvalidOperationException(
      $"The aiStyle 17 projectile did not preserve source gravity or miscText state: " +
      $"position={snapshot.Position}, velocity={snapshot.Velocity}, " +
      $"text={simulation.World.Get<ProjectileMiscTextComponent>(projectile).Value}.");
  }

  Entity ownerEntity = default;
  simulation.World.Query(
    new QueryDescription().WithAll<PlayerLifecycleComponent>(),
    (Entity entity) => ownerEntity = entity);
  if (!simulation.WorldGrid.TrySetTile(30, 32, new WorldTile(true, 1)) ||
      !simulation.WorldGrid.TrySetTile(31, 32, new WorldTile(true, 1)))
  {
    throw new InvalidOperationException("The aiStyle 17 placement fixture could not create support.");
  }

  WorldObjectTileMutation[] footprint =
  [
    new(30, 30, simulation.WorldGrid.GetTile(30, 30), new WorldTile(true, 85, FrameX: 0)),
    new(31, 30, simulation.WorldGrid.GetTile(31, 30), new WorldTile(true, 85, FrameX: 18)),
    new(30, 31, simulation.WorldGrid.GetTile(30, 31), new WorldTile(true, 85, FrameY: 18)),
    new(31, 31, simulation.WorldGrid.GetTile(31, 31), new WorldTile(true, 85, FrameX: 18, FrameY: 18))
  ];
  WorldObjectPlacementResult placement = simulation.TryCommitProjectileWorldObjectPlacement(
    new ProjectileWorldObjectPlacementCommand(
      300,
      projectile,
      ownerEntity,
      30,
      30,
      WorldObjectPlacementRequest.SignObjectType,
      0,
      1,
      "source sign",
      TombstoneReason: ProjectileTombstoneReason.TileHit),
    footprint,
    ownerActive: true,
    projectileActive: true);
  if (!placement.Committed || simulation.CreateSignSnapshots().Count != 1 ||
      simulation.CreateWorldObjectPlacementEvents().Count != 1 ||
      simulation.CreateWorldObjectPlacementEvents()[0].ProjectileTombstoneReason !=
        ProjectileTombstoneReason.TileHit)
  {
    throw new InvalidOperationException("Style-17 placement did not commit sign and event state.");
  }

  simulation.Tick(new SimulationInputBatch());
  ProjectileReplicationSnapshot tombstone = simulation.CreateProjectileReplicationSnapshots().Single();
  if (tombstone.IsActive || tombstone.TombstoneReason != ProjectileTombstoneReason.TileHit)
  {
    throw new InvalidOperationException("Committed style-17 placement did not expire its projectile.");
  }

  WorldObjectPlacementResult inactive = simulation.TryCommitProjectileWorldObjectPlacement(
    new ProjectileWorldObjectPlacementCommand(
      301,
      projectile,
      ownerEntity,
      34,
      30,
      WorldObjectPlacementRequest.SignObjectType,
      0,
      1,
      "should not write"),
    [new WorldObjectTileMutation(
      34,
      30,
      simulation.WorldGrid.GetTile(34, 30),
      new WorldTile(true, 85))],
    ownerActive: true,
    projectileActive: true);
  if (inactive.FailureCode != WorldObjectPlacementFailureCode.ProjectileInactive ||
      simulation.WorldGrid.GetTile(34, 30).IsActive || simulation.CreateSignSnapshots().Count != 1)
  {
    throw new InvalidOperationException("Inactive projectile placement mutated world or sign state.");
  }
}

static void VerifyProjectileWorldObjectPlacementContract()
{
  WorldGrid world = new(400, 300, initializeLegacyEmptyFrames: true);
  using World ownerWorld = World.Create();
  Entity owner = ownerWorld.Create();
  Entity projectile = ownerWorld.Create();
  WorldObjectPlacementRequest request = new(
    Sequence: 4,
    projectile,
    owner,
    OriginX: 20,
    OriginY: 20,
    WorldObjectPlacementRequest.SignObjectType,
    Style: 0,
    Direction: 1,
    SignText: "atomic sign");
  if (!world.TrySetTile(20, 22, new WorldTile(true, 1)) ||
      !world.TrySetTile(21, 22, new WorldTile(true, 1)))
  {
    throw new InvalidOperationException("The atomic placement fixture could not create support.");
  }

  WorldObjectTileMutation[] footprint =
  [
    new(20, 20, world.GetTile(20, 20), new WorldTile(true, 85, FrameX: 0, FrameY: 0)),
    new(21, 20, world.GetTile(21, 20), new WorldTile(true, 85, FrameX: 18, FrameY: 0)),
    new(20, 21, world.GetTile(20, 21), new WorldTile(true, 85, FrameX: 0, FrameY: 18)),
    new(21, 21, world.GetTile(21, 21), new WorldTile(true, 85, FrameX: 18, FrameY: 18))
  ];
  WorldSectionCoordinates section = world.GetSectionCoordinates(20, 20);
  WorldObjectPlacementPlan plan = new(
    request,
    footprint,
    new Dictionary<WorldSectionCoordinates, long> { [section] = world.GetSectionVersion(section) });
  WorldObjectPlacementCommitSystem commit = new();
  HashSet<long> sequences = new();
  WorldObjectPlacementResult result = commit.Commit(
    world,
    plan,
    ownerActive: true,
    projectileActive: true,
    sequences);
  if (!result.Committed || world.GetTile(20, 20).Type != 85 ||
      world.GetTile(21, 20).Type != 85 || result.Sections.Count != 1)
  {
    throw new InvalidOperationException("Atomic projectile placement did not commit its footprint.");
  }

  WorldObjectPlacementResult duplicate = commit.Commit(
    world,
    plan,
    ownerActive: true,
    projectileActive: true,
    sequences);
  if (duplicate.FailureCode != WorldObjectPlacementFailureCode.DuplicateSequence)
  {
    throw new InvalidOperationException("Duplicate projectile placement sequence was accepted.");
  }

  WorldObjectPlacementResult inactiveOwner = commit.Commit(
    world,
    new WorldObjectPlacementPlan(
      request with { Sequence = 5 },
      footprint,
      new Dictionary<WorldSectionCoordinates, long>
      {
        [section] = world.GetSectionVersion(section)
      }),
    ownerActive: false,
    projectileActive: true,
    sequences);
  if (inactiveOwner.FailureCode != WorldObjectPlacementFailureCode.OwnerInactive)
  {
    throw new InvalidOperationException("Inactive placement owner was not rejected.");
  }

  if (!world.TrySetTile(40, 42, new WorldTile(true, 1)) ||
      !world.TrySetTile(41, 42, new WorldTile(true, 1)))
  {
    throw new InvalidOperationException("The generated placement fixture could not create support.");
  }

  if (!SignObjectPlacementPlanFactory.TryCreate(
        world,
        request with { Sequence = 8, OriginX = 40, OriginY = 40 },
        out WorldObjectPlacementPlan generated,
        out WorldObjectPlacementFailureCode generatedFailure) ||
      generatedFailure != WorldObjectPlacementFailureCode.None || generated.Footprint.Count != 4 ||
      generated.Footprint[0].Replacement.Type != WorldObjectPlacementRequest.SignObjectType ||
      generated.Footprint[3].Replacement.FrameX != 18 || generated.Footprint[3].Replacement.FrameY != 18)
  {
    throw new InvalidOperationException("Sign object footprint factory did not produce a deterministic 2x2 plan.");
  }

  WorldObjectPlacementRequest occupiedRequest = request with
  {
    Sequence = 9,
    OriginX = 20,
    OriginY = 20
  };
  if (SignObjectPlacementPlanFactory.TryCreate(
        world,
        occupiedRequest,
        out _,
        out WorldObjectPlacementFailureCode occupiedFailure) ||
      occupiedFailure != WorldObjectPlacementFailureCode.OccupiedTile)
  {
    throw new InvalidOperationException("Occupied sign footprint was not rejected atomically.");
  }

  ProjectileWorldObjectPlacementCommand invalid = new(
    Sequence: -1,
    projectile,
    owner,
    OriginX: 20,
    OriginY: 20,
    ObjectType: 999,
    Style: -1,
    Direction: 0,
    SignText: new string('x', 101));
  if (invalid.Validate(world) != WorldObjectPlacementFailureCode.InvalidSequence)
  {
    throw new InvalidOperationException("Invalid placement sequence did not win normalization.");
  }

  Console.WriteLine("PASS: projectile world-object placement validates and commits atomically");
}

static void VerifyProjectileWorldObjectPlacementActorValidation()
{
  WorldGrid world = new(400, 300, initializeLegacyEmptyFrames: true);
  using DomeSimulation simulation = new(world);
  Entity owner = simulation.World.Create();
  Entity projectile = simulation.World.Create();
  WorldObjectTileMutation[] footprint =
  [
    new(20, 20, world.GetTile(20, 20), new WorldTile(true, 85))
  ];

  WorldObjectPlacementResult inactiveOwner = simulation.TryCommitProjectileWorldObjectPlacement(
    new ProjectileWorldObjectPlacementCommand(
      Sequence: 500,
      projectile,
      owner,
      OriginX: 20,
      OriginY: 20,
      WorldObjectPlacementRequest.SignObjectType,
      Style: 0,
      Direction: 1,
      SignText: "inactive-owner"),
    footprint,
    ownerActive: true,
    projectileActive: true);
  if (inactiveOwner.FailureCode != WorldObjectPlacementFailureCode.OwnerInactive ||
      world.GetTile(20, 20).IsActive || simulation.CreateSignSnapshots().Count != 0)
  {
    throw new InvalidOperationException(
      "A placement with no authoritative owner was accepted or mutated world state.");
  }

  _ = simulation.CreatePlayer(new SimulationVector(2.0f, 2.0f));
  Entity authoritativeOwner = default;
  simulation.World.Query(
    new QueryDescription().WithAll<PlayerLifecycleComponent>(),
    (Entity entity) => authoritativeOwner = entity);
  WorldObjectPlacementResult inactiveProjectile =
    simulation.TryCommitProjectileWorldObjectPlacement(
    new ProjectileWorldObjectPlacementCommand(
      Sequence: 501,
      projectile,
      authoritativeOwner,
      OriginX: 20,
      OriginY: 20,
      WorldObjectPlacementRequest.SignObjectType,
      Style: 0,
      Direction: 1,
      SignText: "inactive-projectile"),
    footprint,
    ownerActive: true,
    projectileActive: true);
  if (inactiveProjectile.FailureCode != WorldObjectPlacementFailureCode.ProjectileInactive ||
      world.GetTile(20, 20).IsActive || simulation.CreateSignSnapshots().Count != 0)
  {
    throw new InvalidOperationException(
      "A placement with no authoritative projectile was accepted or mutated world state.");
  }

  Console.WriteLine("PASS: projectile placement rejects non-authoritative owner and projectile");
}

static void VerifyWorldObjectPlacementRegistrySemantics()
{
  WorldObjectPlacementDefinitionRegistry registry =
    WorldObjectPlacementDefinitionRegistry.CreateVersion4Base();
  if (!registry.TryGet(
        WorldObjectPlacementRequest.SignObjectType,
        out WorldObjectPlacementDefinition definition) ||
      definition.Width != 2 || definition.Height != 2 ||
      definition.OriginOffset != (0, 1) || definition.CoordinateWidth != 16 ||
      definition.CoordinatePadding != 2 || definition.CoordinateHeights.Count != 2 ||
      definition.CoordinateHeights[0] != 16 || definition.CoordinateHeights[1] != 16 ||
       !definition.StyleHorizontal || definition.StyleFullWidth != 36 ||
       definition.AnchorKind != WorldObjectPlacementAnchorKind.Bottom ||
       definition.SupportsAlternate || definition.SupportsRandom ||
       !definition.UsesCustomCanPlace || definition.DrawYOffset != 2 || definition.LavaDeath ||
       definition.DefaultDirection != 0 ||
       !definition.IsValidDirection(-1) || !definition.IsValidDirection(1) ||
       definition.IsValidDirection(0))
  {
    throw new InvalidOperationException(
      "The type-85 placement registry did not preserve the source-backed TileObject definition.");
  }

  WorldGrid world = new(400, 300, initializeLegacyEmptyFrames: true);
  using World ownerWorld = World.Create();
  Entity owner = ownerWorld.Create();
  Entity projectile = ownerWorld.Create();
  WorldObjectPlacementRequest request = new(
    Sequence: 700,
    projectile,
    owner,
    OriginX: 20,
    OriginY: 20,
    WorldObjectPlacementRequest.SignObjectType,
    Style: 2,
    Direction: 1,
    SignText: "registry sign");

  if (SignObjectPlacementPlanFactory.TryCreate(
        world,
        request,
        out _,
        out WorldObjectPlacementFailureCode missingSupportFailure) ||
      missingSupportFailure == WorldObjectPlacementFailureCode.None ||
      world.GetTile(20, 20).IsActive)
  {
    throw new InvalidOperationException(
      "A sign without source-backed bottom support was accepted or mutated the world.");
  }

  if (!world.TrySetTile(20, 22, new WorldTile(true, 1)) ||
      !world.TrySetTile(21, 22, new WorldTile(true, 1)))
  {
    throw new InvalidOperationException("The registry fixture could not create bottom support.");
  }

  if (!SignObjectPlacementPlanFactory.TryCreate(
        world,
        request,
        out WorldObjectPlacementPlan generated,
        out WorldObjectPlacementFailureCode generatedFailure) ||
      generatedFailure != WorldObjectPlacementFailureCode.None ||
      generated.Footprint.Count != 4 ||
      generated.Footprint[0].Replacement.FrameX != 72 ||
      generated.Footprint[1].Replacement.FrameX != 90 ||
      generated.Footprint[3].Replacement.FrameY != 18)
  {
    throw new InvalidOperationException(
      "The source-backed sign registry did not calculate the 2x2 style frame band.");
  }

  if (generated.Footprint is WorldObjectTileMutation[] ||
      generated.SectionVersions is Dictionary<WorldSectionCoordinates, long>)
  {
    throw new InvalidOperationException(
      "The placement plan exposed mutable footprint or section-version storage.");
  }

  if ((request with { ObjectType = 84 }).Validate(world) !=
        WorldObjectPlacementFailureCode.UnsupportedObject ||
      (request with { Style = -1 }).Validate(world) !=
        WorldObjectPlacementFailureCode.InvalidStyle ||
      (request with { Direction = 0 }).Validate(world) !=
        WorldObjectPlacementFailureCode.InvalidDirection)
  {
    throw new InvalidOperationException(
      "The sign placement request did not reject unsupported object, style or direction inputs.");
  }

  WorldObjectTileMutation[] wrongFrameFootprint = [.. generated.Footprint];
  wrongFrameFootprint[0] = wrongFrameFootprint[0] with
  {
    Replacement = wrongFrameFootprint[0].Replacement with { FrameX = 0 }
  };
  WorldObjectPlacementResult wrongFrameResult = new WorldObjectPlacementCommitSystem().Commit(
    world,
    new WorldObjectPlacementPlan(
      request with { Sequence = 702 },
      wrongFrameFootprint,
      generated.SectionVersions),
    ownerActive: true,
    projectileActive: true);
  if (wrongFrameResult.Committed ||
      wrongFrameResult.FailureCode != WorldObjectPlacementFailureCode.InvalidFrame ||
      world.GetTile(20, 20).IsActive)
  {
    throw new InvalidOperationException(
      "A sign footprint with a mismatched source frame was accepted or partially committed.");
  }

  WorldObjectPlacementPlan malformed = new(
    request with { Sequence = 701 },
    [generated.Footprint[0]],
    generated.SectionVersions);
  WorldObjectPlacementResult malformedResult = new WorldObjectPlacementCommitSystem().Commit(
    world,
    malformed,
    ownerActive: true,
    projectileActive: true);
  if (malformedResult.Committed ||
      malformedResult.FailureCode == WorldObjectPlacementFailureCode.None ||
      world.GetTile(20, 20).IsActive)
  {
    throw new InvalidOperationException(
      "A malformed sign footprint was accepted or partially committed.");
  }

  Console.WriteLine("PASS: source-backed sign registry validates support, frames and footprint shape");
}

static void VerifyType5SourceBackedTickAndWaterDefaults()
{
  ProjectileDefinitionRegistry definitions = ProjectileDefinitionRegistry.CreateDefault();
  if (!definitions.TryGet(5, out ProjectileDefinition definition) ||
      definition.LifetimeTicks != 120 || definition.ExtraUpdates != 1 ||
      !definition.IgnoreWater || definition.LiquidPolicy != ProjectileLiquidPolicy.Destroy ||
      definition.LegacyAiStyle != 1 || definition.DamageClass != ProjectileDamageClass.Ranged)
  {
    throw new InvalidOperationException(
      "Type 5 did not retain its source-backed lifetime, extra-update and water policies.");
  }
}

static void VerifyProjectileMaxUpdatesProjection()
{
  if (ProjectileUpdateBudgetPolicy.GetMaxUpdates(0) != 1 ||
      ProjectileUpdateBudgetPolicy.GetMaxUpdates(1) != 2 ||
      ProjectileUpdateBudgetPolicy.GetMaxUpdates(180) != 181)
  {
    throw new InvalidOperationException(
      "Projectile max update budget did not derive from extraUpdates deterministically.");
  }

  try
  {
    _ = ProjectileUpdateBudgetPolicy.GetMaxUpdates(-1);
    throw new InvalidOperationException(
      "Projectile max update budget accepted a negative extraUpdates value.");
  }
  catch (ArgumentOutOfRangeException)
  {
  }

  ProjectileDefinitionComponent definition = new(
    5, 1, 10, 120, new ColliderComponent(0.5f, 0.5f), true, false,
    ExtraUpdates: 1);
  if (definition.MaxUpdates != 2)
  {
    throw new InvalidOperationException(
      $"MaxUpdates did not derive from ExtraUpdates: {definition.MaxUpdates}");
  }
}

static void VerifyProjectileOwnerEligibilityContract()
{
  ProjectileTargetEligibilitySystem eligibility = new();
  ProjectileDefinitionComponent hostile = new(
    3,
    1,
    10,
    30,
    new ColliderComponent(0.5f, 0.5f),
    Friendly: false,
    Hostile: true,
    PlayerDamagePolicy: PlayerDamagePolicy.HostileNonPvp);
  PlayerHandle owner = new(1);
  if (eligibility.CanDamagePlayerTarget(hostile, owner, owner, isPvpEnabled: false) ||
      eligibility.CanDamagePlayerTarget(hostile, owner, default, isPvpEnabled: false))
  {
    throw new InvalidOperationException(
      "Projectile owner eligibility contract admitted an owner or invalid player target.");
  }

  if (!eligibility.CanDamagePlayerTarget(
        hostile,
        owner,
        new PlayerHandle(2),
        isPvpEnabled: false))
  {
    throw new InvalidOperationException(
      "Projectile owner eligibility contract rejected a valid non-owner hostile target.");
  }

  ProjectileDefinitionComponent pvpOnly = hostile with
  {
    PlayerDamagePolicy = PlayerDamagePolicy.PvpOptIn
  };
  if (eligibility.CanDamagePlayerTarget(
        pvpOnly,
        owner,
        new PlayerHandle(2),
        isPvpEnabled: false) ||
      !eligibility.CanDamagePlayerTarget(
        pvpOnly,
        owner,
        new PlayerHandle(2),
        isPvpEnabled: true))
  {
    throw new InvalidOperationException(
      "PVP opt-in policy did not distinguish disabled and enabled world rules.");
  }
}

static void VerifyProjectileOwnerMinionAttackTarget()
{
  using DomeSimulation simulation = new(new WorldGrid(400, 300));
  PlayerHandle owner = simulation.CreatePlayer(new SimulationVector(10.0f, 0.0f));
  NpcHandle target = simulation.CreateNpc(new SimulationVector(14.0f, 0.0f));
  if (!simulation.TrySetPlayerMinionAttackTarget(owner, target) ||
      !simulation.TryGetPlayerMinionAttackTarget(owner, out NpcHandle resolved) ||
      resolved != target ||
      simulation.CreatePlayerStateSnapshot(owner).MinionAttackTarget != target)
  {
    throw new InvalidOperationException(
      "The owner minion attack target was not retained in authoritative player targeting state.");
  }

  simulation.QueueNpcDespawn(new DespawnNpcCommand(target, NpcDespawnReason.Killed));
  simulation.Tick(new SimulationInputBatch());
  if (simulation.TryGetPlayerMinionAttackTarget(owner, out _))
  {
    throw new InvalidOperationException(
      "The owner minion attack target remained resolvable after the NPC became inactive.");
  }

  if (simulation.TrySetPlayerMinionAttackTarget(owner, new NpcHandle(999)) ||
      !simulation.TrySetPlayerMinionAttackTarget(owner, null) ||
      simulation.TryGetPlayerMinionAttackTarget(owner, out _))
  {
    throw new InvalidOperationException(
      "The owner minion attack target did not reject unknown NPCs or clear deterministically.");
  }
}

static void VerifyProjectileOwnerMinionAttackTargetResolution()
{
  if (!ProjectileOwnerMinionTargetPolicy.CanResolve(
        new PlayerHandle(1),
        isMinion: true,
        new NpcHandle(7),
        targetActive: true,
        targetChaseable: true,
        targetHealth: 100) ||
      ProjectileOwnerMinionTargetPolicy.CanResolve(
        default,
        isMinion: true,
        new NpcHandle(7),
        targetActive: true,
        targetChaseable: true,
        targetHealth: 100) ||
      ProjectileOwnerMinionTargetPolicy.CanResolve(
        new PlayerHandle(1),
        isMinion: false,
        new NpcHandle(7),
        targetActive: true,
        targetChaseable: true,
        targetHealth: 100) ||
      ProjectileOwnerMinionTargetPolicy.CanResolve(
        new PlayerHandle(1),
        isMinion: true,
        new NpcHandle(7),
        targetActive: true,
        targetChaseable: false,
        targetHealth: 100) ||
      ProjectileOwnerMinionTargetPolicy.CanResolve(
        new PlayerHandle(1),
        isMinion: true,
        new NpcHandle(7),
        targetActive: true,
        targetChaseable: true,
        targetHealth: 0))
  {
    throw new InvalidOperationException(
      "Projectile owner minion target policy did not fail closed at owner, marker, " +
      "active, chaseable, or health boundaries.");
  }

  ProjectileDefinitionRegistry definitions = new([
    new ProjectileDefinition(
      192,
      1,
      10,
      120,
      new ColliderComponent(1.0f, 1.0f),
      true,
      false,
      -1,
      IsMinion: true,
      MinionSlots: 1.0f)]);
  using DomeSimulation simulation = new(new WorldGrid(400, 300), definitions);
  PlayerHandle owner = simulation.CreatePlayer(new SimulationVector(10.0f, 10.0f));
  NpcHandle npcTarget = simulation.CreateNpc(new SimulationVector(12.0f, 10.0f));
  if (!simulation.TrySetPlayerMinionAttackTarget(owner, npcTarget))
  {
    throw new InvalidOperationException("The minion target fixture could not set its NPC target.");
  }

  simulation.QueueProjectileSpawn(new SpawnProjectileCommand(
    owner,
    10.0f,
    10.0f,
    1,
    0,
    120,
    ProjectileType: 192));
  simulation.Tick(new SimulationInputBatch());
  ProjectileReplicationSnapshot minion = simulation.CreateProjectileReplicationSnapshots()
    .Single(snapshot => snapshot.IsActive);
  if (!simulation.TryGetProjectileOwnerMinionAttackTarget(
        minion.ReplicationId,
        out NpcHandle resolvedTarget) || resolvedTarget != npcTarget)
  {
    throw new InvalidOperationException(
      "The authoritative projectile owner target query did not resolve the selected NPC.");
  }

  Entity projectileEntity = default;
  simulation.World.Query(
    new QueryDescription().WithAll<ProjectileNetworkIdentityComponent>(),
    (Entity entity, ref ProjectileNetworkIdentityComponent identity) =>
    {
      if (identity.Identity == minion.Identity)
      {
        projectileEntity = entity;
      }
    });
  if (projectileEntity == default ||
      simulation.World.Get<ProjectileBehaviorComponent>(projectileEntity).State.TargetId !=
        npcTarget.Value)
  {
    bool hasMinion = projectileEntity != default &&
      simulation.World.Has<ProjectileMinionComponent>(projectileEntity);
    bool resolved = simulation.TryGetProjectileOwnerMinionAttackTarget(
      minion.ReplicationId,
      out NpcHandle diagnosticTarget);
    throw new InvalidOperationException(
      $"The projectile behavior state did not consume the authoritative minion target: " +
      $"entity={projectileEntity}, target={simulation.World.Get<ProjectileBehaviorComponent>(projectileEntity).State.TargetId}, " +
      $"expected={npcTarget.Value}, marker={hasMinion}, query={resolved}, " +
      $"queryTarget={diagnosticTarget.Value}, snapshotMinion={minion.IsMinion}.");
  }

  simulation.QueueNpcDespawn(new DespawnNpcCommand(npcTarget, NpcDespawnReason.Killed));
  simulation.Tick(new SimulationInputBatch());
  if (simulation.TryGetProjectileOwnerMinionAttackTarget(minion.ReplicationId, out _) ||
      simulation.World.Get<ProjectileBehaviorComponent>(projectileEntity).State.TargetId != 0)
  {
    throw new InvalidOperationException(
      "The projectile owner target query resolved an inactive NPC target.");
  }

  Console.WriteLine(
    "PASS: projectile owner minion target policy resolves only active chaseable targets");
}

static void VerifyProjectileRestrikeDelayLifecycle()
{
  ProjectileRestrikeDelayComponent delay = new(2);
  ProjectileRestrikeDelaySystem system = new();
  if (!system.IsBlocked(delay))
  {
    throw new InvalidOperationException("A positive restrike delay must block a retrike.");
  }

  ProjectileRestrikeDelayComponent afterOneTick = system.Tick(delay);
  if (afterOneTick.RemainingTicks != 1 || !system.IsBlocked(afterOneTick))
  {
    throw new InvalidOperationException("Restrike delay did not decrement deterministically.");
  }

  ProjectileRestrikeDelayComponent afterTwoTicks = system.Tick(afterOneTick);
  if (afterTwoTicks.RemainingTicks != 0 || system.IsBlocked(afterTwoTicks))
  {
    throw new InvalidOperationException("Restrike delay did not expire at zero.");
  }

  ProjectileRestrikeDelayComponent defaultDelay = new();
  if (defaultDelay.RemainingTicks != 0 || system.IsBlocked(defaultDelay))
  {
    throw new InvalidOperationException("Default restrike delay must be immediately available.");
  }

  bool rejectedNegative = false;
  try
  {
    _ = new ProjectileRestrikeDelayComponent(-1);
  }
  catch (ArgumentOutOfRangeException)
  {
    rejectedNegative = true;
  }

  if (!rejectedNegative)
  {
    throw new InvalidOperationException("Negative restrike delay was accepted.");
  }
  Console.WriteLine("PASS: projectile restrike delay lifecycle is bounded and deterministic");
}

static void VerifyProjectileNetworkUpdatePolicy()
{
  ProjectileNetworkUpdatePolicy policy = new();
  ProjectileNetworkUpdateComponent state = new();
  state = policy.RequestSecondaryUpdate(state);
  ProjectileNetworkUpdateResult first = policy.Tick(state);
  if (!first.ShouldSend || !first.State.SendRequested || first.State.SecondaryUpdatePending ||
      first.State.NetSpam != 4)
  {
    throw new InvalidOperationException(
      "A pending secondary update did not send and consume five net-spam budget units.");
  }

  ProjectileNetworkUpdateComponent saturated = new(false, 60);
  saturated = policy.RequestSecondaryUpdate(saturated);
  ProjectileNetworkUpdateResult blocked = policy.Tick(saturated);
  if (blocked.ShouldSend || blocked.State.SendRequested || !blocked.State.SecondaryUpdatePending ||
      blocked.State.NetSpam != 59)
  {
    throw new InvalidOperationException(
      "A saturated secondary update did not remain pending while net-spam decayed.");
  }

  ProjectileNetworkUpdateResult retried = policy.Tick(blocked.State);
  if (!retried.ShouldSend || !retried.State.SendRequested || retried.State.SecondaryUpdatePending ||
      retried.State.NetSpam != 63)
  {
    throw new InvalidOperationException(
      "A pending secondary update did not retry after the budget fell below its cap.");
  }

  ProjectileNetworkUpdateResult idle = policy.Tick(retried.State);
  if (idle.ShouldSend || idle.State.SendRequested)
  {
    throw new InvalidOperationException(
      "A consumed projectile network update remained sendable on the following tick.");
  }

  bool rejectedNegative = false;
  try
  {
    _ = new ProjectileNetworkUpdateComponent(false, -1);
  }
  catch (ArgumentOutOfRangeException)
  {
    rejectedNegative = true;
  }

  if (!rejectedNegative)
  {
    throw new InvalidOperationException("Negative projectile net-spam was accepted.");
  }

  Console.WriteLine("PASS: projectile network update policy is bounded and deterministic");
}

static void VerifyProjectilePrimaryUpdateCadence()
{
  ProjectileNetworkUpdatePolicy policy = new();
  ProjectileNetworkUpdateComponent primary = policy.RequestPrimaryUpdate(new());
  ProjectileNetworkUpdateResult first = policy.Tick(primary);
  if (!first.ShouldSend || !first.State.SendRequested || first.State.PrimaryUpdatePending ||
      first.State.SecondaryUpdatePending || first.State.NetSpam != 4)
  {
    throw new InvalidOperationException(
      "A primary projectile update did not consume the available network budget.");
  }

  ProjectileNetworkUpdateComponent saturated = policy.RequestPrimaryUpdate(
    new ProjectileNetworkUpdateComponent(netSpam: 60));
  ProjectileNetworkUpdateResult blocked = policy.Tick(saturated);
  if (blocked.ShouldSend || blocked.State.SendRequested || blocked.State.PrimaryUpdatePending ||
      !blocked.State.SecondaryUpdatePending || blocked.State.NetSpam != 59)
  {
    throw new InvalidOperationException(
      "A saturated primary projectile update did not defer as netUpdate2.");
  }

  ProjectileNetworkUpdateResult retried = policy.Tick(blocked.State);
  if (!retried.ShouldSend || !retried.State.SendRequested || retried.State.PrimaryUpdatePending ||
      retried.State.SecondaryUpdatePending || retried.State.NetSpam != 63)
  {
    throw new InvalidOperationException(
      "A deferred primary projectile update did not retry after net-spam decay.");
  }

  ProjectileNetworkUpdateResult idle = policy.Tick(retried.State);
  if (idle.ShouldSend || idle.State.SendRequested)
  {
    throw new InvalidOperationException(
      "A consumed primary projectile network update remained sendable on the following tick.");
  }

  Console.WriteLine("PASS: primary projectile network update follows legacy deferred cadence");
}

static void VerifyProjectileTombstoneNetworkReset()
{
  using DomeSimulation simulation = new(new WorldGrid(400, 300));
  PlayerHandle player = simulation.CreatePlayer(new SimulationVector(10.0f, 0.0f));
  simulation.QueueProjectileSpawn(new SpawnProjectileCommand(
    player,
    10.0f,
    0.0f,
    1,
    10,
    20));
  simulation.Tick(new SimulationInputBatch());

  Entity projectileEntity = default;
  simulation.World.Query(
    new QueryDescription().WithAll<ProjectileTagComponent, ProjectileNetworkIdentityComponent>(),
    (Entity entity) => projectileEntity = entity);
  if (!simulation.World.IsAlive(projectileEntity))
  {
    throw new InvalidOperationException(
      "The tombstone network-reset fixture did not create an authoritative projectile.");
  }

  simulation.World.Get<ProjectileLifetimeComponent>(projectileEntity).RemainingTicks = 1;
  simulation.World.Get<ProjectileNetworkUpdateComponent>(projectileEntity) =
    new ProjectileNetworkUpdateComponent(
      secondaryUpdatePending: true,
      netSpam: 60,
      primaryUpdatePending: true);
  simulation.Tick(new SimulationInputBatch());

  ProjectileReplicationSnapshot tombstone = simulation
    .CreateProjectileReplicationSnapshots()
    .Single();
  if (tombstone.IsActive || tombstone.NetSpam != 0 || tombstone.PrimaryUpdatePending ||
      tombstone.SecondaryUpdatePending || !tombstone.NetworkUpdateReady)
  {
    throw new InvalidOperationException(
      "Projectile tombstone retained active netUpdate/netSpam state after despawn.");
  }

  using DomeSimulation npcSimulation = new(new WorldGrid(400, 300));
  NpcHandle npc = npcSimulation.CreateNpc(new SimulationVector(10.0f, 0.0f));
  ProjectileDefinition npcDefinition = ProjectileDefinitionRegistry.CreateDefault()
    .OrderedDefinitions
    .Single(definition => definition.ProjectileType == 347);
  _ = npcSimulation.CreateNpcProjectile(new NpcProjectileSpawnRequest(
    npc,
    Sequence: 1,
    npcDefinition,
    Damage: 10,
    Position: new SimulationVector(10.0f, 0.0f),
    Velocity: new SimulationVector(1.0f, 0.0f)));
  Entity npcProjectileEntity = default;
  npcSimulation.World.Query(
    new QueryDescription().WithAll<ProjectileTagComponent, NpcProjectileNetworkIdentityComponent>(),
    (Entity entity) => npcProjectileEntity = entity);
  if (!npcSimulation.World.IsAlive(npcProjectileEntity))
  {
    throw new InvalidOperationException(
      "The NPC tombstone network-reset fixture did not create an authoritative projectile.");
  }

  npcSimulation.World.Get<ProjectileLifetimeComponent>(npcProjectileEntity).RemainingTicks = 1;
  npcSimulation.World.Get<ProjectileNetworkUpdateComponent>(npcProjectileEntity) =
    new ProjectileNetworkUpdateComponent(
      secondaryUpdatePending: true,
      netSpam: 60,
      primaryUpdatePending: true);
  npcSimulation.Tick(new SimulationInputBatch());

  NpcProjectileReplicationSnapshot npcTombstone = npcSimulation
    .CreateNpcProjectileReplicationSnapshots()
    .Single();
  if (npcTombstone.IsActive || npcTombstone.NetSpam != 0 || npcTombstone.PrimaryUpdatePending ||
      npcTombstone.SecondaryUpdatePending || !npcTombstone.NetworkUpdateReady)
  {
    throw new InvalidOperationException(
      "NPC projectile tombstone retained active netUpdate/netSpam state after despawn.");
  }

  Console.WriteLine("PASS: projectile tombstone resets network update state");
}

static void VerifyProjectilePrimaryNetworkUpdateState()
{
  ProjectileNetworkUpdatePolicy policy = new();
  ProjectileNetworkUpdateComponent initial = new();
  ProjectileNetworkUpdateComponent pending = policy.RequestPrimaryUpdate(initial);
  if (!pending.PrimaryUpdatePending || pending.SecondaryUpdatePending || pending.NetSpam != 0)
  {
    throw new InvalidOperationException("Primary projectile network update was not retained.");
  }

  ProjectileNetworkUpdateComponent acknowledged = policy.AcknowledgePrimaryUpdate(pending);
  if (acknowledged.PrimaryUpdatePending)
  {
    throw new InvalidOperationException("Primary projectile network update was not acknowledged.");
  }

  Console.WriteLine("PASS: projectile primary network update state is bounded");
}

static void VerifyProjectileSoundDelayPolicy()
{
  ProjectileSoundDelayPolicy policy = new();
  ProjectileSoundDelayComponent delay = new(2);
  if (policy.IsDelayed(delay) == false)
  {
    throw new InvalidOperationException("A positive sound delay must remain delayed.");
  }

  delay = policy.Tick(delay);
  if (delay.RemainingTicks != 1 || !policy.IsDelayed(delay))
  {
    throw new InvalidOperationException("Sound delay did not decrement on the first tick.");
  }

  delay = policy.Tick(delay);
  if (delay.RemainingTicks != 0 || policy.IsDelayed(delay))
  {
    throw new InvalidOperationException("Sound delay did not become ready at zero.");
  }

  ProjectileSoundDelayComponent sentinel = new(-1);
  if (policy.IsDelayed(sentinel) || policy.Tick(sentinel).RemainingTicks != -1)
  {
    throw new InvalidOperationException("The -1 sound-delay sentinel was not preserved.");
  }

  bool rejectedInvalid = false;
  try
  {
    _ = new ProjectileSoundDelayComponent(-2);
  }
  catch (ArgumentOutOfRangeException)
  {
    rejectedInvalid = true;
  }

  if (!rejectedInvalid)
  {
    throw new InvalidOperationException("An invalid sound-delay sentinel was accepted.");
  }

  Console.WriteLine("PASS: projectile sound delay policy is bounded and deterministic");
}

static void VerifyProjectileTileCollisionState()
{
  ProjectileTileCollisionComponent enabled = new(true);
  ProjectileTileCollisionComponent disabled = new(false);
  if (!enabled.Enabled || disabled.Enabled)
  {
    throw new InvalidOperationException("Projectile tile-collision state did not retain its value.");
  }

  ProjectileTileCollisionPolicy policy = new();
  if (!policy.ShouldCollide(enabled) || policy.ShouldCollide(disabled))
  {
    throw new InvalidOperationException("Projectile tile-collision policy did not gate collision.");
  }

  ProjectileDefinition definition = new(
    ProjectileType: 901,
    BehaviorId: 1,
    Damage: 10,
    LifetimeTicks: 30,
    Collider: new ColliderComponent(1.0f, 1.0f),
    Friendly: true,
    Hostile: false,
    MaximumPenetration: 1,
    CollidesWithTiles: true);
  using World world = World.Create();
  Entity projectile = new ProjectileSpawnSystem().Spawn(
    world,
    new SpawnProjectileCommand(
      new PlayerHandle(1),
      2.0f,
      2.0f,
      1,
      10,
      30,
      ProjectileType: 901),
    definition,
    identity: 1);
  ref ProjectileTileCollisionComponent liveState =
    ref world.Get<ProjectileTileCollisionComponent>(projectile);
  liveState = disabled;
  ProjectileReplicationSnapshot snapshot = new ProjectileReplicationSystem().Project(
    projectile,
    world,
    replicationId: 1,
    revision: 1,
    default);
  if (snapshot.TileCollisionEnabled)
  {
    throw new InvalidOperationException(
      "Mutable projectile tile-collision state was not projected into replication.");
  }

  Console.WriteLine("PASS: projectile tile collision state is authoritative and bounded");
}

static void VerifyProjectileTileCollisionBehaviorOverride()
{
  ProjectileTileCollisionPolicy policy = new();
  ProjectileTileCollisionComponent initial = new(true);
  ProjectileTileCollisionComponent enabled = policy.ApplyLegacyAiOverride(876, 0.0f, initial);
  ProjectileTileCollisionComponent disabled = policy.ApplyLegacyAiOverride(876, 1.0f, initial);
  ProjectileTileCollisionComponent unrelated = policy.ApplyLegacyAiOverride(875, 1.0f, initial);
  if (!enabled.Enabled || disabled.Enabled || !unrelated.Enabled)
  {
    throw new InvalidOperationException(
      "Type-876 tile-collision AI override did not preserve source-backed gating.");
  }

  bool rejectedNonFinite = false;
  try
  {
    _ = policy.ApplyLegacyAiOverride(876, float.NaN, initial);
  }
  catch (ArgumentOutOfRangeException)
  {
    rejectedNonFinite = true;
  }

  if (!rejectedNonFinite)
  {
    throw new InvalidOperationException("Non-finite type-876 tile-collision AI input was accepted.");
  }

  Console.WriteLine("PASS: type-876 tile collision AI override is bounded");
}

static void VerifyProjectileType656LocalAiHitCounter()
{
  ProjectileBehaviorEffectSystem effects = new();
  ProjectileBehaviorComponent behavior = new(
    1,
    new ProjectileBehaviorState(0.0f, 0.0f, 0, 0));
  ProjectileDefinitionComponent type656 = new(
    656,
    1,
    10,
    30,
    new ColliderComponent(1.0f, 1.0f),
    true,
    false,
    1);
  if (!effects.ApplyAcceptedHit(ref behavior, type656) ||
      behavior.State.LocalAi0 != 1.0f)
  {
    throw new InvalidOperationException("Type-656 localAI hit counter did not increment.");
  }

  ProjectileBehaviorComponent unrelatedBehavior = behavior;
  ProjectileDefinitionComponent unrelatedDefinition = type656 with { ProjectileType = 655 };
  if (effects.ApplyAcceptedHit(ref unrelatedBehavior, unrelatedDefinition) ||
      unrelatedBehavior.State.LocalAi0 != behavior.State.LocalAi0)
  {
    throw new InvalidOperationException("Unrelated projectile type received a localAI hit increment.");
  }

  ProjectileDamageComponent thresholdDamage = new(25);
  ProjectileLifetimeComponent thresholdLifetime = new(1200);
  ProjectileSoundDelayComponent thresholdSoundDelay = new();
  ProjectileBehaviorComponent thresholdBehavior = new(
    1,
    new ProjectileBehaviorState(12.0f, 0.0f, 0, 0, 0.0f, 30.0f));
  ProjectileNetworkUpdateComponent thresholdNetwork = new();
  if (!effects.ApplyType656Tick(
        ref thresholdDamage,
        ref thresholdLifetime,
        ref thresholdSoundDelay,
        ref thresholdBehavior,
        ref thresholdNetwork,
        type656) ||
      thresholdDamage.Amount != 0 ||
      thresholdBehavior.State.Primary != 793.0f ||
      thresholdSoundDelay.RemainingTicks != -1 ||
      !thresholdNetwork.PrimaryUpdatePending)
  {
    throw new InvalidOperationException(
      "Type-656 aiStyle-127 threshold did not zero damage and clamp ai[0].");
  }

  ProjectileDamageComponent stableDamage = new(25);
  ProjectileLifetimeComponent stableLifetime = new(1200);
  ProjectileSoundDelayComponent stableSoundDelay = new(-1);
  ProjectileBehaviorComponent stableBehavior = new(
    1,
    new ProjectileBehaviorState(800.0f, 0.0f, 0, 0, 0.0f, 30.0f));
  ProjectileNetworkUpdateComponent stableNetwork = new();
  if (!effects.ApplyType656Tick(
        ref stableDamage,
        ref stableLifetime,
        ref stableSoundDelay,
        ref stableBehavior,
        ref stableNetwork,
        type656) ||
      stableDamage.Amount != 0 ||
      stableBehavior.State.Primary != 801.0f ||
      stableSoundDelay.RemainingTicks != -1 ||
      stableNetwork.PrimaryUpdatePending)
  {
    throw new InvalidOperationException(
      "Type-656 aiStyle-127 threshold rewrote an already-clamped ai[0].");
  }

  ProjectileDamageComponent belowThresholdDamage = new(25);
  ProjectileLifetimeComponent belowThresholdLifetime = new(1200);
  ProjectileSoundDelayComponent belowThresholdSoundDelay = new();
  ProjectileBehaviorComponent belowThresholdBehavior = new(
    1,
    new ProjectileBehaviorState(12.0f, 0.0f, 0, 0, 0.0f, 29.0f));
  ProjectileNetworkUpdateComponent belowThresholdNetwork = new();
  if (!effects.ApplyType656Tick(
        ref belowThresholdDamage,
        ref belowThresholdLifetime,
        ref belowThresholdSoundDelay,
        ref belowThresholdBehavior,
        ref belowThresholdNetwork,
        type656) ||
      belowThresholdDamage.Amount != 25 ||
      belowThresholdBehavior.State.Primary != 13.0f ||
      belowThresholdSoundDelay.RemainingTicks != -1 ||
      belowThresholdNetwork.PrimaryUpdatePending)
  {
    throw new InvalidOperationException(
      "Type-656 aiStyle-127 threshold applied a damage or update side effect too early.");
  }

  ProjectileDamageComponent expiryDamage = new(25);
  ProjectileLifetimeComponent expiryLifetime = new(1200);
  ProjectileSoundDelayComponent expirySoundDelay = new();
  ProjectileBehaviorComponent expiryBehavior = new(
    1,
    new ProjectileBehaviorState(899.0f, 0.0f, 0, 0, 0.0f, 0.0f));
  ProjectileNetworkUpdateComponent expiryNetwork = new();
  if (!effects.ApplyType656Tick(
        ref expiryDamage,
        ref expiryLifetime,
        ref expirySoundDelay,
        ref expiryBehavior,
        ref expiryNetwork,
        type656) ||
      expiryBehavior.State.Primary != 900.0f ||
      expirySoundDelay.RemainingTicks != -1 ||
      expiryLifetime.RemainingTicks != 0)
  {
    throw new InvalidOperationException(
      "Type-656 aiStyle-127 tick did not terminate at the source-backed ai[0] limit.");
  }

  Console.WriteLine("PASS: type-656 localAI hit counter is bounded");
}

static void VerifyProjectileType657Definition()
{
  ProjectileDefinitionRegistry registry = ProjectileDefinitionRegistry.CreateDefault();
  if (!registry.TryGet(657, out ProjectileDefinition definition) ||
      definition.ProjectileType != 657 ||
      definition.LegacyAiStyle != 127 ||
      definition.BehaviorId != 1 ||
      definition.Collider.Width != 0.625f ||
      definition.Collider.Height != 0.625f ||
      !definition.Hostile ||
      definition.Friendly ||
      definition.CollidesWithTiles ||
      definition.MaximumPenetration != -1 ||
      definition.LifetimeTicks != 1200)
  {
    throw new InvalidOperationException(
      "Type-657 aiStyle-127 definition did not match the source-backed hostile contract.");
  }

  Console.WriteLine("PASS: type-657 aiStyle-127 definition is source-backed");
}

static void VerifyProjectileType657Tick()
{
  ProjectileBehaviorEffectSystem effects = new();
  ProjectileDefinitionRegistry registry = ProjectileDefinitionRegistry.CreateDefault();
  if (!registry.TryGet(657, out ProjectileDefinition definition))
  {
    throw new InvalidOperationException("Type-657 definition was not registered.");
  }
  ProjectileDefinitionComponent definitionComponent = new(
    definition.ProjectileType,
    definition.BehaviorId,
    definition.Damage,
    definition.LifetimeTicks,
    definition.Collider,
    definition.Friendly,
    definition.Hostile,
    definition.MaximumPenetration,
    CollidesWithTiles: definition.CollidesWithTiles,
    LegacyAiStyle: definition.LegacyAiStyle);
  ProjectileDamageComponent damage = new(25);
  ProjectileLifetimeComponent lifetime = new(1200);
  ProjectileSoundDelayComponent soundDelay = new();
  ProjectileBehaviorComponent behavior = new(
    definition.BehaviorId,
    new ProjectileBehaviorState(12.0f, 0.0f, 0, 0));
  ProjectileNetworkUpdateComponent networkUpdate = new();
  if (!effects.ApplyType657Tick(
        ref lifetime,
        ref soundDelay,
        ref behavior,
        definitionComponent) ||
      behavior.State.Primary != 13.0f ||
      soundDelay.RemainingTicks != -1 ||
      lifetime.RemainingTicks != 1200 ||
      damage.Amount != 25 ||
      networkUpdate.PrimaryUpdatePending)
  {
    throw new InvalidOperationException(
      "Type-657 aiStyle-127 tick did not preserve the shared progression contract.");
  }

  lifetime = new ProjectileLifetimeComponent(1200);
  soundDelay = new(-1);
  behavior = new ProjectileBehaviorComponent(
    definition.BehaviorId,
    new ProjectileBehaviorState(899.0f, 0.0f, 0, 0));
  if (!effects.ApplyType657Tick(
        ref lifetime,
        ref soundDelay,
        ref behavior,
        definitionComponent) ||
      behavior.State.Primary != 900.0f ||
      lifetime.RemainingTicks != 0 ||
      soundDelay.RemainingTicks != -1)
  {
    throw new InvalidOperationException(
      "Type-657 aiStyle-127 tick did not terminate at ai[0] 900.");
  }

  Console.WriteLine("PASS: type-657 aiStyle-127 tick is source-backed");
}

static void VerifyProjectileType658Definition()
{
  ProjectileDefinitionRegistry registry = ProjectileDefinitionRegistry.CreateDefault();
  if (!registry.TryGet(658, out ProjectileDefinition definition) ||
      definition.ProjectileType != 658 ||
      definition.LegacyAiStyle != 128 ||
      definition.BehaviorId <= 0 ||
      definition.Collider.Width != 0.875f ||
      definition.Collider.Height != 0.875f ||
      !definition.Hostile ||
      definition.Friendly ||
      definition.CollidesWithTiles ||
      !definition.IgnoreWater ||
      definition.MaximumPenetration != 1 ||
      definition.LifetimeTicks != 900)
  {
    throw new InvalidOperationException(
      "Type-658 aiStyle-128 definition did not match the source-backed hostile contract.");
  }

  Console.WriteLine("PASS: type-658 aiStyle-128 definition is source-backed");
}

static void VerifyProjectileType658StateTick()
{
  ProjectileBehaviorEffectSystem effects = new();
  ProjectileLifetimeComponent lifetime = new(900);
  ProjectileSoundDelayComponent soundDelay = new(0);
  ProjectileBehaviorComponent behavior = new(
    1,
    new ProjectileBehaviorState(0.0f, 0.0f, 0, 0));
  VelocityComponent velocity = new(4.0f, -2.0f);
  ProjectileDefinitionComponent definition = new(
    658,
    1,
    0,
    900,
    new ColliderComponent(0.875f, 0.875f),
    false,
    true,
    1,
    CollidesWithTiles: false,
    IgnoreWater: true,
    LegacyAiStyle: 128);

  if (!effects.ApplyType658Tick(
        ref lifetime,
        ref soundDelay,
        ref behavior,
        ref velocity,
        definition) ||
      behavior.State.LocalAi0 != 0.8f ||
      behavior.State.LocalAi1 != 1.0f ||
      velocity.X != 0.0f ||
      velocity.Y != 0.0f ||
      soundDelay.RemainingTicks != -1 ||
      lifetime.RemainingTicks != 900)
  {
    throw new InvalidOperationException(
      "Type-658 aiStyle-128 state did not initialize, stop velocity, and consume sound sentinel.");
  }

  for (int tick = 1; tick < 120; tick++)
  {
    effects.ApplyType658Tick(
      ref lifetime,
      ref soundDelay,
      ref behavior,
      ref velocity,
      definition);
  }

  if (behavior.State.LocalAi1 != 120.0f || lifetime.RemainingTicks != 0)
  {
    throw new InvalidOperationException(
      "Type-658 aiStyle-128 state did not terminate at localAI[1] 120.");
  }

  LocationComponent transform = new(20.1f, 30.9f);
  ProjectileDirectionComponent direction = new(-1);
  ProjectileBehaviorComponent placementBehavior = new(
    1,
    new ProjectileBehaviorState(0.0f, 0.0f, 0, 0));
  if (!effects.ApplyType658TileCenterSnap(
        ref transform,
        ref direction,
        ref placementBehavior,
        new ColliderComponent(0.875f, 0.875f),
        definition) ||
      transform.X != 20.0625f || transform.Y != 31.0625f ||
      direction.Horizontal != 1 || placementBehavior.State.LocalAi0 != 0.8f)
  {
    throw new InvalidOperationException(
      "Type-658 aiStyle-128 tile-center snap did not preserve top-left and direction semantics.");
  }

  Console.WriteLine("PASS: type-658 aiStyle-128 tile-center snap is source-backed");

  Console.WriteLine("PASS: type-658 aiStyle-128 state tick is source-backed");
}

static void VerifyProjectileType658RuntimeState()
{
  using DomeSimulation simulation = new(new WorldGrid(400, 300));
  PlayerHandle owner = simulation.CreatePlayer(new SimulationVector(4.0f, 4.0f));
  simulation.QueueProjectileSpawn(new SpawnProjectileCommand(
    owner,
    20.1f,
    30.9f,
    -1,
    0,
    900,
    ProjectileType: 658));
  // Spawn commands commit after projectile movement; the next tick is the first AI tick.
  simulation.Tick(new SimulationInputBatch());
  simulation.Tick(new SimulationInputBatch());

  ProjectileReplicationSnapshot snapshot =
    simulation.CreateProjectileReplicationSnapshots().Single();
  if (snapshot.ProjectileType != 658 || snapshot.IsActive == false ||
      snapshot.Hostile == false || snapshot.Friendly || snapshot.LegacyAiStyle != 128 ||
      snapshot.CollidesWithTiles || !snapshot.IgnoreWater || snapshot.LocalAi0 != 0.8f ||
      snapshot.LocalAi1 != 1.0f || snapshot.Velocity != new SimulationVector(0.0f, 0.0f) ||
      snapshot.SoundDelay != -1 || snapshot.RemainingLifetime != 899 ||
      snapshot.Position != new SimulationVector(16.0625f, 31.0625f) ||
      snapshot.Direction != 1)
  {
    throw new InvalidOperationException(
      "Type-658 aiStyle-128 runtime state was not projected after an authoritative tick.");
  }

  ProjectileDefinition npcDefinition = ProjectileDefinitionRegistry.CreateDefault().Definitions[658] with
  {
    PlayerDamagePolicy = PlayerDamagePolicy.HostileNonPvp
  };
  using DomeSimulation npcSimulation = new(new WorldGrid(400, 300));
  NpcHandle npcOwner = npcSimulation.CreateNpc(new SimulationVector(4.0f, 4.0f));
  npcSimulation.CreateNpcProjectile(new NpcProjectileSpawnRequest(
    npcOwner,
    Sequence: 0,
    npcDefinition,
    Damage: 1,
    Position: new SimulationVector(20.1f, 30.9f),
    Velocity: new SimulationVector(-4.0f, 0.0f)));
  npcSimulation.Tick(new SimulationInputBatch());
  NpcProjectileReplicationSnapshot npcSnapshot =
    npcSimulation.CreateNpcProjectileReplicationSnapshots().Single();
  if (npcSnapshot.ProjectileType != 658 || !npcSnapshot.IsActive ||
      npcSnapshot.LocalAi0 != 0.8f || npcSnapshot.LocalAi1 != 1.0f ||
      npcSnapshot.Velocity != new SimulationVector(0.0f, 0.0f) ||
      npcSnapshot.Position != new SimulationVector(16.0625f, 31.0625f) ||
      npcSnapshot.Direction != 1)
  {
    throw new InvalidOperationException(
      "Type-658 aiStyle-128 NPC projectile did not preserve tile-center snap semantics.");
  }

  Console.WriteLine("PASS: type-658 aiStyle-128 runtime state reaches the snapshot");
}

static void VerifyProjectileType658ChildSpawn()
{
  ProjectileBehaviorEffectSystem effects = new();
  ProjectileDefinitionComponent definition = new(
    658,
    1,
    0,
    900,
    new ColliderComponent(0.875f, 0.875f),
    false,
    true,
    1,
    CollidesWithTiles: false,
    IgnoreWater: true,
    LegacyAiStyle: 128);
  ProjectileBehaviorComponent behavior = new(
    1,
    new ProjectileBehaviorState(0.8f, 0.0f, 0, 0, LocalAi1: 59.0f));
  if (effects.ShouldSpawnType658Child(definition, behavior))
  {
    throw new InvalidOperationException(
      "Type-658 child spawn became eligible before localAI[1] reached 60.");
  }

  behavior.State = behavior.State with { LocalAi1 = 60.0f };
  if (!effects.ShouldSpawnType658Child(definition, behavior))
  {
    throw new InvalidOperationException(
      "Type-658 child spawn was not admitted at the source localAI[1] boundary.");
  }

  behavior.State = behavior.State with { LocalAi1 = 61.0f };
  if (effects.ShouldSpawnType658Child(definition, behavior))
  {
    throw new InvalidOperationException(
      "Type-658 child spawn was not one-shot after the localAI[1] boundary.");
  }

  using DomeSimulation simulation = new(new WorldGrid(400, 300));
  PlayerHandle owner = simulation.CreatePlayer(new SimulationVector(4.0f, 4.0f));
  simulation.QueueProjectileSpawn(new SpawnProjectileCommand(
    owner,
    20.1f,
    30.9f,
    -1,
    0,
    900,
    ProjectileType: 658));
  simulation.Tick(new SimulationInputBatch());
  for (int tick = 0; tick < 59; tick++)
  {
    simulation.Tick(new SimulationInputBatch());
  }

  if (simulation.CreateProjectileReplicationSnapshots().Any(
        snapshot => snapshot.ProjectileType == 657))
  {
    throw new InvalidOperationException(
      "Type-658 child projectile spawned before the source tick 60 boundary.");
  }

  simulation.Tick(new SimulationInputBatch());
  ProjectileReplicationSnapshot[] snapshots = simulation
    .CreateProjectileReplicationSnapshots()
    .ToArray();
  ProjectileReplicationSnapshot parent = snapshots.Single(
    snapshot => snapshot.ProjectileType == 658);
  ProjectileReplicationSnapshot[] children = snapshots
    .Where(snapshot => snapshot.ProjectileType == 657)
    .ToArray();
  if (children.Length != 1)
  {
    throw new InvalidOperationException(
      "Type-658 child spawn did not commit exactly one type-657 projectile.");
  }

  ProjectileReplicationSnapshot child = children[0];
  SimulationVector expectedCenter = new(
    parent.Position.X + 0.4375f,
    parent.Position.Y + 0.4375f);
  if (!parent.IsActive || parent.LocalAi1 != 60.0f || child.Owner != owner ||
      child.Identity == parent.Identity || child.Identity <= 0 || child.Damage != 30 ||
      child.DefinitionKnockback != 3.0f || child.Velocity != new SimulationVector(0.0f, 0.0f) ||
      child.Position != expectedCenter || child.Direction != 1 || child.RemainingLifetime != 1200 ||
      !child.Hostile || child.Friendly || child.LegacyAiStyle != 127)
  {
    throw new InvalidOperationException(
      "Type-658 child spawn did not preserve the source-backed type-657 command contract.");
  }

  simulation.Tick(new SimulationInputBatch());
  if (simulation.CreateProjectileReplicationSnapshots().Count(
        snapshot => snapshot.ProjectileType == 657) != 1)
  {
    throw new InvalidOperationException(
      "Type-658 child spawn was repeated after the source one-shot boundary.");
  }

  Console.WriteLine("PASS: type-658 aiStyle-128 child spawn is source-backed");
}

static void VerifyProjectileType697LocalAiHitState()
{
  ProjectileBehaviorEffectSystem effects = new();
  ProjectileBehaviorComponent behavior = new(
    1,
    new ProjectileBehaviorState(42.0f, 0.0f, 0, 0));
  ProjectileDefinitionComponent type697 = new(
    697,
    1,
    10,
    30,
    new ColliderComponent(1.0f, 1.0f),
    true,
    false,
    1);
  if (!effects.ApplyAcceptedHit(ref behavior, type697) ||
      behavior.State.LocalAi1 != 1.0f)
  {
    throw new InvalidOperationException(
      "Type-697 localAI hit state did not activate at the source-backed threshold.");
  }

  ProjectileBehaviorComponent belowThreshold = new(
    1,
    new ProjectileBehaviorState(41.0f, 0.0f, 0, 0));
  if (effects.ApplyAcceptedHit(ref belowThreshold, type697) ||
      belowThreshold.State.LocalAi1 != 0.0f)
  {
    throw new InvalidOperationException(
      "Type-697 localAI hit state activated below the source-backed threshold.");
  }

  Console.WriteLine("PASS: type-697 localAI hit state is threshold-bounded");
}

static void VerifyProjectileType607FriendlyTransition()
{
  ProjectileDefinitionRegistry registry = ProjectileDefinitionRegistry.CreateDefault();
  if (!registry.TryGet(607, out ProjectileDefinition definition) ||
      definition.BehaviorId != LegacyType607ProjectileBehavior.Id ||
      definition.LegacyAiStyle != 116 ||
      !definition.Friendly ||
      definition.Hostile ||
      definition.CollidesWithTiles ||
      !definition.IgnoreWater ||
      definition.LifetimeTicks != 600 ||
      definition.MaximumPenetration != -1)
  {
    throw new InvalidOperationException(
      "Type-607 registry defaults did not preserve the source-backed runtime contract.");
  }

  ProjectileBehaviorEffectSystem effects = new();
  ProjectileBehaviorComponent behavior = new(
    LegacyType607ProjectileBehavior.Id,
    new ProjectileBehaviorState(0.0f, 0.0f, 0, 0));
  LocationComponent anchoredTransform = new(0.0f, 0.0f);
  VelocityComponent anchoredVelocity = new(1.0f, 0.0f);
  if (!new ProjectileOwnerAnchoredMeleeSystem().TryAdvance(
        behavior,
        ref anchoredTransform,
        ref anchoredVelocity,
        new LocationComponent(10.0f, 20.0f),
        new VelocityComponent(2.0f, -3.0f)))
  {
    throw new InvalidOperationException("Type-607 owner-anchor movement was rejected.");
  }

  if (anchoredVelocity.X != 98.0f || anchoredVelocity.Y != 3.0f ||
      anchoredTransform.X != -88.0f || anchoredTransform.Y != 17.0f)
  {
    throw new InvalidOperationException(
      $"Type-607 owner-anchor movement mismatch: transform={anchoredTransform}, " +
      $"velocity={anchoredVelocity}.");
  }
  ProjectileFriendlyStateComponent friendlyState = new(true);
  ProjectileNetworkUpdateComponent networkUpdate = new();
  ProjectileDefinitionComponent type607 = new(
    607,
    1,
    10,
    600,
    new ColliderComponent(0.625f, 0.625f),
    true,
    false,
    -1);
  ProjectileTargetEligibilitySystem eligibility = new();
  if (!eligibility.CanDamageNpc(type607, friendlyState.IsFriendly))
  {
    throw new InvalidOperationException(
      "Type-607 friendly projectile was not eligible before its accepted hit.");
  }

  ProjectileDefinitionComponent hostileDefinition = new(
    300,
    8,
    10,
    600,
    new ColliderComponent(1.0f, 1.0f),
    false,
    true);
  if (eligibility.CanDamageNpc(hostileDefinition, runtimeFriendly: true))
  {
    throw new InvalidOperationException(
      "Hostile projectile became NPC-eligible through forged runtime friendly state.");
  }

  if (!effects.ApplyAcceptedHit(
        ref behavior,
        ref friendlyState,
        ref networkUpdate,
        type607) ||
      friendlyState.IsFriendly ||
      behavior.State.Primary != 1.0f ||
      !networkUpdate.PrimaryUpdatePending)
  {
    throw new InvalidOperationException(
      "Type-607 accepted hit did not disable friendly state, set ai[0], and request net update.");
  }

  if (eligibility.CanDamageNpc(type607, friendlyState.IsFriendly))
  {
    throw new InvalidOperationException(
      "Type-607 projectile remained NPC-eligible after the accepted hit transition.");
  }

  ProjectileDefinitionRegistry simulationDefinitions = new(
    ProjectileDefinitionRegistry.CreateDefault().OrderedDefinitions.Select(projectileDefinition =>
      projectileDefinition.ProjectileType == 607
        ? projectileDefinition with { Damage = 10 }
        : projectileDefinition));
  using DomeSimulation simulation = new(new WorldGrid(400, 300), simulationDefinitions);
  PlayerHandle owner = simulation.CreatePlayer(new SimulationVector(10.0f, 0.0f));
  _ = simulation.CreateNpc(new SimulationVector(14.0f, 0.0f));
  simulation.QueueProjectileSpawn(new SpawnProjectileCommand(
    owner,
    10.0f,
    0.0f,
    1,
    10,
    600,
    ProjectileType: 607,
    BehaviorId: LegacyType607ProjectileBehavior.Id,
    ProjectileSpeed: 0.0f));
  simulation.Tick(new SimulationInputBatch());
  simulation.Tick(new SimulationInputBatch());
  simulation.Tick(new SimulationInputBatch());
  bool runtimeFriendly = true;
  float runtimePrimary = 0.0f;
  simulation.World.Query(
    new QueryDescription().WithAll<ProjectileFriendlyStateComponent, ProjectileBehaviorComponent>(),
    (ref ProjectileFriendlyStateComponent state,
      ref ProjectileBehaviorComponent projectileBehavior) =>
    {
      runtimeFriendly = state.IsFriendly;
      runtimePrimary = projectileBehavior.State.Primary;
    });
  ProjectileReplicationSnapshot snapshot =
    simulation.CreateProjectileReplicationSnapshots().Single();
  NpcReplicationSnapshot targetSnapshot = simulation.CreateNpcReplicationSnapshots().Single();
  if (targetSnapshot.Health != 90 ||
      snapshot.Friendly || snapshot.Ai0 != 1.0f || !snapshot.PrimaryUpdatePending)
  {
    throw new InvalidOperationException(
      $"Type-607 simulation hit state mismatch: health={targetSnapshot.Health}, " +
      $"type={snapshot.ProjectileType}, friendly={snapshot.Friendly}, ai0={snapshot.Ai0}, " +
      $"primaryUpdate={snapshot.PrimaryUpdatePending}, runtimeFriendly={runtimeFriendly}, " +
      $"runtimePrimary={runtimePrimary}.");
  }

  using DomeSimulation npcSimulation = new(new WorldGrid(400, 300), simulationDefinitions);
  NpcHandle npcOwner = npcSimulation.CreateNpc(new SimulationVector(10.0f, 0.0f));
  ProjectileDefinition npcDefinition = simulationDefinitions.Definitions.Values.Single(
    projectileDefinition => projectileDefinition.ProjectileType == 300);
  NpcProjectileReplicationSnapshot npcProjectile = npcSimulation.CreateNpcProjectile(
    new NpcProjectileSpawnRequest(
      npcOwner,
      Sequence: 0,
      npcDefinition,
      Damage: 10,
      Position: new SimulationVector(10.0f, 0.0f),
      Velocity: new SimulationVector(1.0f, 0.0f),
      BannerIdToRespondTo: 0));
  Entity npcProjectileEntity = default;
  npcSimulation.World.Query(
    new QueryDescription().WithAll<NpcProjectileOwnerComponent, ProjectileFriendlyStateComponent>(),
    (Entity entity) => npcProjectileEntity = entity);
  if (npcProjectileEntity == default)
  {
    throw new InvalidOperationException("NPC type-607 projectile entity was not materialized.");
  }

  npcSimulation.World.Get<ProjectileFriendlyStateComponent>(npcProjectileEntity).IsFriendly = true;
  npcSimulation.Tick(new SimulationInputBatch());
  NpcProjectileReplicationSnapshot friendlyNpcProjectile =
    npcSimulation.CreateNpcProjectileReplicationSnapshots().Single();
  if (friendlyNpcProjectile.ReplicationId != npcProjectile.ReplicationId ||
      !friendlyNpcProjectile.Friendly)
  {
    throw new InvalidOperationException(
      "NPC projectile replication refresh did not publish runtime friendly state.");
  }

  npcSimulation.World.Get<ProjectileFriendlyStateComponent>(npcProjectileEntity).IsFriendly = false;
  npcSimulation.Tick(new SimulationInputBatch());
  NpcProjectileReplicationSnapshot refreshedNpcProjectile =
    npcSimulation.CreateNpcProjectileReplicationSnapshots().Single();
  if (refreshedNpcProjectile.ReplicationId != npcProjectile.ReplicationId ||
      refreshedNpcProjectile.Friendly)
  {
    throw new InvalidOperationException(
      "NPC projectile replication refresh did not preserve runtime friendly state.");
  }

  Console.WriteLine("PASS: type-607 friendly transition is runtime-authoritative");
}

static void VerifyProjectileAcceptedHitDamageDecay()
{
  ProjectileDefinitionRegistry registry = ProjectileDefinitionRegistry.CreateDefault();
  if (!registry.TryGet(638, out ProjectileDefinition type638) ||
      !registry.TryGet(639, out ProjectileDefinition type639) ||
      !registry.TryGet(640, out ProjectileDefinition type640Definition) ||
      !registry.TryGet(706, out ProjectileDefinition type706) ||
      !registry.TryGet(357, out ProjectileDefinition type357) ||
      !registry.TryGet(451, out ProjectileDefinition type451Definition) ||
      !registry.TryGet(645, out ProjectileDefinition type645Definition) ||
      !registry.TryGet(656, out ProjectileDefinition type656Definition) ||
      !registry.TryGet(876, out ProjectileDefinition type876Definition) ||
      !registry.TryGet(864, out ProjectileDefinition type864Definition) ||
      !registry.TryGet(866, out ProjectileDefinition type866Definition) ||
      type638.LegacyAiStyle != 1 || type638.ExtraUpdates != 5 ||
      type638.Collider.Width != 0.25f || type638.LifetimeTicks != 600 ||
      type639.LegacyAiStyle != 1 || type639.ExtraUpdates != 1 ||
      type639.Collider.Width != 0.625f || type639.LifetimeTicks != 90 ||
      !type639.IsArrow || type640Definition.LegacyAiStyle != 1 ||
      type640Definition.ExtraUpdates != 2 || type640Definition.LifetimeTicks != 90 ||
      type640Definition.Collider.Width != 0.625f || type706.LegacyAiStyle != 1 ||
      type706.Collider.Width != 4.125f || type706.Collider.Height != 4.125f ||
      type706.LifetimeTicks != 300 || type706.MaximumPenetration != -1 ||
      type706.LocalNpcHitCooldownTicks != 10 || !type706.UsesLocalNpcImmunity ||
      type706.DamageClass != ProjectileDamageClass.Ranged || type357.LegacyAiStyle != 1 ||
      type357.Collider.Width != 0.25f || type357.Collider.Height != 0.25f ||
      type357.LifetimeTicks != 600 || type357.MaximumPenetration != 6 ||
      type357.ExtraUpdates != 2 || type357.Scale != 1.2f ||
      type357.DamageClass != ProjectileDamageClass.Ranged || !type357.Friendly ||
      type451Definition.LegacyAiStyle != 81 || type451Definition.Collider.Width != 1.0f ||
      type451Definition.LifetimeTicks != 3600 || type451Definition.MaximumPenetration != 3 ||
      type451Definition.DamageClass != ProjectileDamageClass.Melee || !type451Definition.Friendly ||
      type645Definition.LegacyAiStyle != 1 || type645Definition.Collider.Width != 0.625f ||
      type645Definition.Collider.Height != 0.625f || type645Definition.LifetimeTicks != 3600 ||
      type645Definition.MaximumPenetration != -1 || type645Definition.ExtraUpdates != 5 ||
      type645Definition.DamageClass != ProjectileDamageClass.Magic || !type645Definition.Friendly ||
      type645Definition.CollidesWithTiles || !type645Definition.UsesLocalNpcImmunity ||
      type645Definition.LocalNpcHitCooldownTicks != -1 ||
      type656Definition.LegacyAiStyle != 127 || type656Definition.Collider.Width != 0.625f ||
      type656Definition.Collider.Height != 0.625f || type656Definition.LifetimeTicks != 1200 ||
      type656Definition.MaximumPenetration != -1 || type656Definition.DamageClass != ProjectileDamageClass.Magic ||
      !type656Definition.Friendly || type656Definition.Hostile || type656Definition.CollidesWithTiles ||
      !type656Definition.UsesLocalNpcImmunity || type656Definition.LocalNpcHitCooldownTicks != 8 ||
      type876Definition.LegacyAiStyle != 1 || type876Definition.Collider.Width != 0.25f ||
      type876Definition.Collider.Height != 0.25f || type876Definition.LifetimeTicks != 3600 ||
      type876Definition.MaximumPenetration != 8 || type876Definition.ExtraUpdates != 3 ||
      type876Definition.Scale != 1.4f || type876Definition.DamageClass != ProjectileDamageClass.Magic ||
      !type876Definition.Friendly || type876Definition.Hostile ||
      type864Definition.Collider.Width != 0.625f || type864Definition.Collider.Height != 0.625f ||
      type864Definition.LifetimeTicks != 60 || type864Definition.MaximumPenetration != -1 ||
      type864Definition.LegacyAiStyle != 169 || type864Definition.ArmorPenetration != 25 ||
      !type864Definition.Friendly || !type864Definition.IsMinion ||
      type864Definition.MinionSlots != 1.0f || !type864Definition.IgnoreWater ||
      !type864Definition.UsesLocalNpcImmunity || type864Definition.LocalNpcHitCooldownTicks != 10)
  {
    throw new InvalidOperationException(
      "Type-638/639/640 registry defaults did not preserve source-backed metadata.");
  }

  ProjectileBehaviorEffectSystem effects = new();
  ProjectileDefinitionComponent type640 = new(
    640,
    1,
    10,
    90,
    new ColliderComponent(0.625f, 0.625f),
    true,
    false,
    4);
  foreach (int projectileType in new[] { 638, 639, 640 })
  {
    ProjectileDamageComponent damage = new(10);
    effects.ApplyAcceptedHitDamage(
      ref damage,
      type640 with { ProjectileType = projectileType });
    if (damage.Amount != 9)
    {
      throw new InvalidOperationException(
        $"Type-{projectileType} accepted-hit damage did not apply Oracle decay: {damage.Amount}.");
    }
  }

  ProjectileDamageComponent type706Damage = new(10);
  effects.ApplyAcceptedHitDamage(
    ref type706Damage,
    type640 with { ProjectileType = 706 });
  if (type706Damage.Amount != 9)
  {
    throw new InvalidOperationException(
      $"Type-706 accepted-hit damage did not apply Oracle decay: {type706Damage.Amount}.");
  }

  ProjectileDamageComponent type357Damage = new(10);
  effects.ApplyAcceptedHitDamage(
    ref type357Damage,
    type640 with { ProjectileType = 357 });
  if (type357Damage.Amount != 8)
  {
    throw new InvalidOperationException(
      $"Type-357 accepted-hit damage did not apply Oracle decay: {type357Damage.Amount}.");
  }

  ProjectileDamageComponent type876Damage = new(10);
  ProjectileDefinitionComponent type876HitDefinition = type640 with
  {
    ProjectileType = 876,
    Knockback = 10.0f
  };
  VelocityComponent type876Velocity = new(10.0f, -5.0f);
  ProjectileBehaviorComponent type876Behavior = new(
    1,
    new ProjectileBehaviorState(3.0f, 4.0f, 0, 0));
  ProjectileTileCollisionComponent type876TileCollision = new(true);
  ProjectileNetworkUpdateComponent type876TileNetwork = new();
  effects.ApplyAcceptedHitDamage(
    ref type876Damage,
    type876HitDefinition);
  effects.ApplyAcceptedHitKnockback(ref type876HitDefinition);
  effects.ApplyAcceptedHitVelocity(ref type876Velocity, type876HitDefinition);
  effects.ApplyAcceptedHitTileCollision(
    ref type876Behavior,
    ref type876TileCollision,
    ref type876TileNetwork,
    type876HitDefinition);
  if (type876Damage.Amount != 9 || type876HitDefinition.Knockback != 9.0f ||
      type876Velocity.X != 6.0f || type876Velocity.Y != -3.0f)
  {
    throw new InvalidOperationException(
      $"Type-876 accepted-hit decay mismatch: damage={type876Damage.Amount}, " +
      $"knockback={type876HitDefinition.Knockback}, velocity={type876Velocity}.");
  }
  if (!type876TileCollision.Enabled || type876Behavior.State.Secondary != 0.0f ||
      !type876TileNetwork.PrimaryUpdatePending)
  {
    throw new InvalidOperationException(
      "Type-876 accepted-hit tile-collision state did not preserve enabled state.");
  }

  type876TileCollision = new ProjectileTileCollisionComponent(false);
  type876TileNetwork = new();
  effects.ApplyAcceptedHitTileCollision(
    ref type876Behavior,
    ref type876TileCollision,
    ref type876TileNetwork,
    type876HitDefinition);
  if (type876TileCollision.Enabled || type876Behavior.State.Secondary != 1.0f ||
      !type876TileNetwork.PrimaryUpdatePending)
  {
    throw new InvalidOperationException(
      "Type-876 accepted-hit tile-collision state did not preserve disabled state.");
  }

  ProjectileBehaviorComponent type645HitBehavior = new(
    1,
    new ProjectileBehaviorState(4.0f, 2.0f, 0, 0));
  ProjectileFriendlyStateComponent type645HitFriendly = new(true);
  ProjectileNetworkUpdateComponent type645HitNetwork = new();
  if (!effects.ApplyAcceptedHit(
        ref type645HitBehavior,
        ref type645HitFriendly,
        ref type645HitNetwork,
        type640 with { ProjectileType = 645 },
        1) ||
      type645HitBehavior.State.Primary != 0.0f ||
      type645HitBehavior.State.Secondary != -1.0f ||
      !type645HitNetwork.PrimaryUpdatePending)
  {
    throw new InvalidOperationException(
      "Type-645 accepted hit did not reset its source-backed AI state.");
  }

  type645HitNetwork = new();
  if (effects.ApplyAcceptedHit(
        ref type645HitBehavior,
        ref type645HitFriendly,
        ref type645HitNetwork,
        type640 with { ProjectileType = 645 },
        1) ||
      type645HitNetwork.PrimaryUpdatePending)
  {
    throw new InvalidOperationException(
      "Type-645 repeated accepted hit unexpectedly rewrote its terminal AI state.");
  }

  ProjectileDefinitionComponent type451 = new(
    451,
    1,
    10,
    600,
    new ColliderComponent(1.0f, 1.0f),
    true,
    false,
    3,
    DamageClass: ProjectileDamageClass.Melee,
    LegacyAiStyle: 81);
  ProjectileBehaviorComponent type451Behavior = new(
    1,
    new ProjectileBehaviorState(0.0f, 4.0f, 0, 0));
  ProjectileFriendlyStateComponent type451Friendly = new(true);
  ProjectileNetworkUpdateComponent type451Network = new();
  if (!effects.ApplyAcceptedHit(
        ref type451Behavior,
        ref type451Friendly,
        ref type451Network,
        type451,
        3) ||
      type451Behavior.State.Primary != 3.0f || type451Behavior.State.Secondary != 0.0f ||
      !type451Network.PrimaryUpdatePending)
  {
    throw new InvalidOperationException(
      "Type-451 first accepted hit did not publish the source-backed AI state transition.");
  }

  type451Network = new();
  if (!effects.ApplyAcceptedHit(
        ref type451Behavior,
        ref type451Friendly,
        ref type451Network,
        type451,
        2) ||
      type451Behavior.State.Primary != 0.0f || type451Behavior.State.Secondary != 0.0f ||
      !type451Network.PrimaryUpdatePending)
  {
    throw new InvalidOperationException(
      "Type-451 repeated accepted hit did not publish the source-backed AI state transition.");
  }

  VelocityComponent type451Velocity = new(4.0f, -2.0f);
  type451Network = new();
  if (!effects.ApplyTileCollisionBehavior(
        ref type451Behavior,
        ref type451Velocity,
        ref type451Network,
        type451) ||
      type451Behavior.State.Primary != 1.0f || type451Behavior.State.Secondary != 0.0f ||
      type451Velocity.X != 2.0f || type451Velocity.Y != -1.0f ||
      !type451Network.PrimaryUpdatePending)
  {
    throw new InvalidOperationException(
      "Type-451 tile collision did not publish the source-backed AI/velocity transition.");
  }

  ProjectileDefinitionComponent type645 = new(
    645,
    1,
    10,
    3600,
    new ColliderComponent(0.625f, 0.625f),
    true,
    false,
    -1,
    ExtraUpdates: 5,
    DamageClass: ProjectileDamageClass.Magic,
    UsesLocalNpcImmunity: true,
    LocalNpcHitCooldownTicks: -1,
    CollidesWithTiles: false,
    LegacyAiStyle: 1);
  ProjectileBehaviorComponent type645Behavior = new(
    1,
    new ProjectileBehaviorState(4.0f, 2.0f, 0, 0));
  VelocityComponent type645Velocity = new(3.0f, -1.0f);
  ProjectileNetworkUpdateComponent type645Network = new();
  if (!effects.ApplyTileCollisionBehavior(
        ref type645Behavior,
        ref type645Velocity,
        ref type645Network,
        type645) ||
      type645Behavior.State.Primary != 0.0f || type645Behavior.State.Secondary != -1.0f ||
      type645Velocity.X != 3.0f || type645Velocity.Y != -1.0f ||
      !type645Network.PrimaryUpdatePending)
  {
    throw new InvalidOperationException(
      "Type-645 tile collision did not publish the source-backed AI state transition.");
  }

  ProjectileDefinitionComponent type864 = type640 with
  {
    ProjectileType = 864,
    MaximumBounces = 0,
    ExtraUpdates = 0,
    LegacyAiStyle = 169,
    ArmorPenetration = 25,
    UsesLocalNpcImmunity = true,
    LocalNpcHitCooldownTicks = 10
  };
  ProjectileBehaviorComponent type864Behavior = new(
    1,
    new ProjectileBehaviorState(2.0f, 6.0f, 0, 0));
  ProjectileFriendlyStateComponent type864Friendly = new(true);
  ProjectileNetworkUpdateComponent type864Network = new();
  if (!effects.ApplyAcceptedHit(
        ref type864Behavior,
        ref type864Friendly,
        ref type864Network,
        type864,
        1) ||
      type864Behavior.State.Primary != -1.0f || type864Behavior.State.Secondary != 0.0f ||
      !type864Network.PrimaryUpdatePending)
  {
    throw new InvalidOperationException(
      "Type-864 accepted hit did not reset its source-backed AI state.");
  }

  if (type866Definition.Collider.Width != 1.875f || type866Definition.LifetimeTicks != 3600 ||
      type866Definition.MaximumPenetration != 5 || type866Definition.ExtraUpdates != 1 ||
      type866Definition.LegacyAiStyle != 3 || type866Definition.DamageClass != ProjectileDamageClass.Melee ||
      !type866Definition.Friendly || !type866Definition.UsesLocalNpcImmunity ||
      type866Definition.LocalNpcHitCooldownTicks != -1)
  {
    throw new InvalidOperationException(
      "Type-866 registry defaults did not preserve source-backed metadata.");
  }

  ProjectileBehaviorComponent type866Behavior = new(
    1,
    new ProjectileBehaviorState(0.0f, 2.0f, 0, 0));
  ProjectileFriendlyStateComponent type866Friendly = new(true);
  ProjectileNetworkUpdateComponent type866Network = new();
  ProjectilePenetrationComponent type866Penetration = new(5) { RemainingPenetration = 0 };
  ProjectileDamageComponent type866Damage = new(10);
  if (!effects.ApplyAcceptedHitPenetration(
        ref type866Behavior,
        ref type866Friendly,
        ref type866Network,
        ref type866Penetration,
        ref type866Damage,
        type640 with { ProjectileType = 866 }) ||
      type866Penetration.RemainingPenetration != 1 || type866Damage.Amount != 0 ||
      type866Behavior.State.Secondary != -1.0f || !type866Network.PrimaryUpdatePending)
  {
    throw new InvalidOperationException(
      "Type-866 accepted hit did not preserve its non-tombstone state transition.");
  }

  ProjectileDamageComponent type357TileDamage = new(10);
  ProjectilePenetrationComponent type357Penetration = new(6);
  if (!effects.ApplyTileCollisionDamage(
        ref type357TileDamage,
        ref type357Penetration,
        type640 with
        {
          ProjectileType = 357,
          Collider = new ColliderComponent(0.25f, 0.25f),
          ExtraUpdates = 2,
          Scale = 1.2f
        }) ||
      type357TileDamage.Amount != 9 || type357Penetration.RemainingPenetration != 5)
  {
    throw new InvalidOperationException(
      $"Type-357 tile-collision damage boundary mismatch: " +
      $"damage={type357TileDamage.Amount}, penetration={type357Penetration.RemainingPenetration}.");
  }

  ProjectileDamageComponent unrelatedTileDamage = new(10);
  ProjectilePenetrationComponent unrelatedPenetration = new(6);
  if (effects.ApplyTileCollisionDamage(
        ref unrelatedTileDamage,
        ref unrelatedPenetration,
        type640 with { ProjectileType = 640 }) ||
      unrelatedTileDamage.Amount != 10 || unrelatedPenetration.RemainingPenetration != 6)
  {
    throw new InvalidOperationException(
      "Type-357 tile-collision damage leaked to an unrelated projectile type.");
  }

  WorldGrid type357TileWorld = new(400, 300);
  _ = type357TileWorld.TrySetTile(14, 0, new WorldTile(IsActive: true, Type: 1));
  ProjectileDefinitionRegistry type357TileRegistry = new([
    type357 with
    {
      Damage = 10,
      Collider = new ColliderComponent(0.25f, 0.25f),
      ExtraUpdates = 0,
      Scale = 1.0f
    }]);
  using DomeSimulation type357TileSimulation =
    new(type357TileWorld, type357TileRegistry);
  PlayerHandle type357TileOwner =
    type357TileSimulation.CreatePlayer(new SimulationVector(10.0f, 0.0f));
  type357TileSimulation.QueueProjectileSpawn(new SpawnProjectileCommand(
    type357TileOwner,
    10.0f,
    0.0f,
    1,
    10,
    600,
    ProjectileType: 357,
    BehaviorId: 1,
    ProjectileSpeed: 4.0f));
  type357TileSimulation.Tick(new SimulationInputBatch());
  type357TileSimulation.Tick(new SimulationInputBatch());
  ProjectileReplicationSnapshot type357Bounced =
    type357TileSimulation.CreateProjectileReplicationSnapshots().Single();
  if (!type357Bounced.IsActive || !type357Bounced.Reflected ||
      type357Bounced.Velocity.X >= 0.0f || type357Bounced.Damage != 9)
  {
    throw new InvalidOperationException(
      $"Type-357 tile collision commit mismatch: active={type357Bounced.IsActive}, " +
      $"reflected={type357Bounced.Reflected}, velocity={type357Bounced.Velocity.X}, " +
      $"damage={type357Bounced.Damage}.");
  }

  ProjectileDamageComponent unchanged = new(10);
  ProjectileDefinitionComponent type607 = type640 with { ProjectileType = 607 };
  effects.ApplyAcceptedHitDamage(ref unchanged, type607);
  if (unchanged.Amount != 10)
  {
    throw new InvalidOperationException(
      "Accepted-hit damage decay leaked to an unrelated projectile type.");
  }

  ProjectileDefinitionRegistry simulationDefinitions = new([
    type640Definition with { Damage = 10, ExtraUpdates = 0 }]);
  using DomeSimulation simulation = new(new WorldGrid(400, 300), simulationDefinitions);
  PlayerHandle owner = simulation.CreatePlayer(new SimulationVector(0.0f, 0.0f));
  _ = simulation.CreateNpc(new SimulationVector(14.0f, 0.0f));
  _ = simulation.CreateNpc(new SimulationVector(18.0f, 0.0f));
  simulation.QueueProjectileSpawn(new SpawnProjectileCommand(
    owner,
    10.0f,
    0.0f,
    1,
    10,
    90,
    ProjectileType: 640,
    BehaviorId: 1,
    ProjectileSpeed: 4.0f));
  simulation.Tick(new SimulationInputBatch());
  simulation.Tick(new SimulationInputBatch());
  IReadOnlyList<NpcReplicationSnapshot> firstHit = simulation.CreateNpcReplicationSnapshots();
  if (firstHit.Count != 2 || firstHit[0].Health != 90 || firstHit[1].Health != 100)
  {
    throw new InvalidOperationException(
      $"Type-640 first accepted hit mismatch: " +
      string.Join(
        ",",
        firstHit.Select(snapshot => $"{snapshot.Health}@{snapshot.Position.X}")) +
      $" projectile=" +
      string.Join(",", simulation.CreateProjectileReplicationSnapshots().Select(
        snapshot => $"{snapshot.Position.X}/{snapshot.Damage}/{snapshot.Friendly}")));
  }

  simulation.Tick(new SimulationInputBatch());
  IReadOnlyList<NpcReplicationSnapshot> secondHit = simulation.CreateNpcReplicationSnapshots();
  if (secondHit[0].Health != 90 || secondHit[1].Health != 91)
  {
    throw new InvalidOperationException(
      $"Type-640 subsequent accepted hit mismatch: " +
      string.Join(",", secondHit.Select(snapshot => $"{snapshot.Health}@{snapshot.Position.X}")));
  }

  ProjectileDefinitionRegistry type706Registry = new([
    type706 with { Damage = 10, Collider = new ColliderComponent(0.625f, 0.625f) }]);
  using DomeSimulation type706Simulation = new(new WorldGrid(400, 300), type706Registry);
  PlayerHandle type706Owner = type706Simulation.CreatePlayer(new SimulationVector(0.0f, 0.0f));
  _ = type706Simulation.CreateNpc(new SimulationVector(14.0f, 0.0f));
  _ = type706Simulation.CreateNpc(new SimulationVector(18.0f, 0.0f));
  type706Simulation.QueueProjectileSpawn(new SpawnProjectileCommand(
    type706Owner,
    10.0f,
    0.0f,
    1,
    10,
    90,
    ProjectileType: 706,
    BehaviorId: 1,
    ProjectileSpeed: 4.0f));
  type706Simulation.Tick(new SimulationInputBatch());
  type706Simulation.Tick(new SimulationInputBatch());
  IReadOnlyList<NpcReplicationSnapshot> type706FirstHit =
    type706Simulation.CreateNpcReplicationSnapshots();
  if (type706FirstHit.Count != 2 || type706FirstHit[0].Health != 90 ||
      type706FirstHit[1].Health != 100)
  {
    throw new InvalidOperationException(
      $"Type-706 first accepted hit did not use the original damage: " +
      string.Join(",", type706FirstHit.Select(snapshot => snapshot.Health)));
  }

  type706Simulation.Tick(new SimulationInputBatch());
  IReadOnlyList<NpcReplicationSnapshot> type706SecondHit =
    type706Simulation.CreateNpcReplicationSnapshots();
  if (type706SecondHit[0].Health != 90 || type706SecondHit[1].Health != 91)
  {
    throw new InvalidOperationException(
      $"Type-706 subsequent accepted hit did not use the 0.95 damage decay: " +
      string.Join(",", type706SecondHit.Select(snapshot => snapshot.Health)));
  }

  ProjectileDefinitionRegistry type357Registry = new([
    type357 with
    {
      Damage = 10,
      Collider = new ColliderComponent(0.625f, 0.625f),
      ExtraUpdates = 0,
      Scale = 1.0f
    }]);
  using DomeSimulation type357Simulation = new(new WorldGrid(400, 300), type357Registry);
  PlayerHandle type357Owner = type357Simulation.CreatePlayer(new SimulationVector(0.0f, 0.0f));
  _ = type357Simulation.CreateNpc(new SimulationVector(14.0f, 0.0f));
  _ = type357Simulation.CreateNpc(new SimulationVector(18.0f, 0.0f));
  type357Simulation.QueueProjectileSpawn(new SpawnProjectileCommand(
    type357Owner,
    10.0f,
    0.0f,
    1,
    10,
    600,
    ProjectileType: 357,
    BehaviorId: 1,
    ProjectileSpeed: 4.0f));
  type357Simulation.Tick(new SimulationInputBatch());
  type357Simulation.Tick(new SimulationInputBatch());
  IReadOnlyList<NpcReplicationSnapshot> type357FirstHit =
    type357Simulation.CreateNpcReplicationSnapshots();
  if (type357FirstHit[0].Health != 90 || type357FirstHit[1].Health != 100)
  {
    throw new InvalidOperationException(
      $"Type-357 first accepted hit did not use the original damage: " +
      string.Join(",", type357FirstHit.Select(snapshot => snapshot.Health)));
  }

  type357Simulation.Tick(new SimulationInputBatch());
  IReadOnlyList<NpcReplicationSnapshot> type357SecondHit =
    type357Simulation.CreateNpcReplicationSnapshots();
  if (type357SecondHit[0].Health != 90 || type357SecondHit[1].Health != 92)
  {
    throw new InvalidOperationException(
      $"Type-357 subsequent accepted hit did not use the 0.8 damage decay: " +
      string.Join(",", type357SecondHit.Select(snapshot => snapshot.Health)));
  }

  Console.WriteLine("PASS: type-638/639/640 accepted-hit damage decay is source-bounded");
}

static void VerifyProjectileMinionSpawnSource()
{
  ProjectileMinionSpawnSourceComponent source = new(42);
  if (!source.IsEnabled || source.ItemType != 42 || source.ItemPrefix != 0)
  {
    throw new InvalidOperationException("Minion spawn source did not retain item provenance.");
  }

  ProjectileMinionSpawnSourceComponent empty = new();
  if (empty.IsEnabled || empty.ItemType != 0)
  {
    throw new InvalidOperationException("Default minion spawn source was not empty.");
  }

  bool rejectedInvalid = false;
  try
  {
    _ = new ProjectileMinionSpawnSourceComponent(0, -1);
  }
  catch (ArgumentOutOfRangeException)
  {
    rejectedInvalid = true;
  }

  if (!rejectedInvalid)
  {
    throw new InvalidOperationException("Invalid minion spawn source prefix was accepted.");
  }

  ProjectileDefinition definition = new(
    640,
    1,
    10,
    60,
    new ColliderComponent(1.0f, 1.0f),
    true,
    false,
    -1,
    IsMinion: true,
    MinionSlots: 1.0f);
  using World world = World.Create();
  Entity projectile = new ProjectileSpawnSystem().Spawn(
    world,
    new SpawnProjectileCommand(
      new PlayerHandle(1),
      0.0f,
      0.0f,
      1,
      10,
      60,
      ProjectileType: 640,
      MinionSpawnItemType: 42,
      MinionSpawnItemPrefix: 7),
    definition,
    1);
  if (!world.Has<ProjectileMinionSpawnSourceComponent>(projectile) ||
      world.Get<ProjectileMinionSpawnSourceComponent>(projectile).ItemType != 42 ||
      world.Get<ProjectileMinionSpawnSourceComponent>(projectile).ItemPrefix != 7)
  {
    throw new InvalidOperationException(
      "Minion projectile spawn did not materialize item provenance in ECS state.");
  }

  world.Set(
    projectile,
    new ProjectileNetworkUpdateComponent(
      secondaryUpdatePending: true,
      netSpam: 12,
      primaryUpdatePending: true));
  ProjectileReplicationSnapshot snapshot = new ProjectileReplicationSystem().Project(
    projectile,
    world,
    1,
    1,
    new WorldSectionCoordinates(0, 0));
  if (snapshot.MinionSpawnItemType != 42 || snapshot.MinionSpawnItemPrefix != 7 ||
      !snapshot.SecondaryUpdatePending || snapshot.NetSpam != 12 ||
      !snapshot.PrimaryUpdatePending)
  {
    throw new InvalidOperationException(
      "Minion projectile spawn provenance was not projected into the snapshot contract.");
  }

  Console.WriteLine("PASS: projectile minion spawn source is typed and fail-closed");
}

static void VerifyProjectileSlopeImpactAxis()
{
  WorldGrid world = new(400, 300);
  _ = world.TrySetTile(14, 0, new WorldTile(IsActive: true, Type: 1, Slope: 1));
  ProjectileCollisionSystem collision = new();
  if (!collision.TryGetSolidTileImpact(
        world,
        new LocationComponent(10.0f, 0.0f),
        new LocationComponent(14.0f, 0.0f),
        new ColliderComponent(0.5f, 0.5f),
        out bool reflectHorizontal,
        out bool reflectVertical) ||
      reflectHorizontal || !reflectVertical)
  {
    throw new InvalidOperationException(
      "A top-slope projectile impact did not select the vertical reflection axis.");
  }

  _ = world.TrySetTile(15, 0, new WorldTile(IsActive: true, Type: 1, Slope: 3));
  if (!collision.TryGetSolidTileImpact(
        world,
        new LocationComponent(10.0f, 0.0f),
        new LocationComponent(15.0f, 0.0f),
        new ColliderComponent(0.5f, 0.5f),
        out reflectHorizontal,
        out reflectVertical) ||
      !reflectHorizontal || reflectVertical)
  {
    throw new InvalidOperationException(
      "A bottom-slope projectile impact did not select the horizontal reflection axis.");
  }

  _ = world.TrySetTile(16, 0, new WorldTile(IsActive: true, Type: 1, IsHalfBrick: true));
  if (!collision.TryGetSolidTileImpact(
        world,
        new LocationComponent(10.0f, 0.0f),
        new LocationComponent(16.0f, 0.0f),
        new ColliderComponent(0.5f, 0.5f),
        out reflectHorizontal,
        out reflectVertical) ||
      reflectHorizontal || !reflectVertical)
  {
    throw new InvalidOperationException(
      "A half-brick projectile impact did not select the vertical reflection axis.");
  }
}

static void VerifyLegacyMeleeBuffRegistry()
{
  const int expectedBuffTypeCount = 389;
  FrozenSet<int> expectedMeleeBuffTypes = new HashSet<int>
  {
    71,
    73,
    74,
    75,
    76,
    77,
    78,
    79
  }.ToFrozenSet();
  IReadOnlySet<int> actualMeleeBuffTypes = LegacyMeleeBuffRegistry.RegisterDefaults();
  if (LegacyMeleeBuffRegistry.BuffTypeCount != expectedBuffTypeCount ||
      actualMeleeBuffTypes.Count != expectedMeleeBuffTypes.Count ||
      actualMeleeBuffTypes is not FrozenSet<int> ||
      !expectedMeleeBuffTypes.SetEquals(actualMeleeBuffTypes) ||
      !LegacyMeleeBuffRegistry.IsMeleeBuff(71) ||
      !LegacyMeleeBuffRegistry.IsMeleeBuff(79) ||
      LegacyMeleeBuffRegistry.IsMeleeBuff(70) ||
      LegacyMeleeBuffRegistry.IsMeleeBuff(72) ||
      LegacyMeleeBuffRegistry.IsMeleeBuff(0) ||
      LegacyMeleeBuffRegistry.IsMeleeBuff(-1) ||
      LegacyMeleeBuffRegistry.IsMeleeBuff(expectedBuffTypeCount - 1) ||
      LegacyMeleeBuffRegistry.IsMeleeBuff(expectedBuffTypeCount))
  {
    throw new InvalidOperationException(
      "Legacy melee buff defaults drifted from Version4 or lost default semantics.");
  }

  Console.WriteLine(
    "PASS: legacy melee buff defaults preserve exact immutable eight-entry set");
}

static void VerifyLegacyLightPetBuffRegistry()
{
  Type? registryType = typeof(DomeSimulation).Assembly.GetType(
    "Terraria.Dome.Simulation.StatusEffects.Definitions.LegacyLightPetBuffRegistry");
  if (registryType is null)
  {
    throw new InvalidOperationException(
      "Legacy light pet buff registry owner is missing from the Simulation assembly.");
  }

  const int expectedBuffTypeCount = 389;
  FrozenSet<int> expectedLightPetBuffTypes = new HashSet<int>
  {
    19,
    27,
    57,
    101,
    102,
    152,
    155,
    190,
    201,
    294,
    298,
    299
  }.ToFrozenSet();
  int actualBuffTypeCount = (int)(registryType.GetField("BuffTypeCount")?.GetValue(null) ?? -1);
  IReadOnlySet<int>? actualLightPetBuffTypes = registryType.GetMethod("RegisterDefaults")?.Invoke(
    null,
    null) as IReadOnlySet<int>;
  if (actualLightPetBuffTypes is null)
  {
    throw new InvalidOperationException(
      "Legacy light pet buff registry did not expose an immutable set.");
  }

  bool IsLightPetBuff(int buffType)
  {
    object? result = registryType.GetMethod("IsLightPetBuff")?.Invoke(
      null,
      new object[] { buffType });
    return result is true;
  }

  if (actualBuffTypeCount != expectedBuffTypeCount ||
      actualLightPetBuffTypes.Count != expectedLightPetBuffTypes.Count ||
      actualLightPetBuffTypes is not FrozenSet<int> ||
      !expectedLightPetBuffTypes.SetEquals(actualLightPetBuffTypes) ||
      !IsLightPetBuff(19) ||
      !IsLightPetBuff(299) ||
      IsLightPetBuff(18) ||
      IsLightPetBuff(20) ||
      IsLightPetBuff(0) ||
      IsLightPetBuff(-1) ||
      IsLightPetBuff(expectedBuffTypeCount - 1) ||
      IsLightPetBuff(expectedBuffTypeCount))
  {
    throw new InvalidOperationException(
      "Legacy light pet buff defaults drifted from Version4 or lost default semantics.");
  }

  Console.WriteLine(
    "PASS: legacy light pet buff defaults preserve exact immutable twelve-entry set");
}

static void VerifyLegacyPersistentBuffRegistry()
{
  Type? registryType = typeof(DomeSimulation).Assembly.GetType(
    "Terraria.Dome.Simulation.StatusEffects.Definitions.LegacyPersistentBuffRegistry");
  if (registryType is null)
  {
    throw new InvalidOperationException(
      "Legacy persistent buff registry owner is missing from the Simulation assembly.");
  }

  const int expectedBuffTypeCount = 389;
  FrozenSet<int> expectedPersistentBuffTypes = new HashSet<int>
  {
    71,
    73,
    74,
    75,
    76,
    77,
    78,
    79
  }.ToFrozenSet();
  int actualBuffTypeCount = (int)(registryType.GetField("BuffTypeCount")?.GetValue(null) ?? -1);
  IReadOnlySet<int>? actualPersistentBuffTypes = registryType.GetMethod("RegisterDefaults")?.Invoke(
    null,
    null) as IReadOnlySet<int>;
  if (actualPersistentBuffTypes is null)
  {
    throw new InvalidOperationException(
      "Legacy persistent buff registry did not expose an immutable set.");
  }

  bool IsPersistentBuff(int buffType)
  {
    object? result = registryType.GetMethod("IsPersistentBuff")?.Invoke(
      null,
      new object[] { buffType });
    return result is true;
  }

  if (actualBuffTypeCount != expectedBuffTypeCount ||
      actualPersistentBuffTypes.Count != expectedPersistentBuffTypes.Count ||
      actualPersistentBuffTypes is not FrozenSet<int> ||
      !expectedPersistentBuffTypes.SetEquals(actualPersistentBuffTypes) ||
      !IsPersistentBuff(71) ||
      !IsPersistentBuff(79) ||
      IsPersistentBuff(70) ||
      IsPersistentBuff(72) ||
      IsPersistentBuff(0) ||
      IsPersistentBuff(-1) ||
      IsPersistentBuff(expectedBuffTypeCount - 1) ||
      IsPersistentBuff(expectedBuffTypeCount))
  {
    throw new InvalidOperationException(
      "Legacy persistent buff defaults drifted from Version4 or lost default semantics.");
  }

  Console.WriteLine(
    "PASS: legacy persistent buff defaults preserve exact immutable eight-entry set");
}

static void VerifyLegacyDebuffRegistry()
{
  Type? registryType = typeof(DomeSimulation).Assembly.GetType(
    "Terraria.Dome.Simulation.StatusEffects.Definitions.LegacyDebuffRegistry");
  if (registryType is null)
  {
    throw new InvalidOperationException(
      "Legacy debuff registry owner is missing from the Simulation assembly.");
  }

  const int expectedBuffTypeCount = 389;
  FrozenSet<int> expectedDebuffTypes = new HashSet<int>
  {
    20,
    21,
    22,
    23,
    24,
    25,
    28,
    30,
    31,
    32,
    33,
    34,
    35,
    36,
    37,
    38,
    39,
    43,
    44,
    46,
    47,
    67,
    68,
    69,
    70,
    72,
    80,
    86,
    87,
    88,
    89,
    94,
    103,
    119,
    120,
    137,
    144,
    145,
    146,
    147,
    148,
    149,
    153,
    156,
    157,
    158,
    160,
    163,
    164,
    169,
    183,
    186,
    189,
    194,
    195,
    196,
    197,
    199,
    203,
    204,
    215,
    320,
    321,
    323,
    324,
    332,
    333,
    334,
    344,
    350,
    353
  }.ToFrozenSet();
  int actualBuffTypeCount = (int)(registryType.GetField("BuffTypeCount")?.GetValue(null) ?? -1);
  IReadOnlySet<int>? actualDebuffTypes = registryType.GetMethod("RegisterDefaults")?.Invoke(
    null,
    null) as IReadOnlySet<int>;
  if (actualDebuffTypes is null)
  {
    throw new InvalidOperationException(
      "Legacy debuff registry did not expose an immutable set.");
  }

  bool IsDebuff(int buffType)
  {
    object? result = registryType.GetMethod("IsDebuff")?.Invoke(
      null,
      new object[] { buffType });
    return result is true;
  }

  if (actualBuffTypeCount != expectedBuffTypeCount ||
      actualDebuffTypes.Count != expectedDebuffTypes.Count ||
      actualDebuffTypes is not FrozenSet<int> ||
      !expectedDebuffTypes.SetEquals(actualDebuffTypes) ||
      !IsDebuff(20) ||
      !IsDebuff(353) ||
      IsDebuff(19) ||
      IsDebuff(26) ||
      IsDebuff(0) ||
      IsDebuff(-1) ||
      IsDebuff(expectedBuffTypeCount - 1) ||
      IsDebuff(expectedBuffTypeCount))
  {
    throw new InvalidOperationException(
      "Legacy debuff defaults drifted from Version4 or lost default semantics.");
  }

  Console.WriteLine(
    "PASS: legacy debuff defaults preserve exact immutable 71-entry set");
}

static void VerifyLegacyVanityPetBuffRegistry()
{
  Type? registryType = typeof(DomeSimulation).Assembly.GetType(
    "Terraria.Dome.Simulation.StatusEffects.Definitions.LegacyVanityPetBuffRegistry");
  if (registryType is null)
  {
    throw new InvalidOperationException(
      "Legacy vanity pet buff registry owner is missing from the Simulation assembly.");
  }

  const int expectedBuffTypeCount = 389;
  FrozenSet<int> expectedVanityPetBuffTypes = new HashSet<int>
  {
    40,
    41,
    42,
    45,
    50,
    51,
    52,
    53,
    54,
    55,
    56,
    61,
    65,
    66,
    81,
    82,
    84,
    85,
    91,
    92,
    127,
    136,
    154,
    191,
    200,
    202,
    217,
    218,
    219,
    258,
    259,
    260,
    261,
    262,
    264,
    266,
    267,
    268,
    274,
    284,
    285,
    286,
    287,
    288,
    289,
    290,
    291,
    292,
    293,
    295,
    296,
    297,
    300,
    301,
    302,
    303,
    304,
    317,
    327,
    328,
    329,
    330,
    331,
    341,
    345,
    349,
    351,
    352,
    354,
    356,
    371,
    372,
    373,
    382
  }.ToFrozenSet();
  int actualBuffTypeCount = (int)(registryType.GetField("BuffTypeCount")?.GetValue(null) ?? -1);
  IReadOnlySet<int>? actualVanityPetBuffTypes = registryType.GetMethod("RegisterDefaults")?.Invoke(
    null,
    null) as IReadOnlySet<int>;
  if (actualVanityPetBuffTypes is null)
  {
    throw new InvalidOperationException(
      "Legacy vanity pet buff registry did not expose an immutable set.");
  }

  bool IsVanityPetBuff(int buffType)
  {
    object? result = registryType.GetMethod("IsVanityPetBuff")?.Invoke(
      null,
      new object[] { buffType });
    return result is true;
  }

  if (actualBuffTypeCount != expectedBuffTypeCount ||
      actualVanityPetBuffTypes.Count != expectedVanityPetBuffTypes.Count ||
      actualVanityPetBuffTypes is not FrozenSet<int> ||
      !expectedVanityPetBuffTypes.SetEquals(actualVanityPetBuffTypes) ||
      !IsVanityPetBuff(40) ||
      !IsVanityPetBuff(382) ||
      IsVanityPetBuff(39) ||
      IsVanityPetBuff(0) ||
      IsVanityPetBuff(-1) ||
      IsVanityPetBuff(expectedBuffTypeCount - 1) ||
      IsVanityPetBuff(expectedBuffTypeCount))
  {
    throw new InvalidOperationException(
      "Legacy vanity pet buff defaults drifted from Version4 or lost default semantics.");
  }

  Console.WriteLine(
    "PASS: legacy vanity pet buff defaults preserve exact immutable 74-entry set");
}

static void VerifyDeterministicNpcLoot()
{
  using DomeSimulation first = new(new WorldGrid(400, 300));
  using DomeSimulation second = new(new WorldGrid(400, 300));
  NpcHandle firstNpc = first.CreateNpc(new SimulationVector(30.0f, 0.0f));
  NpcHandle secondNpc = second.CreateNpc(new SimulationVector(30.0f, 0.0f));

  first.QueueNpcDamage(firstNpc, 100);
  second.QueueNpcDamage(secondNpc, 100);
  first.Tick(new SimulationInputBatch());
  second.Tick(new SimulationInputBatch());
  NpcReplicationSnapshot firstNpcSnapshot = first.CreateNpcReplicationSnapshots().Single();
  WorldItemSnapshot firstLoot = first.CreateWorldItemSnapshots().Single();
  WorldItemSnapshot secondLoot = second.CreateWorldItemSnapshots().Single();
  if (firstNpcSnapshot.IsActive || !firstLoot.IsActive ||
      firstLoot.Stack != secondLoot.Stack || firstLoot.Position != secondLoot.Position)
  {
    throw new InvalidOperationException("NPC death did not produce deterministic server-owned loot.");
  }
}
