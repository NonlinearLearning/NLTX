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

const int ExpectedInitialChestCount = 23;
const int ExpectedInitialChestItemCount = ExpectedInitialChestCount * 40;
const int ExpectedJoinStateNetModuleCount = 24;
const int ExpectedConnectionEquipmentFrameCount = 189;
const int MaximumConnectionProjectionFrames = 256;

using DomeServer? server = CreateServer(args, out int port);

using TcpClient client = new();
await client.ConnectAsync("127.0.0.1", port);
NetworkStream stream = client.GetStream();
byte playerSlot = await SendFullClientBootstrapAsync(stream, server);
IReadOnlyList<TerrariaFrame> completionFrames = await ReadCompletionFramesAsync(stream);

AssertCompletionProjection(completionFrames);
await AssertDefaultNpcSettlementFramesAsync(stream, server);
await SendConcurrentActiveTrafficAsync(stream, playerSlot);
await AssertPingIsAcknowledgedAsync(stream);
await AssertActiveConnectionAsync(stream, playerSlot);
await AssertClientProjectileTerminationKeepsConnectionAsync(stream, playerSlot);
await AssertPingIsAcknowledgedAsync(stream);
await AssertActivePlayerEquipmentKeepsConnectionAsync(stream, playerSlot);
await AssertPingIsAcknowledgedAsync(stream);
await AssertActivePlayerVitalsKeepConnectionAsync(stream, playerSlot);
await AssertPingIsAcknowledgedAsync(stream);
await AssertClientSyncedInventoryKeepsConnectionAsync(stream);
await AssertPingIsAcknowledgedAsync(stream);
await AssertClientTalkNpcKeepsConnectionAsync(stream, playerSlot);
await AssertPingIsAcknowledgedAsync(stream);
Console.WriteLine("PASS: full-client bootstrap completes with server-owned player authority replay");

static DomeServer? CreateServer(string[] arguments, out int port)
{
  if (arguments.Length == 0)
  {
    DomeServer server = new();
    server.Start();
    port = server.Port;
    return server;
  }

  if (arguments.Length == 2 && arguments[0] == "--external-port" &&
      int.TryParse(arguments[1], out int externalPort) &&
      externalPort is >= 1 and <= ushort.MaxValue)
  {
    port = externalPort;
    return null;
  }

  throw new ArgumentException(
    "Usage: --external-port <1-65535>, or omit arguments to use an in-process server.");
}

