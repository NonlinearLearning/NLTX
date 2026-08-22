using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation;
using Terraria.Dome.Simulation.Components;
using Terraria.Dome.Simulation.Player.Components;
using Terraria.Dome.Simulation.Player.Systems;
using Terraria.Dome.Simulation.Players;
using Terraria.Dome.Simulation.WorldModel;
using Arch.Core;

VerifyDeathAndRespawnSystems();

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

static PlayerPersistentState CreateAccount(string uuid)
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
    items);
}
