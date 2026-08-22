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
using Terraria.Dome.Server.Replication;
using Terraria.Dome.Simulation;
using Terraria.Dome.Simulation.Items;
using Terraria.Dome.Simulation.Items.Components;
using Terraria.Dome.Simulation.Items.Definitions;
using Terraria.Dome.Simulation.WorldModel;

Terraria.Dome.Simulation.Items.Snapshots.InventorySnapshot inventoryProjection = new(
  new PlayerHandle(1),
  [new Terraria.Dome.Simulation.Items.Snapshots.ItemInstanceSnapshot(
    new ItemStack(1, 2),
    new ItemInstanceStateComponent(
      PrefixId: 4,
      IsFavorited: true,
      IsNewAndShiny: true))],
  0,
  1);
IReadOnlyList<byte[]> inventoryFrames = new InventoryReplicationAssembler().CollectFrames(
  playerSlot: 7,
  inventoryProjection);
if (inventoryFrames.Count != 1)
{
  throw new InvalidOperationException("Inventory replication did not emit the projected slot frame.");
}

PlayerEquipmentPacket projectedInventory = TerrariaPacketCodec.DecodePlayerEquipment(
  inventoryFrames[0]);
if (projectedInventory.PlayerSlot != 7 || projectedInventory.SlotId != 0 ||
    projectedInventory.Stack != 2 || projectedInventory.Prefix != 4 ||
    projectedInventory.ItemType != 1 || !projectedInventory.IsFavorited ||
    !projectedInventory.IsNewAndShiny)
{
  throw new InvalidOperationException(
    "Inventory replication discarded authoritative item instance metadata.");
}

Terraria.Dome.Simulation.Items.Snapshots.InventorySnapshot
  equipmentInventoryProjection = new(
  new PlayerHandle(1),
  [new Terraria.Dome.Simulation.Items.Snapshots.ItemInstanceSnapshot(
    new ItemStack(4, 1),
    new ItemInstanceStateComponent(
      PrefixId: 4,
      IsFavorited: true,
      IsNewAndShiny: true))],
  0,
  1);
Terraria.Dome.Simulation.Items.Snapshots.EquipmentSnapshot equipmentProjection = new(
  new PlayerHandle(1),
  [new ItemEquipmentStateComponent(ItemEquipmentSlot.Head, 0, IsVanity: false)],
  1);
IReadOnlyList<byte[]> equipmentFrames = new EquipmentReplicationAssembler().CollectFrames(
  playerSlot: 7,
  equipmentProjection,
  equipmentInventoryProjection);
if (equipmentFrames.Count != 1)
{
  throw new InvalidOperationException(
    "Equipment replication did not emit the equipped slot frame.");
}

PlayerEquipmentPacket projectedEquipment = TerrariaPacketCodec.DecodePlayerEquipment(
  equipmentFrames[0]);
if (projectedEquipment.PlayerSlot != 7 || projectedEquipment.SlotId != 59 ||
    projectedEquipment.Stack != 1 || projectedEquipment.Prefix != 4 ||
    projectedEquipment.ItemType != 4 || !projectedEquipment.IsFavorited ||
    !projectedEquipment.IsNewAndShiny)
{
  throw new InvalidOperationException(
    "Equipment replication discarded the authoritative source inventory instance.");
}

using DomeServer server = new(new WorldGrid(width: 4200, height: 1200));
int expectedItemId = server.SpawnWorldItem(
  new ItemStack(1, 1),
  new SimulationVector(2020.0f, 0.0f));
server.Start();
using TcpClient winnerClient = new();
using TcpClient observerClient = new();
using TcpClient hiddenClient = new();
await winnerClient.ConnectAsync("127.0.0.1", server.Port);
await observerClient.ConnectAsync("127.0.0.1", server.Port);
await hiddenClient.ConnectAsync("127.0.0.1", server.Port);
NetworkStream winner = winnerClient.GetStream();
NetworkStream observer = observerClient.GetStream();
NetworkStream hidden = hiddenClient.GetStream();
byte winnerSlot;
try
{
  winnerSlot = await ActivateAsync(winner, 2000, 0, "Winner");
  _ = await ActivateAsync(observer, 2000, 0, "Observer");
  _ = await ActivateAsync(hidden, 100, 0, "Hidden");
}
catch (Exception exception)
{
  throw new InvalidOperationException(
    $"Item loopback activation failed. SessionFault={server.LastSessionFault}; " +
    $"SimulationFault={server.SimulationFault}.",
    exception);
}

await UnlockVisibilityAsync(winner, winnerSlot);
await UnlockVisibilityAsync(observer, playerSlot: 2);
await UnlockVisibilityAsync(hidden, playerSlot: 3);
await Task.Delay(TimeSpan.FromMilliseconds(100));

await winner.WriteAsync(TerrariaPacketCodec.Encode(new PlayerEquipmentPacket(
  winnerSlot,
  SlotId: 59,
  Stack: 99,
  Prefix: 0,
  ItemType: 4,
  IsFavorited: false,
  IsNewAndShiny: false)));
await Task.Delay(TimeSpan.FromMilliseconds(100));
if (server.CreateInventorySnapshot(winnerSlot).Count != 0)
{
  throw new InvalidOperationException(
    "A forged client SyncEquipment frame mutated server-owned inventory state.");
}