static async Task<byte> SendFullClientBootstrapAsync(NetworkStream stream, DomeServer? server)
{
  await stream.WriteAsync(TerrariaPacketCodec.Encode(new HelloPacket()));
  TerrariaFrame userSlotFrame = TerrariaFrameCodec.Decode(await ReadFrameAsync(stream));
  if (userSlotFrame.MessageId != TerrariaMessageId.SetUserSlot)
  {
    throw new InvalidOperationException("Server did not assign a player slot.");
  }

  byte playerSlot = userSlotFrame.Payload.Span[0];
  TerrariaFrame initialNetModule = TerrariaFrameCodec.Decode(await ReadFrameAsync(stream));
  if (initialNetModule.MessageId != TerrariaMessageId.NetModules ||
      !initialNetModule.Payload.Span.SequenceEqual(new byte[] { 0, 0, 0, 0 }))
  {
    throw new InvalidOperationException(
      "Full-client bootstrap did not receive the initial NetModules state after SetUserSlot.");
  }

  TerrariaColor color = new(0, 0, 0);
  await stream.WriteAsync(TerrariaPacketCodec.Encode(new PlayerProfilePacket(
    playerSlot,
    0,
    0,
    0.0f,
    0,
    "FullClientBootstrap",
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
    "11111111-1111-1111-1111-111111111111")));
  await stream.WriteAsync(TerrariaPacketCodec.EncodePlayerLifeMana(playerSlot, 100, 100));
  await stream.WriteAsync(TerrariaPacketCodec.EncodePlayerMana(playerSlot, 20, 20));
  await stream.WriteAsync(TerrariaPacketCodec.Encode(new PlayerBuffsPacket(playerSlot, [])));
  await stream.WriteAsync(TerrariaPacketCodec.Encode(new PlayerLoadoutPacket(playerSlot, 0, 0)));
  await stream.WriteAsync(TerrariaPacketCodec.Encode(new PlayerEquipmentPacket(
    playerSlot,
    0,
    1,
    0,
    1,
    IsFavorited: false,
    IsNewAndShiny: false)));
  await stream.WriteAsync(TerrariaPacketCodec.EncodeRequestWorldData());
  TerrariaFrame worldData = TerrariaFrameCodec.Decode(await ReadFrameAsync(stream));
  if (worldData.MessageId != TerrariaMessageId.WorldData)
  {
    throw new InvalidOperationException("Full-client bootstrap did not receive world data.");
  }

  const short authoritativeSpawnX = 2100;
  const short authoritativeSpawnY = 300;
  const short clientRequestedSpawn = -1;
  await stream.WriteAsync(TerrariaPacketCodec.EncodeSpawnTileData(
    new SpawnTileDataRequestPacket(clientRequestedSpawn, clientRequestedSpawn, 0)));
  TerrariaFrame refreshedWorldData = TerrariaFrameCodec.Decode(await ReadFrameAsync(stream));
  if (refreshedWorldData.MessageId != TerrariaMessageId.WorldData)
  {
    throw new InvalidOperationException(
      "The initial tile request did not refresh WorldData before the section stream.");
  }

  TerrariaFrame statusText = TerrariaFrameCodec.Decode(await ReadFrameAsync(stream));
  if (statusText.MessageId != TerrariaMessageId.StatusTextSize)
  {
    throw new InvalidOperationException("The initial world stream did not begin with status text.");
  }

  bool containsActiveTile = false;
  bool containsSpawnTile = false;
  int chestItemCount = 0;
  int chestSizeCount = 0;
  int anglerQuestCount = 0;
  int cavernMonsterTypesCount = 0;
  int npcBuffCount = 0;
  int npcCount = 0;
  int sectionCount = 0;
  int towerShieldStrengthsCount = 0;
  int worldBiomeTypesCount = 0;
  List<TerrariaFrame> joinStateNetModules = new();
  while (true)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(await ReadFrameAsync(stream));
    if (frame.MessageId == TerrariaMessageId.TileSection)
    {
      containsActiveTile |= ContainsActiveTile(frame);
      containsSpawnTile |= ContainsTile(frame, authoritativeSpawnX, authoritativeSpawnY);
      sectionCount++;
      continue;
    }

    if (frame.MessageId == TerrariaMessageId.SyncChestSize)
    {
      chestSizeCount++;
      continue;
    }

    if (frame.MessageId == TerrariaMessageId.SyncChestItem)
    {
      chestItemCount++;
      continue;
    }

    if (frame.MessageId == TerrariaMessageId.SyncNPC)
    {
      npcCount++;
      continue;
    }

    if (frame.MessageId == TerrariaMessageId.NpcBuffs)
    {
      npcBuffCount++;
      continue;
    }

    if (frame.MessageId == TerrariaMessageId.NetModules)
    {
      joinStateNetModules.Add(frame);
      continue;
    }

    if (frame.MessageId == TerrariaMessageId.WorldBiomeTypes)
    {
      worldBiomeTypesCount++;
      continue;
    }

    if (frame.MessageId == TerrariaMessageId.AnglerQuest)
    {
      anglerQuestCount++;
      continue;
    }

    if (frame.MessageId == TerrariaMessageId.TowerShieldStrengths)
    {
      towerShieldStrengthsCount++;
      continue;
    }

    if (frame.MessageId == TerrariaMessageId.CavernMonsterTypes)
    {
      cavernMonsterTypesCount++;
      continue;
    }

    if (frame.MessageId == TerrariaMessageId.InitialSpawn)
    {
      break;
    }

    throw new InvalidOperationException("The initial world stream contained an unexpected frame.");
  }

  if (sectionCount != 15 || chestSizeCount != ExpectedInitialChestCount ||
       chestItemCount != ExpectedInitialChestItemCount || worldBiomeTypesCount != 1 ||
       towerShieldStrengthsCount != 1 || cavernMonsterTypesCount != 1 || anglerQuestCount != 1 ||
       npcCount != 2 || npcBuffCount != 2 || !containsActiveTile || !containsSpawnTile)
  {
    throw new InvalidOperationException(
      $"The default server did not project its initial terrain and chest objects. " +
      $"sections={sectionCount}, chests={chestSizeCount}, items={chestItemCount}, " +
      $"worldBiome={worldBiomeTypesCount}, towers={towerShieldStrengthsCount}, " +
      $"cavern={cavernMonsterTypesCount}, quest={anglerQuestCount}, " +
      $"npcs={npcCount}, buffs={npcBuffCount}, active={containsActiveTile}, " +
      $"spawn={containsSpawnTile}, snapshotNpcs={server?.LatestSnapshot.Npcs.Count}, " +
      $"snapshotActiveNpcs={server?.LatestSnapshot.Npcs.Count(npc => npc.Health > 0)}, " +
      $"replicationNpcs={server?.CreateNpcReplicationSnapshots().Count}.");
  }

  AssertJoinStateNetModules(joinStateNetModules);

  await stream.WriteAsync(TerrariaPacketCodec.Encode(new PlayerSpawnPacket(
    playerSlot,
    clientRequestedSpawn,
    clientRequestedSpawn,
    0,
    0,
    0,
    0,
    0)));
  return playerSlot;
}

