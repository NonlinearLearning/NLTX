using System.Net.Sockets;
using System.Threading.Channels;

using Terraria.Communication;

namespace Terraria.Network;

/// <summary>
/// Delivers committed outbound intents outside the ECS update and reports local transport
/// evidence without claiming client acknowledgement.
/// </summary>
public sealed class CommunicationOutboundDeliveryPump : ICommunicationOutboundPort, IAsyncDisposable
{
  private readonly ICommunicationTransportPort _transport;
  private readonly Channel<OutboundPacketIntent> _queue;
  private readonly Func<OutboundIntentDelivery, ValueTask>? _onDelivery;
  private readonly Task _worker;
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
    _worker = Task.Run(RunAsync);
  }

  public OutboundIntentDelivery? LastDelivery => Volatile.Read(ref _lastDelivery);

  public bool TrySubmit(OutboundPacketIntent intent)
  {
    ArgumentNullException.ThrowIfNull(intent);
    return _queue.Writer.TryWrite(intent);
  }

  public async ValueTask DisposeAsync()
  {
    _queue.Writer.TryComplete();
    await _worker.ConfigureAwait(false);
  }

  private async Task RunAsync()
  {
    await foreach (OutboundPacketIntent intent in _queue.Reader.ReadAllAsync().ConfigureAwait(false))
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
      Volatile.Write(ref _lastDelivery, delivery);
      if (_onDelivery is not null)
      {
        await _onDelivery(delivery).ConfigureAwait(false);
      }
    }
  }
}
