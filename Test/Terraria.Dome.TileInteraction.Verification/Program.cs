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
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Server.Validation;
using Terraria.Dome.Server;
using Terraria.Dome.Simulation;
using Terraria.Dome.Simulation.StatusEffects.Components;
using Terraria.Dome.Simulation.StatusEffects.Systems;
using Terraria.Dome.Simulation.Player.Components;
using Terraria.Dome.Simulation.Player.Commands;

TerrariaSession session = new(assignedPlayerSlot: 1);

BuffCollectionComponent buffs = new(maximumCount: 2);
buffs.Add(1, durationTicks: 2, new PlayerHandle(1));
buffs.Add(2, durationTicks: 1, new PlayerHandle(1));
new BuffDurationSystem().Tick(buffs);
if (buffs.Count != 1 || buffs.Entries[0].Type != 1)
{
  throw new InvalidOperationException("Buff duration did not expire entries at the tick boundary.");
}

PlayerInteractionComponent interaction = new();
interaction.SetTarget(7, PlayerInteractionMode.OpenChest);
UsePlayerInteractionCommand command = new(new PlayerHandle(1), 7, PlayerInteractionMode.OpenChest);
if (!interaction.HasTarget || interaction.TargetId != command.TargetId ||
    interaction.Mode != command.Mode)
{
  throw new InvalidOperationException("Player interaction did not preserve a typed target and mode.");
}

Console.WriteLine("PASS: bounded status effects and typed player interactions are simulation-owned");
AdvanceSessionToActive(session);
TerrariaPacketDispatcher dispatcher = new();
byte[] placeTileFrame = TerrariaFrameCodec.Encode(new TerrariaFrame(
  (TerrariaMessageId)17,
  new byte[] { 1, 10, 0, 20, 0, 7, 0, 0 }));

TerrariaPacketDispatchResult result = dispatcher.Dispatch(session, placeTileFrame);
if (result.Outcome != TerrariaPacketDispatchOutcome.TileManipulationAccepted ||
    result.TileManipulation != new TileManipulationIntent(
      TileManipulationAction.PlaceTile,
      10,
      20,
      7,
      0))
{
  throw new InvalidOperationException("Message 17 was not accepted as a tile manipulation intent.");
}

AssertInvalidTileManipulation(session, dispatcher, new byte[] { 2, 10, 0, 20, 0, 7, 0, 0 });
AssertInvalidTileManipulation(session, dispatcher, new byte[] { 1, 10, 0, 20, 0, 7, 0 });

Console.WriteLine("PASS: V1456 message 17 routes an exact tile manipulation envelope");

WorldGrid world = new(400, 300);
world.EnqueueTileChange(new TileChangeCommand(2, 10, 10, TileChangeKind.Place, 2));
world.EnqueueTileChange(new TileChangeCommand(1, 10, 10, TileChangeKind.Place, 1));
if (world.GetSectionVersion(new WorldSectionCoordinates(0, 0)) != 0)
{
  throw new InvalidOperationException("Tile changes committed before the simulation tick boundary.");
}

world.CommitTileChanges();
WorldSectionSnapshot changedSection = world.CreateSectionSnapshot(new WorldSectionCoordinates(0, 0));
if (changedSection.GetTile(10, 10) != new WorldTile(true, 2) || changedSection.Version != 2 ||
    world.GetSectionVersion(new WorldSectionCoordinates(1, 0)) != 0)
{
  throw new InvalidOperationException("Tile changes were not committed deterministically by section.");
}

Console.WriteLine("PASS: tile changes commit at the simulation boundary in sequence order");

WorldGrid environmentWorld = new(4200, 1200);
DefaultWorldEnvironmentConvergence environment =
  DefaultWorldEnvironmentConvergence.Create(environmentWorld);
List<WorldEnvironmentChange> environmentChanges = new();
while (!environment.IsComplete)
{
  environmentChanges.AddRange(environment.Advance(environmentWorld));
}

if (environmentChanges.Count(change => change.Kind == WorldEnvironmentChangeKind.TileSquare) != 23 ||
    environmentChanges.Count(change => change.Kind == WorldEnvironmentChangeKind.TileManipulation) != 1 ||
    environmentChanges.Count(change => change.Kind == WorldEnvironmentChangeKind.Liquid) != 106)
{
  throw new InvalidOperationException(
    "Default world environment convergence did not retain its authoritative change inventory.");
}

WorldEnvironmentChange firstSquare = environmentChanges.First(
  change => change.Kind == WorldEnvironmentChangeKind.TileSquare);