static void AssertJoinStateNetModules(IReadOnlyList<TerrariaFrame> frames)
{
  if (frames.Count != ExpectedJoinStateNetModuleCount)
  {
    throw new InvalidOperationException(
      $"Expected {ExpectedJoinStateNetModuleCount} join-state NetModules, received {frames.Count}.");
  }

  ReadOnlySpan<byte> banner = frames[0].Payload.Span;
  if (banner.Length != 1765 ||
      !banner[..5].SequenceEqual(new byte[] { 10, 0, 0, 37, 1 }))
  {
    throw new InvalidOperationException("Join-state Banner NetModule did not encode the default state.");
  }

  for (ushort powerId = 0; powerId < 15; powerId++)
  {
    ReadOnlySpan<byte> permission = frames[powerId + 1].Payload.Span;
    if (!permission.SequenceEqual(new byte[]
      {
        9,
        0,
        0,
        (byte)powerId,
        0,
        2
      }))
    {
      throw new InvalidOperationException(
        $"Creative permission NetModule {powerId} was not projected in join order.");
    }
  }

  int[] expectedCreativePowerIds = [0, 5, 8, 9, 10, 11, 12, 13];
  int[] expectedCreativeStateLengths = [5, 37, 8, 5, 5, 37, 8, 5];
  for (int index = 0; index < expectedCreativePowerIds.Length; index++)
  {
    ReadOnlySpan<byte> state = frames[index + 16].Payload.Span;
    if (state.Length != expectedCreativeStateLengths[index] || state[0] != 5 || state[1] != 0 ||
        state[2] != expectedCreativePowerIds[index] || state[3] != 0 || state[4] != 0)
    {
      throw new InvalidOperationException(
        $"Creative state NetModule {index} was not projected in join order.");
    }

    if (expectedCreativePowerIds[index] == 11 &&
        (state[5] != byte.MaxValue || state[^1] != 127))
    {
      throw new InvalidOperationException("Creative power 11 did not preserve its default bitset.");
    }
  }
}

static bool ContainsActiveTile(TerrariaFrame section)
{
  using MemoryStream compressed = new(section.Payload.ToArray(), writable: false);
  using DeflateStream decompressor = new(compressed, CompressionMode.Decompress);
  using BinaryReader reader = new(decompressor);
  _ = reader.ReadInt32();
  _ = reader.ReadInt32();
  int width = reader.ReadInt16();
  int height = reader.ReadInt16();
  int tileCount = width * height;
  int tileIndex = 0;
  while (tileIndex < tileCount)
  {
    byte flags = reader.ReadByte();
    bool isActive = (flags & 2) != 0;
    if (isActive)
    {
      return true;
    }

    int repeatCount = (flags & 0x80) != 0 ? reader.ReadUInt16() :
      (flags & 0x40) != 0 ? reader.ReadByte() : 0;
    tileIndex += repeatCount + 1;
  }

  return false;
}

