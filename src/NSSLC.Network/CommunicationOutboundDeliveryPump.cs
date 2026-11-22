using System.Net.Sockets;
using System.Threading.Channels;

using Terraria.Communication;

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
/// </remarks>
public sealed class CommunicationOutboundDeliveryPump : ICommunicationOutboundPort, IAsyncDisposable
{
  private readonly ICommunicationTransportPort _transport;
  private readonly Channel<OutboundPacketIntent> _queue;
  private readonly Channel<OutboundPacketIntent> _realtimeQueue;
  private readonly Func<OutboundIntentDelivery, ValueTask>? _onDelivery;
  private readonly Task _normalWorker;
  private readonly Task _realtimeWorker;
  private OutboundIntentDelivery? _lastDelivery;

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
    _realtimeQueue = Channel.CreateBounded<OutboundPacketIntent>(new BoundedChannelOptions(capacity)
    {
      FullMode = BoundedChannelFullMode.Wait,
      SingleReader = true,
      SingleWriter = false,
      AllowSynchronousContinuations = false
    });
    _normalWorker = Task.Run(() => RunAsync(_queue.Reader));
    _realtimeWorker = Task.Run(() => RunAsync(_realtimeQueue.Reader));
  }

  public OutboundIntentDelivery? LastDelivery => Volatile.Read(ref _lastDelivery);

  public bool TrySubmit(OutboundPacketIntent intent)
  {
    ArgumentNullException.ThrowIfNull(intent);
    return intent.Priority == OutboundPacketPriority.Realtime
        ? _realtimeQueue.Writer.TryWrite(intent)
        : _queue.Writer.TryWrite(intent);
  }

  public async ValueTask DisposeAsync()
  {
    _queue.Writer.TryComplete();
    _realtimeQueue.Writer.TryComplete();
    await Task.WhenAll(_normalWorker, _realtimeWorker).ConfigureAwait(false);
  }

  private async Task RunAsync(ChannelReader<OutboundPacketIntent> reader)
  {
    await foreach (OutboundPacketIntent intent in
        reader.ReadAllAsync().ConfigureAwait(false))
    {
      await DeliverAsync(intent).ConfigureAwait(false);
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
}
