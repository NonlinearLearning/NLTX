using System;
using System.Linq;
using Arch.Core;
using Terraria.Dome.Simulation;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.Components;
using Terraria.Dome.Simulation.Items;
using Terraria.Dome.Simulation.Projectile.Definitions;
using Terraria.Dome.Simulation.Projectile.Systems;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Combat.Verification.Fixtures;

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
VerifyStableReplicationAndCombatLifecycle();
VerifyNpcContactDamage();
VerifyPlayerDamageDifficultyModes();
VerifyProjectileSweepPreventsNpcTunneling();
VerifyProjectileTileCollision();
VerifyDeterministicNpcLoot();
VerifyForgedProjectileOwnerRejected();
VerifyProjectileVelocityInputsRejected();
VerifyProjectileLifetimeBoundary();
VerifyProjectilePenetrationBoundary();
VerifyProjectileTargetEligibility();
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
Console.WriteLine("PASS: authoritative combat records, contact damage, collision, cooldown and loot");

static void VerifyBoundedVitalRegeneration()
{
  using DomeSimulation simulation = new(new WorldGrid(400, 300));
  PlayerHandle player = simulation.CreatePlayer(new SimulationVector(2.0f, 0.0f));
  simulation.QueuePlayerDamage(player, 10);
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
    player,
    10.0f,
    0.0f,
    1,
    10,
    -1));
  simulation.Tick(new SimulationInputBatch());
  if (simulation.CreateProjectileReplicationSnapshots().Count != 0)
  {
    throw new InvalidOperationException("Projectile spawn accepted a non-positive lifetime.");
  }

  simulation.QueueProjectileSpawn(new SpawnProjectileCommand(
    player,
    10.0f,
    0.0f,
    1,
    10,
    20,
    MaximumPenetration: 0));
  simulation.Tick(new SimulationInputBatch());
  if (simulation.CreateProjectileReplicationSnapshots().Count != 0)
  {
    throw new InvalidOperationException("Projectile spawn accepted non-positive penetration.");
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
  if (simulation.CreateProjectileReplicationSnapshots().Count != 1)
  {
    throw new InvalidOperationException("Projectile spawn rejected the legacy infinite penetration value.");
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
}

static void VerifyProjectileLifetimeBoundary()
{
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

  using DomeSimulation simulation = new(new WorldGrid(400, 300));
  PlayerHandle player = simulation.CreatePlayer(new SimulationVector(10.0f, 0.0f));
  simulation.QueueProjectileSpawn(new SpawnProjectileCommand(
    player,
    10.0f,
    0.0f,
    1,
    10,
    1));
  simulation.Tick(new SimulationInputBatch());
  simulation.Tick(new SimulationInputBatch());

  ProjectileReplicationSnapshot projectile = simulation
    .CreateProjectileReplicationSnapshots().Single();
  if (projectile.IsActive || projectile.RemainingLifetime > 0 || projectile.Revision <= 0)
  {
    throw new InvalidOperationException(
      "A projectile did not expire at its authoritative one-tick lifetime boundary.");
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
  using DomeSimulation simulation = new(world);
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
