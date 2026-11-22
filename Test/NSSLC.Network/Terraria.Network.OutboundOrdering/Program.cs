using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using ArchWorld = Arch.Core.World;
using Terraria.Network;
using Terraria.Session;
using Terraria.Entity;
using Terraria.Network;
using Terraria.World;

// Verifies that an authoritative NPC position snapshot cannot be queued behind a tile backlog:
//
//   1. the delivery pump dispatches realtime intents on their own loop, so realtime latency does
//      not grow with normal (tile/world) volume;
//   2. a connection keeps reserved frame slots for realtime frames, so a saturated normal send
//      path can neither refuse nor borrow them;
//   3. the ECS outbound projection drains a bounded number of intents per tick, in order, without
//      dropping a rejected intent;
//   4. the world owner keeps unsent tile mutations when the projection's per-tick intake is full.

int failures = 0;

void Check(string name, bool ok, string detail)
{
  Console.WriteLine($"{(ok ? "PASS" : "FAIL")} {name} | {detail}");
  if (!ok)
  {
    failures++;
  }
}

(double P50, double P95) light = await MeasurePumpLatency(normalPerTick: 1);
(double P50, double P95) heavy = await MeasurePumpLatency(normalPerTick: 24);
bool isolated = heavy.P95 <= (light.P95 * 3.0) + 8.0 && heavy.P50 <= light.P50 + 4.0;
Check(
    "pump-realtime-latency-does-not-scale-with-tile-volume",
    isolated,
    $"light p50={light.P50:F2}ms p95={light.P95:F2}ms | " +
        $"heavy p50={heavy.P50:F2}ms p95={heavy.P95:F2}ms");

await VerifyReservedRealtimeCapacity();
VerifyProjectionBudgetAndOrder();
VerifyWorldMutationIntakeBound();
await VerifyTickBatchSealOrdering();
await VerifyWriteFailureStopsTheConnection();

Console.WriteLine();
if (failures == 0)
{
  Console.WriteLine("PASS: realtime NPC snapshots are isolated from the tile outbound backlog.");
  return 0;
}

Console.WriteLine($"FAIL: {failures} check(s) failed.");
return 1;

static OutboundPacketIntent CreateIntent(
    SessionEpoch epoch,
    byte messageId,
    int size,
    OutboundPacketPriority priority,
    byte marker)
{
  byte[] payload = new byte[size];
  payload[0] = marker;
  return new OutboundPacketIntent(
      [epoch],
      null,
      messageId,
      payload,
      CommitVersion.Initial,
      priority);
}

async Task<(double P50, double P95)> MeasurePumpLatency(int normalPerTick)
{
  const int ticks = 150;
  const double serviceMs = 2.0;
  SessionEpoch epoch = SessionEpoch.Create(1);
  ConcurrentDictionary<OutboundPacketIntent, long> submittedAt =
      new(ReferenceEqualityComparer.Instance);
  ConcurrentQueue<(OutboundPacketIntent Intent, long DeliveredAt)> deliveries = new();

  ValueTask OnDelivery(OutboundIntentDelivery delivery)
  {
    deliveries.Enqueue((delivery.Intent, Stopwatch.GetTimestamp()));
    return ValueTask.CompletedTask;
  }

  GateTransport transport = new(TimeSpan.FromMilliseconds(serviceMs));
  await using CommunicationOutboundDeliveryPump pump =
      new(transport, capacity: 65536, onDelivery: OnDelivery);

  long periodTicks = Stopwatch.Frequency / 60;
  long nextTick = Stopwatch.GetTimestamp();
  for (int tick = 0; tick < ticks; tick++)
  {
    nextTick += periodTicks;
    for (int index = 0; index < normalPerTick; index++)
    {
      OutboundPacketIntent normal = CreateIntent(
          epoch, 20, 40, OutboundPacketPriority.Normal, (byte)index);
      submittedAt[normal] = Stopwatch.GetTimestamp();
      pump.TrySubmit(normal);
    }

    OutboundPacketIntent npc = CreateIntent(epoch, 23, 40, OutboundPacketPriority.Realtime, 0);
    submittedAt[npc] = Stopwatch.GetTimestamp();
    pump.TrySubmit(npc);

    long waitMs = (nextTick - Stopwatch.GetTimestamp()) * 1000 / Stopwatch.Frequency;
    if (waitMs > 0)
    {
      await Task.Delay((int)waitMs);
    }
  }

  await Task.Delay(600);

  List<double> latencies = [];
  foreach ((OutboundPacketIntent intent, long deliveredAt) in deliveries)
  {
    if (intent.MessageId == 23 && submittedAt.TryGetValue(intent, out long start))
    {
      latencies.Add(Stopwatch.GetElapsedTime(start, deliveredAt).TotalMilliseconds);
    }
  }

  latencies.Sort();
  double Percentile(double fraction) => latencies.Count == 0
      ? double.NaN
      : latencies[Math.Min(latencies.Count - 1,
          (int)Math.Floor(fraction * (latencies.Count - 1)))];
  return (Percentile(0.50), Percentile(0.95));
}

