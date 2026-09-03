using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Terraria.Dome.Protocol.V1456.Npc;
using Terraria.Dome.Protocol.V1456.Packets;
using Terraria.Dome.Protocol.V1456.Protocol;
using Terraria.Dome.Server;
using Terraria.Dome.Simulation;
using Terraria.Dome.Simulation.Items.Events;
using Terraria.Dome.Simulation.WorldModel;

const short CombatSpawnX = 2100;
const short CombatSpawnY = 4;
const int WorldHeight = 1200;
const int WorldWidth = 4200;

WorldGrid world = new(WorldWidth, WorldHeight);
for (int x = 0; x < WorldWidth; x++)
{
  _ = world.TrySetTile(x, 0, new WorldTile(true, 1));
}

WorldMetadata metadata = new("Combat Verification", new WorldSeed(1456), WorldWidth, WorldHeight);
DomeSimulationSnapshot serverSnapshot;
using (DomeSimulation setup = new(world))
{
  _ = setup.CreateNpc(new SimulationVector(CombatSpawnX + 20.0f, CombatSpawnY));
  serverSnapshot = setup.CreatePersistenceSnapshot(metadata);
}

using DomeServer server = new(serverSnapshot);
server.Start();
using TcpClient attackerClient = new();
using TcpClient observerClient = new();
using TcpClient hiddenClient = new();
await attackerClient.ConnectAsync("127.0.0.1", server.Port);
await observerClient.ConnectAsync("127.0.0.1", server.Port);
await hiddenClient.ConnectAsync("127.0.0.1", server.Port);
NetworkStream attacker = attackerClient.GetStream();
NetworkStream observer = observerClient.GetStream();
NetworkStream hidden = hiddenClient.GetStream();
byte attackerSlot = await ActivateAsync(attacker, CombatSpawnX, CombatSpawnY, "Attacker");
byte observerSlot = await ActivateAsync(observer, (short)(CombatSpawnX - 200), CombatSpawnY, "Observer");
byte hiddenSlot = await ActivateAsync(hidden, 100, CombatSpawnY, "Hidden");
await observer.WriteAsync(TerrariaPacketCodec.EncodePlayerControls(
  new PlayerControlIntent(
    observerSlot,
    MoveLeft: false,
    MoveRight: false,
    Jump: false,
    UseItem: false,
    FacingRight: true,
    SelectedItem: 0),
  (CombatSpawnX - 200) * TerrariaWorldCoordinates.PixelsPerTile,
  CombatSpawnY * TerrariaWorldCoordinates.PixelsPerTile));
await hidden.WriteAsync(TerrariaPacketCodec.EncodePlayerControls(
  new PlayerControlIntent(
    hiddenSlot,
    MoveLeft: false,
    MoveRight: false,
    Jump: false,
    UseItem: false,
    FacingRight: true,
    SelectedItem: 0),
  100.0f * TerrariaWorldCoordinates.PixelsPerTile,
  CombatSpawnY * TerrariaWorldCoordinates.PixelsPerTile));
await Task.Delay(TimeSpan.FromMilliseconds(100));

for (int index = 0; index < 1_200 && !server.LatestSnapshot.Npcs.Any(npc => npc.Health == 0); index++)
{
  PlayerControlIntent fire = new(
    attackerSlot,
    MoveLeft: true,
    MoveRight: false,
    Jump: false,
    UseItem: true,
    FacingRight: true,
    SelectedItem: 0);
  await attacker.WriteAsync(TerrariaPacketCodec.EncodePlayerControls(fire, 0.0f, 0.0f));
  await Task.Delay(TimeSpan.FromMilliseconds(16));
}

