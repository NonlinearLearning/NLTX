using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Terraria.Dome.Protocol.V1456.Dispatch;
using Terraria.Dome.Protocol.V1456.Packets;
using Terraria.Dome.Protocol.V1456.Protocol;
using Terraria.Dome.Protocol.V1456.Session;
using Terraria.Dome.Server;
using Terraria.Dome.Server.Replication;
using Terraria.Dome.Simulation;
using Terraria.Dome.Simulation.WorldModel;

IReadOnlyList<byte[]> projectedPlayerFrames = PlayerStateProjection.CreateFrames(
  1,
  new PlayerSnapshot(
    new PlayerHandle(1),
    new SimulationVector(20.0f, 5.0f),
    new SimulationVector(1.0f, 0.0f),
    1,
    true,
    80,
    true,
    0,
    "22222222-2222-2222-2222-222222222222",
    1,
    15,
    20),
  new PlayerStateSnapshot(
    new PlayerHandle(1),
    true,
    80,
    100,
    0,
    "22222222-2222-2222-2222-222222222222",
    1,
    15,
    20));
TerrariaMessageId[] projectedMessageIds = projectedPlayerFrames
  .Select(frame => TerrariaFrameCodec.Decode(frame).MessageId)
  .ToArray();
if (!projectedMessageIds.SequenceEqual([
      TerrariaMessageId.PlayerActive,
      TerrariaMessageId.PlayerLifeMana,
      TerrariaMessageId.PlayerControls,
      TerrariaMessageId.ItemRotationAndAnimation]))
{
  throw new InvalidOperationException(
    "Player replication projection did not derive the expected typed V1456 frames.");
}

Console.WriteLine("PASS: player replication frames are projected from immutable snapshots");

WorldGrid world = new(4200, 1200);
SectionVisibilitySelector selector = new();
PlayerSnapshot firstPosition = new(
  new PlayerHandle(1),
  new SimulationVector(2000.0f, 0.0f),
  new SimulationVector(0.0f, 0.0f),
  1,
  true,
  100,
  true,
  0);
PlayerSnapshot movedPosition = firstPosition with
{
  Position = new SimulationVector(2400.0f, 0.0f)
};
IReadOnlyList<WorldSectionCoordinates> firstSections = selector.Select(world, firstPosition);
IReadOnlyList<WorldSectionCoordinates> movedSections = selector.Select(world, movedPosition);
if (!firstSections.Contains(new WorldSectionCoordinates(8, 0)) ||
    !movedSections.Contains(new WorldSectionCoordinates(13, 0)) ||
    movedSections.Contains(new WorldSectionCoordinates(8, 0)))
{
  throw new InvalidOperationException("PVS did not follow the authoritative player section.");
}

SessionReplicationState cursor = new((_, _) => Task.CompletedTask);
IReadOnlyList<WorldSectionSnapshot> initial = CreateSnapshots(world, firstSections);
if (cursor.ReplaceVisibleSections(initial).Count != 15 ||
    cursor.VisibleSections.Count != 15)
{
  throw new InvalidOperationException("PVS did not initialize the session cursor.");
}

IReadOnlyList<WorldSectionSnapshot> moved = CreateSnapshots(world, movedSections);
if (cursor.ReplaceVisibleSections(moved).Count == 0 ||
    cursor.VisibleSections.Contains(new WorldSectionCoordinates(8, 0)) ||
    !cursor.VisibleSections.Contains(new WorldSectionCoordinates(13, 0)))
{
  throw new InvalidOperationException("PVS did not replace departed section cursor state.");
}

cursor.Dispose();
Console.WriteLine("PASS: authoritative player movement replaces per-session PVS sections");

