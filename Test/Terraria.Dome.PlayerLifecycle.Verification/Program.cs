using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation;
using Terraria.Dome.Simulation.Components;
using Terraria.Dome.Simulation.Player.Components;
using Terraria.Dome.Simulation.Player.Systems;
using Terraria.Dome.Simulation.Player.Definitions;
using Terraria.Dome.Simulation.Players;
using Terraria.Dome.Simulation.WorldModel;
using Arch.Core;

VerifyDeathAndRespawnSystems();
VerifyWellFedContract();
VerifySpawnAreaPolicy();

const string uuid = "2eecdeea-c45e-456f-8244-75ec32da6172";
PlayerPersistentState account = CreateAccount(uuid);
using DomeSimulation simulation = new(new WorldGrid(400, 300));
PlayerHandle player = simulation.CreatePlayer(
  account,
  new SimulationVector(12.0f, 4.0f),
  assignedSlot: 7);

PlayerStateSnapshot state = simulation.CreatePlayerStateSnapshot(player);
if (!state.IsActive || state.AccountUuid != uuid || state.AssignedSlot != 7 ||
    state.RespawnTicks != 0)
{
  throw new InvalidOperationException("Player identity and lifecycle state were not projected together.");
}

simulation.DestroyPlayer(player);
if (!simulation.TryGetPlayerPersistentState(uuid, out PlayerPersistentState? restored) ||
    restored is null || restored.Uuid != uuid)
{
  throw new InvalidOperationException("Destroying a runtime player deleted the account record.");
}

Console.WriteLine("PASS: player identity, lifecycle and account ownership are separated");

static void VerifyWellFedContract()
{
  WellFedStateComponent state = new();
  state.Eat(foodRank: 3, foodBuffTime: 72010);
  if (state.TimeLeftRank1 != 0 || state.TimeLeftRank2 != 10 ||
      state.TimeLeftRank3 != 72000 || state.Rank != 3)
  {
    throw new InvalidOperationException("WellFed Eat did not preserve capped rank counters.");
  }

  state.Update();
  if (state.TimeLeftRank3 != 71999 || state.TimeLeftRank2 != 10 || state.Rank != 3)
  {
    throw new InvalidOperationException("WellFed Update did not decay the highest rank first.");
  }

  state.Clear();
  if (state.TimeLeft != 0 || state.Rank != 0)
  {
    throw new InvalidOperationException("WellFed Clear did not remove all rank counters.");
  }

  const string uuid = "2eecdeea-c45e-456f-8244-75ec32da6172";
  PlayerPersistentState account = CreateAccount(uuid);
  using DomeSimulation simulation = new(new WorldGrid(400, 300));
  PlayerHandle player = simulation.CreatePlayer(account, new SimulationVector(2.0f, 2.0f));
  if (!simulation.ApplyConsumeWellFedCommand(new ConsumeWellFedCommand(
        player,
        foodRank: 2,
        foodBuffTime: 120)) ||
      !simulation.TryGetWellFedState(player, out WellFedStateComponent? runtime) ||
      runtime is null || runtime.Rank != 2 || runtime.TimeLeftRank1 != 0 ||
      runtime.TimeLeftRank2 != 120)
  {
    throw new InvalidOperationException("WellFed player command did not update server-owned state.");
  }

  simulation.DestroyPlayer(player);
  if (!simulation.TryGetPlayerPersistentState(uuid, out PlayerPersistentState? restored) ||
      restored is null || restored.WellFedTimeLeftRank1 != 0 ||
      restored.WellFedTimeLeftRank2 != 120)
  {
    throw new InvalidOperationException("WellFed state did not persist with the player account.");
  }

  PlayerHandle restoredPlayer = simulation.CreatePlayer(restored, new SimulationVector(2.0f, 2.0f));
  if (!simulation.ClearWellFed(restoredPlayer) ||
      !simulation.TryGetWellFedState(restoredPlayer, out WellFedStateComponent? cleared) ||
      cleared is null || cleared.TimeLeft != 0)
  {
    throw new InvalidOperationException("WellFed clear command did not update server-owned state.");
  }

  Console.WriteLine("PASS: WellFed oracle counters, command ownership and persistence");
}