async Task VerifyReservedRealtimeCapacity()
{
  PacketTcpConnectionOptions options = new()
  {
    MaximumPendingSendFrames = 8,
    MaximumPendingSendBytes = 2 * 1024 * 1024,
    MaximumPendingSendRequests = 4,
    MaximumPendingRealtimeSendFrames = 2,
    MaximumPendingRealtimeSendBytes = 256 * 1024,
    MaximumPendingRealtimeSendRequests = 4,
  };

  TcpListener listener = new(IPAddress.Loopback, 0);
  listener.Start();
  int port = ((IPEndPoint)listener.LocalEndpoint).Port;
  using TcpClient client = new();
  Task<TcpClient> accept = Task.Run(async () => await listener.AcceptTcpClientAsync());
  await client.ConnectAsync(IPAddress.Loopback, port);
  using TcpClient peerClient = await accept;
  peerClient.NoDelay = true;

  SessionEpoch epoch = SessionEpoch.Create(1);
  await using PacketTcpConnection connection = new(
      client.Client,
      epoch,
      static (_, _) => ValueTask.CompletedTask,
      options);

  Task run = connection.RunAsync(CancellationToken.None);

  // The peer does not read, so the single normal frame blocks in the socket and holds the only
  // normal frame slot. Normal admission and the send loop are both saturated.
  List<Task<SendReceipt>> blockingNormals = [];
  for (int index = 0; index < 8; index++)
  {
    blockingNormals.Add(connection.SendAsync(
        CreateIntent(epoch, 20, 65000, OutboundPacketPriority.Normal, (byte)index),
        CancellationToken.None).AsTask());
  }

  await Task.Delay(200);
  bool normalWriteBlocked = !blockingNormals[^1].IsCompleted;

  // A realtime frame is admitted and queued from its own reserved capacity instead of being
  // refused or waiting on the normal allowance.
  Task<SendReceipt> realtime = connection.SendAsync(
      CreateIntent(epoch, 23, 64, OutboundPacketPriority.Realtime, 0),
      CancellationToken.None).AsTask();
  await Task.Delay(150);
  bool realtimeQueuedNotRefused = !realtime.IsCompleted;

  CancellationTokenSource drainStop = new();
  Task drain = Task.Run(async () =>
  {
    byte[] buffer = new byte[64 * 1024];
    NetworkStream stream = peerClient.GetStream();
    try
    {
      while (!drainStop.IsCancellationRequested)
      {
        int read = await stream.ReadAsync(buffer, drainStop.Token);
        if (read <= 0)
        {
          return;
        }
      }
    }
    catch (OperationCanceledException)
    {
    }
    catch (IOException)
    {
    }
  });

  SendReceipt realtimeReceipt = await realtime;
  SendReceipt[] normalReceipts = await Task.WhenAll(blockingNormals);
  Check(
      "connection-realtime-not-blocked-by-normal-backlog",
      normalWriteBlocked &&
          realtimeQueuedNotRefused &&
          realtimeReceipt.Certainty == TransportSendCertainty.LocallySent &&
          normalReceipts.All(r => r.Certainty == TransportSendCertainty.LocallySent),
      $"normalBlocked={normalWriteBlocked} realtimeQueued={realtimeQueuedNotRefused} " +
          $"realtime={realtimeReceipt.Certainty} " +
          $"normal={string.Join("|", normalReceipts.Select(r => r.Certainty))}");

  drainStop.Cancel();
  await drain;
  await connection.DisposeAsync();
  listener.Stop();
  await Task.WhenAny(run, Task.Delay(5000));
}
void VerifyProjectionBudgetAndOrder()
{
  const int budget = 4;
  using ArchWorld world = ArchWorld.Create();
  RecordingOutboundPort port = new(accept: true);
  CommunicationOutboundProjectionSystem projection = new(world, port, budget);

  SessionEpoch epoch = SessionEpoch.Create(1);
  CommunicationOutboundMessageBufferComponent buffer = new();
  world.Create(new CommunicationSessionComponent(epoch), buffer);
  using EcsServerCommandBuffer commands = new();
  EcsServerTickContext context = new(world, 1, 1f / 60f, commands);

  for (int index = 0; index < 10; index++)
  {
    buffer.Enqueue(CreateIntent(epoch, 20, 8, OutboundPacketPriority.Normal, (byte)index));
  }

  projection.Update(in context);

  Check(
      "projection-bounded-per-tick",
      port.Submitted.Count == budget && buffer.Count == 10 - budget,
      $"submitted={port.Submitted.Count} remaining={buffer.Count}");

  bool ordered = true;
  for (int index = 0; index < port.Submitted.Count; index++)
  {
    ordered &= port.Submitted[index].Payload.Span[0] == index;
  }

  Check("projection-preserves-order", ordered, "first intents leave first");

  port.Accept = false;
  projection.Update(in context);
  int afterReject = buffer.Count;
  port.Accept = true;
  int submittedBeforeRetry = port.Submitted.Count;
  projection.Update(in context);
  byte firstAfterRetry = port.Submitted[submittedBeforeRetry].Payload.Span[0];
  Check(
      "projection-retries-rejected-intent-in-place",
      afterReject == 10 - budget && firstAfterRetry == budget,
      $"remainingAfterReject={afterReject} firstAfterRetry={firstAfterRetry}");

  using ArchWorld realtimeWorld = ArchWorld.Create();
  RecordingOutboundPort realtimePort = new(accept: true);
  CommunicationOutboundProjectionSystem realtimeProjection =
      new(realtimeWorld, realtimePort, budget);
  CommunicationOutboundMessageBufferComponent realtimeBuffer = new();
  realtimeWorld.Create(new CommunicationSessionComponent(SessionEpoch.Create(2)), realtimeBuffer);
  realtimeBuffer.Enqueue(CreateIntent(
      SessionEpoch.Create(2), 20, 8, OutboundPacketPriority.Normal, 1));
  realtimeBuffer.Enqueue(CreateIntent(
      SessionEpoch.Create(2), 23, 8, OutboundPacketPriority.Realtime, 2));
  using EcsServerCommandBuffer realtimeCommands = new();
  EcsServerTickContext realtimeContext = new(realtimeWorld, 1, 1f / 60f, realtimeCommands);
  realtimeProjection.Update(in realtimeContext);
  Check(
      "projection-takes-realtime-first",
      realtimePort.Submitted.Count == 2 && realtimePort.Submitted[0].MessageId == 23,
      $"first={realtimePort.Submitted[0].MessageId} count={realtimePort.Submitted.Count}");
}