SessionReplicationState clientPositionCursor = new((_, _) => Task.CompletedTask);
_ = clientPositionCursor.ReplaceVisibleSections(CreateSnapshots(world, firstSections));
clientPositionCursor.SetClientViewPosition(new SimulationVector(2400.0f, 0.0f));
WorldSectionReplication worldReplication = new(world);
if (worldReplication.UpdatePlayerVisibility(firstPosition, clientPositionCursor).Count == 0 ||
    !clientPositionCursor.VisibleSections.Contains(new WorldSectionCoordinates(13, 0)) ||
    clientPositionCursor.VisibleSections.Contains(new WorldSectionCoordinates(8, 0)))
{
  throw new InvalidOperationException(
    "Client PlayerControls position did not advance the authoritative PVS view.");
}

clientPositionCursor.Dispose();
Console.WriteLine("PASS: client control position advances the PVS view without replacing simulation authority");

PlayerReplicationCursor playerCursor = new();
PlayerReplicationState initialPlayer = new(
  1,
  new SimulationVector(10.0f, 0.0f),
  new SimulationVector(0.0f, 0.0f),
  1,
  true,
  100);
if (playerCursor.CollectChanged([initialPlayer]).Count != 1 ||
    playerCursor.CollectChanged([initialPlayer]).Count != 0 ||
    playerCursor.CollectChanged([initialPlayer with
    {
      Position = new SimulationVector(11.0f, 0.0f)
    }]).Count != 1)
{
  throw new InvalidOperationException("Player replication cursor did not emit only changed state.");
}

Console.WriteLine("PASS: player replication cursor suppresses unchanged snapshots");

VerifyMountCompatibilityStateIsSessionLocal();
Console.WriteLine("PASS: mount compatibility state remains outside Simulation commands");

using DomeServer server = new();
server.Start();
using TcpClient firstClient = new();
await firstClient.ConnectAsync("127.0.0.1", server.Port);
await ActivateAsync(firstClient.GetStream(), 2000, 280);
await WaitForPlayerCountAsync(server, 1);
firstClient.Dispose();
await WaitForPlayerCountAsync(server, 0);

using TcpClient replacementClient = new();
await replacementClient.ConnectAsync("127.0.0.1", server.Port);
NetworkStream replacementStream = replacementClient.GetStream();
byte replacementSlot = await ActivateAsync(replacementStream, 2000, 280);
await WaitForPlayerCountAsync(server, 1);

PlayerSnapshot authoritativeBeforeClientView = server.LatestSnapshot.Players.First(
  player => player.Player.Value == replacementSlot);
await replacementStream.WriteAsync(TerrariaPacketCodec.EncodePlayerControls(
  new PlayerControlIntent(
    replacementSlot,
    MoveLeft: false,
    MoveRight: false,
    Jump: false,
    UseItem: false,
    FacingRight: true,
    SelectedItem: 0),
  positionX: 2400.0f * TerrariaWorldCoordinates.PixelsPerTile,
  positionY: authoritativeBeforeClientView.Position.Y * TerrariaWorldCoordinates.PixelsPerTile));

bool receivedClientViewSection = false;
using (CancellationTokenSource clientViewTimeout = new(TimeSpan.FromSeconds(2)))
{
  while (!clientViewTimeout.IsCancellationRequested)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(await ReadFrameWithTokenAsync(
      replacementStream,
      clientViewTimeout.Token));
    if (frame.MessageId == TerrariaMessageId.TileSection &&
        GetSectionOriginX(frame) == 2600)
    {
      receivedClientViewSection = true;
      break;
    }
  }
}

if (!receivedClientViewSection)
{
  throw new InvalidOperationException(
    "PlayerControls client position did not stream the newly visible world section.");
}

PlayerSnapshot authoritativeAfterClientView = server.LatestSnapshot.Players.First(
  player => player.Player.Value == replacementSlot);
if (MathF.Abs(
      authoritativeAfterClientView.Position.X - authoritativeBeforeClientView.Position.X) >
    0.01f)
{
  throw new InvalidOperationException(
    "PlayerControls client position replaced the server-owned simulation position.");
}

