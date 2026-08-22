using System;
using System.IO;
using System.IO.Compression;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Terraria.Dome.Protocol.V1456.Packets;
using Terraria.Dome.Protocol.V1456.Protocol;
using Terraria.Dome.Server;
using Terraria.Dome.Simulation.WorldModel;

const int FrameReadTimeoutSeconds = 15;

using DomeServer server = new();
_ = server.World.TrySetTile(2100, 300, new WorldTile(IsActive: true, Type: 1));
server.Start();

using TcpClient client = new();
await client.ConnectAsync("127.0.0.1", server.Port);
using NetworkStream stream = client.GetStream();
await stream.WriteAsync(TerrariaPacketCodec.Encode(new HelloPacket()));
TerrariaFrame userSlot = TerrariaFrameCodec.Decode(await ReadFrameAsync(stream));
if (userSlot.MessageId != TerrariaMessageId.SetUserSlot)
{
  throw new InvalidOperationException("Server did not assign a player slot.");
}

TerrariaFrame initialNetModules = TerrariaFrameCodec.Decode(await ReadFrameAsync(stream));
if (initialNetModules.MessageId != TerrariaMessageId.NetModules ||
    !initialNetModules.Payload.Span.SequenceEqual(new byte[] { 0, 0, 0, 0 }))
{
  throw new InvalidOperationException("Server did not send the initial NetModules state.");
}

PlayerProfilePacket profile = new(
  PlayerSlot: 1,
  SkinVariant: 0,
  VoiceVariant: 1,
  VoicePitchOffset: 0.0f,
  Hair: 0,
  Name: "WorldVerifier",
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
  ConsumableFlags: 0);
await stream.WriteAsync(TerrariaPacketCodec.Encode(profile));
await stream.WriteAsync(TerrariaPacketCodec.Encode(new PlayerUuidPacket(
  "22222222-2222-2222-2222-222222222222")));
await stream.WriteAsync(TerrariaPacketCodec.EncodeRequestWorldData());
_ = await ReadFrameAsync(stream);
await stream.WriteAsync(TerrariaPacketCodec.EncodeSpawnTileData(
  new SpawnTileDataRequestPacket(-1, -1, 0)));

TerrariaFrame statusText;
try
{
  statusText = TerrariaFrameCodec.Decode(await ReadFrameAsync(stream));
}
catch (EndOfStreamException exception)
{
  throw new InvalidOperationException(
    $"Server closed before the initial world stream. SessionFault={server.LastSessionFault}; " +
    $"SimulationFault={server.SimulationFault}.",
    exception);
}
if (statusText.MessageId == TerrariaMessageId.WorldData)
{
  statusText = TerrariaFrameCodec.Decode(await ReadFrameAsync(stream));
}

if (statusText.MessageId != TerrariaMessageId.StatusTextSize)
{
  throw new InvalidOperationException("Server did not begin the initial world stream with StatusTextSize.");
}

bool foundAuthoritativeTile = false;
while (true)
{
  TerrariaFrame sectionFrame = TerrariaFrameCodec.Decode(await ReadFrameAsync(stream));
  if (sectionFrame.MessageId != TerrariaMessageId.TileSection)
  {
    if (sectionFrame.MessageId == TerrariaMessageId.SyncChestSize ||
        sectionFrame.MessageId == TerrariaMessageId.SyncChestItem ||
        sectionFrame.MessageId == TerrariaMessageId.SyncNPC ||
        sectionFrame.MessageId == TerrariaMessageId.NpcBuffs ||
        sectionFrame.MessageId == TerrariaMessageId.NetModules ||
        sectionFrame.MessageId == TerrariaMessageId.WorldBiomeTypes ||
        sectionFrame.MessageId == TerrariaMessageId.TowerShieldStrengths ||
        sectionFrame.MessageId == TerrariaMessageId.CavernMonsterTypes ||
        sectionFrame.MessageId == TerrariaMessageId.AnglerQuest ||
        sectionFrame.MessageId == TerrariaMessageId.InitialSpawn)
    {
      if (sectionFrame.MessageId == TerrariaMessageId.InitialSpawn)
      {
        break;
      }

      continue;
    }

    throw new InvalidOperationException("Server initial world stream contained an unexpected frame.");
  }

  foundAuthoritativeTile |= ContainsActiveTileAt(sectionFrame, 2100, 300);
}

