using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using MinimalImplementation.Contracts;

namespace MinimalImplementation.Server.Host;

/// <summary>Delivers captured owner batches outside the commit loop and returns their receipts.</summary>
public sealed class ServerOutboundDeliveryPump
{
  private sealed record PendingDelivery(
    IOutboundDeliverySettlement Settlement,
    ServerConnectionHost.OutboundTurn? Turn,
    Func<bool>? CanSend);

  private readonly ServerLoop _commitLoop;
  private readonly ServerConnectionHost _transport;
  private readonly Channel<PendingDelivery> _queue;
  private readonly Task _worker;
  private OutboundDeliverySettlementResult? _lastSettlement;

  public ServerOutboundDeliveryPump(
    ServerLoop commitLoop,
    ServerConnectionHost transport,
    int capacity)
  {
    _commitLoop = commitLoop ?? throw new ArgumentNullException(nameof(commitLoop));
    _transport = transport ?? throw new ArgumentNullException(nameof(transport));
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(capacity);
    _queue = Channel.CreateBounded<PendingDelivery>(new BoundedChannelOptions(capacity)
    {
      FullMode = BoundedChannelFullMode.Wait,
      SingleReader = true,
      SingleWriter = true,
      AllowSynchronousContinuations = false
    });
    _worker = Task.Run(RunAsync);
  }

  public Task Completion => _worker;

  public OutboundDeliverySettlementResult? LastSettlement => Volatile.Read(ref _lastSettlement);

  /// <summary>
  /// Called only from the owner commit window. A full/closed queue settles as unsubmitted in
  /// that same window so a captured in-flight batch can become eligible for retry.
  /// </summary>
  public bool TryEnqueue(
      IOutboundDeliverySettlement settlement,
      Func<bool>? canSend = null)
  {
    ArgumentNullException.ThrowIfNull(settlement);
    ServerConnectionHost.OutboundTurn? turn = _transport.ReserveOutboundTurn(
      _commitLoop, null, settlement.Intents);
    bool enqueued = false;
    try {
      if (!_worker.IsCompleted && _queue.Writer.TryWrite(
        new PendingDelivery(settlement, turn, canSend))) {
        enqueued = true;
        return true;
      }

      var deliveries = new List<OutboundIntentDelivery>(settlement.Intents.Count);
      foreach (OutboundPacketIntent intent in settlement.Intents) {
        deliveries.Add(new OutboundIntentDelivery(intent, Array.Empty<SendReceipt>()));
      }

      Volatile.Write(ref _lastSettlement, settlement.Settle(deliveries));
      return false;
    } finally {
      if (!enqueued) {
        turn?.Dispose();
      }
    }
  }

  /// <summary>Stops admission and drains sends and owner settlements before world retirement.</summary>
  public Task CompleteAsync(CancellationToken cancellationToken = default)
  {
    _queue.Writer.TryComplete();
    return _worker.WaitAsync(cancellationToken);
  }

  private async Task RunAsync() {
    try {
      await foreach (PendingDelivery pending in _queue.Reader.ReadAllAsync()) {
        using ServerConnectionHost.OutboundTurn? outputOrder = pending.Turn;
        if (outputOrder is not null) {
          await outputOrder.Ready.ConfigureAwait(false);
        }

        IOutboundDeliverySettlement settlement = pending.Settlement;
        var deliveries = new List<OutboundIntentDelivery>(settlement.Intents.Count);
        foreach (OutboundPacketIntent intent in settlement.Intents) {
          if (pending.CanSend is not null && !pending.CanSend()) {
            deliveries.Add(new OutboundIntentDelivery(
              intent,
              CreateNotSubmittedReceipts(intent)));
            continue;
          }

          IReadOnlyList<SendReceipt> receipts;
          try {
            receipts = await _transport.SendAsync(intent).ConfigureAwait(false);
          } catch (Exception exception) when (
            exception is IOException or SocketException or ObjectDisposedException) {
            // Submission may have reached a socket before its failure became visible. Keep the
            // outcome unknown and let the owner retain dirty state; never fabricate success.
            var failures = new List<SendReceipt>(intent.TargetEpochs.Count);
            foreach (SessionEpoch target in intent.TargetEpochs) {
              if (intent.IgnoreEpoch != target) {
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

          deliveries.Add(new OutboundIntentDelivery(intent, receipts));
        }

        await _commitLoop.SubmitAsync(
          () => Volatile.Write(ref _lastSettlement, settlement.Settle(deliveries)))
          .ConfigureAwait(false);
      }
    } finally {
      // An unexpected worker/settlement failure must not strand queued turns and block the
      // router forever. Preserve the worker failure; only release their ordering resources.
      _queue.Writer.TryComplete();
      while (_queue.Reader.TryRead(out PendingDelivery? abandoned)) {
        abandoned.Turn?.Dispose();
      }
    }
  }

  private static IReadOnlyList<SendReceipt> CreateNotSubmittedReceipts(
      OutboundPacketIntent intent) {
    var receipts = new List<SendReceipt>(intent.TargetEpochs.Count);
    foreach (SessionEpoch target in intent.TargetEpochs) {
      if (intent.IgnoreEpoch != target) {
        receipts.Add(new SendReceipt(
          TransportSendCertainty.NotSubmitted,
          target,
          new PacketSequence(0),
          0,
          null));
      }
    }

    return receipts.AsReadOnly();
  }
}
