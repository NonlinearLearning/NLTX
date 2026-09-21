using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Terraria.Dome.Protocol.V1456.Packets;
using Terraria.Dome.Protocol.V1456.Protocol;
using Terraria.Dome.Server;
using Terraria.Dome.Simulation.Liquid.Components;
using Terraria.Dome.Simulation.WorldModel;
using PipelineLiquidSourceComponent = Terraria.Dome.Simulation.Liquid.Components.LiquidSourceComponent;

const int FrameReadTimeoutSeconds = 15;
const int SourceX = 2100;
const int SourceY = 300;

using DomeServer server = new();
_ = server.World.TrySetLiquid(SourceX, SourceY, 128, (byte)LiquidType.Water);
_ = server.World.TrySetLiquid(SourceX + 1, SourceY, 128, (byte)LiquidType.Lava);
server.Start();

using TcpClient client = new();
await client.ConnectAsync("127.0.0.1", server.Port);
using NetworkStream stream = client.GetStream();
byte playerSlot = await ActivateAsync(stream, "LiquidVerifier", -1, -1);
using TcpClient hiddenClient = new();
await hiddenClient.ConnectAsync("127.0.0.1", server.Port);
using NetworkStream hiddenStream = hiddenClient.GetStream();
byte hiddenPlayerSlot = await ActivateAsync(hiddenStream, "LiquidHidden", 100, 300);
await UnlockVisibilityAsync(hiddenStream, hiddenPlayerSlot, 100.0f, 300.0f);
await Task.Delay(TimeSpan.FromMilliseconds(100));

if (!await server.QueueLiquidSourceAsync(
    new PipelineLiquidSourceComponent(SourceX, SourceY, 128, LiquidType.Water, 1)))
{
  throw new InvalidOperationException("Server rejected the runtime liquid source fixture.");
}

bool receivedSource = false;
bool receivedTarget = false;
bool receivedConsumedNeighbor = false;
bool hiddenReceivedLiquid = false;
try
{
  for (int index = 0; index < 1_000 && (!receivedSource || !receivedTarget); index++)
  {
    byte[] frameBytes = await ReadFrameAsync(stream);
    TerrariaFrame frame = TerrariaFrameCodec.Decode(frameBytes);
    if (frame.MessageId != TerrariaMessageId.NetModules)
    {
      continue;
    }

    NetModulePacket module = TerrariaPacketCodec.DecodeNetModule(frameBytes);
    if (module.ModuleId != 0)
    {
      continue;
    }

    IReadOnlyList<WorldLiquidSnapshot> updates = TerrariaPacketCodec.DecodeLiquidNetModule(
      frameBytes);
    receivedSource |= updates.Any(update =>
      update.X == SourceX && update.Y == SourceY && update.Amount == 0);
    receivedTarget |= updates.Any(update =>
      update.X == SourceX && update.Y == SourceY - 1 && update.Amount == 32);
    receivedConsumedNeighbor |= updates.Any(update =>
      update.X == SourceX + 1 && update.Y == SourceY && update.Amount == 0);
  }
}
catch (OperationCanceledException exception)
{
  throw new InvalidOperationException(
    $"Liquid module was not observed. source={receivedSource}, target={receivedTarget}, " +
    $"worldSource={server.World.GetTile(SourceX, SourceY).LiquidAmount}, " +
    $"worldTarget={server.World.GetTile(SourceX, SourceY - 1).LiquidAmount}, " +
    $"tile={server.World.GetTile(SourceX, SourceY).Type}, " +
    $"rejectedOutbound={server.NetworkIsolation.RejectedOutboundCount}, " +
    $"simulationFault={server.SimulationFault}.",
    exception);
}

hiddenReceivedLiquid = await ContainsLiquidChangeAsync(hiddenStream, SourceX, SourceY);

if (!receivedSource || !receivedTarget || !receivedConsumedNeighbor ||
    !server.World.GetTile(SourceX, SourceY).IsActive ||
    server.World.GetTile(SourceX, SourceY).Type != 56 ||
    server.World.GetTile(SourceX, SourceY).LiquidAmount != 0 ||
    server.World.GetTile(SourceX + 1, SourceY).LiquidAmount != 0 ||
    hiddenReceivedLiquid)
{
  throw new InvalidOperationException(
    $"Runtime liquid replication was incomplete. slot={playerSlot}, " +
    $"source={receivedSource}, target={receivedTarget}, " +
    $"consumedNeighbor={receivedConsumedNeighbor}, " +
    $"hiddenReceivedLiquid={hiddenReceivedLiquid}, " +
    $"tile={server.World.GetTile(SourceX, SourceY).Type}, " +
    $"simulationFault={server.SimulationFault}.");
}

Console.WriteLine("PASS: runtime liquid commit is replicated through visible NetLiquidModule state");

