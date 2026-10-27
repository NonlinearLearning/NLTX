using System.Net;
using Arch.Core;
using ArchWorld = Arch.Core.World;
using Terraria.Entity;

namespace Terraria.Communication;

/// <summary>
/// Composes the TCP host, ECS session ingress and outbound delivery pump for one server host.
/// </summary>
public sealed class CommunicationHostRuntime : IAsyncDisposable
{
  private readonly TcpCommunicationHost _host;
  private readonly CommunicationOutboundDeliveryPump _delivery;
  private ulong _nextEpoch;

  public CommunicationHostRuntime(
      ArchWorld world,
      IPEndPoint endpoint,
      int outboundCapacity = 128,
      TcpListenerAdapterOptions? listenerOptions = null,
      PacketTcpConnectionOptions? connectionOptions = null,
      TimeProvider? timeProvider = null)
  {
    ArgumentNullException.ThrowIfNull(world);
    ArgumentNullException.ThrowIfNull(endpoint);
    Ingress = new CommunicationSessionIngress(world);
    _host = new TcpCommunicationHost(
        endpoint,
        AllocateEpoch,
        CreateEnvelope,
        EnqueueInboundAsync,
        CloseSessionAsync,
        static exception => ValueTask.FromException(exception),
        listenerOptions,
        connectionOptions,
        timeProvider,
        OpenSessionAsync);
    _delivery = new CommunicationOutboundDeliveryPump(_host, outboundCapacity);
  }

  public CommunicationSessionIngress Ingress { get; }

  public bool IsRunning => _host.IsRunning;

  public int ActiveConnectionCount => _host.ActiveConnectionCount;

  public ICommunicationOutboundPort OutboundPort => _delivery;

  public Task StartAsync(CancellationToken cancellationToken = default)
  {
    return _host.StartAsync(cancellationToken);
  }

  public Task StopAsync()
  {
    return _host.StopAsync();
  }

  public async ValueTask DisposeAsync()
  {
    await _host.DisposeAsync().ConfigureAwait(false);
    await _delivery.DisposeAsync().ConfigureAwait(false);
  }

  private SessionEpoch AllocateEpoch()
  {
    ulong value = Interlocked.Increment(ref _nextEpoch);
    return SessionEpoch.Create(value);
  }

  private static InboundPacketEnvelope CreateEnvelope(InboundRawFrame frame)
  {
    return new InboundPacketEnvelope(
        frame.Epoch,
        frame.Sequence,
        frame.ReceivedAt,
        frame.Frame.MessageId,
        frame.Frame.ModuleId,
        frame.Frame.FrameBytes,
        CommunicationSessionStage.Active.ToString(),
        null,
        null,
        null);
  }

  private ValueTask EnqueueInboundAsync(
      InboundCommunicationPacket packet,
      CancellationToken cancellationToken)
  {
    cancellationToken.ThrowIfCancellationRequested();
    return Ingress.TryEnqueue(packet)
        ? ValueTask.CompletedTask
        : ValueTask.FromException(
            new InvalidOperationException("The inbound packet belongs to an unknown or closed session."));
  }

  private ValueTask CloseSessionAsync(ConnectionClosed closed)
  {
    Ingress.TryClose(closed);
    return ValueTask.CompletedTask;
  }

  private ValueTask OpenSessionAsync(SessionEpoch epoch)
  {
    return Ingress.TryOpen(epoch, out _) && Ingress.TryActivate(epoch)
        ? ValueTask.CompletedTask
        : ValueTask.FromException(
            new InvalidOperationException("The communication session epoch is already open."));
  }
}
