using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Terraria.Dome.Protocol.V1456.Packets;
using Terraria.Dome.Protocol.V1456.Protocol;
using Terraria.Dome.Server;
using Terraria.Dome.Simulation.WorldModel;

using DomeServer server = new(new WorldGrid(4200, 1200));
server.Start();

using TcpClient firstClient = new();
await firstClient.ConnectAsync("127.0.0.1", server.Port);
using NetworkStream first = firstClient.GetStream();
byte firstSlot = await ActivateAsync(first, "Capability-First");

byte[] offer = ContractExtensionCodec.EncodeCapabilityOffer(
  new ContractCapabilityOffer(SignDeletionVersions: 1, ChestTransferRevisionVersions: 1));
await first.WriteAsync(offer);
TerrariaFrame ackFrame = TerrariaFrameCodec.Decode(await ReadFrameAsync(first));
ContractCapabilityAck ack = ContractExtensionCodec.DecodeCapabilityAck(
  TerrariaFrameCodec.Encode(ackFrame));
if (ack != new ContractCapabilityAck(1, 1))
{
  throw new InvalidOperationException("TCP capability handshake did not return the negotiated intersection.");
}

await first.WriteAsync(offer);
if (await HasCapabilityAckWithinAsync(first, TimeSpan.FromMilliseconds(500)))
{
  throw new InvalidOperationException("TCP duplicate capability negotiation produced a response.");
}

Console.WriteLine("PASS: TCP capability handshake returns ack and rejects duplicate negotiation");

using TcpClient reconnectClient = new();
await reconnectClient.ConnectAsync("127.0.0.1", server.Port);
using NetworkStream reconnect = reconnectClient.GetStream();
_ = await ActivateAsync(reconnect, "Capability-Reconnect");
await reconnect.WriteAsync(offer);
TerrariaFrame reconnectAckFrame = TerrariaFrameCodec.Decode(await ReadFrameAsync(reconnect));
ContractCapabilityAck reconnectAck = ContractExtensionCodec.DecodeCapabilityAck(
  TerrariaFrameCodec.Encode(reconnectAckFrame));
if (reconnectAck != new ContractCapabilityAck(1, 1))
{
  throw new InvalidOperationException("A reconnecting TCP session did not renegotiate capabilities.");
}

Console.WriteLine("PASS: reconnecting TCP session negotiates capabilities independently");

static async Task<byte> ActivateAsync(NetworkStream stream, string name)
{
  await stream.WriteAsync(TerrariaPacketCodec.Encode(new HelloPacket()));
  TerrariaFrame userSlot = TerrariaFrameCodec.Decode(await ReadFrameAsync(stream));
  byte slot = userSlot.Payload.Span[0];
  _ = await ReadFrameAsync(stream);
  TerrariaColor color = new(0, 0, 0);
  await stream.WriteAsync(TerrariaPacketCodec.Encode(new PlayerProfilePacket(
    slot,
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
  await stream.WriteAsync(TerrariaPacketCodec.Encode(new PlayerUuidPacket(
    $"11111111-1111-1111-1111-{slot:D12}")));
  await stream.WriteAsync(TerrariaPacketCodec.EncodeRequestWorldData());
  _ = await ReadFrameAsync(stream);
  await stream.WriteAsync(TerrariaPacketCodec.EncodeSpawnTileData(
    new SpawnTileDataRequestPacket(2100, 300, 0)));
  TerrariaFrame initialSpawn;
  do
  {
    initialSpawn = TerrariaFrameCodec.Decode(await ReadFrameAsync(stream));
  }
  while (initialSpawn.MessageId != TerrariaMessageId.InitialSpawn);

  await stream.WriteAsync(TerrariaPacketCodec.Encode(new PlayerSpawnPacket(
    slot,
    2100,
    300,
    0,
    0,
    0,
    0,
    0)));
  bool connected = false;
  while (!connected)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(await ReadFrameAsync(stream));
    connected = frame.MessageId == TerrariaMessageId.FinishedConnectingToServer;
  }

  return slot;
}

static async Task<bool> HasCapabilityAckWithinAsync(NetworkStream stream, TimeSpan timeout)
{
  using CancellationTokenSource cancellation = new(timeout);
  try
  {
    while (!cancellation.IsCancellationRequested)
    {
      TerrariaFrame frame = TerrariaFrameCodec.Decode(
        await ReadFrameAsync(stream, cancellation.Token));
      if (frame.MessageId != TerrariaMessageId.NetModules || frame.Payload.Length < 5 ||
          frame.Payload.Span[0] != 15 || frame.Payload.Span[1] != 0)
      {
        continue;
      }

      try
      {
        _ = ContractExtensionCodec.DecodeCapabilityAck(TerrariaFrameCodec.Encode(frame));
        return true;
      }
      catch (InvalidDataException)
      {
      }
    }
  }
  catch (InvalidDataException)
  {
    return false;
  }
  catch (EndOfStreamException)
  {
    return false;
  }
  catch (OperationCanceledException)
  {
    return false;
  }

  return false;
}

static async Task<byte[]> ReadFrameAsync(NetworkStream stream, CancellationToken cancellationToken = default)
{
  byte[] lengthBytes = new byte[2];
  await stream.ReadExactlyAsync(lengthBytes, cancellationToken);
  int length = lengthBytes[0] | lengthBytes[1] << 8;
  if (length < 3)
  {
    throw new InvalidDataException("TCP frame length is invalid.");
  }

  byte[] frame = new byte[length];
  frame[0] = lengthBytes[0];
  frame[1] = lengthBytes[1];
  await stream.ReadExactlyAsync(frame.AsMemory(2), cancellationToken);
  return frame;
}
