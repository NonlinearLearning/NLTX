using Terraria.Diagnostics;
using Terraria.Session;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading.Channels;


namespace Terraria.Network;

/// <summary>
/// Delivers committed outbound intents outside the ECS update and reports local transport
/// evidence without claiming client acknowledgement.
/// </summary>
/// <remarks>
/// Realtime and normal intents have separate queues and separate dispatch loops. One loop waiting
/// on a slow socket write therefore cannot hold back the other: authoritative NPC position
/// snapshots keep flowing while a tile/world backlog drains. Order is preserved inside each
/// priority band; cross-band order is deliberately not preserved because the two bands carry
/// independent facts.
/// <para>
/// 副作用登记：本类型向宿主投递队列写入意图与批边界，并驱动传输端口发送；自身不直接做 I/O。
/// tick 批开启期间实时意图先暂存在本地列表，闭合时整体写入实时通道，闭合标记排在其后，
/// 因此批内帧与标记在通道内保持提交顺序。
/// </para>
/// </remarks>
public sealed class CommunicationOutboundDeliveryPump : ICommunicationOutboundPort, IAsyncDisposable
{
  private readonly ICommunicationTransportPort _transport;
  private readonly Channel<OutboundPacketIntent> _queue;
  private readonly Channel<RealtimeItem> _realtimeQueue;
  private readonly Func<OutboundIntentDelivery, ValueTask>? _onDelivery;
  private readonly Task _normalWorker;
  private readonly Task _realtimeWorker;
  private OutboundIntentDelivery? _lastDelivery;

  /// <summary>
  /// 当前 tick 批的暂存列表；非 null 表示批已开启。闭合时以交换方式取出，避免与提交竞态。
  /// </summary>
  private List<OutboundPacketIntent>? _staging;

  // tick 批计数：batch_size 记录每批帧数，batch_wait 记录“闭合到连接放行”的耗时，
  // 供判断发送侧是否跟不上。日志节流到 ≤1 条/秒，且只在实时 worker 上打点，不占 ECS 线程。
  private readonly long[] _batchWaits = new long[512];
  private long _batchCount;
  private long _batchFrameTotal;
  private long _batchWaitIndex;
  private long _mandatoryDropped;
  private long _batchSuperseded;
  private long _lastMetricsLogTicks;
  private long _windowBatches;
  private long _windowFrames;

  public CommunicationOutboundDeliveryPump(
      ICommunicationTransportPort transport,
      int capacity,
      Func<OutboundIntentDelivery, ValueTask>? onDelivery = null)
  {
    _transport = transport ?? throw new ArgumentNullException(nameof(transport));
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(capacity);
    _onDelivery = onDelivery;
    _queue = Channel.CreateBounded<OutboundPacketIntent>(new BoundedChannelOptions(capacity)
    {
      FullMode = BoundedChannelFullMode.Wait,
      SingleReader = true,
      SingleWriter = false,
      AllowSynchronousContinuations = false
    });
    _realtimeQueue = Channel.CreateBounded<RealtimeItem>(new BoundedChannelOptions(capacity)
    {
      FullMode = BoundedChannelFullMode.Wait,
      SingleReader = true,
      SingleWriter = false,
      AllowSynchronousContinuations = false
    });
    _normalWorker = Task.Run(() => RunAsync(_queue.Reader));
    _realtimeWorker = Task.Run(RunRealtimeAsync);
  }

  public OutboundIntentDelivery? LastDelivery => Volatile.Read(ref _lastDelivery);

  public bool TrySubmit(OutboundPacketIntent intent)
  {
    ArgumentNullException.ThrowIfNull(intent);

    if (intent.Priority != OutboundPacketPriority.Realtime)
    {
      return _queue.Writer.TryWrite(intent);
    }

    List<OutboundPacketIntent>? staging = Volatile.Read(ref _staging);
    if (staging is null)
    {
      return EnqueueRealtime(RealtimeItem.ForIntent(intent));
    }

    lock (staging)
    {
      staging.Add(intent);
    }

    return true;
  }

  public void BeginTickBatch(long tick)
  {
    _ = tick;
    Volatile.Write(ref _staging, []);
    _transport.BeginTickBatch(tick);
  }

  public void SealTickBatch(long tick)
  {
    List<OutboundPacketIntent>? staged = Interlocked.Exchange(ref _staging, null);
    int frameCount = 0;
    if (staged is not null)
    {
      List<OutboundPacketIntent> batch;
      lock (staged)
      {
        batch = [.. staged];
      }

      frameCount = batch.Count;
      foreach (OutboundPacketIntent intent in batch)
      {
        _ = EnqueueRealtime(RealtimeItem.ForIntent(intent));
      }
    }

    // 闭合标记必须排在本批帧之后进入同一条通道，宿主才会在帧全部入队后才放行普通流量。
    _ = EnqueueRealtime(RealtimeItem.ForSeal(tick, Stopwatch.GetTimestamp(), frameCount));
  }

