using System.Net;
using Arch.Core;
using ArchWorld = Arch.Core.World;
using Terraria.Diagnostics;
using Terraria.Entity;

using Terraria.Communication;

namespace Terraria.Network;

/// <summary>
/// Composes the TCP host, ECS session ingress and outbound delivery pump for one server host.
/// </summary>
/// <remarks>
/// Session lifecycle callbacks run at the host/ECS boundary. They may create or release domain
/// entities, but packet handling remains queued for the registered ECS preparation system.
/// </remarks>
public sealed class CommunicationHostRuntime : IAsyncDisposable
{
  private readonly TcpCommunicationHost _host;
  private readonly CommunicationOutboundDeliveryPump _delivery;
  private readonly Func<SessionEpoch, ValueTask>? _onSessionOpened;
  private readonly Func<ConnectionClosed, ValueTask>? _onSessionClosed;
  private ulong _nextEpoch;

  public CommunicationHostRuntime(
      ArchWorld world,
      IPEndPoint endpoint,
      int outboundCapacity = 128,
      TcpListenerAdapterOptions? listenerOptions = null,
      PacketTcpConnectionOptions? connectionOptions = null,
      TimeProvider? timeProvider = null,
      Func<SessionEpoch, ValueTask>? onSessionOpened = null,
      Func<ConnectionClosed, ValueTask>? onSessionClosed = null)
  {
    ArgumentNullException.ThrowIfNull(world);
    ArgumentNullException.ThrowIfNull(endpoint);
    _onSessionOpened = onSessionOpened;
    _onSessionClosed = onSessionClosed;
    Ingress = new CommunicationSessionIngress(world);
    _host = new TcpCommunicationHost(
        endpoint,
        AllocateEpoch,
        CreateEnvelope,
        EnqueueInboundAsync,
        CloseSessionAsync,
        HandleConnectionFailureAsync,
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

  private InboundPacketEnvelope CreateEnvelope(InboundRawFrame frame)
  {
    CommunicationSessionStage stage = Ingress.TryGetStage(frame.Epoch, out CommunicationSessionStage current)
        ? current
        : CommunicationSessionStage.Connected;
    return new InboundPacketEnvelope(
        frame.Epoch,
        frame.Sequence,
        frame.ReceivedAt,
        frame.Frame.MessageId,
        frame.Frame.ModuleId,
        frame.Frame.FrameBytes,
        stage.ToString(),
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
    return _onSessionClosed is null
        ? ValueTask.CompletedTask
        : _onSessionClosed(closed);
  }

  private static ValueTask HandleConnectionFailureAsync(Exception exception)
  {
    // A single peer reset or malformed frame belongs to that connection. Do not fault the
    // listener task: the host must continue accepting later Terraria clients on port 7777.
    ServerLog.Warning(
        "ecs.connection.failed",
        $"type={exception.GetType().Name} detail={ServerLog.Quote(exception.Message)}");
    return ValueTask.CompletedTask;
  }

  private async ValueTask OpenSessionAsync(SessionEpoch epoch)
  {
    if (!Ingress.TryOpen(epoch, out _))
    {
      throw new InvalidOperationException(
          "The communication session epoch is already open.");
    }

    if (_onSessionOpened is not null)
    {
      await _onSessionOpened(epoch).ConfigureAwait(false);
    }
  }
}