TerrariaFrame squareFrame = TerrariaFrameCodec.Decode(TerrariaPacketCodec.EncodeTileSquare(
  environmentWorld,
  firstSquare.X,
  firstSquare.Y,
  firstSquare.Width,
  firstSquare.Height,
  firstSquare.ChangeType));
if (squareFrame.MessageId != TerrariaMessageId.TileSquare || squareFrame.Payload.Length < 10)
{
  throw new InvalidOperationException("Authoritative TileSquare projection was not V1456-shaped.");
}

WorldEnvironmentChange liquidChange = environmentChanges.First(
  change => change.Kind == WorldEnvironmentChangeKind.Liquid && change.X == 1837);
WorldTile liquidTile = environmentWorld.GetTile(liquidChange.X, liquidChange.Y);
TerrariaFrame liquidFrame = TerrariaFrameCodec.Decode(TerrariaPacketCodec.EncodeLiquidNetModule([
  new WorldLiquidSnapshot(
    liquidChange.X,
    liquidChange.Y,
    liquidTile.LiquidAmount,
    liquidTile.LiquidType)
]));
if (liquidFrame.MessageId != TerrariaMessageId.NetModules || liquidFrame.Payload.Length != 10)
{
  throw new InvalidOperationException("Authoritative liquid convergence was not encoded as NetLiquidModule.");
}

Console.WriteLine("PASS: default environment convergence projects typed TileSquare and liquid changes");

TileInteractionValidator validator = new();
WorldGrid validationWorld = new(400, 300);
WorldSectionCoordinates visibleSection = new(0, 0);
PlayerSnapshot player = new(
  new PlayerHandle(1),
  new SimulationVector(10.0f, 10.0f),
  new SimulationVector(0.0f, 0.0f),
  1,
  true,
  100,
  true,
  0);
TileManipulationIntent validIntent = new(TileManipulationAction.PlaceTile, 11, 10, 3, 0);
TileInteractionValidation valid = validator.Validate(
  validIntent,
  player,
  validationWorld,
  new HashSet<WorldSectionCoordinates> { visibleSection },
  0,
  3);
if (!valid.IsAccepted || valid.Command is null)
{
  throw new InvalidOperationException("Server validator rejected a valid visible tile action.");
}

AssertRejected(TileInteractionRejection.OutOfWorld, validIntent with { X = -1 });
AssertRejected(TileInteractionRejection.SectionNotVisible, validIntent with { X = 210 });
AssertRejected(TileInteractionRejection.OutOfRange, validIntent with { X = 30 });
AssertRejected(TileInteractionRejection.ActionBudgetExceeded, validIntent, 32);
AssertRejected(TileInteractionRejection.UnsupportedAction,
  validIntent with { Action = (TileManipulationAction)2 });
if (validationWorld.GetSectionVersion(visibleSection) != 0)
{
  throw new InvalidOperationException("Rejected tile actions mutated the authoritative world.");
}

Console.WriteLine("PASS: tile interaction authority rejects invalid actions without mutation");

using DomeServer server = new();
server.Start();
using TcpClient firstClient = new();
using TcpClient secondClient = new();
using TcpClient hiddenClient = new();
await firstClient.ConnectAsync("127.0.0.1", server.Port);
await secondClient.ConnectAsync("127.0.0.1", server.Port);
await hiddenClient.ConnectAsync("127.0.0.1", server.Port);
NetworkStream firstStream = firstClient.GetStream();
NetworkStream secondStream = secondClient.GetStream();
NetworkStream hiddenStream = hiddenClient.GetStream();
await ActivateAsync(firstStream, 2000, 300);
await ActivateAsync(secondStream, 2000, 300);
await ActivateAsync(hiddenStream, 100, 300);
await firstStream.WriteAsync(TerrariaPacketCodec.EncodePlayerControls(new PlayerControlIntent(
  1,
  MoveLeft: false,
  MoveRight: false,
  Jump: false,
  UseItem: false,
  FacingRight: true,
  SelectedItem: 0),
  32000.0f,
  4800.0f));
await secondStream.WriteAsync(TerrariaPacketCodec.EncodePlayerControls(new PlayerControlIntent(
  2,
  MoveLeft: false,
  MoveRight: false,
  Jump: false,
  UseItem: false,
  FacingRight: true,
  SelectedItem: 0),
  32000.0f,
  4800.0f));