Console.WriteLine(
  "PASS: protocol PlayerControls position streams a new section without changing authority");

bool receivedSelfReplication = false;
using (CancellationTokenSource selfReplicationTimeout = new(TimeSpan.FromMilliseconds(400)))
{
  try
  {
    while (!selfReplicationTimeout.IsCancellationRequested)
    {
      TerrariaFrame frame = TerrariaFrameCodec.Decode(await ReadFrameWithTokenAsync(
        replacementStream,
        selfReplicationTimeout.Token));
      if ((frame.MessageId == TerrariaMessageId.PlayerActive ||
           frame.MessageId == TerrariaMessageId.PlayerLifeMana ||
           frame.MessageId == TerrariaMessageId.PlayerControls) &&
          !frame.Payload.IsEmpty && frame.Payload.Span[0] == replacementSlot)
      {
        receivedSelfReplication = true;
        break;
      }
    }
  }
  catch (OperationCanceledException)
  {
  }
}

if (receivedSelfReplication)
{
  throw new InvalidOperationException("A player received an unsolicited self-replication frame.");
}

Console.WriteLine("PASS: source session does not receive self-replication frames");

await VerifyMountControlsKeepSessionAliveAsync(server, replacementStream, replacementSlot);
Console.WriteLine("PASS: mount PlayerControls remains session-local and the session stays alive");

long tickBeforeItemUse = server.LatestSnapshot.Tick;
PlayerControlIntent itemUseControls = new(
  replacementSlot,
  MoveLeft: false,
  MoveRight: false,
  Jump: false,
  UseItem: true,
  FacingRight: true,
  SelectedItem: 0);
await replacementStream.WriteAsync(TerrariaPacketCodec.EncodePlayerControls(
  itemUseControls,
  0.0f,
  0.0f));
await replacementStream.WriteAsync(TerrariaPacketCodec.EncodePing());

bool receivedPing = false;
List<TerrariaMessageId> framesBeforePing = new();
using (CancellationTokenSource pingTimeout = new(TimeSpan.FromSeconds(3)))
{
  try
  {
    while (!pingTimeout.IsCancellationRequested)
    {
      TerrariaFrame frame = TerrariaFrameCodec.Decode(await ReadFrameWithTokenAsync(
        replacementStream,
        pingTimeout.Token));
      framesBeforePing.Add(frame.MessageId);
      if (frame.MessageId == TerrariaMessageId.Ping)
      {
        receivedPing = true;
        break;
      }
    }
  }
  catch (OperationCanceledException)
  {
  }
}

await WaitForItemUseAsync(server, tickBeforeItemUse);
if (!receivedPing)
{
  throw new InvalidOperationException(
    "A replacement session stopped responding after player item use. " +
    $"Tick={server.LatestSnapshot.Tick}, SimulationFault={server.SimulationFault}, " +
    $"SessionFault={server.LastSessionFault}, Frames={string.Join(',', framesBeforePing)}.");
}

Console.WriteLine("PASS: replacement session remains responsive after player item use");

const int MovementControlFrameCount = 360;
for (int index = 0; index < MovementControlFrameCount; index++)
{
  PlayerControlIntent controls = new(
    replacementSlot,
    MoveLeft: false,
    MoveRight: true,
    Jump: false,
    UseItem: false,
    FacingRight: true,
    SelectedItem: 0);
  await replacementStream.WriteAsync(TerrariaPacketCodec.EncodePlayerControls(controls, 0.0f, 0.0f));
  await Task.Delay(TimeSpan.FromMilliseconds(20));
}

bool receivedEnteredSection = false;
using (CancellationTokenSource sectionTimeout = new(TimeSpan.FromSeconds(3)))
{
  try
  {
    while (!sectionTimeout.IsCancellationRequested)
    {
      TerrariaFrame frame = TerrariaFrameCodec.Decode(await ReadFrameWithTokenAsync(
        replacementStream,
        sectionTimeout.Token));
      if (frame.MessageId == TerrariaMessageId.TileSection && GetSectionOriginX(frame) == 2600)
      {
        receivedEnteredSection = true;
        break;
      }
    }
  }
  catch (OperationCanceledException)
  {
  }
}