static bool ContainsTile(TerrariaFrame section, int tileX, int tileY)
{
  using MemoryStream compressed = new(section.Payload.ToArray(), writable: false);
  using DeflateStream decompressor = new(compressed, CompressionMode.Decompress);
  using BinaryReader reader = new(decompressor);
  int originX = reader.ReadInt32();
  int originY = reader.ReadInt32();
  int width = reader.ReadInt16();
  int height = reader.ReadInt16();
  return tileX >= originX && tileX < originX + width &&
    tileY >= originY && tileY < originY + height;
}

static async Task<IReadOnlyList<TerrariaFrame>> ReadCompletionFramesAsync(NetworkStream stream)
{
  List<TerrariaFrame> frames = new();
  for (int index = 0; index < MaximumConnectionProjectionFrames; index++)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(await ReadFrameAsync(stream));
    frames.Add(frame);
    if (frame.MessageId == TerrariaMessageId.FinishedConnectingToServer)
    {
      return frames;
    }
  }

  throw new InvalidOperationException(
    $"Server did not finish the full-client bootstrap. " +
    $"Frames={frames.Count}, IDs={string.Join(',', frames.Select(frame => frame.MessageId))}.");
}

static void AssertCompletionProjection(IReadOnlyList<TerrariaFrame> frames)
{
  List<TerrariaMessageId> messageIds = new(frames.Count);
  for (int index = 0; index < frames.Count; index++)
  {
    messageIds.Add(frames[index].MessageId);
  }

  int finished = IndexOf(messageIds, TerrariaMessageId.FinishedConnectingToServer);
  int hostStatus = IndexOf(messageIds, TerrariaMessageId.HostStatus);
  int npcHomes = Count(messageIds, TerrariaMessageId.NpcHome);
  if (finished != messageIds.Count - 1 || hostStatus < 0 || hostStatus >= finished ||
      npcHomes != 2 || Count(messageIds, TerrariaMessageId.SyncPlayer) != 1 ||
      Count(messageIds, TerrariaMessageId.PlayerLifeMana) != 1 ||
      Count(messageIds, TerrariaMessageId.ItemRotationAndAnimation) != 1 ||
      Count(messageIds, TerrariaMessageId.PlayerBuffs) != 1 ||
      Count(messageIds, TerrariaMessageId.SyncLoadout) != 1 ||
      Count(messageIds, TerrariaMessageId.SyncEquipment) != ExpectedConnectionEquipmentFrameCount ||
      Count(messageIds, TerrariaMessageId.PlayerActive) != 1 ||
      Count(messageIds, TerrariaMessageId.PlayerControls) != 1 ||
      IndexOf(messageIds, TerrariaMessageId.SyncPlayer) >= hostStatus ||
      IndexOf(messageIds, TerrariaMessageId.PlayerLifeMana) >= hostStatus ||
      IndexOf(messageIds, TerrariaMessageId.ItemRotationAndAnimation) >= hostStatus ||
      IndexOf(messageIds, TerrariaMessageId.PlayerBuffs) >= hostStatus ||
      IndexOf(messageIds, TerrariaMessageId.SyncLoadout) >= hostStatus ||
      IndexOf(messageIds, TerrariaMessageId.PlayerActive) >= hostStatus ||
      IndexOf(messageIds, TerrariaMessageId.PlayerControls) >= hostStatus ||
      IndexOf(messageIds, TerrariaMessageId.PlayerSpawn) >= 0 ||
      IndexOf(messageIds, TerrariaMessageId.Ping) >= 0)
  {
    throw new InvalidOperationException(
      "Full-client completion did not contain one ordered server-owned authority projection.");
  }
}

static int Count(IReadOnlyList<TerrariaMessageId> messageIds, TerrariaMessageId messageId)
{
  int count = 0;
  for (int index = 0; index < messageIds.Count; index++)
  {
    if (messageIds[index] == messageId)
    {
      count++;
    }
  }

  return count;
}