await hiddenStream.WriteAsync(TerrariaPacketCodec.EncodePlayerControls(new PlayerControlIntent(
  3,
  MoveLeft: false,
  MoveRight: false,
  Jump: false,
  UseItem: false,
  FacingRight: true,
  SelectedItem: 0),
  1600.0f,
  4800.0f));
await Task.Delay(TimeSpan.FromMilliseconds(100));

PlayerSnapshot mutationActor = server.LatestSnapshot.FindPlayer(new PlayerHandle(1));
int mutationTileX = (int)MathF.Floor(mutationActor.Position.X) + 5;
int mutationTileY = (int)MathF.Floor(mutationActor.Position.Y);
byte[] mutation = CreatePlaceTileMutation(mutationTileX, mutationTileY, 3);
byte[] orderedMutation = CreatePlaceTileMutation(mutationTileX + 1, mutationTileY, 4);
await firstStream.WriteAsync(mutation);
await firstStream.WriteAsync(orderedMutation);
await WaitForTileChangesAsync(server.World, mutationTileX, mutationTileY);
bool firstReceived;
bool secondReceived;
List<string> receivedFrameDiagnostics = new();
try
{
  firstReceived = await ReceiveTileDeltaAsync(
    firstStream,
    server.World,
    mutationTileX,
    mutationTileY,
    3,
    "first visible delta",
    receivedFrameDiagnostics);
  secondReceived = await ReceiveTileDeltaAsync(
    secondStream,
    server.World,
    mutationTileX,
    mutationTileY,
    3,
    "second visible delta",
    receivedFrameDiagnostics);
}
catch (TimeoutException exception)
{
  WorldTile currentTile = server.World.GetTile(mutationTileX, mutationTileY);
  throw new InvalidOperationException(
    $"Visible tile delta timed out. tile={currentTile.Type}, active={currentTile.IsActive}, " +
    $"pending={server.PendingProtocolCommandCount}, sessionFault={server.LastSessionFault}, " +
    $"simulationFault={server.SimulationFault}, players=" +
    string.Join(",", server.LatestSnapshot.Players.Select(player =>
      $"{player.AssignedSlot}:{player.Position.X:F1},{player.Position.Y:F1}")) + ", frames=" +
    string.Join(",", receivedFrameDiagnostics) + ".",
    exception);
}

if (!firstReceived || !secondReceived)
{
  throw new InvalidOperationException("Visible sessions did not receive the authoritative tile delta.");
}

using (CancellationTokenSource hiddenTimeout = new(TimeSpan.FromMilliseconds(250)))
{
  try
  {
    while (!hiddenTimeout.IsCancellationRequested)
    {
      TerrariaFrame frame = TerrariaFrameCodec.Decode(await ReadFrameWithTokenAsync(
        hiddenStream,
        hiddenTimeout.Token));
      if (frame.MessageId == TerrariaMessageId.TileSection &&
          IsCurrentAuthoritativeSection(
            frame,
            server.World,
            mutationTileX,
            mutationTileY,
            3))
      {
        throw new InvalidOperationException("A hidden session received the tile mutation delta.");
      }
    }
  }
  catch (OperationCanceledException)
  {
  }
}

static async Task<bool> ReceiveTileDeltaAsync(
  NetworkStream stream,
  WorldGrid world,
  int worldX,
  int worldY,
  ushort tileType,
  string stage,
  List<string> receivedFrameDiagnostics)
{
  using CancellationTokenSource timeout = new(TimeSpan.FromSeconds(3));
  try
  {
    while (!timeout.IsCancellationRequested)
    {
      TerrariaFrame frame = TerrariaFrameCodec.Decode(await ReadFrameWithTokenAsync(
        stream,
        timeout.Token));
      receivedFrameDiagnostics.Add(DescribeFrame(frame));
      if (frame.MessageId == TerrariaMessageId.TileSection &&
          IsCurrentAuthoritativeSection(frame, world, worldX, worldY, tileType))
      {
        return true;
      }
    }
  }
  catch (OperationCanceledException)
  {
  }

  throw new TimeoutException($"Timed out while waiting for {stage}.");
}

static async Task WaitForTileChangesAsync(WorldGrid world, int tileX, int tileY)
{
  using CancellationTokenSource timeout = new(TimeSpan.FromSeconds(3));
  while (!timeout.IsCancellationRequested &&
         (world.GetTile(tileX, tileY).Type != 3 ||
          world.GetTile(tileX + 1, tileY).Type != 4))
  {
    await Task.Delay(TimeSpan.FromMilliseconds(10), timeout.Token);
  }

  if (world.GetTile(tileX, tileY).Type != 3 || world.GetTile(tileX + 1, tileY).Type != 4)
  {
    throw new InvalidOperationException("Tile mutations did not commit before replication.");
  }
}

