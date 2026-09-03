using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Terraria.Dome.Protocol.V1456.Packets;
using Terraria.Dome.Protocol.V1456.Protocol;
using Terraria.Dome.Server;

using DomeServer server = new();
server.Start();
using TcpClient client = new();
await client.ConnectAsync("127.0.0.1", server.Port);
NetworkStream stream = client.GetStream();
byte playerSlot = await ActivateAsync(stream, 2000, 0);

PlayerControlIntent controls = new(
  playerSlot,
  MoveLeft: false,
  MoveRight: true,
  Jump: false,
  UseItem: false,
  FacingRight: true,
  SelectedItem: 0);
byte[] frame = TerrariaPacketCodec.EncodePlayerControls(controls, 0.0f, 0.0f);
for (int index = 0; index < DomeServer.MaximumPendingProtocolCommands * 4; index++)
{
  await stream.WriteAsync(frame);
}

await Task.Delay(TimeSpan.FromMilliseconds(100));
if (server.PendingProtocolCommandCount > DomeServer.MaximumPendingProtocolCommands ||
    server.DroppedProtocolCommandCount == 0)
{
  throw new InvalidOperationException("Server protocol command queue did not enforce its flood budget.");
}

long tickBefore = server.LatestSnapshot.Tick;
await Task.Delay(TimeSpan.FromMilliseconds(100));
if (server.LatestSnapshot.Tick <= tickBefore || server.LatestSnapshot.Players.Count != 1)
{
  throw new InvalidOperationException("Packet flood blocked the simulation loop or disconnected the session.");
}

Console.WriteLine("PASS: packet flood is bounded and does not stop simulation progress");

static async Task<byte> ActivateAsync(NetworkStream stream, short spawnX, short spawnY)
{
  await stream.WriteAsync(TerrariaPacketCodec.Encode(new HelloPacket()));
  TerrariaFrame userSlot = TerrariaFrameCodec.Decode(await ReadFrameAsync(stream, "user slot"));
  TerrariaFrame initialNetModules = TerrariaFrameCodec.Decode(
    await ReadFrameAsync(stream, "initial NetModules"));
  if (initialNetModules.MessageId != TerrariaMessageId.NetModules)
  {
    throw new InvalidOperationException("Server did not emit initial NetModules after SetUserSlot.");
  }

  await stream.WriteAsync(TerrariaPacketCodec.Encode(CreateProfile(userSlot.Payload.Span[0])));
  await stream.WriteAsync(TerrariaPacketCodec.Encode(new PlayerUuidPacket(
    "66666666-6666-6666-6666-666666666666")));
  await stream.WriteAsync(TerrariaPacketCodec.EncodeRequestWorldData());
  TerrariaFrame worldData = TerrariaFrameCodec.Decode(await ReadFrameAsync(stream, "world data"));
  if (worldData.MessageId != TerrariaMessageId.WorldData)
  {
    throw new InvalidOperationException("Server did not emit WorldData after the profile bootstrap.");
  }

  await stream.WriteAsync(TerrariaPacketCodec.EncodeSpawnTileData(
    new SpawnTileDataRequestPacket(spawnX, spawnY, 0)));
  bool receivedInitialSpawn = false;
  for (int index = 0; index < 2_000; index++)
  {
    TerrariaFrame initialFrame = TerrariaFrameCodec.Decode(
      await ReadFrameAsync(stream, $"initial world frame {index}"));
    if (initialFrame.MessageId == TerrariaMessageId.InitialSpawn)
    {
      receivedInitialSpawn = true;
      break;
    }
  }

  if (!receivedInitialSpawn)
  {
    throw new InvalidOperationException("Server did not finish the initial world stream.");
  }

  await stream.WriteAsync(TerrariaPacketCodec.Encode(new PlayerSpawnPacket(
    userSlot.Payload.Span[0],
    spawnX,
    spawnY,
    0,
    0,
    0,
    0,
    0)));
  bool receivedHostStatus = false;
  for (int index = 0; index < 256; index++)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(
      await ReadFrameAsync(stream, "connection completion"));
    receivedHostStatus |= frame.MessageId == TerrariaMessageId.HostStatus;
    if (frame.MessageId == TerrariaMessageId.FinishedConnectingToServer)
    {
      if (!receivedHostStatus)
      {
        throw new InvalidOperationException(
          "Server finished connecting before it emitted HostStatus.");
      }

      return userSlot.Payload.Span[0];
    }
  }

  throw new InvalidOperationException("Server did not finish the bootstrap frame stream.");
}

static PlayerProfilePacket CreateProfile(byte playerSlot)
{
  TerrariaColor color = new(0, 0, 0);
  return new PlayerProfilePacket(
    playerSlot,
    0,
    0,
    0.0f,
    0,
    "LoadVerifier",
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

static async Task<byte[]> ReadFrameAsync(NetworkStream stream, string stage)
{
  using CancellationTokenSource timeout = new(TimeSpan.FromSeconds(3));
  try
  {
    byte[] prefix = new byte[2];
    await stream.ReadExactlyAsync(prefix, timeout.Token);
    int frameLength = prefix[0] | prefix[1] << 8;
    byte[] frame = new byte[frameLength];
    prefix.CopyTo(frame, 0);
    await stream.ReadExactlyAsync(frame.AsMemory(2), timeout.Token);
    return frame;
  }
  catch (OperationCanceledException exception)
  {
    throw new TimeoutException($"Timed out while waiting for {stage}.", exception);
  }
}