  /// <summary>取当前累计的批指标快照，用于诊断输出。</summary>
  public OutboundBatchMetrics SnapshotBatchMetrics()
  {
    long batches = Interlocked.Read(ref _batchCount);
    long frames = Interlocked.Read(ref _batchFrameTotal);
    long waits = Interlocked.Read(ref _batchWaitIndex);
    long[] sorted = new long[(int)Math.Min(waits, _batchWaits.Length)];
    for (int index = 0; index < sorted.Length; index++)
    {
      sorted[index] = Interlocked.Read(ref _batchWaits[index]);
    }

    Array.Sort(sorted);
    double p99 = sorted.Length == 0
        ? 0
        : sorted[Math.Min(sorted.Length - 1, (int)(sorted.Length * 0.99))] /
            (double)Stopwatch.Frequency * 1000;

    return new OutboundBatchMetrics(
        batches,
        batches == 0 ? 0 : frames / (double)batches,
        p99,
        Interlocked.Read(ref _mandatoryDropped),
        Interlocked.Read(ref _batchSuperseded));
  }

  private void RecordBatch(int frameCount, long sealEnqueuedTicks)
  {
    Interlocked.Increment(ref _batchCount);
    Interlocked.Add(ref _batchFrameTotal, frameCount);
    Interlocked.Add(ref _windowBatches, 1);
    Interlocked.Add(ref _windowFrames, frameCount);

    long index = Interlocked.Increment(ref _batchWaitIndex) - 1;
    Interlocked.Exchange(
        ref _batchWaits[(int)(index % _batchWaits.Length)],
        Stopwatch.GetTimestamp() - sealEnqueuedTicks);

    // 节流：一个自然秒内最多输出一条。
    long now = Stopwatch.GetTimestamp();
    long last = Interlocked.Read(ref _lastMetricsLogTicks);
    if (last != 0 && now - last < Stopwatch.Frequency)
    {
      return;
    }

    Interlocked.Exchange(ref _lastMetricsLogTicks, now);
    long windowBatches = Interlocked.Exchange(ref _windowBatches, 0);
    long windowFrames = Interlocked.Exchange(ref _windowFrames, 0);
    OutboundBatchMetrics metrics = SnapshotBatchMetrics();
    ServerLog.Info(
        "ecs.outbound.batch",
        $"batchesPerSec={windowBatches} framesPerSec={windowFrames} " +
        $"batchSize={metrics.MeanBatchSize:F1} batchWaitP99Ms={metrics.BatchWaitP99Ms:F2} " +
        $"mandatoryDropped={metrics.MandatoryDropped} " +
        $"batchSuperseded={metrics.BatchSuperseded}");
  }

  public async ValueTask DisposeAsync()
  {
    _queue.Writer.TryComplete();
    _realtimeQueue.Writer.TryComplete();
    await Task.WhenAll(_normalWorker, _realtimeWorker).ConfigureAwait(false);
  }

  private bool EnqueueRealtime(RealtimeItem item)
  {
    if (_realtimeQueue.Writer.TryWrite(item))
    {
      return true;
    }

    // 队列满时改为后台等待写入；暂存帧此时已全部入队，顺序仍然成立。
    _ = Task.Run(async () =>
    {
      try
      {
        await _realtimeQueue.Writer.WriteAsync(item).ConfigureAwait(false);
      }
      catch (ChannelClosedException)
      {
        // 泵已关闭，丢弃剩余项。
      }
    });

    return false;
  }

  private async Task RunAsync(ChannelReader<OutboundPacketIntent> reader)
  {
    await foreach (OutboundPacketIntent intent in
        reader.ReadAllAsync().ConfigureAwait(false))
    {
      await DeliverAsync(intent).ConfigureAwait(false);
    }
  }

  private async Task RunRealtimeAsync()
  {
    await foreach (RealtimeItem item in
        _realtimeQueue.Reader.ReadAllAsync().ConfigureAwait(false))
    {
      if (item.IsSeal)
      {
        await _transport.SealTickBatchAsync(item.SealTick).ConfigureAwait(false);
        RecordBatch(item.FrameCount, item.EnqueuedTicks);
        continue;
      }

      await DeliverAsync(item.Intent!).ConfigureAwait(false);
    }
  }

  private async Task DeliverAsync(OutboundPacketIntent intent)
  {
    IReadOnlyList<SendReceipt> receipts;
    try
    {
      receipts = await _transport.SendAsync(intent).ConfigureAwait(false);
    }
    catch (Exception exception) when (
        exception is IOException or SocketException or ObjectDisposedException)
    {
      var failures = new List<SendReceipt>(intent.TargetEpochs.Count);
      foreach (SessionEpoch target in intent.TargetEpochs)
      {
        if (target != intent.IgnoreEpoch)
        {
          failures.Add(new SendReceipt(
              TransportSendCertainty.OutcomeUnknown,
              target,
              new PacketSequence(0),
              0,
              ProtocolErrorCode.TransportFailure));
        }
      }

      receipts = failures.AsReadOnly();
    }

    var delivery = new OutboundIntentDelivery(intent, receipts);
    Interlocked.Exchange(ref _lastDelivery, delivery);
    if (_onDelivery is not null)
    {
      await _onDelivery(delivery).ConfigureAwait(false);
    }
  }

  /// <summary>实时通道里的一项：要么是一个待发意图，要么是一个 tick 批闭合标记。</summary>
  private readonly record struct RealtimeItem(
      OutboundPacketIntent? Intent,
      long SealTick,
      bool IsSeal,
      int FrameCount,
      long EnqueuedTicks)
  {
    public static RealtimeItem ForIntent(OutboundPacketIntent intent) =>
        new(intent, 0, false, 0, 0);

    public static RealtimeItem ForSeal(long tick, long enqueuedTicks, int frameCount) =>
        new(null, tick, true, frameCount, enqueuedTicks);
  }
}