static byte[] CreatePlaceTileMutation(int x, int y, ushort tileType)
{
  return TerrariaFrameCodec.Encode(new TerrariaFrame(
    TerrariaMessageId.TileManipulation,
    new byte[]
    {
      (byte)TileManipulationAction.PlaceTile,
      (byte)x,
      (byte)(x >> 8),
      (byte)y,
      (byte)(y >> 8),
      (byte)tileType,
      (byte)(tileType >> 8),
      0
    }));
}

static string DescribeFrame(TerrariaFrame frame)
{
  if (frame.MessageId != TerrariaMessageId.TileSection)
  {
    return frame.MessageId.ToString();
  }

  using MemoryStream compressed = new(frame.Payload.ToArray(), writable: false);
  using System.IO.Compression.DeflateStream decompressor = new(
    compressed,
    System.IO.Compression.CompressionMode.Decompress);
  using BinaryReader reader = new(decompressor);
  int originX = reader.ReadInt32();
  int originY = reader.ReadInt32();
  short width = reader.ReadInt16();
  short height = reader.ReadInt16();
  return $"TileSection({originX},{originY},{width},{height})";
}

Console.WriteLine("PASS: loopback tile mutation is replicated only to visible sessions");

using CancellationTokenSource orderingTimeout = new(TimeSpan.FromSeconds(3));
while (!orderingTimeout.IsCancellationRequested &&
       (server.World.GetTile(mutationTileX, mutationTileY).Type != 3 ||
        server.World.GetTile(mutationTileX + 1, mutationTileY).Type != 4))
{
  await Task.Delay(TimeSpan.FromMilliseconds(10), orderingTimeout.Token);
}

if (server.World.GetTile(mutationTileX, mutationTileY).Type != 3 ||
    server.World.GetTile(mutationTileX + 1, mutationTileY).Type != 4)
{
  throw new InvalidOperationException(
    "Sequential client commands did not commit in server sequence order.");
}

Console.WriteLine("PASS: real session commands commit in monotonic server sequence order");

void AssertRejected(
  TileInteractionRejection expected,
  TileManipulationIntent intent,
  int actionsThisTick = 0)
{
  TileInteractionValidation validation = validator.Validate(
    intent,
    player,
    validationWorld,
    new HashSet<WorldSectionCoordinates> { visibleSection },
    actionsThisTick,
    4);
  if (validation.IsAccepted || validation.Rejection != expected)
  {
    throw new InvalidOperationException($"Expected rejection {expected}.");
  }
}

static void AssertInvalidTileManipulation(
  TerrariaSession session,
  TerrariaPacketDispatcher dispatcher,
  byte[] payload)
{
  try
  {
    _ = dispatcher.Dispatch(session, TerrariaFrameCodec.Encode(new TerrariaFrame(
      TerrariaMessageId.TileManipulation,
      payload)));
  }
  catch (InvalidDataException)
  {
    return;
  }

  throw new InvalidOperationException("Unsupported TileManipulation packet was accepted.");
}

static async Task ActivateAsync(NetworkStream stream, short spawnX, short spawnY)
{
  await stream.WriteAsync(TerrariaPacketCodec.Encode(new HelloPacket()));
  TerrariaFrame userSlot = TerrariaFrameCodec.Decode(await ReadFrameAsync(stream, "user slot"));
  TerrariaFrame initialNetModules = TerrariaFrameCodec.Decode(await ReadFrameAsync(
    stream,
    "initial NetModules"));
  if (initialNetModules.MessageId != TerrariaMessageId.NetModules)
  {
    throw new InvalidOperationException("Session did not send initial NetModules.");
  }

  await stream.WriteAsync(TerrariaPacketCodec.Encode(CreateProfile(userSlot.Payload.Span[0])));
  await stream.WriteAsync(TerrariaPacketCodec.Encode(new PlayerUuidPacket(
    "22222222-2222-2222-2222-222222222222")));
  await stream.WriteAsync(TerrariaPacketCodec.EncodeRequestWorldData());
  _ = await ReadFrameAsync(stream, "world data");
  await stream.WriteAsync(TerrariaPacketCodec.EncodeSpawnTileData(
    new SpawnTileDataRequestPacket(spawnX, spawnY, 0)));
  TerrariaFrame refreshedWorldData = TerrariaFrameCodec.Decode(await ReadFrameAsync(
    stream,
    "refreshed world data"));
  if (refreshedWorldData.MessageId != TerrariaMessageId.WorldData)
  {
    throw new InvalidOperationException("Initial tile request did not refresh WorldData.");
  }

  TerrariaFrame statusText = TerrariaFrameCodec.Decode(await ReadFrameAsync(
    stream,
    "initial status text"));
  if (statusText.MessageId != TerrariaMessageId.StatusTextSize)
  {
    throw new InvalidOperationException("Initial world stream did not begin with status text.");
  }

  await ReadUntilMessageAsync(stream, TerrariaMessageId.InitialSpawn, "initial spawn");
  await stream.WriteAsync(TerrariaPacketCodec.Encode(new PlayerSpawnPacket(
    userSlot.Payload.Span[0],
    spawnX,
    spawnY,
    0,
    0,
    0,
    0,
    0)));
  await ReadUntilMessageAsync(
    stream,
    TerrariaMessageId.FinishedConnectingToServer,
    "finished connecting");
}

