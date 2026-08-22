using System;
using System.IO;
using System.IO.Compression;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Terraria.Dome.Protocol.V1456.Packets;
using Terraria.Dome.Protocol.V1456.Protocol;
using Terraria.Dome.Server;
using Terraria.Dome.Simulation;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Simulation.WorldModel.Systems;

WorldGrid world = new(4200, 1200);
WorldMetadata metadata = new(
  "blood-moon-loopback",
  new WorldSeed(21),
  4200,
  1200,
  spawnX: 2100,
  spawnY: 300);
using DomeSimulation meteorSimulation = new(world);
if (!meteorSimulation.TryQueueWorldMeteorImpact(new WorldMeteorImpactCommand(2100, 600, 1)))
{
  throw new InvalidOperationException("The loopback meteor fixture could not queue its impact.");
}

meteorSimulation.Tick(new SimulationInputBatch());
DomeSimulationSnapshot snapshot = new(
  world.CreateSnapshot(metadata),
  [],
  [],
  1,
  worldClock: new WorldClockSnapshot(1, 1, false, false, 1),
  worldRules: new WorldRuleState(
    rainTimeTicks: 6000,
    rainStrength: 0.5f,
    windSpeedTarget: 0.4f,
    windSpeedCurrent: 0.2f),
  progression: new WorldProgressionState(isBloodMoon: true, slimeRainTimeTicks: 6000));
using DomeServer server = new(snapshot);
server.Start();
using TcpClient client = new();
await client.ConnectAsync("127.0.0.1", server.Port);
using NetworkStream stream = client.GetStream();
TerrariaFrame worldData;
try
{
  worldData = await ActivateAsync(stream, 2100, 300);
}
catch (Exception exception)
{
  throw new InvalidOperationException(
    $"World rules loopback activation failed. Server fault: {server.LastSessionFault}; " +
    $"simulation fault: {server.SimulationFault}",
    exception);
}
if (worldData.Payload.Length < 5 || worldData.Payload.Span[4] != 2)
{
  throw new InvalidOperationException("Loopback WorldData did not contain the blood-moon flag.");
}

(float windSpeedTarget, float maximumRaining, byte eventFlags3) = ReadWorldDataProjection(worldData);
if (windSpeedTarget != 0.4f)
{
  throw new InvalidOperationException(
    $"Loopback WorldData did not contain authoritative wind target. Value={windSpeedTarget}");
}

if (maximumRaining != 0.5f)
{
  throw new InvalidOperationException(
    $"Loopback WorldData did not contain authoritative rain strength. Value={maximumRaining}");
}

if ((eventFlags3 & 4) == 0)
{
  throw new InvalidOperationException("Loopback WorldData did not contain the slime-rain flag.");
}

await stream.WriteAsync(CreateRequestSectionFrame(sectionX: 10, sectionY: 4));
TerrariaFrame meteorSection = TerrariaFrameCodec.Decode(
  await ReadFrameAsync(stream, CancellationToken.None));
if (meteorSection.MessageId != TerrariaMessageId.TileSection ||
    !ContainsActiveTileType(meteorSection, 2115, 605, WorldMeteorImpactSystem.MeteoriteTileType))
{
  throw new InvalidOperationException(
    "Real TCP section replication did not contain the authoritative meteorite tile.");
}

Console.WriteLine("PASS: real TCP section replication carries the meteor impact");

int worldTimeFrameCount = 0;
using CancellationTokenSource timeout = new(TimeSpan.FromSeconds(2));
try
{
  while (!timeout.IsCancellationRequested)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(await ReadFrameAsync(stream, timeout.Token));
    if (frame.MessageId == TerrariaMessageId.SetTime)
    {
      worldTimeFrameCount++;
    }
  }
}
catch (OperationCanceledException)
{
}

if (worldTimeFrameCount != 0)
{
  throw new InvalidOperationException(
    $"Stable world entry emitted unexpected SetTime frames. Frames={worldTimeFrameCount}.");
}

Console.WriteLine("PASS: stable world entry does not redundantly project world time");

static (float WindSpeedTarget, float MaximumRaining, byte EventFlags3) ReadWorldDataProjection(
  TerrariaFrame worldData)
{
  using MemoryStream stream = new(worldData.Payload.ToArray());
  using BinaryReader reader = new(stream);
  _ = reader.ReadInt32();
  _ = reader.ReadByte();
  _ = reader.ReadByte();
  for (int index = 0; index < 6; index++)
  {
    _ = reader.ReadInt16();
  }

  _ = reader.ReadInt32();
  _ = reader.ReadString();
  _ = reader.ReadByte();
  _ = reader.ReadBytes(16);
  _ = reader.ReadUInt64();
  _ = reader.ReadByte();
  for (int index = 0; index < 16; index++)
  {
    _ = reader.ReadByte();
  }

  float windSpeedTarget = reader.ReadSingle();
  _ = reader.ReadByte();
  for (int index = 0; index < 3; index++)
  {
    _ = reader.ReadInt32();
  }

  for (int index = 0; index < 4; index++)
  {
    _ = reader.ReadByte();
  }

  for (int index = 0; index < 3; index++)
  {
    _ = reader.ReadInt32();
  }

  for (int index = 0; index < 17; index++)
  {
    _ = reader.ReadByte();
  }

  float maximumRaining = reader.ReadSingle();
  _ = reader.ReadByte();
  _ = reader.ReadByte();
  byte eventFlags3 = reader.ReadByte();
  return (windSpeedTarget, maximumRaining, eventFlags3);
}

