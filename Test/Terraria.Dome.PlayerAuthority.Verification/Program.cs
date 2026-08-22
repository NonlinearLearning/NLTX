using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Terraria.Dome.Simulation.Components;
using Terraria.Dome.Simulation.Physics.Systems;
using Terraria.Dome.Simulation;
using Terraria.Dome.Simulation.Players;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Protocol.V1456.Packets;
using Terraria.Dome.Protocol.V1456.Protocol;
using Terraria.Dome.Server;

WorldGrid world = new(400, 300);
TileCollisionSystem collision = new();

_ = world.TrySetTile(10, 0, new WorldTile(true, 1));
TransformComponent fallingTransform = new(10.0f, 2.0f);
VelocityComponent fallingVelocity = new(0.0f, -2.0f);
PhysicsStateComponent fallingPhysics = new();
collision.MoveAndResolve(
  world,
  ref fallingTransform,
  ref fallingVelocity,
  ref fallingPhysics,
  new ColliderComponent(1.0f, 2.0f));
if (fallingTransform.Y != 1.0f || fallingVelocity.Y != 0.0f || !fallingPhysics.IsGrounded)
{
  throw new InvalidOperationException("Player did not land on the authoritative solid tile.");
}

_ = world.TrySetTile(10, 4, new WorldTile(true, 1));
TransformComponent risingTransform = new(10.0f, 1.0f);
VelocityComponent risingVelocity = new(0.0f, 3.0f);
PhysicsStateComponent risingPhysics = new();
collision.MoveAndResolve(
  world,
  ref risingTransform,
  ref risingVelocity,
  ref risingPhysics,
  new ColliderComponent(1.0f, 2.0f));
if (risingTransform.Y != 2.0f || risingVelocity.Y != 0.0f || risingPhysics.IsGrounded)
{
  throw new InvalidOperationException("Player did not stop against the authoritative ceiling tile.");
}

_ = world.TrySetTile(12, 1, new WorldTile(true, 1));
TransformComponent wallTransform = new(10.0f, 1.0f);
VelocityComponent wallVelocity = new(3.0f, 0.0f);
PhysicsStateComponent wallPhysics = new();
collision.MoveAndResolve(
  world,
  ref wallTransform,
  ref wallVelocity,
  ref wallPhysics,
  new ColliderComponent(1.0f, 2.0f));
if (wallTransform.X != 11.0f || wallVelocity.X != 0.0f)
{
  throw new InvalidOperationException("Player did not stop at the authoritative wall tile.");
}

Console.WriteLine("PASS: tile-authoritative player collision resolves floor, ceiling and wall");

WorldGrid simulationWorld = new(400, 300);
_ = simulationWorld.TrySetTile(20, 0, new WorldTile(true, 1));
using DomeSimulation simulation = new(simulationWorld);
PlayerHandle simulatedPlayer = simulation.CreatePlayer(new SimulationVector(20.0f, 4.0f));
simulation.Tick(new SimulationInputBatch());
simulation.Tick(new SimulationInputBatch());
simulation.Tick(new SimulationInputBatch());
PlayerSnapshot simulatedSnapshot = simulation.CreateSnapshot().FindPlayer(simulatedPlayer);
if (simulatedSnapshot.Position.Y != 1.0f || !simulatedSnapshot.IsGrounded)
{
  throw new InvalidOperationException(
    $"DomeSimulation did not use WorldGrid tile collision: " +
    $"y={simulatedSnapshot.Position.Y:F1}, grounded={simulatedSnapshot.IsGrounded}.");
}

Console.WriteLine("PASS: simulation tick uses tile-authoritative collision");

using DomeSimulation lifecycleSimulation = new(new WorldGrid(400, 300));
PlayerHandle lifecyclePlayer = lifecycleSimulation.CreatePlayer(new SimulationVector(10.0f, 0.0f));
lifecycleSimulation.QueuePlayerDamage(lifecyclePlayer, 100);
lifecycleSimulation.Tick(new SimulationInputBatch());
PlayerStateSnapshot dead = lifecycleSimulation.CreatePlayerStateSnapshot(lifecyclePlayer);
if (dead.IsActive || dead.Health != 0 || dead.RespawnTicks == 0)
{
  throw new InvalidOperationException("Queued damage did not produce a server-owned death state.");
}

PlayerSnapshot deadBeforeInput = lifecycleSimulation.CreateSnapshot().FindPlayer(lifecyclePlayer);
lifecycleSimulation.Tick(new SimulationInputBatch(new PlayerInput(
  lifecyclePlayer,
  MoveLeft: false,
  MoveRight: true,
  Jump: false,
  Fire: true)));
