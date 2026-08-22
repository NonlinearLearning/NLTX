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
using Terraria.Dome.Server.Persistence;
using Terraria.Dome.Simulation;
using Terraria.Dome.Simulation.Items;
using Terraria.Dome.Simulation.WorldModel;

string temporaryPath = Path.Combine(
  Path.GetTempPath(),
  $"Terraria.Dome.Persistence.Loopback.{Guid.NewGuid():N}.dome");
try
{
  WorldGrid world = new(4200, 1200);
  WorldMetadata metadata = new("Reload loopback", new WorldSeed(55), 4200, 1200);
  using DomeSimulation source = new(world);
  _ = source.CreateNpc(new SimulationVector(2010.0f, 0.0f));
  _ = source.SpawnWorldItem(
    new ItemStack(1, 3),
    new SimulationVector(2005.0f, 300.0f));
  DomeSimulationSnapshot snapshot = source.CreatePersistenceSnapshot(metadata);
  DomeStateSaveCoordinator persistence = new();
  persistence.Save(temporaryPath, snapshot);
  DomeStateRecoveryResult recovery = persistence.TryLoad(temporaryPath);
  if (!recovery.IsSuccess || recovery.Snapshot is null)
  {
    throw new InvalidOperationException("The restart candidate was not recovered.");
  }

  using DomeServer restartedServer = new(recovery.Snapshot);
  restartedServer.Start();
  using TcpClient client = new();
  await client.ConnectAsync("127.0.0.1", restartedServer.Port);
  NetworkStream stream = client.GetStream();
  IReadOnlySet<TerrariaMessageId> bootstrapMessages = await ActivateAsync(stream, 2000, 0);

  bool sawNpc = bootstrapMessages.Contains(TerrariaMessageId.SyncNPC);
  bool sawItem = false;
  using CancellationTokenSource timeout = new(TimeSpan.FromSeconds(3));
  try
  {
    while (!timeout.IsCancellationRequested && (!sawNpc || !sawItem))
    {
      TerrariaFrame frame = TerrariaFrameCodec.Decode(
        await ReadFrameAsync(stream, timeout.Token));
      sawNpc |= frame.MessageId == TerrariaMessageId.SyncNPC;
      sawItem |= frame.MessageId == TerrariaMessageId.SyncItem;
    }
  }
  catch (OperationCanceledException)
  {
  }

  if (!sawNpc || !sawItem || restartedServer.LatestSnapshot.Players.Count != 1)
  {
    throw new InvalidOperationException(
      $"Reloaded entities were not projected to the new session. NPC={sawNpc}, " +
      $"Item={sawItem}, Players={restartedServer.LatestSnapshot.Players.Count}, " +
      $"Bootstrap={string.Join(',', bootstrapMessages)}, " +
      $"NpcStates={string.Join(',', restartedServer.CreateNpcReplicationSnapshots().Select(npc =>
        $"{npc.ReplicationId}:{npc.IsActive}:{npc.Health}"))}, " +
      $"Items={restartedServer.CreateWorldItemSnapshots().Count}.");
  }

  Console.WriteLine("PASS: save/restart/reload preserves world entities and recreates only session players");
}
finally
{
  if (File.Exists(temporaryPath))
  {
    File.Delete(temporaryPath);
  }
}

static async Task<IReadOnlySet<TerrariaMessageId>> ActivateAsync(
  NetworkStream stream,
  short spawnX,
  short spawnY)
{
  HashSet<TerrariaMessageId> messages = new();
  await stream.WriteAsync(TerrariaPacketCodec.Encode(new HelloPacket()));
  TerrariaFrame slot = TerrariaFrameCodec.Decode(
    await ReadFrameAsync(stream, CancellationToken.None));
  messages.Add(slot.MessageId);
  TerrariaFrame initialNetModules = TerrariaFrameCodec.Decode(
    await ReadFrameAsync(stream, CancellationToken.None));
  messages.Add(initialNetModules.MessageId);
  TerrariaColor color = new(0, 0, 0);
  await stream.WriteAsync(TerrariaPacketCodec.Encode(new PlayerProfilePacket(
    slot.Payload.Span[0], 0, 0, 0.0f, 0, "Reload", 0, 0, 0,
    color, color, color, color, color, color, color, 0, 0, 0)));
  await stream.WriteAsync(TerrariaPacketCodec.Encode(new PlayerUuidPacket(
    "55555555-5555-5555-5555-555555555555")));
  await stream.WriteAsync(TerrariaPacketCodec.EncodeRequestWorldData());
  _ = await ReadFrameAsync(stream, CancellationToken.None);
  await stream.WriteAsync(TerrariaPacketCodec.EncodeSpawnTileData(
    new SpawnTileDataRequestPacket(spawnX, spawnY, 0)));
  TerrariaFrame worldData = TerrariaFrameCodec.Decode(
    await ReadFrameAsync(stream, CancellationToken.None));
  messages.Add(worldData.MessageId);
  while (true)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(
      await ReadFrameAsync(stream, CancellationToken.None));
    messages.Add(frame.MessageId);
    if (frame.MessageId == TerrariaMessageId.InitialSpawn)
    {
      break;
    }
  }

  await stream.WriteAsync(TerrariaPacketCodec.Encode(new PlayerSpawnPacket(
    slot.Payload.Span[0], spawnX, spawnY, 0, 0, 0, 0, 0)));
  while (true)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(
      await ReadFrameAsync(stream, CancellationToken.None));
    messages.Add(frame.MessageId);
    if (frame.MessageId == TerrariaMessageId.FinishedConnectingToServer)
    {
      break;
    }
  }

  return messages;
}

static async Task<byte[]> ReadFrameAsync(NetworkStream stream, CancellationToken cancellationToken)
{
  byte[] prefix = new byte[2];
  await stream.ReadExactlyAsync(prefix, cancellationToken);
  int length = prefix[0] | prefix[1] << 8;
  byte[] frame = new byte[length];
  prefix.CopyTo(frame, 0);
  await stream.ReadExactlyAsync(frame.AsMemory(2), cancellationToken);
  return frame;
}
