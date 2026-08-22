using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Terraria.Dome.Protocol.V1456.Packets;
using Terraria.Dome.Protocol.V1456.Protocol;
using Terraria.Dome.Server;
using Terraria.Dome.Simulation.Items;
using Terraria.Dome.Simulation.WorldModel;

using DomeServer server = new(new WorldGrid(width: 4200, height: 1200));
const short visibleChestX = 2000;
const short visibleChestY = 300;
_ = server.CreateChest(visibleChestX, visibleChestY);
int chestId = server.CreateChest(2000, 0);
server.SetChestItem(chestId, 0, new ItemStack(1, 3));
int signId = server.CreateSign(2002, 0, "Initial");
server.Start();
using TcpClient firstClient = new();
using TcpClient secondClient = new();
using TcpClient hiddenClient = new();
await firstClient.ConnectAsync("127.0.0.1", server.Port);
await secondClient.ConnectAsync("127.0.0.1", server.Port);
await hiddenClient.ConnectAsync("127.0.0.1", server.Port);
NetworkStream first = firstClient.GetStream();
NetworkStream second = secondClient.GetStream();
NetworkStream hidden = hiddenClient.GetStream();
byte firstSlot = await ActivateAsync(first, 2000, 0, "First");
byte secondSlot = await ActivateAsync(second, 2000, 0, "Second");
byte hiddenSlot = await ActivateAsync(hidden, 100, 0, "Hidden");

await UnlockVisibilityAsync(first, firstSlot);
await UnlockVisibilityAsync(second, secondSlot);
await UnlockVisibilityAsync(hidden, hiddenSlot);
await Task.Delay(TimeSpan.FromMilliseconds(100));

await first.WriteAsync(TerrariaPacketCodec.EncodeChestOpen(new ChestOpenIntent(
  firstSlot, chestId, 2000, 0)));
await WaitForChestOpenerAsync(server, chestId, firstSlot);
await second.WriteAsync(TerrariaPacketCodec.EncodeChestOpen(new ChestOpenIntent(
  secondSlot, chestId, 2000, 0)));
await hidden.WriteAsync(TerrariaPacketCodec.EncodeChestOpen(new ChestOpenIntent(
  hiddenSlot, chestId, 2000, 0)));

int firstItems = await CountFramesAsync(first, TerrariaMessageId.SyncChestItem, TimeSpan.FromSeconds(3));
int secondItems = await CountFramesAsync(second, TerrariaMessageId.SyncChestItem, TimeSpan.FromMilliseconds(500));
int hiddenItems = await CountFramesAsync(hidden, TerrariaMessageId.SyncChestItem, TimeSpan.FromMilliseconds(500));
if (firstItems != 40 || secondItems != 0 || hiddenItems != 0 ||
    server.CreateChestSnapshots().Single(chest => chest.ChestId == chestId).Opener?.Value != firstSlot)
{
  var chest = server.CreateChestSnapshots().Single(snapshot => snapshot.ChestId == chestId);
  string players = string.Join(",", server.LatestSnapshot.Players.Select(player =>
    $"{player.AssignedSlot}:{player.Position.X:F1},{player.Position.Y:F1}"));
  throw new InvalidOperationException(
    $"Chest open contention or PVS ownership was not authoritative. first={firstItems}, " +
    $"second={secondItems}, hidden={hiddenItems}, opener={chest.Opener?.Value}, " +
    $"pending={server.PendingProtocolCommandCount}, sessionFault={server.LastSessionFault}, " +
    $"simulationFault={server.SimulationFault}, players={players}.");
}

Console.WriteLine("PASS: two-session chest contention is authoritative and PVS-limited");

await first.WriteAsync(TerrariaPacketCodec.EncodeChestTransfer(new ChestTransferIntent(
  firstSlot, chestId, 0, 0, true)));
await second.WriteAsync(TerrariaPacketCodec.EncodeChestTransfer(new ChestTransferIntent(
  secondSlot, chestId, 1, 0, true)));
