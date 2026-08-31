using System;
using Arch.Core;
using Terraria.Dome.Simulation;
using Terraria.Dome.Simulation.Items;
using Terraria.Dome.Simulation.Items.Components;
using Terraria.Dome.Simulation.Npc;
using Terraria.Dome.Simulation.Players;
using Terraria.Dome.Simulation.Player.Components;
using Terraria.Dome.Simulation.Projectile;
using Terraria.Dome.Simulation.WorldModel;

World world = World.Create();
PlayerStore store = new();
Entity firstEntity = world.Create(new PlayerLifecycleComponent { IsActive = true });
Entity secondEntity = world.Create(new PlayerLifecycleComponent { IsActive = true });
PlayerHandle first = new(1);
PlayerHandle second = new(2);
store.Add(first, firstEntity);
store.Add(second, secondEntity);
if (store.Count != 2 || !store.ContainsKey(first) || store[first] != firstEntity)
{
  throw new InvalidOperationException("PlayerStore did not retain server-owned player entities.");
}

bool duplicateRejected = false;
try
{
  store.Add(first, secondEntity);
}
catch (ArgumentException)
{
  duplicateRejected = true;
}

if (!duplicateRejected)
{
  throw new InvalidOperationException("PlayerStore accepted a duplicate player handle.");
}

try
{
  store.Add(default, secondEntity);
  throw new InvalidOperationException("PlayerStore accepted an invalid player handle.");
}
catch (ArgumentOutOfRangeException)
{
}

if (!store.Remove(first, out Entity removed) || removed != firstEntity || store.Remove(first, out _))
{
  throw new InvalidOperationException("PlayerStore destruction was not idempotent.");
}

NpcStore npcStore = new();
NpcHandle npc = new(1);
npcStore.Add(npc, firstEntity);
try
{
  npcStore.Add(default, secondEntity);
  throw new InvalidOperationException("NpcStore accepted an invalid NPC handle.");
}
catch (ArgumentOutOfRangeException)
{
}
if (!npcStore.ContainsKey(npc) || npcStore[npc] != firstEntity ||
    !npcStore.Remove(npc, out Entity removedNpc) || removedNpc != firstEntity ||
    npcStore.Remove(npc, out _))
{
  throw new InvalidOperationException("NpcStore did not preserve idempotent ownership removal.");
}

ProjectileStore projectileStore = new();
projectileStore.Add(secondEntity, replicationId: 4);
if (!projectileStore.TryGetValue(secondEntity, out int replicationId) || replicationId != 4 ||
    !projectileStore.Remove(secondEntity, out replicationId) || replicationId != 4 ||
    projectileStore.Remove(secondEntity, out _))
{
  throw new InvalidOperationException(
    "ProjectileStore did not preserve idempotent replication ownership removal.");
}

WorldItemStore worldItemStore = new(world);
WorldItemComponent activeItem = new(
  1,
  new ItemStack(1, 2),
  new SimulationVector(2.0f, 3.0f),
  IsActive: true,
  Revision: 1,
  new WorldSectionCoordinates(0, 0),
  WorldState: ItemWorldStateComponent.Active(1));
worldItemStore.Add(activeItem);
try
{
  worldItemStore.Add(activeItem with
  {
    ReplicationId = 2,
    Position = new SimulationVector(float.NaN, 0.0f)
  });
  throw new InvalidOperationException("WorldItemStore accepted a non-finite item position.");
}
catch (ArgumentOutOfRangeException)
{
}
worldItemStore[activeItem.ReplicationId] = activeItem with
{
  IsActive = false,
  Revision = 2,
  Stack = ItemStack.Empty,
  WorldState = ItemWorldStateComponent.FromReplicationSnapshot(false, 2)
};
if (!worldItemStore.TryGetValue(1, out WorldItemComponent item) || item.IsActive ||
    item.Revision != 2 || worldItemStore.Count != 1)
{
  throw new InvalidOperationException("WorldItemStore did not retain its inactive replication tombstone.");
}

try
{
  worldItemStore.RecordPickup(activeItem.ReplicationId, default);
  throw new InvalidOperationException("WorldItemStore accepted an invalid pickup owner.");
}
catch (ArgumentOutOfRangeException)
{
}

PlayerLifecycleSystem lifecycleSystem = new();
PlayerLifecycleComponent lifecycle = new()
{
  IsActive = false,
  RespawnTicks = 1,
  Spawn = new SimulationVector(12.0f, 4.0f)
};
world.Set(secondEntity, lifecycle);
PlayerHandle? respawned = null;
lifecycleSystem.Advance(
  world,
  store,
  (player, spawn) => respawned = player);
if (respawned != second || store.Count != 1)
{
  throw new InvalidOperationException("PlayerLifecycleSystem did not enqueue the owned respawn.");
}

using DomeSimulation simulation = new(new WorldGrid(400, 300));
PlayerHandle destroyed = simulation.CreatePlayer(new SimulationVector(10.0f, 0.0f));
PlayerHandle survivor = simulation.CreatePlayer(new SimulationVector(20.0f, 0.0f));
simulation.QueuePlayerDamage(survivor, 100);
simulation.Tick(new SimulationInputBatch());
PlayerSnapshot inactive = simulation.CreateSnapshot().FindPlayer(survivor);
if (inactive.IsActive)
{
  throw new InvalidOperationException("An inactive player tombstone was not retained for replication.");
}

if (!simulation.DestroyPlayer(destroyed) || simulation.DestroyPlayer(destroyed))
{
  throw new InvalidOperationException("DomeSimulation player destruction was not idempotent.");
}

SimulationSnapshot snapshot = simulation.CreateSnapshot();
if (snapshot.Players.Count != 1 || snapshot.Players[0].Player != survivor)
{
  throw new InvalidOperationException("Destroyed players leaked into the simulation snapshot.");
}

world.Destroy(firstEntity);
world.Destroy(secondEntity);
world.Dispose();
Console.WriteLine("PASS: player ownership, lifecycle and snapshot contracts");