void VerifyWorldMutationIntakeBound()
{
  const int bound = 3;
  WorldRuntime runtime = new(width: 32, height: 32);
  WorldTileState stone = new(Active: true, Type: 1, Wall: 0, FrameX: 0, FrameY: 0);
  for (int x = 0; x < 12; x++)
  {
    WorldInteractionResult result = runtime.TrySetTile(new WorldTileCoordinate(x, 1), stone);
    if (!result.Succeeded)
    {
      Check("world-mutation-intake-bounded-and-preserved", false, $"setup rejected: {result}");
      return;
    }
  }

  IReadOnlyList<WorldMutation> first = runtime.DrainPendingMutations(bound);
  IReadOnlyList<WorldMutation> second = runtime.DrainPendingMutations(bound);
  IReadOnlyList<WorldMutation> rest = runtime.DrainPendingMutations(1000);
  IReadOnlyList<WorldMutation> empty = runtime.DrainPendingMutations(bound);

  Check(
      "world-mutation-intake-bounded-and-preserved",
      first.Count == bound && second.Count == bound && rest.Count == 6 && empty.Count == 0,
      $"first={first.Count} second={second.Count} rest={rest.Count} empty={empty.Count}");
}

// Verifies the tick batch gate: a batch's realtime frames reach the transport before the seal
// marker, exactly one seal is emitted per batch, and normal traffic is not lost while gated.
async Task VerifyTickBatchSealOrdering()
{
  GateTransport batchTransport = new(TimeSpan.Zero);
  await using CommunicationOutboundDeliveryPump pump = new(batchTransport, capacity: 64);
  SessionEpoch epoch = SessionEpoch.Create(9);

  pump.BeginTickBatch(7);
  pump.TrySubmit(CreateIntent(epoch, 23, 8, OutboundPacketPriority.Realtime, 1));
  pump.TrySubmit(CreateIntent(epoch, 23, 8, OutboundPacketPriority.Realtime, 2));
  pump.TrySubmit(CreateIntent(epoch, 20, 8, OutboundPacketPriority.Normal, 3));
  pump.SealTickBatch(7);

  // 假传输不模拟连接层闸门，普通流量由独立 worker 投递，因此这里只等闭合标记出现；
  // “普通帧不得先于批内实时帧写出”由 PacketTcpConnection 的批门保证，不在本层断言。
  var deadline = Stopwatch.StartNew();
  while (!batchTransport.SequenceContains("seal:") && deadline.ElapsedMilliseconds < 2000)
  {
    await Task.Delay(5);
  }

  List<string> sequence;
  lock (batchTransport.Sequence)
  {
    sequence = [.. batchTransport.Sequence];
  }

  int lastRealtime = sequence.FindLastIndex(
      static entry => entry.StartsWith("send:23", StringComparison.Ordinal));
  int sealIndex = sequence.FindIndex(
      static entry => entry.StartsWith("seal:", StringComparison.Ordinal));
  Check(
      "batch-realtime-frames-precede-seal",
      lastRealtime >= 0 && sealIndex > lastRealtime,
      $"sequence=[{string.Join(',', sequence)}]");

  Check(
      "batch-seal-emitted-exactly-once",
      batchTransport.BegunBatches.Count == 1 && batchTransport.SealedBatchTicks.Count == 1,
      $"begun={batchTransport.BegunBatches.Count} sealed={batchTransport.SealedBatchTicks.Count}");

  Check(
      "batch-does-not-drop-normal-traffic",
      sequence.Contains("send:20"),
      $"normalDelivered={sequence.Contains("send:20")}");
}