static void VerifySpawnAreaPolicy()
{
  WorldGrid world = new(400, 300);
  SimulationVector position = new(10.0f, 8.0f);
  if (!PlayerSpawnAreaPolicy.IsValid(world, position))
  {
    throw new InvalidOperationException("An empty spawn area was rejected.");
  }

  if (!world.TrySetTile(10, 6, new WorldTile(IsActive: true, Type: 1)) ||
      PlayerSpawnAreaPolicy.IsValid(world, position))
  {
    throw new InvalidOperationException("A solid tile in the spawn area was accepted.");
  }

  _ = world.TrySetTile(10, 6, default);
  if (!world.TrySetLiquid(10, 6, 64, 0) || PlayerSpawnAreaPolicy.IsValid(world, position))
  {
    throw new InvalidOperationException("Liquid in the spawn area was accepted.");
  }

  if (PlayerSpawnAreaPolicy.IsValid(world, new SimulationVector(float.NaN, 8.0f)))
  {
    throw new InvalidOperationException("A non-finite spawn coordinate was accepted.");
  }

  using DomeSimulation simulation = new(world);
  PlayerHandle player = simulation.CreatePlayer(position);
  simulation.QueuePlayerDamage(player, 100);
  simulation.Tick(new SimulationInputBatch());
  simulation.Tick(new SimulationInputBatch());
  simulation.Tick(new SimulationInputBatch());
  simulation.Tick(new SimulationInputBatch());
  if (!world.TrySetTile(10, 6, new WorldTile(IsActive: true, Type: 1)))
  {
    throw new InvalidOperationException("The blocked spawn fixture could not be created.");
  }

  simulation.QueueRespawnPlayer(player, position);
  simulation.Tick(new SimulationInputBatch());
  if (simulation.CreatePlayerStateSnapshot(player).IsActive)
  {
    throw new InvalidOperationException("Respawn commit accepted a blocked spawn area.");
  }

  _ = world.TrySetTile(10, 6, default);
  simulation.QueueRespawnPlayer(player, position);
  simulation.Tick(new SimulationInputBatch());
  if (!simulation.CreatePlayerStateSnapshot(player).IsActive)
  {
    throw new InvalidOperationException("Respawn commit rejected a clear spawn area.");
  }

  Console.WriteLine("PASS: spawn area policy rejects solid, liquid and non-finite positions");
}