static async Task ReadUntilMessageAsync(
  NetworkStream stream,
  TerrariaMessageId expectedMessageId,
  string stage)
{
  const int MaximumFrames = 1_100;
  for (int index = 0; index < MaximumFrames; index++)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(await ReadFrameAsync(stream, stage));
    if (frame.MessageId == expectedMessageId)
    {
      return;
    }
  }

  throw new InvalidOperationException(
    $"Session did not send {expectedMessageId} within {MaximumFrames} frames at {stage}.");
}

static async Task<byte[]> ReadFrameAsync(NetworkStream stream, string stage)
{
  using CancellationTokenSource timeout = new(TimeSpan.FromSeconds(3));
  try
  {
    return await ReadFrameWithTokenAsync(stream, timeout.Token);
  }
  catch (OperationCanceledException exception)
  {
    throw new TimeoutException($"Timed out while waiting for {stage}.", exception);
  }
}

static async Task<byte[]> ReadFrameWithTokenAsync(
  NetworkStream stream,
  CancellationToken cancellationToken = default)
{
  byte[] prefix = new byte[2];
  await stream.ReadExactlyAsync(prefix, cancellationToken);
  int frameLength = prefix[0] | prefix[1] << 8;
  byte[] frame = new byte[frameLength];
  prefix.CopyTo(frame, 0);
  await stream.ReadExactlyAsync(frame.AsMemory(2), cancellationToken);
  return frame;
}

static bool IsCurrentAuthoritativeSection(
  TerrariaFrame sectionFrame,
  WorldGrid world,
  int worldX,
  int worldY,
  ushort type)
{
  using MemoryStream compressed = new(sectionFrame.Payload.ToArray(), writable: false);
  using System.IO.Compression.DeflateStream decompressor = new(
    compressed,
    System.IO.Compression.CompressionMode.Decompress);
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

  WorldTile tile = world.GetTile(worldX, worldY);
  if (!tile.IsActive || tile.Type != type)
  {
    return false;
  }

  WorldSectionCoordinates coordinates = world.GetSectionCoordinates(worldX, worldY);
  TerrariaFrame expected = TerrariaFrameCodec.Decode(TerrariaPacketCodec.Encode(
    world.CreateSectionSnapshot(coordinates)));
  return sectionFrame.Payload.Span.SequenceEqual(expected.Payload.Span);
}

static void AdvanceSessionToActive(TerrariaSession session)
{
  byte[] hello = TerrariaPacketCodec.Encode(new HelloPacket());
  _ = session.AcceptHello(hello);
  _ = session.AcceptPlayerProfile(TerrariaPacketCodec.Encode(CreateProfile(1)));
  _ = session.AcceptPlayerUuid(TerrariaPacketCodec.Encode(new PlayerUuidPacket(
    "22222222-2222-2222-2222-222222222222")));
  _ = session.AcceptRequestWorldData(TerrariaPacketCodec.EncodeRequestWorldData());
  _ = session.AcceptSpawnTileData(TerrariaPacketCodec.EncodeSpawnTileData(
    new SpawnTileDataRequestPacket(2000, 300, 0)));
  session.MarkInitialWorldStreamSent();
  _ = session.AcceptPlayerSpawn(TerrariaPacketCodec.Encode(new PlayerSpawnPacket(
    1,
    2000,
    300,
    0,
    0,
    0,
    0,
    0)));
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
    "TileVerifier",
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
