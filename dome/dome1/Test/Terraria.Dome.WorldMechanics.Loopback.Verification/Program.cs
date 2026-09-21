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
using Terraria.Dome.Simulation.WorldModel;

using DomeServer server = new(CreateMechanicsWorld());
const short DoorX = 2000;
const short DoorY = 21;
const short TrapdoorX = 2004;
const short TrapdoorY = 21;
const short TallGateX = 2008;
const short TallGateY = 21;
int doorId = server.CreateDoor(DoorX, DoorY);
int trapdoorId = server.CreateTrapdoor(TrapdoorX, TrapdoorY, opensDown: true);
int tallGateId = server.CreateTallGate(TallGateX, TallGateY);
server.Start();
using TcpClient actorClient = new();
using TcpClient observerClient = new();
using TcpClient hiddenClient = new();
await actorClient.ConnectAsync("127.0.0.1", server.Port);
await observerClient.ConnectAsync("127.0.0.1", server.Port);
await hiddenClient.ConnectAsync("127.0.0.1", server.Port);
NetworkStream actor = actorClient.GetStream();
NetworkStream observer = observerClient.GetStream();
NetworkStream hidden = hiddenClient.GetStream();
byte actorSlot = await ActivateAsync(actor, TrapdoorX, TrapdoorY, "Actor");
byte observerSlot = await ActivateAsync(observer, DoorX, DoorY, "Observer");
byte hiddenSlot = await ActivateAsync(hidden, 100, DoorY, "Hidden");
await UnlockVisibilityAsync(actor, actorSlot);
await UnlockVisibilityAsync(observer, observerSlot);
await UnlockVisibilityAsync(hidden, hiddenSlot);
await Task.Delay(TimeSpan.FromMilliseconds(100));

await actor.WriteAsync(TerrariaPacketCodec.EncodeDoorToggle(new DoorToggleIntent(
  DoorToggleAction.OpenDoor,
  DoorX,
  DoorY,
  true)));
await hidden.WriteAsync(TerrariaPacketCodec.EncodeDoorToggle(new DoorToggleIntent(
  DoorToggleAction.OpenDoor,
  DoorX,
  DoorY,
  true)));

await actor.WriteAsync(TerrariaPacketCodec.EncodeDoorToggle(new DoorToggleIntent(
  DoorToggleAction.CloseDoor,
  DoorX,
  DoorY,
  false)));
await actor.WriteAsync(TerrariaPacketCodec.EncodeDoorToggle(new DoorToggleIntent(
  DoorToggleAction.ShiftTrapdoor,
  DoorX,
  DoorY,
  true)));
await actor.WriteAsync(TerrariaPacketCodec.EncodeDoorToggle(new DoorToggleIntent(
  DoorToggleAction.ShiftTrapdoorReverse,
  TrapdoorX + 1,
  TrapdoorY + 1,
  true)));
await actor.WriteAsync(TerrariaPacketCodec.EncodeDoorToggle(new DoorToggleIntent(
  DoorToggleAction.ShiftTrapdoor,
  TrapdoorX + 1,
  TrapdoorY + 1,
  true)));
await actor.WriteAsync(TerrariaPacketCodec.EncodeDoorToggle(new DoorToggleIntent(
  DoorToggleAction.OpenTallGate,
  TallGateX,
  TallGateY,
  false)));
await actor.WriteAsync(TerrariaPacketCodec.EncodeDoorToggle(new DoorToggleIntent(
  DoorToggleAction.CloseTallGate,
  TallGateX,
  TallGateY,
  true)));

IReadOnlyList<DoorToggleIntent> observerDoorTransitions = await ReadDoorTransitionsAsync(
  observer,
  TimeSpan.FromSeconds(2));
int hiddenDoorFrames = await CountFramesAsync(
  hidden,
  TerrariaMessageId.ToggleDoorState,
  TimeSpan.FromMilliseconds(400));
DoorSnapshot door = server.CreateDoorSnapshots().Single(candidate => candidate.DoorId == doorId);
DoorSnapshot trapdoor = server.CreateDoorSnapshots().Single(candidate => candidate.DoorId == trapdoorId);
DoorSnapshot tallGate = server.CreateDoorSnapshots().Single(candidate => candidate.DoorId == tallGateId);
bool replicatedTrapdoorOpen = observerDoorTransitions.Any(transition =>
  transition.Action == DoorToggleAction.ShiftTrapdoorReverse &&
  transition.TileX == TrapdoorX + 1 && transition.TileY == TrapdoorY + 1 &&
  transition.Direction);
bool replicatedTrapdoorClose = observerDoorTransitions.Any(transition =>
  transition.Action == DoorToggleAction.ShiftTrapdoor &&
  transition.TileX == TrapdoorX + 1 && transition.TileY == TrapdoorY + 1 &&
  transition.Direction);
bool replicatedTallGateOpen = observerDoorTransitions.Any(transition =>
  transition.Action == DoorToggleAction.OpenTallGate && transition.TileX == TallGateX &&
  transition.TileY == TallGateY && !transition.Direction);