if (!foundAuthoritativeTile)
{
  throw new InvalidOperationException(
    "Server did not stream the authoritative WorldGrid tile during world entry.");
}

Console.WriteLine("PASS: loopback V1456 world entry uses authoritative WorldGrid sections");

await stream.WriteAsync(TerrariaPacketCodec.Encode(new PlayerSpawnPacket(
  userSlot.Payload.Span[0],
  -1,
  -1,
  0,
  0,
  0,
  0,
  0)));
await WaitForConnectionCompletionAsync(stream);

const ushort RequestedSectionX = 0;
const ushort RequestedSectionY = 0;
await stream.WriteAsync(CreateRequestSectionFrame(RequestedSectionX, RequestedSectionY));
TerrariaFrame requestedSection = TerrariaFrameCodec.Decode(await ReadFrameAsync(stream));
if (requestedSection.MessageId != TerrariaMessageId.TileSection ||
    GetSectionOrigin(requestedSection) != (0, 0))
{
  throw new InvalidOperationException(
    "Server did not stream the client-requested world section after world entry.");
}

Console.WriteLine("PASS: active V1456 session receives a requested unloaded world section");

static bool ContainsActiveTileAt(TerrariaFrame sectionFrame, int worldX, int worldY)
{
  using MemoryStream compressed = new(sectionFrame.Payload.ToArray(), writable: false);
  using DeflateStream decompressor = new(compressed, CompressionMode.Decompress);
  using BinaryReader reader = new(decompressor);
  int originX = reader.ReadInt32();
  int originY = reader.ReadInt32();
  int width = reader.ReadInt16();
  int height = reader.ReadInt16();
  if (worldX < originX || worldX >= originX + width ||
      worldY < originY || worldY >= originY + height)
  {
    return false;
  }

  int expectedIndex = worldX - originX + (worldY - originY) * width;
  int index = 0;
  while (index < width * height)
  {
    byte flags = reader.ReadByte();
    bool isActive = (flags & 2) != 0;
    ushort type = 0;
    if (isActive)
    {
      type = reader.ReadByte();
      if ((flags & 0x20) != 0)
      {
        type |= (ushort)(reader.ReadByte() << 8);
      }
    }

    int repeatCount = (flags & 0x80) != 0 ? reader.ReadUInt16() :
      (flags & 0x40) != 0 ? reader.ReadByte() : 0;
    if (expectedIndex >= index && expectedIndex <= index + repeatCount)
    {
      return isActive && type == 1;
    }

    index += repeatCount + 1;
  }

  return false;
}

static byte[] CreateRequestSectionFrame(ushort sectionX, ushort sectionY)
{
  byte[] payload =
  [
    (byte)sectionX,
    (byte)(sectionX >> 8),
    (byte)sectionY,
    (byte)(sectionY >> 8)
  ];
  return TerrariaFrameCodec.Encode(new TerrariaFrame((TerrariaMessageId)159, payload));
}

static (int X, int Y) GetSectionOrigin(TerrariaFrame sectionFrame)
{
  using MemoryStream compressed = new(sectionFrame.Payload.ToArray(), writable: false);
  using DeflateStream decompressor = new(compressed, CompressionMode.Decompress);
  using BinaryReader reader = new(decompressor);
  return (reader.ReadInt32(), reader.ReadInt32());
}

static async Task WaitForConnectionCompletionAsync(NetworkStream stream)
{
  const int MaximumConnectionFrames = 1_100;
  for (int index = 0; index < MaximumConnectionFrames; index++)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(await ReadFrameAsync(stream));
    if (frame.MessageId == TerrariaMessageId.FinishedConnectingToServer)
    {
      return;
    }
  }

  throw new InvalidOperationException("Server did not complete the initial world connection.");
}

static async Task<byte[]> ReadFrameAsync(NetworkStream stream)
{
  using CancellationTokenSource cancellation = new(
    TimeSpan.FromSeconds(FrameReadTimeoutSeconds));
  byte[] prefix = new byte[2];
  await stream.ReadExactlyAsync(prefix, cancellation.Token);
  int length = prefix[0] | prefix[1] << 8;
  byte[] body = new byte[length - 2];
  await stream.ReadExactlyAsync(body, cancellation.Token);
  return [.. prefix, .. body];
}
