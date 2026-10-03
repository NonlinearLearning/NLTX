using System.Threading.Channels;
using Terraria.Network;

namespace NSSLC.Infrastructure.Network;

public sealed class PacketConnection : IPacketConnection {
  private sealed class SendItem {
    public readonly TaskCompletionSource<PacketWriteReceipt> Completion = new(
        TaskCreationOptions.RunContinuationsAsynchronously);
    public required byte[] Frame;
    public required long Sequence;
    public required CancellationToken Cancellation;
    public required PacketByteBudget Budget;
    public required SemaphoreSlim Slots;
    public required long QueuedAt;
    public long LastProgress;
    public TaskCompletionSource Progress = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public Func<bool>? Admission;
    public LinkedListNode<SendItem>? Node;
    public CancellationTokenRegistration Registration;
    public long Sent;
    public bool Accepted;
    public bool Finished;
  }

  private readonly object _receiveGate = new();
  private readonly object _sendGate = new();
  private readonly IPacketTransport _transport;
  private readonly ProtocolProfile _profile;
  private readonly PacketDirection _receiveDirection;
  private readonly PacketDirection _sendDirection;
  private readonly PacketConnectionOptions _options;
  private readonly PacketByteBudget _global;
  private readonly PacketByteBudget _sendBudget;
  private readonly PacketByteBudget _controlBudget;
  private readonly SemaphoreSlim _controlSlots;
  private readonly int _controlReserve;
  private readonly PacketFrameAssembler _framer;
  private readonly Channel<byte[]> _incoming;
  private readonly SemaphoreSlim _slots;
  private readonly SemaphoreSlim _sendSignal = new(0);
  private readonly CancellationTokenSource _lifetime = new();
  private readonly CancellationTokenSource _readerStopping = new();
  private readonly LinkedList<SendItem> _queued = new();
  private readonly Task _sender;
  private readonly ITimer _partialTimer;
  private readonly TimeProvider _time;
  private SendItem? _current;
  private Exception? _terminalError;
  private bool _closed;
  private volatile bool _discardIncoming;
  private long _receivedSequence;
  private long _sentSequence;
  private long _lastPartialProgress;
  private int _reading;
  private int _waiting;
  private int _disposed;

  public ConnectionIdentity Identity { get; }
  public string ProfileKey => _profile.Key;
  public Task Completion => _sender;
  internal Func<byte, ReadOnlyMemory<byte>, bool>? Admission { get; set; }

  internal PacketConnection(ConnectionIdentity identity, IPacketTransport transport,
      ProtocolProfile profile, PacketDirection receiveDirection, PacketConnectionOptions options,
      PacketByteBudget global, TimeProvider? timeProvider = null) {
    options.Validate();
    Identity = identity;
    _transport = transport;
    _profile = profile;
    _receiveDirection = receiveDirection;
    _sendDirection = receiveDirection == PacketDirection.ClientToServer
        ? PacketDirection.ServerToClient : PacketDirection.ClientToServer;
    _options = options;
    _global = global;
    int controlItems = Math.Min(options.ControlReserveItems, options.SendItems - 1);
    _controlReserve = controlItems > 0
        ? Math.Min(options.ControlReserveBytes, options.SendBytes - options.MaximumFrameBytes) : 0;
    if (_controlReserve < 3) {
      _controlReserve = 0;
      controlItems = 0;
    }
    _sendBudget = new(options.SendBytes - _controlReserve);
    _controlBudget = new(Math.Max(1, _controlReserve));
    _controlSlots = new(Math.Max(1, controlItems), Math.Max(1, controlItems));
    _framer = new(options.MaximumFrameBytes, new(options.ReceiveBytes), global);
    _incoming = Channel.CreateBounded<byte[]>(new BoundedChannelOptions(options.ReceiveItems) {
      FullMode = BoundedChannelFullMode.Wait,
      SingleReader = false,
      SingleWriter = false,
      AllowSynchronousContinuations = false
    });
    _slots = new(options.SendItems - controlItems, options.SendItems - controlItems);
    _time = timeProvider ?? TimeProvider.System;
    _lastPartialProgress = _time.GetTimestamp();
    _partialTimer = _time.CreateTimer(CheckPartialFrame, null,
        options.PartialFrameTimeout, options.PartialFrameTimeout);
    _sender = SendLoopAsync();
  }