static void VerifyDeathAndRespawnSystems()
{
  PlayerLifecycleComponent lifecycle = new()
  {
    IsActive = true,
    RespawnTicks = 0,
    Spawn = new SimulationVector(12.0f, 4.0f)
  };
  HealthComponent health = new(0, 100);
  PlayerDeathSystem deathSystem = new();
  if (!deathSystem.TryBeginDeath(ref lifecycle, health, respawnDelayTicks: 3) ||
      lifecycle.IsActive || lifecycle.RespawnTicks != 3)
  {
    throw new InvalidOperationException("Player death did not create one inactive respawn state.");
  }

  if (deathSystem.TryBeginDeath(ref lifecycle, health, respawnDelayTicks: 3))
  {
    throw new InvalidOperationException("Player death was emitted more than once for one transition.");
  }

  try
  {
    _ = deathSystem.TryBeginDeath(ref lifecycle, health, respawnDelayTicks: 3601);
    throw new InvalidOperationException("Player death accepted a timer above the source maximum.");
  }
  catch (ArgumentOutOfRangeException)
  {
  }

  TransformComponent transform = new(1.0f, 2.0f);
  VelocityComponent velocity = new(3.0f, 4.0f);
  PlayerRespawnSystem respawnSystem = new();
  if (respawnSystem.TryRespawn(
        ref lifecycle,
        ref transform,
        ref velocity,
        ref health,
        lifecycle.Spawn))
  {
    throw new InvalidOperationException("Player respawn bypassed its configured delay.");
  }

  lifecycle.RespawnTicks = 0;
  if (!respawnSystem.TryRespawn(
        ref lifecycle,
        ref transform,
        ref velocity,
        ref health,
        lifecycle.Spawn) ||
      !lifecycle.IsActive || health.Current != health.Maximum ||
      transform.X != lifecycle.Spawn.X || transform.Y != lifecycle.Spawn.Y ||
      velocity.X != 0.0f || velocity.Y != 0.0f)
  {
    throw new InvalidOperationException("Player respawn did not restore the authoritative runtime state.");
  }

  lifecycle.IsActive = false;
  lifecycle.RespawnTicks = -1;
  if (respawnSystem.TryRespawn(
        ref lifecycle,
        ref transform,
        ref velocity,
        ref health,
        lifecycle.Spawn))
  {
    throw new InvalidOperationException("Player respawn accepted a forged negative timer.");
  }

  lifecycle.RespawnTicks = 0;
  if (respawnSystem.TryRespawn(
        ref lifecycle,
        ref transform,
        ref velocity,
        ref health,
        new SimulationVector(float.NaN, 0.0f)))
  {
    throw new InvalidOperationException("Player respawn accepted a non-finite spawn coordinate.");
  }

  lifecycle.IsActive = false;
  if (!respawnSystem.TryRespawn(
        ref lifecycle,
        ref transform,
        ref velocity,
        ref health,
        lifecycle.Spawn) ||
      respawnSystem.TryRespawn(
        ref lifecycle,
        ref transform,
        ref velocity,
        ref health,
        lifecycle.Spawn))
  {
    throw new InvalidOperationException("Player respawn accepted duplicate commands in one state transition.");
  }

  using World lifecycleWorld = World.Create();
  Entity lifecycleEntity = lifecycleWorld.Create(new PlayerLifecycleComponent
  {
    IsActive = false,
    RespawnTicks = -1,
    Spawn = lifecycle.Spawn
  });
  Dictionary<PlayerHandle, Entity> lifecyclePlayers = new()
  {
    [new PlayerHandle(1)] = lifecycleEntity
  };
  bool respawnEnqueued = false;
  new PlayerLifecycleSystem().Advance(
    lifecycleWorld,
    lifecyclePlayers,
    (_, _) => respawnEnqueued = true);
  PlayerLifecycleComponent normalized = lifecycleWorld.Get<PlayerLifecycleComponent>(lifecycleEntity);
  if (normalized.RespawnTicks != 0 || respawnEnqueued)
  {
    throw new InvalidOperationException(
      "Player lifecycle did not clamp a forged negative timer before respawn scheduling.");
  }

  lifecyclePlayers[new PlayerHandle(2)] = default;
  new PlayerLifecycleSystem().Advance(
    lifecycleWorld,
    lifecyclePlayers,
    (_, _) => throw new InvalidOperationException("Stale player entity scheduled a respawn."));
}

static PlayerPersistentState CreateAccount(
  string uuid,
  int wellFedTimeLeftRank1 = 0,
  int wellFedTimeLeftRank2 = 0,
  int wellFedTimeLeftRank3 = 0)
{
  PlayerPersistentItem[] items = new PlayerPersistentItem[PlayerPersistentState.ItemSlotCount];
  for (int index = 0; index < items.Length; index++)
  {
    items[index] = new PlayerPersistentItem(index, 0, 0, 0, false, false);
  }

  return new PlayerPersistentState(
    uuid,
    new PlayerPersistentProfile("Lifecycle"),
    100,
    100,
    20,
    20,
    [],
    selectedLoadout: 0,
    accessoryVisibility: 0,
    items,
    wellFedTimeLeftRank1,
    wellFedTimeLeftRank2,
    wellFedTimeLeftRank3);
}