if (!receivedEnteredSection)
{
  PlayerSnapshot? replacementPlayer = server.LatestSnapshot.Players.FirstOrDefault(
    player => player.Player.Value == replacementSlot);
  throw new InvalidOperationException(
    "Moving into a new PVS section did not stream that section. " +
    $"Tick={server.LatestSnapshot.Tick}, Position={replacementPlayer?.Position}.");
}

Console.WriteLine("PASS: authoritative movement streams the newly entered PVS section");

using TcpClient observerClient = new();
await observerClient.ConnectAsync("127.0.0.1", server.Port);
NetworkStream observerStream = observerClient.GetStream();
byte observerSlot = await ActivateAsync(observerStream, 2600, 280);
PlayerControlIntent observerControls = new(
  observerSlot,
  MoveLeft: false,
  MoveRight: false,
  Jump: false,
  UseItem: false,
  FacingRight: true,
  SelectedItem: 0);
await observerStream.WriteAsync(TerrariaPacketCodec.EncodePlayerControls(
  observerControls,
  0.0f,
  0.0f));
await Task.Delay(TimeSpan.FromMilliseconds(20));
for (int index = 0; index < 3; index++)
{
  PlayerControlIntent controls = new(
    replacementSlot,
    MoveLeft: false,
    MoveRight: true,
    Jump: false,
    UseItem: false,
    FacingRight: true,
    SelectedItem: 0);
  await replacementStream.WriteAsync(TerrariaPacketCodec.EncodePlayerControls(controls, 0.0f, 0.0f));
  await Task.Delay(TimeSpan.FromMilliseconds(20));
}

bool observerReceivedMovement = false;
using (CancellationTokenSource observerTimeout = new(TimeSpan.FromSeconds(3)))
{
  try
  {
    while (!observerTimeout.IsCancellationRequested)
    {
      TerrariaFrame frame = TerrariaFrameCodec.Decode(await ReadFrameWithTokenAsync(
        observerStream,
        observerTimeout.Token));
      if (frame.MessageId != TerrariaMessageId.PlayerControls ||
          frame.Payload.Span[0] != replacementSlot)
      {
        continue;
      }

      float projectedX = BitConverter.ToSingle(frame.Payload.Span.Slice(6, 4));
      if (projectedX > 2210.0f)
      {
        observerReceivedMovement = true;
        break;
      }
    }
  }
  catch (OperationCanceledException)
  {
  }
}

if (!observerReceivedMovement)
{
  throw new InvalidOperationException("An eligible observer did not receive player movement state.");
}

Console.WriteLine("PASS: eligible observer receives server-owned player movement state");

Console.WriteLine("PASS: disconnect destroys the server player and allows a replacement session");

static void VerifyMountCompatibilityStateIsSessionLocal()
{
  const byte playerSlot = 7;
  TerrariaSession session = CreateActiveSession(playerSlot);
  TerrariaPacketDispatchResult result = new TerrariaPacketDispatcher().Dispatch(
    session,
    CreateMountPlayerControlsFrame(playerSlot));
  if (result.PlayerControls is not PlayerControlIntent controls ||
      controls.PlayerSlot != playerSlot ||
      session.LegacyPlayerControls?.MountType != 1 ||
      result.LegacyPlayerControls?.MountType != 1)
  {
    throw new InvalidOperationException(
      "Mount PlayerControls did not preserve session-local compatibility state.");
  }
}