bool observerSawNpc = false;
bool observerSawProjectile = false;
bool observerSawProjectileDespawn = false;
bool observerSawNpcDeath = false;
int npcFrameCount = 0;
int projectileFrameCount = 0;
int projectileDespawnFrameCount = 0;
using CancellationTokenSource observerTimeout = new(TimeSpan.FromSeconds(5));
try
{
  while (!observerTimeout.IsCancellationRequested && !observerSawNpcDeath)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(await ReadFrameAsync(observer, observerTimeout.Token));
    if (frame.MessageId == TerrariaMessageId.SyncNPC)
    {
      NpcSyncPacket npc = NpcSyncPacketCodec.Decode(TerrariaFrameCodec.Encode(frame));
      npcFrameCount++;
      observerSawNpc = true;
      observerSawNpcDeath |= npc.Life == 0;
    }
    else if (frame.MessageId == TerrariaMessageId.SyncProjectile)
    {
      projectileFrameCount++;
      observerSawProjectile = true;
    }
    else if (frame.MessageId == TerrariaMessageId.KillProjectile)
    {
      projectileDespawnFrameCount++;
      observerSawProjectileDespawn = true;
    }
  }
}
catch (OperationCanceledException)
{
}

if (!observerSawNpc || !observerSawProjectile || !observerSawProjectileDespawn ||
    !observerSawNpcDeath)
{
  string npcPositions = string.Join(",", server.LatestSnapshot.Npcs.Select(npc =>
    $"{npc.Npc.Value}:{npc.Position.X:F1},{npc.Position.Y:F1}"));
  string playerPositions = string.Join(",", server.LatestSnapshot.Players.Select(player =>
    $"{player.Player.Value}:{player.Position.X:F1},{player.Position.Y:F1}"));
  throw new InvalidOperationException(
    $"Eligible observer did not receive the complete combat lifecycle. " +
    $"Npc={npcFrameCount}, Projectile={projectileFrameCount}, Kill={projectileDespawnFrameCount}, " +
    $"Health={string.Join(',', server.LatestSnapshot.Npcs.Select(npc => npc.Health))}, " +
    $"Npcs={npcPositions}, Players={playerPositions}, " +
    $"Fault={server.SimulationFault?.GetType().Name}:{server.SimulationFault?.Message}.");
}

bool hiddenSawTargetCombat = false;
string? hiddenCombatFrame = null;
using CancellationTokenSource hiddenTimeout = new(TimeSpan.FromMilliseconds(400));
try
{
  while (!hiddenTimeout.IsCancellationRequested)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(await ReadFrameAsync(hidden, hiddenTimeout.Token));
    if (frame.MessageId == TerrariaMessageId.SyncNPC)
    {
      NpcSyncPacket npc = NpcSyncPacketCodec.Decode(TerrariaFrameCodec.Encode(frame));
      if ((npc.Identity == 1 || npc.Identity == 2) && npc.Life > 0 && npc.Life != 100)
      {
        hiddenSawTargetCombat = true;
        hiddenCombatFrame = $"SyncNPC identity={npc.Identity}, life={npc.Life}";
        break;
      }
    }

    if (frame.MessageId == TerrariaMessageId.SyncProjectile ||
        frame.MessageId == TerrariaMessageId.KillProjectile)
    {
      hiddenSawTargetCombat = true;
      hiddenCombatFrame = frame.MessageId.ToString();
      break;
    }
  }
}
catch (OperationCanceledException)
{
}