int transferFrames = await CountFramesAsync(first, TerrariaMessageId.SyncChestItem, TimeSpan.FromSeconds(2));
if (!server.CreateChestSnapshots().Single(chest => chest.ChestId == chestId).Slots[0].IsEmpty ||
    server.CreateInventorySnapshot(firstSlot).Single().Quantity <= 0 ||
    server.CreateInventorySnapshot(secondSlot).Count != 0)
{
  string firstInventoryItems = string.Join(",", server.CreateInventorySnapshot(firstSlot).Select(
    item => string.Format("{0}:{1}", item.ItemType, item.Quantity)));
  string secondInventoryItems = string.Join(",", server.CreateInventorySnapshot(secondSlot).Select(
    item => string.Format("{0}:{1}", item.ItemType, item.Quantity)));
  Console.WriteLine($"DIAG transferFrames={transferFrames} chest={server.CreateChestSnapshots().Single(chest => chest.ChestId == chestId).Slots[0].Quantity} first={firstInventoryItems} second={secondInventoryItems}");
  throw new InvalidOperationException("Chest transfer did not preserve exclusive server ownership.");
}

Console.WriteLine("PASS: two-session chest item transfer is authoritative and exclusive");

await first.WriteAsync(TerrariaPacketCodec.EncodeSignUpdate(new SignUpdateIntent(
  firstSlot, signId, 2002, 0, "Updated")));
await hidden.WriteAsync(TerrariaPacketCodec.EncodeSignUpdate(new SignUpdateIntent(
  hiddenSlot, signId, 2002, 0, "Forged")));
int secondSigns = await CountFramesAsync(second, TerrariaMessageId.OpenSignResponse, TimeSpan.FromSeconds(2));
int hiddenSigns = await CountFramesAsync(hidden, TerrariaMessageId.OpenSignResponse, TimeSpan.FromMilliseconds(400));
if (secondSigns == 0 || hiddenSigns != 0 ||
    server.CreateSignSnapshots().Single().Text != "Updated")
{
  throw new InvalidOperationException("Sign update, PVS projection or hidden-session rejection failed.");
}

Console.WriteLine("PASS: two-session sign update is authoritative and PVS-limited");

await first.WriteAsync(TerrariaPacketCodec.EncodeSignOpenRequest(
  new SignOpenRequestPacket(2002, 0)));
await AssertSignOpenResponseAsync(first, firstSlot, signId, "Updated");
Console.WriteLine("PASS: source OpenSignRequest receives a targeted V1456 sign response");

static async Task WaitForChestOpenerAsync(DomeServer server, int chestId, byte opener)
{
  using CancellationTokenSource timeout = new(TimeSpan.FromSeconds(3));
  while (!timeout.IsCancellationRequested)
  {
    var chest = server.CreateChestSnapshots().Single(snapshot => snapshot.ChestId == chestId);
    if (chest.Opener?.Value == opener)
    {
      return;
    }

    await Task.Delay(TimeSpan.FromMilliseconds(10), timeout.Token);
  }

  throw new TimeoutException("The first chest-open request did not reach the authoritative state.");
}