static TerrariaSession CreateActiveSession(byte playerSlot)
{
  TerrariaSession session = new(playerSlot);
  _ = session.AcceptHello(TerrariaPacketCodec.Encode(new HelloPacket()));
  _ = session.AcceptPlayerProfile(TerrariaPacketCodec.Encode(CreateProfile(playerSlot)));
  _ = session.AcceptPlayerUuid(TerrariaPacketCodec.Encode(new PlayerUuidPacket(
    "22222222-2222-2222-2222-222222222222")));
  _ = session.AcceptRequestWorldData(TerrariaPacketCodec.EncodeRequestWorldData());
  _ = session.AcceptSpawnTileData(TerrariaPacketCodec.EncodeSpawnTileData(
    new SpawnTileDataRequestPacket(2000, 300, 0)));
  session.MarkInitialWorldStreamSent();
  _ = session.AcceptPlayerSpawn(TerrariaPacketCodec.Encode(new PlayerSpawnPacket(
    playerSlot,
    2000,
    300,
    0,
    0,
    0,
    0,
    0)));
  return session;
}

static async Task VerifyMountControlsKeepSessionAliveAsync(
  DomeServer server,
  NetworkStream stream,
  byte playerSlot)
{
  await stream.WriteAsync(CreateMountPlayerControlsFrame(playerSlot));
  await stream.WriteAsync(TerrariaPacketCodec.EncodePing());

  bool receivedPing = false;
  using (CancellationTokenSource timeout = new(TimeSpan.FromSeconds(3)))
  {
    try
    {
      while (!timeout.IsCancellationRequested)
      {
        TerrariaFrame frame = TerrariaFrameCodec.Decode(await ReadFrameWithTokenAsync(
          stream,
          timeout.Token));
        if (frame.MessageId == TerrariaMessageId.Ping)
        {
          receivedPing = true;
          break;
        }
      }
    }
    catch (OperationCanceledException)
    {
    }
    catch (IOException)
    {
    }
  }

  if (!receivedPing || server.LatestSnapshot.Players.Count != 1)
  {
    throw new InvalidOperationException(
      "A legal mount PlayerControls frame stopped the active session. " +
      $"PlayerCount={server.LatestSnapshot.Players.Count}, " +
      $"SessionFault={server.LastSessionFault}.");
  }
}

static byte[] CreateMountPlayerControlsFrame(byte playerSlot)
{
  using MemoryStream payload = new();
  using (BinaryWriter writer = new(payload))
  {
    writer.Write(playerSlot);
    writer.Write((byte)0);
    writer.Write((byte)(1 << 7));
    writer.Write((byte)0);
    writer.Write((byte)0);
    writer.Write((byte)0);
    writer.Write(2000.0f);
    writer.Write(0.0f);
    writer.Write((ushort)1);
  }

  return TerrariaFrameCodec.Encode(new TerrariaFrame(
    TerrariaMessageId.PlayerControls,
    payload.ToArray()));
}