await Task.Delay(TimeSpan.FromMilliseconds(250));
int worldItemCount = server.CreateWorldItemSnapshots().Count;
IReadOnlyList<ItemReplicationSnapshot> replicatedItems = server.CreateItemReplicationSnapshots();
IReadOnlyList<WorldItemCreatedEvent> createdItems = server.CreateWorldItemCreatedEvents();
IReadOnlyList<WorldItemPickedUpEvent> pickedUpItems = server.CreateWorldItemPickedUpEvents();
if (hiddenSawTargetCombat || worldItemCount != 1 || replicatedItems.Count != 1 ||
    replicatedItems[0].Revision != 1 || replicatedItems[0].WorldState.SpawnSource <= 0)
{
  string createdSummary = string.Join(",", createdItems.Select(item =>
    $"id={item.ReplicationId}:type={item.Stack.ItemType}:qty={item.Stack.Quantity}:" +
    $"pos={item.Position.X:F1},{item.Position.Y:F1}:rev={item.Revision}"));
  string pickedUpSummary = string.Join(",", pickedUpItems.Select(item =>
    $"id={item.ReplicationId}:player={item.Player.Value}:qty={item.AcceptedQuantity}:" +
    $"rev={item.Revision}"));
  throw new InvalidOperationException(
    $"Combat PVS or authoritative NPC loot ownership was violated. " +
    $"Items={worldItemCount}, Created={createdItems.Count}[{createdSummary}], " +
    $"PickedUp={pickedUpItems.Count}[{pickedUpSummary}], Frame={hiddenCombatFrame}, " +
    $"WorldItems={string.Join(',', server.CreateWorldItemSnapshots().Select(item =>
      $"{item.ReplicationId}:{item.Position.X:F1},{item.Position.Y:F1}:" +
      $"active={item.IsActive}:stack={item.Stack.Quantity}"))}, " +
    $"Players={string.Join(',', server.LatestSnapshot.Players.Select(player =>
      $"{player.Player.Value}:{player.Position.X:F1},{player.Position.Y:F1}"))}, " +
    $"WorldRevision={string.Join(',', replicatedItems.Select(item => item.Revision))}, " +
    $"WorldSource={string.Join(',', replicatedItems.Select(item => item.WorldState.SpawnSource))}.");
}

Console.WriteLine("PASS: two-session combat is PVS-limited and produces one server-owned drop");

static async Task<byte> ActivateAsync(NetworkStream stream, short spawnX, short spawnY, string name)
{
  await stream.WriteAsync(TerrariaPacketCodec.Encode(new HelloPacket()));
  TerrariaFrame userSlot = TerrariaFrameCodec.Decode(await ReadFrameAsync(stream, CancellationToken.None));
  await stream.WriteAsync(TerrariaPacketCodec.Encode(CreateProfile(userSlot.Payload.Span[0], name)));
  await stream.WriteAsync(TerrariaPacketCodec.Encode(new PlayerUuidPacket(
    $"00000000-0000-0000-0000-{userSlot.Payload.Span[0]:x12}")));
  await stream.WriteAsync(TerrariaPacketCodec.EncodeRequestWorldData());
  _ = await ReadFrameAsync(stream, CancellationToken.None);
  await stream.WriteAsync(TerrariaPacketCodec.EncodeSpawnTileData(
    new SpawnTileDataRequestPacket(spawnX, spawnY, 0)));
  _ = await ReadFrameAsync(stream, CancellationToken.None);
  for (int index = 0; index < 15; index++)
  {
    _ = await ReadFrameAsync(stream, CancellationToken.None);
  }

  _ = await ReadFrameAsync(stream, CancellationToken.None);
  await stream.WriteAsync(TerrariaPacketCodec.Encode(new PlayerSpawnPacket(
    userSlot.Payload.Span[0], spawnX, spawnY, 0, 0, 0, 0, 0)));
  _ = await ReadFrameAsync(stream, CancellationToken.None);
  _ = await ReadFrameAsync(stream, CancellationToken.None);
  return userSlot.Payload.Span[0];
}

static PlayerProfilePacket CreateProfile(byte playerSlot, string name)
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

static async Task<byte[]> ReadFrameAsync(NetworkStream stream, CancellationToken cancellationToken)
{
  byte[] prefix = new byte[2];
  await stream.ReadExactlyAsync(prefix, cancellationToken);
  int frameLength = prefix[0] | prefix[1] << 8;
  byte[] frame = new byte[frameLength];
  prefix.CopyTo(frame, 0);
  await stream.ReadExactlyAsync(frame.AsMemory(2), cancellationToken);
  return frame;
}