static async Task<TerrariaFrame> ActivateAsync(NetworkStream stream, short spawnX, short spawnY)
{
  await stream.WriteAsync(TerrariaPacketCodec.Encode(new HelloPacket()));
  TerrariaFrame userSlot = TerrariaFrameCodec.Decode(
    await ReadFrameAsync(stream, CancellationToken.None));
  TerrariaFrame initialNetModules = TerrariaFrameCodec.Decode(
    await ReadFrameAsync(stream, CancellationToken.None));
  if (initialNetModules.MessageId != TerrariaMessageId.NetModules ||
      !initialNetModules.Payload.Span.SequenceEqual(new byte[] { 0, 0, 0, 0 }))
  {
    throw new InvalidOperationException("World rules session did not receive initial NetModules.");
  }

  TerrariaColor color = new(0, 0, 0);
  await stream.WriteAsync(TerrariaPacketCodec.Encode(new PlayerProfilePacket(
    userSlot.Payload.Span[0], 0, 0, 0.0f, 0, "Clock", 0, 0, 0,
    color, color, color, color, color, color, color, 0, 0, 0)));
  await stream.WriteAsync(TerrariaPacketCodec.Encode(new PlayerUuidPacket(
    "44444444-4444-4444-4444-444444444444")));
  await stream.WriteAsync(TerrariaPacketCodec.EncodeRequestWorldData());
  TerrariaFrame worldData = TerrariaFrameCodec.Decode(
    await ReadFrameAsync(stream, CancellationToken.None));
  if (worldData.MessageId != TerrariaMessageId.WorldData)
  {
    throw new InvalidOperationException("World rules session did not receive WorldData.");
  }

  await stream.WriteAsync(TerrariaPacketCodec.EncodeSpawnTileData(
    new SpawnTileDataRequestPacket(spawnX, spawnY, 0)));
  TerrariaFrame refreshedWorldData = TerrariaFrameCodec.Decode(
    await ReadFrameAsync(stream, CancellationToken.None));
  if (refreshedWorldData.MessageId != TerrariaMessageId.WorldData)
  {
    throw new InvalidOperationException("World rules session did not receive refreshed WorldData.");
  }

  while (true)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(
      await ReadFrameAsync(stream, CancellationToken.None));
    if (frame.MessageId == TerrariaMessageId.InitialSpawn)
    {
      break;
    }
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
  bool receivedCompletion = false;
  bool receivedProfile = false;
  bool receivedLife = false;
  bool receivedMana = false;
  bool receivedBuffs = false;
  bool receivedLoadout = false;
  bool receivedActive = false;
  bool receivedControls = false;
  for (int index = 0; index < 256 && !receivedCompletion; index++)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(
      await ReadFrameAsync(stream, CancellationToken.None));
    if (frame.MessageId == TerrariaMessageId.SyncPlayer)
    {
      receivedProfile = true;
    }
    else if (frame.MessageId == TerrariaMessageId.PlayerLifeMana)
    {
      receivedLife = true;
    }
    else if (frame.MessageId == TerrariaMessageId.ItemRotationAndAnimation)
    {
      receivedMana = true;
    }
    else if (frame.MessageId == TerrariaMessageId.PlayerBuffs)
    {
      receivedBuffs = true;
    }
    else if (frame.MessageId == TerrariaMessageId.SyncLoadout)
    {
      receivedLoadout = true;
    }
    else if (frame.MessageId == TerrariaMessageId.PlayerActive)
    {
      receivedActive = true;
    }
    else if (frame.MessageId == TerrariaMessageId.PlayerControls)
    {
      receivedControls = true;
    }
    else if (frame.MessageId == TerrariaMessageId.HostStatus)
    {
      receivedHostStatus = frame.Payload.Span.SequenceEqual(
        new byte[] { userSlot.Payload.Span[0], 1 });
    }
    else if (frame.MessageId == TerrariaMessageId.FinishedConnectingToServer)
    {
      receivedCompletion = true;
    }
  }

  if (!receivedProfile || !receivedLife || !receivedMana || !receivedBuffs ||
      !receivedLoadout || !receivedActive || !receivedControls ||
      !receivedHostStatus || !receivedCompletion)
  {
    throw new InvalidOperationException(
      "World rules session did not receive a complete authority bootstrap before completion.");
  }

  return worldData;
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

static bool ContainsActiveTileType(
  TerrariaFrame sectionFrame,
  int worldX,
  int worldY,
  ushort expectedType)
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

    int repeatCount = (flags & 0x80) != 0
      ? reader.ReadUInt16()
      : (flags & 0x40) != 0
        ? reader.ReadByte()
        : 0;
    if (expectedIndex >= index && expectedIndex <= index + repeatCount)
    {
      return isActive && type == expectedType;
    }

    index += repeatCount + 1;
  }

  return false;
}