static async Task AssertActiveConnectionAsync(NetworkStream stream, byte playerSlot)
{
  await stream.WriteAsync(TerrariaPacketCodec.EncodePlayerControls(new PlayerControlIntent(
    playerSlot,
    MoveLeft: false,
    MoveRight: true,
    Jump: false,
    UseItem: false,
    FacingRight: true,
    SelectedItem: 0),
    2100.0f,
    300.0f));
  _ = await ReadFrameAsync(stream);
}

static async Task AssertDefaultNpcSettlementFramesAsync(NetworkStream stream, DomeServer? server)
{
  int npcFrameCount = 0;
  HashSet<short> npcIdentities = new();
  using CancellationTokenSource collectionTimeout = new(TimeSpan.FromSeconds(3));
  try
  {
    while (!collectionTimeout.IsCancellationRequested && npcIdentities.Count < 2)
    {
      TerrariaFrame frame = TerrariaFrameCodec.Decode(
        await ReadFrameWithTokenAsync(stream, collectionTimeout.Token));
      if (frame.MessageId == TerrariaMessageId.SyncNPC && frame.Payload.Length >= 2)
      {
        npcFrameCount++;
        npcIdentities.Add(BitConverter.ToInt16(frame.Payload.Span[..2]));
      }
    }
  }
  catch (OperationCanceledException exception)
  {
    string npcState = server is null
      ? "external"
      : string.Join(",", server.CreateNpcReplicationSnapshots().Select(npc =>
        $"{npc.ReplicationId}:{npc.Position.X:F1},{npc.Position.Y:F1}," +
        $"section={npc.Section.X},{npc.Section.Y},active={npc.IsActive}"));
    throw new InvalidOperationException(
      $"Default NPC settlement did not quiesce after {npcFrameCount} frames. " +
      $"Npcs={npcState}.",
      exception);
  }

  if (npcIdentities.Count < 2)
  {
    string npcState = server is null
      ? "external"
      : string.Join(",", server.CreateNpcReplicationSnapshots().Select(npc =>
        $"{npc.ReplicationId}:{npc.Position.X:F1},{npc.Position.Y:F1}," +
        $"section={npc.Section.X},{npc.Section.Y},active={npc.IsActive}"));
    throw new InvalidOperationException(
      $"Default NPC settlement did not emit both replication identities. " +
      $"frames={npcFrameCount}, identities={npcIdentities.Count}, " +
      $"tick={server?.LatestSnapshot.Tick}, Npcs={npcState}.");
  }
}

static async Task SendConcurrentActiveTrafficAsync(NetworkStream stream, byte playerSlot)
{
  byte[] playerZonePayload = [playerSlot, 0, 0, 0, 0, 0, 0];
  await stream.WriteAsync(TerrariaFrameCodec.Encode(new TerrariaFrame(
    TerrariaMessageId.SyncPlayerZone,
    playerZonePayload)));
  await stream.WriteAsync(TerrariaPacketCodec.Encode(new PlayerBuffsPacket(playerSlot, [])));
  await stream.WriteAsync(TerrariaPacketCodec.EncodePlayerControls(new PlayerControlIntent(
    playerSlot,
    MoveLeft: false,
    MoveRight: true,
    Jump: false,
    UseItem: false,
    FacingRight: true,
    SelectedItem: 0),
    2100.0f,
    300.0f));
  await stream.WriteAsync(CreateClientProjectileFrame(playerSlot));
  byte[] uniqueTownNpcInfoRequest = [0x05, 0x00, 0x38, 0x01, 0x00];
  await stream.WriteAsync(uniqueTownNpcInfoRequest);
  await stream.WriteAsync(TerrariaPacketCodec.EncodePing());
}

static byte[] CreateClientProjectileFrame(byte playerSlot)
{
  using MemoryStream payload = new();
  using (BinaryWriter writer = new(payload, System.Text.Encoding.UTF8, leaveOpen: true))
  {
    writer.Write((short)0);
    writer.Write(0.0f);
    writer.Write(0.0f);
    writer.Write(0.0f);
    writer.Write(0.0f);
    writer.Write(playerSlot);
    writer.Write((short)0);
    writer.Write((byte)0);
  }

  return TerrariaFrameCodec.Encode(new TerrariaFrame(
    TerrariaMessageId.SyncProjectile,
    payload.ToArray()));
}