  public async ValueTask<PacketMessage?> ReadPacketAsync(
      CancellationToken cancellationToken = default) {
    if (Interlocked.CompareExchange(ref _reading, 1, 0) != 0) {
      throw new InvalidOperationException("Only one packet reader is allowed per connection.");
    }
    try {
      if (_discardIncoming) {
        throw _terminalError!;
      }
      while (await _incoming.Reader.WaitToReadAsync(cancellationToken).ConfigureAwait(false)) {
        cancellationToken.ThrowIfCancellationRequested();
        if (_discardIncoming) {
          throw _terminalError!;
        }
        if (!_incoming.Reader.TryRead(out byte[]? frame)) {
          continue;
        }
        try {
          byte id = frame[2];
          ReadOnlyMemory<byte> body = frame.AsMemory(3);
          if (Admission is not null && !Admission.Invoke(id, body)) {
            throw new PacketProtocolException("AdmissionRejected", id);
          }
          PacketBinding binding = _profile.Find(_receiveDirection, id);
          object packet;
          if (id == 10) {
            // Once claimed, a frame is delivered despite caller cancellation.
            await _global.LargeCodecGate.WaitAsync(_readerStopping.Token).ConfigureAwait(false);
            try {
              packet = binding.Decode(body);
            } finally {
              _global.LargeCodecGate.Release();
            }
          } else {
            packet = binding.Decode(body);
          }
          return new(_profile.Key, _receiveDirection, id, Identity,
              ++_receivedSequence, packet);
        } catch (OperationCanceledException) when (_readerStopping.IsCancellationRequested) {
          if (_terminalError is not null) {
            throw _terminalError;
          }
          throw new IOException("Connection closed while waiting to decode its claimed frame.");
        } catch (PacketProtocolException error) {
          lock (_receiveGate) {
            _terminalError = error;
            _discardIncoming = true;
            DrainIncoming();
          }
          NotifyClosed(Identity.Epoch, error);
          _transport.Close();
          throw;
        } finally {
          _framer.Release(frame);
        }
      }
      if (_terminalError is not null) {
        throw _terminalError;
      }
      return null;
    } finally {
      Volatile.Write(ref _reading, 0);
    }
  }

  public ValueTask<PacketWriteReceipt> WritePacketAsync<TPacket>(TPacket packet,
      CancellationToken cancellationToken = default) {
    ArgumentNullException.ThrowIfNull(packet);
    if (packet.GetType() != typeof(TPacket)) {
      throw new PacketEncodingException("Use the packet's exact registered type.");
    }
    return WriteObjectAsync(packet, cancellationToken);
  }

  internal ValueTask<PacketWriteReceipt> WriteObjectAsync(object packet,
      CancellationToken cancellationToken = default, Func<bool>? admission = null) {
    PacketBinding binding = _profile.Find(_sendDirection, packet.GetType());
    return QueueEncodedAsync(() => binding.Encode(packet), binding.MessageId,
        binding.MessageId == 10, cancellationToken, admission);
  }

  internal ValueTask<PacketWriteReceipt> WriteSnapshotFrameAsync(ReadOnlyMemory<byte> frame,
      CancellationToken cancellationToken, Func<bool> admission) {
    if (_sendDirection != PacketDirection.ServerToClient || frame.Length < 3
        || frame.Span[2] != 10
        || System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(frame.Span) != frame.Length) {
      throw new PacketEncodingException("Invalid shared section snapshot frame.");
    }
    return QueueEncodedAsync(() => frame.ToArray(), 10, false, cancellationToken, admission);
  }