static async Task<byte> ActivateAsync(NetworkStream stream, short spawnX, short spawnY)
{
  await stream.WriteAsync(TerrariaPacketCodec.Encode(new HelloPacket()));
  TerrariaFrame userSlot = TerrariaFrameCodec.Decode(await ReadFrameAsync(stream, "user slot"));
  TerrariaFrame initialNetModules = TerrariaFrameCodec.Decode(await ReadFrameAsync(
    stream,
    "initial NetModules"));
  if (initialNetModules.MessageId != TerrariaMessageId.NetModules ||
      !initialNetModules.Payload.Span.SequenceEqual(new byte[] { 0, 0, 0, 0 }))
  {
    throw new InvalidOperationException("Server did not send the expected initial NetModules state.");
  }

  await stream.WriteAsync(TerrariaPacketCodec.Encode(CreateProfile(userSlot.Payload.Span[0])));
  await stream.WriteAsync(TerrariaPacketCodec.Encode(new PlayerUuidPacket(
    "22222222-2222-2222-2222-222222222222")));
  await stream.WriteAsync(TerrariaPacketCodec.EncodeRequestWorldData());
  _ = await ReadFrameAsync(stream, "world data");
  await stream.WriteAsync(TerrariaPacketCodec.EncodeSpawnTileData(
    new SpawnTileDataRequestPacket(spawnX, spawnY, 0)));
  _ = await ReadFrameAsync(stream, "initial status text");
  for (int index = 0; index < 15; index++)
  {
    _ = await ReadFrameAsync(stream, $"initial section {index}");
  }

  _ = await ReadFrameAsync(stream, "initial spawn");
  await stream.WriteAsync(TerrariaPacketCodec.Encode(new PlayerSpawnPacket(
    userSlot.Payload.Span[0],
    spawnX,
    spawnY,
    0,
    0,
    0,
    0,
    0)));
  bool receivedFinishedConnecting = false;
  const int maximumActivationFrames = 10_000;
  for (int index = 0; index < maximumActivationFrames; index++)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(await ReadFrameAsync(
      stream,
      "player projection"));
    if (frame.MessageId == TerrariaMessageId.FinishedConnectingToServer)
    {
      receivedFinishedConnecting = true;
      break;
    }
  }

  if (!receivedFinishedConnecting)
  {
    throw new InvalidOperationException(
      $"Server did not complete the player projection within {maximumActivationFrames} frames.");
  }

  return userSlot.Payload.Span[0];
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
    "SessionVerifier",
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

static async Task<byte[]> ReadFrameWithTokenAsync(
  NetworkStream stream,
  CancellationToken cancellationToken)
{
  byte[] prefix = new byte[2];
  await stream.ReadExactlyAsync(prefix, cancellationToken);
  int frameLength = prefix[0] | prefix[1] << 8;
  byte[] frame = new byte[frameLength];
  prefix.CopyTo(frame, 0);
  await stream.ReadExactlyAsync(frame.AsMemory(2), cancellationToken);
  return frame;
}

static async Task WaitForPlayerCountAsync(DomeServer server, int expectedCount)
{
  using CancellationTokenSource timeout = new(TimeSpan.FromSeconds(3));
  while (!timeout.IsCancellationRequested)
  {
    if (server.LatestSnapshot.Players.Count == expectedCount)
    {
      return;
    }

    await Task.Delay(TimeSpan.FromMilliseconds(10), timeout.Token);
  }

  throw new InvalidOperationException($"Server did not reach {expectedCount} active players.");
}

static async Task WaitForItemUseAsync(DomeServer server, long tickBeforeItemUse)
{
  using CancellationTokenSource timeout = new(TimeSpan.FromSeconds(1));
  while (!timeout.IsCancellationRequested)
  {
    if (server.LatestSnapshot.Tick > tickBeforeItemUse &&
        server.LatestSnapshot.Projectiles.Count > 0)
    {
      return;
    }

    await Task.Delay(TimeSpan.FromMilliseconds(10), timeout.Token);
  }

  throw new InvalidOperationException(
    "Player item use did not advance simulation or create a projectile. " +
    $"Tick={server.LatestSnapshot.Tick}, SimulationFault={server.SimulationFault}, " +
    $"SessionFault={server.LastSessionFault}.");
}

static int GetSectionOriginX(TerrariaFrame sectionFrame)
{
  using System.IO.Compression.DeflateStream decompressor = new(
    new System.IO.MemoryStream(sectionFrame.Payload.ToArray(), writable: false),
    System.IO.Compression.CompressionMode.Decompress);
  using System.IO.BinaryReader reader = new(decompressor);
  return reader.ReadInt32();
}

static IReadOnlyList<WorldSectionSnapshot> CreateSnapshots(
  WorldGrid world,
  IReadOnlyList<WorldSectionCoordinates> sections)
{
  List<WorldSectionSnapshot> snapshots = new(sections.Count);
  for (int index = 0; index < sections.Count; index++)
  {
    snapshots.Add(world.CreateSectionSnapshot(sections[index]));
  }

  return snapshots;
}