// Verifies the fourth integrity law: once a frame write fails the connection is not reusable.
// A peer that disappears mid-stream must make the connection refuse every later write instead of
// resuming on a half-written frame.
async Task VerifyWriteFailureStopsTheConnection()
{
  PacketTcpConnectionOptions options = new()
  {
    MaximumPendingSendFrames = 8,
    MaximumPendingSendBytes = 1024 * 1024,
    MaximumPendingSendRequests = 8,
  };

  TcpListener listener = new(IPAddress.Loopback, 0);
  listener.Start();
  int port = ((IPEndPoint)listener.LocalEndpoint).Port;
  using TcpClient client = new();
  Task<TcpClient> accept = Task.Run(async () => await listener.AcceptTcpClientAsync());
  await client.ConnectAsync(IPAddress.Loopback, port);
  using TcpClient peerClient = await accept;
  SessionEpoch epoch = SessionEpoch.Create(31);
  await using PacketTcpConnection connection = new(
      client.Client,
      epoch,
      static (_, _) => ValueTask.CompletedTask,
      options);
  Task run = connection.RunAsync(CancellationToken.None);

  // Drop the peer, then force a write: the socket fails and the connection must close itself.
  peerClient.Close();
  await Task.Delay(100);
  SendReceipt firstFailure = await connection.SendAsync(
      CreateIntent(epoch, 23, 4096, OutboundPacketPriority.Realtime, 1),
      CancellationToken.None);
  await Task.Delay(200);

  int refused = 0;
  for (int index = 0; index < 3; index++)
  {
    SendReceipt later = await connection.SendAsync(
        CreateIntent(epoch, 20, 16, OutboundPacketPriority.Normal, (byte)index),
        CancellationToken.None);
    if (later.Certainty == TransportSendCertainty.NotSubmitted)
    {
      refused++;
    }
  }

  Check(
      "write-failure-closes-connection",
      firstFailure.Certainty is TransportSendCertainty.Failed
          or TransportSendCertainty.OutcomeUnknown
          or TransportSendCertainty.NotSubmitted,
      $"firstCertainty={firstFailure.Certainty}");

  Check(
      "no-write-after-failure",
      refused == 3,
      $"refused={refused}/3");

  await run.WaitAsync(TimeSpan.FromSeconds(5)).ContinueWith(static _ => { });
  listener.Stop();
}

