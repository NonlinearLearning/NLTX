using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Terraria.Dome.Protocol.V1456.Packets;
using Terraria.Dome.Protocol.V1456.Protocol;
using Terraria.Dome.Server;
using Terraria.Dome.Simulation;
using Terraria.Dome.Simulation.WorldModel;

const int WorldHeight = 1200;
const int WorldWidth = 4200;
WorldGrid world = new(WorldWidth, WorldHeight);
for (int x = 0; x < WorldWidth; x++)
{
  _ = world.TrySetTile(x, 0, new WorldTile(true, 1));
}

WorldMetadata metadata = new("Lifecycle Verification", new WorldSeed(1456), WorldWidth, WorldHeight);
DomeSimulationSnapshot serverSnapshot;
using (DomeSimulation setup = new(world))
{
  _ = setup.CreateNpc(new SimulationVector(2100.0f, 1.0f));
  serverSnapshot = setup.CreatePersistenceSnapshot(metadata);
}

using DomeServer server = new(serverSnapshot);
server.Start();
using TcpClient ownerClient = new();
using TcpClient observerClient = new();
using TcpClient hiddenClient = new();
await ownerClient.ConnectAsync("127.0.0.1", server.Port);
await observerClient.ConnectAsync("127.0.0.1", server.Port);
await hiddenClient.ConnectAsync("127.0.0.1", server.Port);
NetworkStream owner = ownerClient.GetStream();
NetworkStream observer = observerClient.GetStream();
NetworkStream hidden = hiddenClient.GetStream();
byte ownerSlot = await ActivateAsync(owner, 2100, 1, "Owner");
byte observerSlot = await ActivateAsync(observer, 2105, 1, "Observer");
byte hiddenSlot = await ActivateAsync(hidden, 100, 1, "Hidden");
await observer.WriteAsync(TerrariaPacketCodec.EncodePlayerControls(
  new PlayerControlIntent(
    observerSlot,
    MoveLeft: false,
    MoveRight: false,
    Jump: false,
    UseItem: false,
    FacingRight: true,
    SelectedItem: 0),
  positionX: 2105.0f * TerrariaWorldCoordinates.PixelsPerTile,
  positionY: TerrariaWorldCoordinates.PixelsPerTile));
await hidden.WriteAsync(TerrariaPacketCodec.EncodePlayerControls(
  new PlayerControlIntent(
    hiddenSlot,
    MoveLeft: false,
    MoveRight: false,
    Jump: false,
    UseItem: false,
    FacingRight: true,
    SelectedItem: 0),
  positionX: 100.0f * TerrariaWorldCoordinates.PixelsPerTile,
  positionY: TerrariaWorldCoordinates.PixelsPerTile));
await Task.Delay(TimeSpan.FromMilliseconds(100));

using CancellationTokenSource observerTimeout = new(TimeSpan.FromSeconds(8));
bool sawDeath = false;
bool sawRespawn = false;
try
{
  while (!observerTimeout.IsCancellationRequested && !sawRespawn)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(
      await ReadFrameAsync(observer, observerTimeout.Token));
    if (frame.Payload.Length == 0 || frame.Payload.Span[0] != ownerSlot)
    {
      continue;
    }

    if (frame.MessageId == TerrariaMessageId.PlayerActive &&
        frame.Payload.Span[1] == 0)
    {
      sawDeath = true;
    }

    if (sawDeath && frame.MessageId == TerrariaMessageId.PlayerActive &&
        frame.Payload.Span[1] == 1)
    {
      sawRespawn = true;
    }

    if (frame.MessageId == TerrariaMessageId.PlayerLifeMana &&
        frame.Payload.Length >= 5 &&
        BitConverter.ToInt16(frame.Payload.Span.Slice(1, 2)) == 0)
    {
      sawDeath = true;
    }
  }
}
catch (OperationCanceledException)
{
}