  private async ValueTask<PacketWriteReceipt> QueueEncodedAsync(Func<byte[]> encode, byte messageId,
      bool largeCodec, CancellationToken cancellationToken, Func<bool>? admission) {
    bool control = _controlReserve != 0 && messageId is 2 or 3 or 37 or 139;
    PacketByteBudget sendBudget = control ? _controlBudget : _sendBudget;
    SemaphoreSlim slots = control ? _controlSlots : _slots;
    int reservation = control ? Math.Min(_controlReserve, _options.MaximumFrameBytes)
        : _options.MaximumFrameBytes;
    if (Interlocked.Increment(ref _waiting) > _options.MaximumWaitingWriters) {
      Interlocked.Decrement(ref _waiting);
      throw new PacketSendException(PacketSendCertainty.NotSubmitted, "SendCapacityExceeded");
    }
    bool slot = false;
    int local = 0;
    int global = 0;
    SendItem item;
    using var waiting = CancellationTokenSource.CreateLinkedTokenSource(
        cancellationToken, _lifetime.Token);
    waiting.CancelAfter(_options.SendTimeout);
    try {
      await slots.WaitAsync(waiting.Token).ConfigureAwait(false);
      slot = true;
      await sendBudget.ReserveAsync(reservation, waiting.Token)
          .ConfigureAwait(false);
      local = reservation;
      await _global.ReserveAsync(local, waiting.Token).ConfigureAwait(false);
      global = local;
      byte[] frame;
      if (largeCodec) {
        await _global.LargeCodecGate.WaitAsync(waiting.Token).ConfigureAwait(false);
        try {
          frame = encode.Invoke();
        } finally {
          _global.LargeCodecGate.Release();
        }
      } else {
        frame = encode.Invoke();
      }
      if (frame.Length > reservation) {
        throw new PacketEncodingException("Frame exceeds the configured profile limit.");
      }
      sendBudget.Release(local - frame.Length);
      _global.Release(global - frame.Length);
      local = frame.Length;
      global = frame.Length;
      lock (_sendGate) {
        waiting.Token.ThrowIfCancellationRequested();
        if (_closed) {
          throw new PacketSendException(PacketSendCertainty.NotSubmitted, "ConnectionClosed");
        }
        if (admission is not null && !admission.Invoke()) {
          throw new PacketSendException(PacketSendCertainty.NotSubmitted, "TargetExpired");
        }
        item = new SendItem {
          Frame = frame, Sequence = ++_sentSequence, Cancellation = cancellationToken,
          Admission = admission, Budget = sendBudget, Slots = slots, QueuedAt = _time.GetTimestamp()
        };
        item.Node = _queued.AddLast(item);
        local = 0;
        global = 0;
        slot = false;
      }
      item.Registration = cancellationToken.Register(() => CancelQueued(item));
      _sendSignal.Release();
    } catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested) {
      throw new PacketSendException(PacketSendCertainty.NotSubmitted, "SendAdmissionTimeoutOrClosed");
    } finally {
      Interlocked.Decrement(ref _waiting);
      if (local != 0) {
        sendBudget.Release(local);
      }
      if (global != 0) {
        _global.Release(global);
      }
      if (slot) {
        slots.Release();
      }
    }
    try {
      return await item.Completion.Task.ConfigureAwait(false);
    } finally {
      item.Registration.Dispose();
    }
  }

  internal void ReceiveBytes(long epoch, ReadOnlySpan<byte> data) {
    if (epoch != Identity.Epoch) {
      return;
    }
    PacketProtocolException? failure = null;
    lock (_receiveGate) {
      if (_closed) {
        return;
      }
      try {
        _framer.Append(data, PublishFrame);
        if (!data.IsEmpty) {
          _lastPartialProgress = _time.GetTimestamp();
        }
      } catch (PacketProtocolException error) {
        failure = error;
      }
    }
    if (failure is not null) {
      NotifyClosed(epoch, failure);
      _transport.Close();
    }
  }

  internal void NotifySent(long epoch, long bytes) {
    if (epoch != Identity.Epoch) {
      return;
    }
    bool invalid = false;
    lock (_sendGate) {
      if (_closed || _current is null || _current.Finished) {
        return;
      }
      _current.Sent += bytes;
      if (bytes <= 0 || _current.Sent > _current.Frame.Length) {
        invalid = true;
        Finish(_current, new PacketSendException(
            PacketSendCertainty.OutcomeUnknown, "InvalidSendCompletion"));
      } else if (_current.Accepted && _current.Sent == _current.Frame.Length) {
        Finish(_current, null);
      } else {
        _current.LastProgress = _time.GetTimestamp();
        TaskCompletionSource progress = _current.Progress;
        _current.Progress = new(TaskCreationOptions.RunContinuationsAsynchronously);
        progress.TrySetResult();
      }
    }
    if (invalid) {
      NotifyClosed(epoch, new PacketSendException(
          PacketSendCertainty.OutcomeUnknown, "InvalidSendCompletion"));
      _transport.Close();
    }
  }

  internal void NotifyClosed(long epoch, Exception? error = null) {
    if (epoch != Identity.Epoch) {
      return;
    }
    lock (_receiveGate) {
      if (_closed) {
        return;
      }
      if (error is null && _framer.HasPartial) {
        error = new PacketProtocolException("IncompleteFrameAtEof");
      }
      _terminalError = error;
      if (error is PacketProtocolException protocol && protocol.Code != "IncompleteFrameAtEof") {
        _discardIncoming = true;
      }
      Volatile.Write(ref _closed, true);
      _framer.Dispose();
      _incoming.Writer.TryComplete();
      if (_discardIncoming) {
        DrainIncoming();
      }
    }
    lock (_sendGate) {
      if (_current is not null) {
        Finish(_current, new PacketSendException(
            PacketSendCertainty.OutcomeUnknown, "ConnectionClosed", error));
      }
      while (_queued.First is not null) {
        SendItem item = _queued.First.Value;
        _queued.RemoveFirst();
        item.Node = null;
        Finish(item, new PacketSendException(
            PacketSendCertainty.NotSubmitted, "ConnectionClosed", error));
      }
    }
    _lifetime.Cancel();
    if (_discardIncoming) {
      _readerStopping.Cancel();
    }
  }

  public async ValueTask DisposeAsync() {
    if (Interlocked.Exchange(ref _disposed, 1) != 0) {
      await _sender.ConfigureAwait(false);
      return;
    }
    _readerStopping.Cancel();
    NotifyClosed(Identity.Epoch);
    _transport.Close();
    await _sender.ConfigureAwait(false);
    _partialTimer.Dispose();
    DrainIncoming();
  }

  private void DrainIncoming() {
    while (_incoming.Reader.TryRead(out byte[]? frame)) {
      _framer.Release(frame);
    }
  }

  private void PublishFrame(byte[] frame) {
    if (!_incoming.Writer.TryWrite(frame)) {
      _framer.Release(frame);
      throw new PacketProtocolException("ReceiveCapacityExceeded");
    }
  }

  private void CheckPartialFrame(object? state) {
    bool expired;
    lock (_receiveGate) {
      expired = !_closed && _framer.HasPartial
          && _time.GetElapsedTime(_lastPartialProgress) >= _options.PartialFrameTimeout;
    }
    if (expired) {
      NotifyClosed(Identity.Epoch, new PacketProtocolException("PartialFrameTimeout"));
      _transport.Close();
    }
  }

  private void CancelQueued(SendItem item) {
    lock (_sendGate) {
      if (item.Node is null || item.Finished) {
        return;
      }
      _queued.Remove(item.Node);
      item.Node = null;
      item.Finished = true;
      ReleaseItem(item);
      item.Completion.TrySetCanceled(item.Cancellation);
    }
  }

  private async Task SendLoopAsync() {
    try {
      while (true) {
        await _sendSignal.WaitAsync(_lifetime.Token).ConfigureAwait(false);
        SendItem item;
        lock (_sendGate) {
          if (_closed) {
            return;
          }
          if (_queued.First is null) {
            continue;
          }
          item = _queued.First.Value;
          if (item.Cancellation.IsCancellationRequested) {
            CancelQueued(item);
            continue;
          }
          if (_time.GetElapsedTime(item.QueuedAt) >= _options.SendTimeout) {
            _queued.RemoveFirst();
            item.Node = null;
            Finish(item, new PacketSendException(PacketSendCertainty.NotSubmitted, "SendQueueTimeout"));
            continue;
          }
          if (item.Admission is not null && !item.Admission.Invoke()) {
            _queued.RemoveFirst();
            item.Node = null;
            Finish(item, new PacketSendException(PacketSendCertainty.NotSubmitted, "TargetExpired"));
            continue;
          }
          _queued.RemoveFirst();
          item.Node = null;
          _current = item;
          item.LastProgress = _time.GetTimestamp();
        }
        item.Registration.Unregister();
        bool accepted = _transport.Submit(item.Frame);
        bool uncertain = false;
        lock (_sendGate) {
          if (!item.Finished) {
            item.Accepted = accepted;
            if (!accepted) {
              uncertain = item.Sent != 0;
              Finish(item, new PacketSendException(item.Sent == 0
                  ? PacketSendCertainty.NotSubmitted : PacketSendCertainty.OutcomeUnknown,
                  "TransportRejected"));
            } else if (item.Sent == item.Frame.Length) {
              Finish(item, null);
            }
          }
        }
        if (uncertain) {
          NotifyClosed(Identity.Epoch, new PacketSendException(
              PacketSendCertainty.OutcomeUnknown, "TransportRejectedAfterSend"));
          _transport.Close();
        }
        try {
          await WaitForSendAsync(item).ConfigureAwait(false);
        } catch (TimeoutException error) {
          NotifyClosed(Identity.Epoch, error);
          _transport.Close();
        } catch (PacketSendException) {
          // The public write task retains the precise submission certainty.
        }
        lock (_sendGate) {
          _current = null;
        }
      }
    } catch (OperationCanceledException) when (_lifetime.IsCancellationRequested) {
    } catch (Exception error) {
      NotifyClosed(Identity.Epoch, error);
      _transport.Close();
    }
  }

  private void Finish(SendItem item, Exception? error) {
    if (item.Finished) {
      return;
    }
    item.Finished = true;
    ReleaseItem(item);
    if (error is not null) {
      item.Completion.TrySetException(error);
    } else {
      item.Completion.TrySetResult(new(Identity, item.Sequence, item.Frame[2], item.Frame.Length));
    }
  }

  private async Task WaitForSendAsync(SendItem item) {
    while (true) {
      Task progress;
      TimeSpan remaining;
      lock (_sendGate) {
        if (item.Completion.Task.IsCompleted) {
          break;
        }
        progress = item.Progress.Task;
        remaining = _options.SendTimeout - _time.GetElapsedTime(item.LastProgress);
      }
      if (remaining <= TimeSpan.Zero) {
        throw new TimeoutException("The current frame made no sending progress.");
      }
      await Task.WhenAny(item.Completion.Task, progress)
          .WaitAsync(remaining, _time, _lifetime.Token).ConfigureAwait(false);
    }
    await item.Completion.Task.ConfigureAwait(false);
  }

  private void ReleaseItem(SendItem item) {
    item.Budget.Release(item.Frame.Length);
    _global.Release(item.Frame.Length);
    item.Slots.Release();
  }
}