static async Task<byte> ActivateAsync(NetworkStream stream, short spawnX, short spawnY, string name)
{
  await stream.WriteAsync(TerrariaPacketCodec.Encode(new HelloPacket()));
  TerrariaFrame userSlot = TerrariaFrameCodec.Decode(await ReadFrameAsync(stream, CancellationToken.None));
  TerrariaFrame initialNetModules = TerrariaFrameCodec.Decode(
    await ReadFrameAsync(stream, CancellationToken.None));
  if (initialNetModules.MessageId != TerrariaMessageId.NetModules ||
      !initialNetModules.Payload.Span.SequenceEqual(new byte[] { 0, 0, 0, 0 }))
  {
    throw new InvalidOperationException("Server did not send the expected initial NetModules state.");
  }

  TerrariaColor color = new(0, 0, 0);
  await stream.WriteAsync(TerrariaPacketCodec.Encode(new PlayerProfilePacket(
    userSlot.Payload.Span[0], 0, 0, 0.0f, 0, name, 0, 0, 0,
    color, color, color, color, color, color, color, 0, 0, 0)));
  await stream.WriteAsync(TerrariaPacketCodec.Encode(new PlayerUuidPacket(
    "11111111-1111-1111-1111-111111111111")));
  await stream.WriteAsync(TerrariaPacketCodec.EncodeRequestWorldData());
  _ = await ReadFrameAsync(stream, CancellationToken.None);
  await stream.WriteAsync(TerrariaPacketCodec.EncodeSpawnTileData(new SpawnTileDataRequestPacket(spawnX, spawnY, 0)));
  int chestSizeFrames = 0;
  int chestItemFrames = 0;
  while (true)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(
      await ReadFrameAsync(stream, CancellationToken.None));
    if (frame.MessageId == TerrariaMessageId.SyncChestSize)
    {
      chestSizeFrames++;
    }

    if (frame.MessageId == TerrariaMessageId.SyncChestItem)
    {
      chestItemFrames++;
    }

    if (frame.MessageId == TerrariaMessageId.InitialSpawn)
    {
      break;
    }
  }

  if (chestSizeFrames != 1 || chestItemFrames != 40)
  {
    throw new InvalidOperationException(
      "Initial visible chest replication did not precede InitialSpawn.");
  }

  await stream.WriteAsync(TerrariaPacketCodec.Encode(new PlayerSpawnPacket(userSlot.Payload.Span[0], spawnX, spawnY, 0, 0, 0, 0, 0)));
  TerrariaFrame? hostStatus = null;
  TerrariaFrame? completion = null;
  while (hostStatus is null || completion is null)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(await ReadFrameAsync(stream, CancellationToken.None));
    if (frame.MessageId == TerrariaMessageId.HostStatus)
    {
      hostStatus = frame;
    }
    else if (frame.MessageId == TerrariaMessageId.FinishedConnectingToServer)
    {
      completion = frame;
    }
  }

  if (!hostStatus.Value.Payload.Span.SequenceEqual(new byte[] { userSlot.Payload.Span[0], 1 }))
  {
    throw new InvalidOperationException(
      "World object session did not receive HostStatus before completion after PlayerSpawn.");
  }

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

static async Task AssertSignOpenResponseAsync(
  NetworkStream stream,
  byte playerSlot,
  int signId,
  string expectedText)
{
  using CancellationTokenSource cancellation = new(TimeSpan.FromSeconds(2));
  while (!cancellation.IsCancellationRequested)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(await ReadFrameAsync(stream, cancellation.Token));
    if (frame.MessageId != TerrariaMessageId.OpenSignResponse)
    {
      continue;
    }

    SignUpdateIntent sign = TerrariaPacketCodec.DecodeSignUpdate(TerrariaFrameCodec.Encode(frame));
    if (sign.PlayerSlot == playerSlot && sign.SignId == signId && sign.Text == expectedText &&
        !sign.SuppressOpenSign)
    {
      return;
    }
  }

  throw new InvalidOperationException("OpenSignRequest did not receive a targeted source sign frame.");
}

static async Task<int> CountFramesAsync(NetworkStream stream, TerrariaMessageId messageId, TimeSpan timeout)
{
  int count = 0;
  using CancellationTokenSource cancellation = new(timeout);
  try
  {
    while (!cancellation.IsCancellationRequested)
    {
      TerrariaFrame frame = TerrariaFrameCodec.Decode(await ReadFrameAsync(stream, cancellation.Token));
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

static async Task<byte[]> ReadFrameAsync(NetworkStream stream, CancellationToken cancellationToken)
{
  byte[] prefix = new byte[2];
  await stream.ReadExactlyAsync(prefix, cancellationToken);
  int frameLength = prefix[0] | prefix[1] << 8;
  if (frameLength < 2)
  {
    throw new InvalidDataException("The peer closed before sending a complete frame.");
  }
  byte[] frame = new byte[frameLength];
  prefix.CopyTo(frame, 0);
  await stream.ReadExactlyAsync(frame.AsMemory(2), cancellationToken);
  return frame;
}