if (!sawDeath || !sawRespawn)
{
  throw new InvalidOperationException(
      $"Observer did not receive authoritative death and respawn. " +
      $"Death={sawDeath}, Respawn={sawRespawn}, Tick={server.LatestSnapshot.Tick}, " +
      $"Players={string.Join(',', server.LatestSnapshot.Players.Select(player =>
        $"{player.Player.Value}:{player.Health}:{player.IsActive}"))}, " +
      $"Npcs={string.Join(',', server.CreateNpcReplicationSnapshots().Select(npc =>
        $"{npc.ReplicationId}:{npc.Position.X:F1},{npc.Position.Y:F1}:" +
        $"{npc.Health}:{npc.IsActive}:{npc.BehaviorId}:{npc.HasTarget}:" +
        $"{npc.TargetStableId}:{npc.Velocity.X:F1},{npc.Velocity.Y:F1}"))}, " +
      $"SimulationFault={server.SimulationFault}, LastSessionFault={server.LastSessionFault}.");
}

bool hiddenSawOwnerLifecycle = false;
using CancellationTokenSource hiddenTimeout = new(TimeSpan.FromMilliseconds(500));
try
{
  while (!hiddenTimeout.IsCancellationRequested)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(
      await ReadFrameAsync(hidden, hiddenTimeout.Token));
    if ((frame.MessageId == TerrariaMessageId.PlayerActive ||
         frame.MessageId == TerrariaMessageId.PlayerLifeMana) &&
        frame.Payload.Length > 0 && frame.Payload.Span[0] == ownerSlot)
    {
      hiddenSawOwnerLifecycle = true;
      break;
    }
  }
}
catch (OperationCanceledException)
{
}

if (hiddenSawOwnerLifecycle)
{
  throw new InvalidOperationException("Hidden session received lifecycle state outside its PVS.");
}

await owner.WriteAsync(TerrariaPacketCodec.EncodePlayerControls(
  new PlayerControlIntent(
    ownerSlot,
    MoveLeft: false,
   MoveRight: false,
    Jump: false,
    UseItem: false,
    FacingRight: true,
    SelectedItem: 0),
  positionX: 9999.0f,
  positionY: 9999.0f));
await Task.Delay(TimeSpan.FromMilliseconds(100));
if (server.LatestSnapshot.Players.Count != 3 ||
    server.LatestSnapshot.Players[0].Position.X > 2100.0f)
{
  throw new InvalidOperationException("A client position assertion altered server-owned lifecycle state.");
}

Console.WriteLine("PASS: player death and automatic respawn are authoritative and PVS-limited");

static async Task<byte> ActivateAsync(
  NetworkStream stream,
  short spawnX,
  short spawnY,
  string name)
{
  await stream.WriteAsync(TerrariaPacketCodec.Encode(new HelloPacket()));
  TerrariaFrame userSlot = TerrariaFrameCodec.Decode(
    await ReadFrameAsync(stream, CancellationToken.None));
  TerrariaColor color = new(0, 0, 0);
  await stream.WriteAsync(TerrariaPacketCodec.Encode(new PlayerProfilePacket(
    userSlot.Payload.Span[0],
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
    0)));
  await stream.WriteAsync(TerrariaPacketCodec.EncodePlayerLifeMana(
    userSlot.Payload.Span[0],
    25,
    25));
  string uuid = name switch
  {
    "Owner" => "22222222-2222-2222-2222-222222222222",
    "Observer" => "33333333-3333-3333-3333-333333333333",
    _ => "44444444-4444-4444-4444-444444444444"
  };
  await stream.WriteAsync(TerrariaPacketCodec.Encode(new PlayerUuidPacket(uuid)));
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
    userSlot.Payload.Span[0],
    spawnX,
    spawnY,
    0,
    0,
    0,
    0,
    0)));
  _ = await ReadFrameAsync(stream, CancellationToken.None);
  _ = await ReadFrameAsync(stream, CancellationToken.None);
  return userSlot.Payload.Span[0];
}

static async Task<byte[]> ReadFrameAsync(
  NetworkStream stream,
  CancellationToken cancellationToken)
{
  byte[] prefix = new byte[2];
  await stream.ReadExactlyAsync(prefix, cancellationToken);
  int frameLength = prefix[0] | prefix[1] << 8;
  if (frameLength < 2)
  {
    throw new InvalidDataException("Invalid frame length.");
  }

  byte[] frame = new byte[frameLength];
  prefix.CopyTo(frame, 0);
  await stream.ReadExactlyAsync(frame.AsMemory(2), cancellationToken);
  return frame;
}