sealed class RecordingOutboundPort : ICommunicationOutboundPort
{
  public RecordingOutboundPort(bool accept)
  {
    Accept = accept;
  }

  public bool Accept { get; set; }

  public List<OutboundPacketIntent> Submitted { get; } = [];

  public List<long> OpenedBatches { get; } = [];

  public List<long> SealedBatchTicks { get; } = [];

  public bool TrySubmit(OutboundPacketIntent intent)
  {
    if (!Accept)
    {
      return false;
    }

    Submitted.Add(intent);
    return true;
  }

  public void BeginTickBatch(long tick)
  {
    OpenedBatches.Add(tick);
  }

  public void SealTickBatch(long tick)
  {
    SealedBatchTicks.Add(tick);
  }
}

sealed class GateTransport : ICommunicationTransportPort
{
  private readonly TimeSpan _service;

  public GateTransport(TimeSpan service)
  {
    _service = service;
  }

  public List<long> BegunBatches { get; } = [];

  public List<long> SealedBatchTicks { get; } = [];

  public List<string> Sequence { get; } = [];

  public bool SequenceContains(string prefix)
  {
    lock (Sequence)
    {
      return Sequence.Exists(entry => entry.StartsWith(prefix, StringComparison.Ordinal));
    }
  }

  public void BeginTickBatch(long tick)
  {
    lock (BegunBatches)
    {
      BegunBatches.Add(tick);
    }

    lock (Sequence)
    {
      Sequence.Add($"begin:{tick}");
    }
  }

  public ValueTask SealTickBatchAsync(long tick, CancellationToken cancellationToken = default)
  {
    lock (SealedBatchTicks)
    {
      SealedBatchTicks.Add(tick);
    }

    lock (Sequence)
    {
      Sequence.Add($"seal:{tick}");
    }

    return ValueTask.CompletedTask;
  }

  public async ValueTask<IReadOnlyList<SendReceipt>> SendAsync(
      OutboundPacketIntent intent,
      CancellationToken cancellationToken = default)
  {
    if (_service > TimeSpan.Zero)
    {
      await Task.Delay(_service, cancellationToken).ConfigureAwait(false);
    }

    var receipts = new List<SendReceipt>(intent.TargetEpochs.Count);
    lock (Sequence)
    {
      Sequence.Add($"send:{intent.MessageId}");
    }

    foreach (SessionEpoch target in intent.TargetEpochs)
    {
      receipts.Add(new SendReceipt(
          TransportSendCertainty.LocallySent,
          target,
          new PacketSequence(1),
          1,
          null));
    }

    return receipts.AsReadOnly();
  }
}