static async Task<byte> ActivateAsync(
  NetworkStream stream,
  string name,
  short spawnX,
  short spawnY)
{
  await stream.WriteAsync(TerrariaPacketCodec.Encode(new HelloPacket()));
  TerrariaFrame userSlot = TerrariaFrameCodec.Decode(await ReadFrameAsync(stream));
  if (userSlot.MessageId != TerrariaMessageId.SetUserSlot)
  {
    throw new InvalidOperationException("Server did not assign the liquid verifier player slot.");
  }

  TerrariaFrame initialNetModules = TerrariaFrameCodec.Decode(await ReadFrameAsync(stream));
  if (initialNetModules.MessageId != TerrariaMessageId.NetModules)
  {
    throw new InvalidOperationException("Server did not send initial NetModules state.");
  }

  byte playerSlot = userSlot.Payload.Span[0];
  await stream.WriteAsync(TerrariaPacketCodec.Encode(new PlayerProfilePacket(
    playerSlot,
    SkinVariant: 0,
    VoiceVariant: 1,
    VoicePitchOffset: 0.0f,
    Hair: 0,
    Name: name,
    HairDye: 0,
    AccessoryVisibility: 0,
    HideMisc: 0,
    HairColor: new TerrariaColor(0, 0, 0),
    SkinColor: new TerrariaColor(0, 0, 0),
    EyeColor: new TerrariaColor(0, 0, 0),
    ShirtColor: new TerrariaColor(0, 0, 0),
    UnderShirtColor: new TerrariaColor(0, 0, 0),
    PantsColor: new TerrariaColor(0, 0, 0),
    ShoeColor: new TerrariaColor(0, 0, 0),
    DifficultyFlags: 0,
    BiomeTorchFlags: 0,
    ConsumableFlags: 0)));
  await stream.WriteAsync(TerrariaPacketCodec.Encode(new PlayerUuidPacket(
    "33333333-3333-3333-3333-333333333333")));
  await stream.WriteAsync(TerrariaPacketCodec.EncodeRequestWorldData());
  _ = await ReadFrameAsync(stream);
  await stream.WriteAsync(TerrariaPacketCodec.EncodeSpawnTileData(
    new SpawnTileDataRequestPacket(spawnX, spawnY, 0)));

  TerrariaFrame statusText = TerrariaFrameCodec.Decode(await ReadFrameAsync(stream));
  if (statusText.MessageId == TerrariaMessageId.WorldData)
  {
    statusText = TerrariaFrameCodec.Decode(await ReadFrameAsync(stream));
  }

  if (statusText.MessageId != TerrariaMessageId.StatusTextSize)
  {
    throw new InvalidOperationException("Liquid verifier did not receive world stream status.");
  }

  while (true)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(await ReadFrameAsync(stream));
    if (frame.MessageId == TerrariaMessageId.InitialSpawn)
    {
      break;
    }
  }

  await stream.WriteAsync(TerrariaPacketCodec.Encode(new PlayerSpawnPacket(
    playerSlot,
    spawnX,
    spawnY,
    0,
    0,
    0,
    0,
    0)));
  for (int index = 0; index < 1_100; index++)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(await ReadFrameAsync(stream));
    if (frame.MessageId == TerrariaMessageId.FinishedConnectingToServer)
    {
      return playerSlot;
    }
  }

  throw new InvalidOperationException("Liquid verifier session did not finish connecting.");
}

static Task UnlockVisibilityAsync(
  NetworkStream stream,
  byte playerSlot,
  float tileX,
  float tileY)
{
  return stream.WriteAsync(TerrariaPacketCodec.EncodePlayerControls(
    new PlayerControlIntent(
      playerSlot,
      MoveLeft: false,
      MoveRight: false,
      Jump: false,
      UseItem: false,
      FacingRight: true,
      SelectedItem: 0),
    tileX * TerrariaWorldCoordinates.PixelsPerTile,
    tileY * TerrariaWorldCoordinates.PixelsPerTile)).AsTask();
}

static async Task<bool> ContainsLiquidChangeAsync(
  NetworkStream stream,
  int x,
  int y)
{
  using CancellationTokenSource cancellation = new(TimeSpan.FromMilliseconds(500));
  try
  {
    while (!cancellation.IsCancellationRequested)
    {
      byte[] frameBytes = await ReadFrameAsync(stream, cancellation.Token);
      TerrariaFrame frame = TerrariaFrameCodec.Decode(frameBytes);
      if (frame.MessageId != TerrariaMessageId.NetModules)
      {
        continue;
      }

      NetModulePacket module = TerrariaPacketCodec.DecodeNetModule(frameBytes);
      if (module.ModuleId != 0)
      {
        continue;
      }

      IReadOnlyList<WorldLiquidSnapshot> updates =
        TerrariaPacketCodec.DecodeLiquidNetModule(frameBytes);
      if (updates.Any(update => update.X == x && update.Y == y))
      {
        return true;
      }
    }
  }
  catch (OperationCanceledException)
  {
  }

  return false;
}

static async Task<byte[]> ReadFrameAsync(
  NetworkStream stream,
  CancellationToken cancellationToken = default)
{
  using CancellationTokenSource cancellation = new(
    TimeSpan.FromSeconds(FrameReadTimeoutSeconds));
  CancellationToken readCancellation = cancellationToken == default
    ? cancellation.Token
    : cancellationToken;
  byte[] prefix = new byte[2];
  await stream.ReadExactlyAsync(prefix, readCancellation);
  int length = prefix[0] | prefix[1] << 8;
  byte[] body = new byte[length - 2];
  await stream.ReadExactlyAsync(body, readCancellation);
  return [.. prefix, .. body];
}
