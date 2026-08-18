using System;
using System.Collections.Generic;
using System.Linq;
using Arch.Core;
using Terraria.Dome.Simulation;
using Terraria.Dome.Simulation.Combat.Components;
using Terraria.Dome.Simulation.Combat.Systems;
using Terraria.Dome.Simulation.Items;
using Terraria.Dome.Simulation.Npc.Commands;
using Terraria.Dome.Simulation.Npc.Components;
using Terraria.Dome.Simulation.Npc.Definitions;
using Terraria.Dome.Simulation.Npc.Snapshots;
using Terraria.Dome.Simulation.Npc.Systems;
using Terraria.Dome.Simulation.WorldModel;
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
Entity firstPlayer = default;
Entity secondPlayer = default;
NpcTargetComponent selectedTarget = targetSelection.SelectTarget(
  new SimulationVector(0.0f, 0.0f),
  [
    new NpcTargetCandidate(secondPlayer, 2, new SimulationVector(5.0f, 0.0f), true, 100),
    new NpcTargetCandidate(firstPlayer, 1, new SimulationVector(-5.0f, 0.0f), true, 100),
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

if (typeof(NpcBehaviorStateComponent).GetFields().Any(field =>
      field.FieldType.IsArray || typeof(IDictionary<,>).IsAssignableFrom(field.FieldType)))
{
  throw new InvalidOperationException("NPC behavior state must remain typed and bounded, not array-backed.");
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
  NpcLifecycleComponent lifecycleComponent = spawnWorld.Get<NpcLifecycleComponent>(spawnResult.Entity);
  NpcReplicationComponent replicationComponent =
    spawnWorld.Get<NpcReplicationComponent>(spawnResult.Entity);
  if (definitionComponent.DefinitionId != 1 || !lifecycleComponent.IsActive ||
      replicationComponent.ReplicationId != 8)
  {
    throw new InvalidOperationException("NPC spawn commit did not attach identity and lifecycle components.");
  }
}
else
{
  throw new InvalidOperationException("Valid NPC spawn command was rejected.");
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

DamageResolutionSystem damageResolution = new();
SimulationComponents.HealthComponent damageHealth = new(100, 100);
ImmunityComponent damageImmunity = new();
if (!damageResolution.TryResolve(
      ref damageHealth,
      new DefenseComponent(5),
      ref damageImmunity,
      rawAmount: 20,
      out int appliedDamage) || appliedDamage != 15 || damageHealth.Current != 85)
{
  throw new InvalidOperationException("NPC damage resolution did not apply defense reduction.");
}

damageImmunity.RemainingTicks = 2;
if (damageResolution.TryResolve(
      ref damageHealth,
      new DefenseComponent(0),
      ref damageImmunity,
      rawAmount: 20,
      out _))
{
  throw new InvalidOperationException("NPC damage resolution ignored hit immunity.");
}

if (damageResolution.TryResolve(
      ref damageHealth,
      new DefenseComponent(0),
      ref damageImmunity,
      rawAmount: 0,
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