bool replicatedTallGateClose = observerDoorTransitions.Any(transition =>
  transition.Action == DoorToggleAction.CloseTallGate && transition.TileX == TallGateX &&
  transition.TileY == TallGateY && transition.Direction);
if (observerDoorTransitions.Count == 0 || hiddenDoorFrames != 0 || door.IsOpen ||
    door.Revision != 3 || trapdoor.IsOpen || tallGate.IsOpen || !replicatedTrapdoorOpen ||
    !replicatedTrapdoorClose || !replicatedTallGateOpen || !replicatedTallGateClose)
{
  throw new InvalidOperationException(
    $"Door mutation, PVS projection or hidden-session rejection failed. " +
    $"ObserverFrames={observerDoorTransitions.Count}, HiddenFrames={hiddenDoorFrames}, " +
    $"DoorIsOpen={door.IsOpen}, DoorRevision={door.Revision}, " +
    $"TrapdoorIsOpen={trapdoor.IsOpen}, TallGateIsOpen={tallGate.IsOpen}, " +
    $"SessionFault={server.LastSessionFault}, SimulationFault={server.SimulationFault}");
}

Console.WriteLine("PASS: door actions are authoritative, PVS-limited, and source-compatible");

Console.WriteLine("PASS: trapdoor and tall-gate transitions are authoritative and source-shaped");

static async Task<byte> ActivateAsync(NetworkStream stream, short spawnX, short spawnY, string name)
{
  await stream.WriteAsync(TerrariaPacketCodec.Encode(new HelloPacket()));
  TerrariaFrame slot = TerrariaFrameCodec.Decode(
    await ReadFrameAsync(stream, CancellationToken.None));
  TerrariaColor color = new(0, 0, 0);
  await stream.WriteAsync(TerrariaPacketCodec.Encode(new PlayerProfilePacket(
    slot.Payload.Span[0], 0, 0, 0.0f, 0, name, 0, 0, 0,
    color, color, color, color, color, color, color, 0, 0, 0)));
  await stream.WriteAsync(TerrariaPacketCodec.Encode(new PlayerUuidPacket(GetPlayerUuid(name))));
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
    slot.Payload.Span[0],
    spawnX,
    spawnY,
    0,
    0,
    0,
    0,
    0)));
  _ = await ReadFrameAsync(stream, CancellationToken.None);
  _ = await ReadFrameAsync(stream, CancellationToken.None);
  return slot.Payload.Span[0];
}

static WorldGrid CreateMechanicsWorld()
{
  WorldGrid world = new(4200, 1200);
  for (int x = 0; x < world.Width; x++)
  {
    _ = world.TrySetTile(x, 20, new WorldTile(true, 1));
  }

  return world;
}

static string GetPlayerUuid(string name)
{
  return name switch
  {
    "Actor" => "a9c487e8-8f75-4398-87f3-28a5c3d172c0",
    "Observer" => "0b25b34b-2bc3-4f19-9a78-faa5ff3d2f2e",
    "Hidden" => "ba9dfc4c-a1e9-4810-8569-539004362a0c",
    _ => throw new ArgumentOutOfRangeException(nameof(name))
  };
}

static Task UnlockVisibilityAsync(NetworkStream stream, byte playerSlot)
{
  PlayerControlIntent controls = new(
    playerSlot,
    MoveLeft: false,
    MoveRight: false,
    Jump: false,
    UseItem: false,
    FacingRight: true,
    SelectedItem: 0);
  return stream.WriteAsync(TerrariaPacketCodec.EncodePlayerControls(controls, 0.0f, 0.0f)).AsTask();
}

static async Task<int> CountFramesAsync(
  NetworkStream stream,
  TerrariaMessageId messageId,
  TimeSpan timeout)
{
  int count = 0;
  using CancellationTokenSource cancellation = new(timeout);
  try
  {
    while (!cancellation.IsCancellationRequested)
    {
      TerrariaFrame frame = TerrariaFrameCodec.Decode(
        await ReadFrameAsync(stream, cancellation.Token));
      if (frame.MessageId == messageId)
      {
        count++;
      }
    }
  }
  catch (OperationCanceledException)
  {
  }

  return count;
}

static async Task<IReadOnlyList<DoorToggleIntent>> ReadDoorTransitionsAsync(
  NetworkStream stream,
  TimeSpan timeout)
{
  List<DoorToggleIntent> transitions = new();
  using CancellationTokenSource cancellation = new(timeout);
  try
  {
    while (!cancellation.IsCancellationRequested)
    {
      TerrariaFrame frame = TerrariaFrameCodec.Decode(
        await ReadFrameAsync(stream, cancellation.Token));
      if (frame.MessageId == TerrariaMessageId.ToggleDoorState)
      {
        transitions.Add(TerrariaPacketCodec.DecodeDoorToggle(TerrariaFrameCodec.Encode(frame)));
      }
    }
  }
  catch (OperationCanceledException)
  {
  }

  return transitions;
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