bool observerSawItem = false;
using CancellationTokenSource observerTimeout = new(TimeSpan.FromSeconds(5));
try
{
  while (!observerTimeout.IsCancellationRequested)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(
      await ReadFrameAsync(observer, observerTimeout.Token));
    observerSawItem |= frame.MessageId == TerrariaMessageId.SyncItem;
  }
}
catch (OperationCanceledException)
{
}

IReadOnlyList<WorldItemSnapshot> worldItems = server.CreateWorldItemSnapshots();
if (!observerSawItem || worldItems.Count != 1 || worldItems[0].ReplicationId != expectedItemId ||
    !worldItems[0].IsActive)
{
  throw new InvalidOperationException("The eligible observer did not receive the server-owned item.");
}

bool hiddenSawItem = false;
using CancellationTokenSource hiddenTimeout = new(TimeSpan.FromMilliseconds(400));
try
{
  while (!hiddenTimeout.IsCancellationRequested)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(
      await ReadFrameAsync(hidden, hiddenTimeout.Token));
    hiddenSawItem |= frame.MessageId == TerrariaMessageId.SyncItem;
  }
}
catch (OperationCanceledException)
{
}

if (hiddenSawItem)
{
  throw new InvalidOperationException("A session outside the item PVS received SyncItem state.");
}

if (server.CreateInventorySnapshot(winnerSlot).Count != 0 ||
    !server.CreateWorldItemSnapshots().Single().IsActive)
{
  throw new InvalidOperationException("A passive world item unexpectedly changed server-owned state.");
}

await hidden.WriteAsync(TerrariaPacketCodec.EncodeItemReplication(
  new Terraria.Dome.Simulation.ItemReplicationSnapshot(
    999,
    new ItemStack(1, 99),
    new Terraria.Dome.Simulation.SimulationVector(100, 0),
    true,
    1,
    new Terraria.Dome.Simulation.WorldModel.WorldSectionCoordinates(0, 0))));
await Task.Delay(TimeSpan.FromMilliseconds(100));
if (server.CreateWorldItemSnapshots().Count != 1)
{
  throw new InvalidOperationException("A client SyncItem assertion created authoritative world state.");
}

server.QueueDestroyWorldItem(expectedItemId, expectedRevision: 1);
bool observerSawTombstone = false;
using CancellationTokenSource tombstoneTimeout = new(TimeSpan.FromSeconds(5));
try
{
  while (!tombstoneTimeout.IsCancellationRequested)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(
      await ReadFrameAsync(observer, tombstoneTimeout.Token));
    if (frame.MessageId != TerrariaMessageId.SyncItem ||
        BitConverter.ToInt16(frame.Payload.Span[..2]) != expectedItemId)
    {
      continue;
    }

    observerSawTombstone = BitConverter.ToInt16(frame.Payload.Span.Slice(18, 2)) == 0 &&
      frame.Payload.Span[21] == 0 &&
      BitConverter.ToInt16(frame.Payload.Span.Slice(22, 2)) == 0;
    if (observerSawTombstone)
    {
      break;
    }
  }
}
catch (OperationCanceledException)
{
}

if (!observerSawTombstone || server.CreateWorldItemSnapshots().Single().IsActive)
{
  throw new InvalidOperationException(
    "An authoritative world-item tombstone was not PVS-replicated as an inactive SyncItem.");
}

Console.WriteLine("PASS: SyncItem is PVS-limited, server-owned and tombstone-replicated");

static async Task<byte> ActivateAsync(NetworkStream stream, short spawnX, short spawnY, string name)
{
  await stream.WriteAsync(TerrariaPacketCodec.Encode(new HelloPacket()));
  TerrariaFrame userSlot = TerrariaFrameCodec.Decode(
    await ReadFrameAsync(stream, CancellationToken.None));
  TerrariaColor color = new(0, 0, 0);
  await stream.WriteAsync(TerrariaPacketCodec.Encode(new PlayerProfilePacket(
    userSlot.Payload.Span[0], 0, 0, 0.0f, 0, name, 0, 0, 0,
    color, color, color, color, color, color, color, 0, 0, 0)));
  await stream.WriteAsync(TerrariaPacketCodec.Encode(new PlayerUuidPacket(
    $"00000000-0000-0000-0000-{userSlot.Payload.Span[0]:x12}")));
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
    userSlot.Payload.Span[0], spawnX, spawnY, 0, 0, 0, 0, 0)));
  _ = await ReadFrameAsync(stream, CancellationToken.None);
  _ = await ReadFrameAsync(stream, CancellationToken.None);
  return userSlot.Payload.Span[0];
}

static Task UnlockVisibilityAsync(NetworkStream stream, byte playerSlot)
{
  return stream.WriteAsync(TerrariaPacketCodec.EncodePlayerControls(new PlayerControlIntent(
    playerSlot,
    MoveLeft: false,
    MoveRight: false,
    Jump: false,
    UseItem: false,
    FacingRight: true,
    SelectedItem: 0),
    positionX: 0.0f,
    positionY: 0.0f)).AsTask();
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