static async Task AssertClientProjectileTerminationKeepsConnectionAsync(
  NetworkStream stream,
  byte playerSlot)
{
  byte[] capturedClientTermination = [0x06, 0x00, 0x1D, 0x01, 0x00, playerSlot];
  await stream.WriteAsync(capturedClientTermination);
}

static async Task AssertActivePlayerEquipmentKeepsConnectionAsync(
  NetworkStream stream,
  byte playerSlot)
{
  for (int slotId = 0; slotId <= 6; slotId++)
  {
    await stream.WriteAsync(TerrariaPacketCodec.Encode(new PlayerEquipmentPacket(
      playerSlot,
      slotId,
      0,
      0,
      0,
      IsFavorited: false,
      IsNewAndShiny: false)));
  }
}

static async Task AssertActivePlayerVitalsKeepConnectionAsync(NetworkStream stream, byte playerSlot)
{
  await stream.WriteAsync(TerrariaPacketCodec.EncodePlayerLifeMana(playerSlot, 1, 20));
  await stream.WriteAsync(TerrariaPacketCodec.EncodePlayerMana(playerSlot, 1, 20));
}

static async Task AssertClientSyncedInventoryKeepsConnectionAsync(NetworkStream stream)
{
  byte[] clientSyncedInventory = [0x03, 0x00, 0x8A];
  await stream.WriteAsync(clientSyncedInventory);
}

static async Task AssertClientTalkNpcKeepsConnectionAsync(NetworkStream stream, byte playerSlot)
{
  byte[] clearedTalkNpc = [0x06, 0x00, 0x28, playerSlot, 0xFF, 0xFF];
  await stream.WriteAsync(clearedTalkNpc);
}

static async Task AssertPingIsAcknowledgedAsync(NetworkStream stream)
{
  await stream.WriteAsync(TerrariaPacketCodec.EncodePing());
  List<TerrariaMessageId> receivedMessageIds = new();
  for (int index = 0; index < MaximumConnectionProjectionFrames; index++)
  {
    TerrariaFrame response = TerrariaFrameCodec.Decode(await ReadFrameAsync(stream));
    receivedMessageIds.Add(response.MessageId);
    if (response.MessageId == TerrariaMessageId.Ping && response.Payload.IsEmpty)
    {
      return;
    }
  }

  throw new InvalidOperationException(
    $"Server did not acknowledge the full-client Ping packet. Received: " +
    string.Join(", ", receivedMessageIds));
}

static int IndexOf(IReadOnlyList<TerrariaMessageId> messageIds, TerrariaMessageId messageId)
{
  for (int index = 0; index < messageIds.Count; index++)
  {
    if (messageIds[index] == messageId)
    {
      return index;
    }
  }

  return -1;
}

static async Task<byte[]> ReadFrameAsync(NetworkStream stream)
{
  using CancellationTokenSource timeout = new(TimeSpan.FromSeconds(5));
  try
  {
    return await ReadFrameWithTokenAsync(stream, timeout.Token);
  }
  catch (OperationCanceledException exception)
  {
    throw new TimeoutException(
      "Timed out while waiting for a full-client bootstrap frame.",
      exception);
  }
}

static async Task<byte[]> ReadFrameWithTokenAsync(
  NetworkStream stream,
  CancellationToken cancellationToken)
{
  byte[] prefix = new byte[2];
  await stream.ReadExactlyAsync(prefix, cancellationToken);
  int frameLength = prefix[0] | prefix[1] << 8;
  if (frameLength < 3)
  {
    throw new InvalidDataException("Server sent an invalid Terraria frame length.");
  }

  byte[] frame = new byte[frameLength];
  prefix.CopyTo(frame, 0);
  await stream.ReadExactlyAsync(frame.AsMemory(2), cancellationToken);
  return frame;
}