PlayerSnapshot deadAfterInput = lifecycleSimulation.CreateSnapshot().FindPlayer(lifecyclePlayer);
if (deadAfterInput.Position != deadBeforeInput.Position ||
    lifecycleSimulation.CreateSnapshot().Projectiles.Count != 0)
{
  throw new InvalidOperationException("A dead player accepted movement or projectile input.");
}

lifecycleSimulation.QueueRespawnPlayer(lifecyclePlayer, new SimulationVector(30.0f, 0.0f));
lifecycleSimulation.Tick(new SimulationInputBatch());
if (lifecycleSimulation.CreatePlayerStateSnapshot(lifecyclePlayer).IsActive)
{
  throw new InvalidOperationException("An early respawn was accepted before the server timer elapsed.");
}

for (int index = 0; index < 3; index++)
{
  lifecycleSimulation.Tick(new SimulationInputBatch());
}

PlayerStateSnapshot respawned = lifecycleSimulation.CreatePlayerStateSnapshot(lifecyclePlayer);
PlayerSnapshot respawnedPlayer = lifecycleSimulation.CreateSnapshot().FindPlayer(lifecyclePlayer);
if (!respawned.IsActive || respawned.Health != 100 ||
    respawnedPlayer.Position != new SimulationVector(10.0f, 0.0f))
{
  throw new InvalidOperationException("Server-owned respawn did not restore the original server spawn state.");
}

Console.WriteLine("PASS: player death and respawn lifecycle is simulation-authoritative");

TerrariaFrame playerActive = TerrariaFrameCodec.Decode(TerrariaPacketCodec.EncodePlayerActive(4, true));
if (playerActive.MessageId != TerrariaMessageId.PlayerActive ||
    !playerActive.Payload.Span.SequenceEqual(new byte[] { 4, 1 }))
{
  throw new InvalidOperationException("Player lifecycle projection did not use V1456 PlayerActive.");
}

Console.WriteLine("PASS: player lifecycle has a typed V1456 active-state projection");

TerrariaFrame playerLifeMana = TerrariaFrameCodec.Decode(
  TerrariaPacketCodec.EncodePlayerLifeMana(4, 75, 100));
if (playerLifeMana.MessageId != TerrariaMessageId.PlayerLifeMana ||
    !playerLifeMana.Payload.Span.SequenceEqual(new byte[] { 4, 75, 0, 100, 0 }))
{
  throw new InvalidOperationException("Player health projection did not use V1456 PlayerLifeMana.");
}

Console.WriteLine("PASS: player health has a typed V1456 life/mana projection");

const string accountUuid = "2eecdeea-c45e-456f-8244-75ec32da6172";
using DomeServer authorityServer = new();
authorityServer.Start();
PlayerPersistentState firstConnection = await ConnectWithBootstrapAsync(
  authorityServer,
  accountUuid,
  "First import",
  life: 120,
  mana: 80,
  buff: 190,
  selectedLoadout: 1,
  accessoryVisibility: 3,
  itemStack: 7);
PlayerPersistentState secondConnection = await ConnectWithBootstrapAsync(
  authorityServer,
  accountUuid,
  "Client overwrite",
  life: 40,
  mana: 20,
  buff: 21,
  selectedLoadout: 2,
  accessoryVisibility: 7,
  itemStack: 3);
if (firstConnection.Profile.Name != "First import" || firstConnection.Life != 120 ||
    firstConnection.Mana != 80 || firstConnection.Buffs.Count != 1 ||
    firstConnection.Buffs[0].Type != 190 || firstConnection.SelectedLoadout != 1 ||
    firstConnection.Items[0].Stack != 7 || secondConnection.Profile.Name != "First import" ||
    secondConnection.Life != 120 || secondConnection.Mana != 80 ||
    secondConnection.Buffs.Count != 1 || secondConnection.Buffs[0].Type != 190 ||
    secondConnection.SelectedLoadout != 1 || secondConnection.AccessoryVisibility != 3 ||
    secondConnection.Items.Count != PlayerPersistentState.ItemSlotCount ||
    secondConnection.Items[0].Stack != 7 || secondConnection.Items[0].ItemType != 1)
{
  throw new InvalidOperationException(
    "The second UUID login did not receive the first server-owned player account state.");
}

Console.WriteLine("PASS: server restores UUID-owned player state and rejects client overwrite");

