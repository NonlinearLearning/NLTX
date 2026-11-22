namespace Terraria.Communication;

public sealed class CommunicationSessionIngress : IDisposable
{
  private readonly object _gate = new();
  private readonly Queue<CommunicationIngressEvent> _queue = [];
  private readonly Queue<CommunicationIngressEvent> _overflowCloseEvents = [];
  private readonly int _capacity;
  private long _nextIngressSequence;
  private long _acceptedCount;
  private long _discardedCount;
  private long _queuedCount;
  private bool _completed;
  private bool _disposed;

  public CommunicationSessionIngress(int capacity = 1024)
  {
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(capacity);
    _capacity = capacity;
  }

  public int Capacity => _capacity;

  public int Count => (int)Math.Max(0, Volatile.Read(ref _queuedCount));

  public long AcceptedCount => Interlocked.Read(ref _acceptedCount);

  public long DiscardedCount => Interlocked.Read(ref _discardedCount);

  public bool IsCompleted
  {
    get
    {
      lock (_gate)
      {
        return _completed;
      }
    }
  }

  public CommunicationIngressEnqueueResult TryOpen(
      SessionEpoch epoch,
      CancellationToken cancellationToken = default)
  {
    return Enqueue(
        cancellationToken,
        sequence => new CommunicationIngressEvent(
            CommunicationIngressEventKind.Open,
            sequence,
            epoch,
            new PacketSequence(0),
            default,
            0,
            null,
            ReadOnlyMemory<byte>.Empty,
            0,
            null));
  }

  public CommunicationIngressEnqueueResult TryEnqueuePacket(
      SessionEpoch epoch,
      PacketSequence packetSequence,
      DateTimeOffset receivedAt,
      byte messageId,
      ushort? moduleId,
      ReadOnlyMemory<byte> frameBytes,
      int payloadOffset,
      CancellationToken cancellationToken = default)
  {
    return Enqueue(
        cancellationToken,
        sequence => new CommunicationIngressEvent(
            CommunicationIngressEventKind.Packet,
            sequence,
            epoch,
            packetSequence,
            receivedAt,
            messageId,
            moduleId,
            frameBytes,
            payloadOffset,
            null));
  }

  public CommunicationIngressEnqueueResult TryClose(
      ConnectionClosed closed,
      CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(closed);
    return Enqueue(
        cancellationToken,
        sequence => new CommunicationIngressEvent(
            CommunicationIngressEventKind.Close,
            sequence,
            closed.Epoch,
            new PacketSequence(0),
            default,
            0,
            null,
            ReadOnlyMemory<byte>.Empty,
            0,
            closed.Reason));
  }

  public bool TryDequeue(out CommunicationIngressEvent? ingressEvent)
  {
    lock (_gate)
    {
      ObjectDisposedException.ThrowIf(_disposed, this);
      if (_queue.Count == 0 && _overflowCloseEvents.Count == 0)
      {
        ingressEvent = null;
        return false;
      }

      if (_queue.Count == 0)
      {
        ingressEvent = _overflowCloseEvents.Dequeue();
      }
      else if (_overflowCloseEvents.Count == 0)
      {
        ingressEvent = _queue.Dequeue();
      }
      else if (_queue.Peek().IngressSequence <
          _overflowCloseEvents.Peek().IngressSequence)
      {
        ingressEvent = _queue.Dequeue();
      }
      else
      {
        ingressEvent = _overflowCloseEvents.Dequeue();
      }

      Interlocked.Decrement(ref _queuedCount);
      return true;
    }
  }

  public void Complete()
  {
    lock (_gate)
    {
      if (_completed)
      {
        return;
      }

      _completed = true;
    }
  }

  public void Dispose()
  {
    lock (_gate)
    {
      if (_disposed)
      {
        return;
      }

      _disposed = true;
      _completed = true;
      _queue.Clear();
      _overflowCloseEvents.Clear();
    }
  }

  private CommunicationIngressEnqueueResult Enqueue(
      CancellationToken cancellationToken,
      Func<long, CommunicationIngressEvent> createEvent)
  {
    ArgumentNullException.ThrowIfNull(createEvent);
    lock (_gate)
    {
      if (_disposed || _completed)
      {
        Interlocked.Increment(ref _discardedCount);
        return CommunicationIngressEnqueueResult.Closed;
      }

      if (cancellationToken.IsCancellationRequested)
      {
        Interlocked.Increment(ref _discardedCount);
        return CommunicationIngressEnqueueResult.Cancelled;
      }

      long sequence = checked(_nextIngressSequence + 1);
      CommunicationIngressEvent ingressEvent = createEvent(sequence);
      if (_queue.Count >= _capacity)
      {
        if (ingressEvent.Kind == CommunicationIngressEventKind.Close)
        {
          _overflowCloseEvents.Enqueue(ingressEvent);
          _nextIngressSequence = sequence;
          Interlocked.Increment(ref _queuedCount);
          Interlocked.Increment(ref _acceptedCount);
          return CommunicationIngressEnqueueResult.Accepted;
        }

        Interlocked.Increment(ref _discardedCount);
        return CommunicationIngressEnqueueResult.Full;
      }

      _queue.Enqueue(ingressEvent);
      _nextIngressSequence = sequence;
      Interlocked.Increment(ref _queuedCount);
      Interlocked.Increment(ref _acceptedCount);
      return CommunicationIngressEnqueueResult.Accepted;
    }
  }
}
