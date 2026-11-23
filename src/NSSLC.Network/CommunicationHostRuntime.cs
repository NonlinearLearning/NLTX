using System.Net;
using Terraria.Diagnostics;
using Terraria.Session;

namespace Terraria.Network;

public sealed class CommunicationHostRuntime : IAsyncDisposable
{
  private readonly TcpCommunicationHost _host;
  private readonly CommunicationOutboundDeliveryPump _delivery;
  private long _nextEpoch;

  public CommunicationHostRuntime(IPEndPoint endpoint, int outboundCapacity = 128, TcpListenerAdapterOptions? listenerOptions = null, PacketTcpConnectionOptions? connectionOptions = null, TimeProvider? timeProvider = null, int inboundCapacity = 1024)
  {
    ArgumentNullException.ThrowIfNull(endpoint);
    Ingress = new CommunicationSessionIngress(inboundCapacity);
    _host = new TcpCommunicationHost(endpoint, AllocateEpoch, EnqueueInboundAsync, CloseSessionAsync, HandleConnectionFailureAsync, listenerOptions, connectionOptions, timeProvider, OpenSessionAsync);
    _delivery = new CommunicationOutboundDeliveryPump(_host, outboundCapacity);
  }

  public CommunicationSessionIngress Ingress { get; }
  public bool IsRunning => _host.IsRunning;
  public int ActiveConnectionCount => _host.ActiveConnectionCount;
  public ICommunicationOutboundPort OutboundPort => _delivery;

  /// <summary>汇总所有连接的 tick 批准时性度量。</summary>
  public TickBatchTimeliness TickBatchTimeliness => _host.SnapshotTickBatchTimeliness();

  public Task StartAsync(CancellationToken cancellationToken = default) => _host.StartAsync(cancellationToken);
  public Task StopAsync() => _host.StopAsync();

  public async ValueTask DisposeAsync()
  {
    await _host.DisposeAsync().ConfigureAwait(false);
    Ingress.Complete();
    await _delivery.DisposeAsync().ConfigureAwait(false);
    Ingress.Dispose();
  }

  private SessionEpoch AllocateEpoch()
  {
    long value = Interlocked.Increment(ref _nextEpoch);
    if (value <= 0)
    {
      throw new InvalidOperationException("The communication session epoch allocator exhausted its range.");
    }

    return SessionEpoch.Create((ulong)value);
  }

  private ValueTask EnqueueInboundAsync(
      InboundRawFrame frame,
      CancellationToken cancellationToken)
  {
    ArgumentNullException.ThrowIfNull(frame);
    cancellationToken.ThrowIfCancellationRequested();
    CommunicationIngressEnqueueResult result = Ingress.TryEnqueuePacket(
        frame.Epoch,
        frame.Sequence,
        frame.ReceivedAt,
        frame.Frame.MessageId,
        frame.Frame.ModuleId,
        frame.Frame.FrameBytes,
        PacketFrameCodec.HeaderBytes,
        cancellationToken);
    return result switch
    {
      CommunicationIngressEnqueueResult.Accepted => ValueTask.CompletedTask,
      CommunicationIngressEnqueueResult.Cancelled => ValueTask.FromCanceled(cancellationToken),
      CommunicationIngressEnqueueResult.Full => ValueTask.FromException(new InvalidOperationException("The bounded communication ingress queue is full.")),
      CommunicationIngressEnqueueResult.Closed => ValueTask.FromException(new InvalidOperationException("The communication ingress queue is closed.")),
      _ => ValueTask.FromException(new InvalidOperationException("The communication ingress enqueue result is unknown."))
    };
  }

  private ValueTask CloseSessionAsync(ConnectionClosed closed)
  {
    CommunicationIngressEnqueueResult result = Ingress.TryClose(closed);
    if (result is CommunicationIngressEnqueueResult.Full or CommunicationIngressEnqueueResult.Cancelled)
    {
      ServerLog.Warning(
          "ecs.session.close_dropped",
          $"epoch={closed.Epoch.Value} reason={ServerLog.Quote(closed.Reason)} result={result}");
    }

    return ValueTask.CompletedTask;
  }

  private ValueTask OpenSessionAsync(SessionEpoch epoch)
  {
    CommunicationIngressEnqueueResult result = Ingress.TryOpen(epoch);
    return result switch
    {
      CommunicationIngressEnqueueResult.Accepted => ValueTask.CompletedTask,
      CommunicationIngressEnqueueResult.Full => ValueTask.FromException(new InvalidOperationException("The bounded communication ingress queue is full while opening a session.")),
      CommunicationIngressEnqueueResult.Closed => ValueTask.FromException(new InvalidOperationException("The communication ingress queue is closed while opening a session.")),
      CommunicationIngressEnqueueResult.Cancelled => ValueTask.FromException(new InvalidOperationException("A session open event cannot be cancelled.")),
      _ => ValueTask.FromException(new InvalidOperationException("The communication ingress enqueue result is unknown."))
    };
  }

  private static ValueTask HandleConnectionFailureAsync(Exception exception)
  {
    ArgumentNullException.ThrowIfNull(exception);
    ServerLog.Warning(
        "ecs.connection.failed",
        $"type={exception.GetType().Name} detail={ServerLog.Quote(exception.Message)}");
    return ValueTask.CompletedTask;
  }
}