static async Task<PlayerPersistentState> ConnectWithBootstrapAsync(
  DomeServer server,
  string uuid,
  string name,
  int life,
  int mana,
  ushort buff,
  byte selectedLoadout,
  ushort accessoryVisibility,
  int itemStack)
{
  using TcpClient client = new();
  await client.ConnectAsync("127.0.0.1", server.Port);
  using NetworkStream stream = client.GetStream();
  await stream.WriteAsync(TerrariaPacketCodec.Encode(new HelloPacket()));
  SetUserSlotPacket userSlot = TerrariaPacketCodec.DecodeSetUserSlot(await ReadFrameAsync(stream));
  byte playerSlot = userSlot.PlayerSlot;
  if (TerrariaFrameCodec.Decode(await ReadFrameAsync(stream)).MessageId != TerrariaMessageId.NetModules)
  {
    throw new InvalidOperationException("The server did not emit initial NetModules after SetUserSlot.");
  }

  await stream.WriteAsync(TerrariaPacketCodec.Encode(CreateAuthorityProfile(playerSlot, name)));
  await stream.WriteAsync(TerrariaPacketCodec.Encode(new PlayerUuidPacket(uuid)));
  await stream.WriteAsync(TerrariaPacketCodec.EncodePlayerLifeMana(playerSlot, life, 200));
  await stream.WriteAsync(TerrariaPacketCodec.EncodePlayerMana(playerSlot, mana, 200));
  await stream.WriteAsync(TerrariaPacketCodec.Encode(new PlayerBuffsPacket(playerSlot, [buff])));
  await stream.WriteAsync(TerrariaPacketCodec.Encode(new PlayerLoadoutPacket(
    playerSlot,
    selectedLoadout,
    accessoryVisibility)));
  await stream.WriteAsync(TerrariaPacketCodec.Encode(new PlayerEquipmentPacket(
    playerSlot,
    0,
    itemStack,
    0,
    1,
    false,
    false)));
  await stream.WriteAsync(TerrariaPacketCodec.EncodeRequestWorldData());
  if (TerrariaFrameCodec.Decode(await ReadFrameAsync(stream)).MessageId != TerrariaMessageId.WorldData)
  {
    throw new InvalidOperationException("The server did not return WorldData after bootstrap.");
  }

  await stream.WriteAsync(TerrariaPacketCodec.EncodeSpawnTileData(
    new SpawnTileDataRequestPacket(2100, 300, 0)));
  bool receivedInitialSpawn = false;
  for (int index = 0; index < 2_000; index++)
  {
    TerrariaFrame initialFrame = TerrariaFrameCodec.Decode(await ReadFrameAsync(stream));
    if (initialFrame.MessageId == TerrariaMessageId.InitialSpawn)
    {
      receivedInitialSpawn = true;
      break;
    }
  }

  if (!receivedInitialSpawn)
  {
    throw new InvalidOperationException("The server did not finish the initial world stream.");
  }

  await stream.WriteAsync(TerrariaPacketCodec.Encode(new PlayerSpawnPacket(
    playerSlot,
    2100,
    300,
    0,
    0,
    0,
    0,
    0)));
  bool receivedFinishedConnecting = false;
  for (int index = 0; index < 1_100; index++)
  {
    TerrariaFrame decoded = TerrariaFrameCodec.Decode(await ReadFrameAsync(stream));
    if (decoded.MessageId == TerrariaMessageId.FinishedConnectingToServer)
    {
      receivedFinishedConnecting = true;
      break;
    }
  }

  if (!receivedFinishedConnecting)
  {
    throw new InvalidOperationException(
      $"The server did not finish the bootstrap stream: {server.LastSessionFault}");
  }

  DomeSimulationSnapshot snapshot = server.CreatePersistenceSnapshot(
    new WorldMetadata("Authority Verification", new WorldSeed(1456), 4200, 1200));
  PlayerPersistentState? account = snapshot.PlayerAccounts.SingleOrDefault(
    candidate => candidate.Uuid == uuid);
  if (account is null)
  {
    throw new InvalidOperationException("The server did not persist the bootstrap UUID account.");
  }

  return account;
}

static PlayerProfilePacket CreateAuthorityProfile(byte playerSlot, string name)
{
  TerrariaColor color = new(0, 0, 0);
  return new PlayerProfilePacket(
    playerSlot,
    0,
    0,
    0.0f,
    0,
    name,
    0,
    0,
    0,
    color,
    color,
    color,
    color,
    color,
    color,
    color,
    0,
    0,
    0);
}

static async Task<byte[]> ReadFrameAsync(NetworkStream stream)
{
  using CancellationTokenSource cancellation = new(TimeSpan.FromSeconds(5));
  byte[] prefix = new byte[2];
  await stream.ReadExactlyAsync(prefix, cancellation.Token);
  int frameLength = prefix[0] | prefix[1] << 8;
  byte[] frame = new byte[frameLength];
  prefix.CopyTo(frame, 0);
  await stream.ReadExactlyAsync(frame.AsMemory(2), cancellation.Token);
  return frame;
}
