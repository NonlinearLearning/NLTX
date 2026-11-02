using System.Net.Sockets;
using System.Threading.Channels;

using Terraria.Communication;

namespace Terraria.Network;

public sealed class PacketTcpConnection : IAsyncDisposable
{
  private readonly object _lifecycleGate = new();
  private readonly Socket _socket;
  private readonly SessionEpoch _epoch;
  private readonly PacketTcpConnectionOptions _options;
  private readonly Func<InboundRawFrame, CancellationToken, ValueTask> _onFrame;
  private readonly Func<ConnectionClosed, ValueTask>? _onClosed;
  private readonly Func<Memory<byte>, CancellationToken, ValueTask<int>> _receiveBytes;
  private readonly TimeProvider _timeProvider;
  private readonly PacketFrameAssembler _assembler;
  private readonly Channel<PendingSend> _sendQueue;
  private readonly SemaphoreSlim _sendFrameSlots;
  private readonly ConnectionByteBudget _sendBudget;
  private readonly CancellationTokenSource _stop = new();
  private readonly TaskCompletionSource<bool> _runCompleted =
    new(TaskCreationOptions.RunContinuationsAsynchronously);
  private readonly TaskCompletionSource<bool> _disposeCompleted =
    new(TaskCreationOptions.RunContinuationsAsynchronously);
  private readonly TaskCompletionSource<bool> _sendsCompleted =
    new(TaskCreationOptions.RunContinuationsAsynchronously);
  private bool _runStarted;
  private bool _stopRequested;
  private bool _disposed;
  private int _activeSends;
  private int _pendingSendAdmissions;
  private string? _closeReason;
  private long _receiveSequence;
  private long _sendSequence;
  private int _lastSendCertainty = (int)TransportSendCertainty.NotSubmitted;

  /// <summary>
  /// Creates a connection whose receive adapter fills the supplied buffer and returns its byte
  /// count, or zero at EOF. The adapter must honor cancellation; it defaults to the socket read.
  /// Frame callbacks must also honor cancellation so incomplete-frame deadlines can stop dispatch.
  /// </summary>
  public PacketTcpConnection(
    Socket socket,
    SessionEpoch epoch,
    Func<InboundRawFrame, CancellationToken, ValueTask> onFrame,
    PacketTcpConnectionOptions? options = null,
    TimeProvider? timeProvider = null,
    Func<ConnectionClosed, ValueTask>? onClosed = null,
    Func<Memory<byte>, CancellationToken, ValueTask<int>>? receiveBytes = null)
  {
    ArgumentNullException.ThrowIfNull(socket);
    ArgumentNullException.ThrowIfNull(onFrame);
    if (!epoch.IsValid)
    {
      throw new ArgumentException("A TCP connection must have a valid session epoch.", nameof(epoch));
    }

    _options = options ?? new PacketTcpConnectionOptions();
    _options.Validate();
    _socket = socket;
    _epoch = epoch;
    _onFrame = onFrame;
    _onClosed = onClosed;
    _receiveBytes = receiveBytes ?? ReceiveSocketBytesAsync;
    _timeProvider = timeProvider ?? TimeProvider.System;
    _assembler = new PacketFrameAssembler(
      _options.MaximumReceiveBufferedBytes,
      _options.MaximumPendingReceiveFrames);
    _sendQueue = Channel.CreateBounded<PendingSend>(new BoundedChannelOptions(
      _options.MaximumPendingSendFrames)
    {
      AllowSynchronousContinuations = false,
      FullMode = BoundedChannelFullMode.Wait,
      SingleReader = true,
      SingleWriter = false
    });
    _sendFrameSlots = new SemaphoreSlim(
      _options.MaximumPendingSendFrames,
      _options.MaximumPendingSendFrames);
    _sendBudget = new ConnectionByteBudget(_options.MaximumPendingSendBytes);
  }

  public SessionEpoch Epoch => _epoch;
  public bool IsRunning
  {
    get
    {
      lock (_lifecycleGate)
      {
        return _runStarted && !_runCompleted.Task.IsCompleted;
      }
    }
  }

  public int BufferedReceiveBytes => _assembler.BufferedByteCount;
  public long LastReceivedSequence => Interlocked.Read(ref _receiveSequence);
  public TransportSendCertainty LastSendCertainty =>
    (TransportSendCertainty)Volatile.Read(ref _lastSendCertainty);

  public async Task RunAsync(CancellationToken cancellationToken)
  {
    lock (_lifecycleGate)
    {
      ObjectDisposedException.ThrowIf(_disposed, this);
      if (_runStarted)
      {
        throw new InvalidOperationException("A TCP connection can only be run once.");
      }

      if (_stopRequested)
      {
        throw new InvalidOperationException("A stopped TCP connection cannot be run.");
      }

      _runStarted = true;
    }

    using CancellationTokenSource linkedStop =
      CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _stop.Token);
    Task sendTask = SendLoopAsync(linkedStop.Token);
    try
    {
      await ReceiveLoopAsync(linkedStop.Token).ConfigureAwait(false);
      SetCloseReason(linkedStop.IsCancellationRequested ? "LocalShutdown" : "RemoteClosed");
    }
    catch (OperationCanceledException) when (linkedStop.IsCancellationRequested)
    {
      SetCloseReason("LocalShutdown");
    }
    catch (PacketFrameFormatException)
    {
      SetCloseReason("ProtocolFailure");
      throw;
    }
    catch (SocketException)
    {
      SetCloseReason("TransportFailure");
      throw;
    }
    catch
    {
      SetCloseReason("InboundDispatchFailure");
      throw;
    }
    finally
    {
      try
      {
        RequestStop(_closeReason ?? "LocalShutdown");
      }
      finally
      {
        try
        {
          await sendTask.ConfigureAwait(false);
        }
        finally
        {
          _socket.Dispose();
          try
          {
            await _sendsCompleted.Task.ConfigureAwait(false);
            _assembler.Dispose();
            await NotifyClosedAsync().ConfigureAwait(false);
          }
          finally
          {
            _runCompleted.TrySetResult(true);
          }
        }
      }
    }
  }

  /// <summary>
  /// Admits one immutable outbound frame. Caller cancellation before queue transfer releases
  /// local leases in this method; after transfer, the single send reader or final drain owns
  /// completion and release. A returned receipt is local transport evidence, not client ack.
  /// </summary>
  public async ValueTask<SendReceipt> SendAsync(
    OutboundPacketIntent intent,
    CancellationToken cancellationToken)
  {
    ArgumentNullException.ThrowIfNull(intent);
    if (intent.IgnoreEpoch == _epoch || !intent.TargetEpochs.Contains(_epoch))
    {
      return CreateReceipt(
        TransportSendCertainty.NotSubmitted,
        new PacketSequence(NextSendSequence()),
        0,
        ProtocolErrorCode.StaleEpoch);
    }

    if (intent.Payload.Length > PacketFrameCodec.MaximumPayloadBytes ||
      intent.MessageId == PacketFrameCodec.NetModulesMessageId &&
      intent.Payload.Length < sizeof(ushort))
    {
      return CreateReceipt(
        TransportSendCertainty.NotSubmitted,
        new PacketSequence(NextSendSequence()),
        0,
        ProtocolErrorCode.InvalidLength);
    }

    int frameLength = PacketFrameCodec.HeaderBytes + intent.Payload.Length;
    lock (_lifecycleGate)
    {
      if (!_runStarted || _stopRequested || _disposed)
      {
        return CreateReceipt(
          TransportSendCertainty.NotSubmitted,
          new PacketSequence(NextSendSequence()),
          0,
          ProtocolErrorCode.TransportClosed);
      }

      if (_pendingSendAdmissions >= _options.MaximumPendingSendRequests)
      {
        return CreateReceipt(
          TransportSendCertainty.NotSubmitted,
          new PacketSequence(NextSendSequence()),
          0,
          ProtocolErrorCode.RateLimited);
      }

      _pendingSendAdmissions++;
      _activeSends++;
    }

    bool queued = false;
    bool slotAcquired = false;
    ConnectionByteBudget.Lease? byteLease = null;
    ConnectionByteBudget.Lease? rawLease = null;
    try
    {
      long admissionStarted = _timeProvider.GetTimestamp();
      using CancellationTokenSource deadline =
        CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _stop.Token);
      deadline.CancelAfter(_options.SendDeadline);
      await _sendFrameSlots.WaitAsync(deadline.Token).ConfigureAwait(false);
      slotAcquired = true;
      byteLease = await _sendBudget.AcquireAsync(frameLength, deadline.Token).ConfigureAwait(false);
      rawLease = await ConnectionByteBudget.SharedRaw.AcquireAsync(
        frameLength, deadline.Token).ConfigureAwait(false);
      deadline.Token.ThrowIfCancellationRequested();
      if (HasSendAdmissionExpired(admissionStarted)) {
        return CreateReceipt(
          TransportSendCertainty.NotSubmitted,
          new PacketSequence(NextSendSequence()),
          0,
          ProtocolErrorCode.RateLimited);
      }

      byte[] frameBytes = PacketFrameCodec.Encode(intent.MessageId, intent.Payload.Span);
      var pending = new PendingSend(
        frameBytes,
        new PacketSequence(NextSendSequence()),
        byteLease,
        rawLease,
        deadline,
        admissionStarted,
        cancellationToken);
      lock (_lifecycleGate)
      {
        deadline.Token.ThrowIfCancellationRequested();
        bool admissionExpired = HasSendAdmissionExpired(admissionStarted);
        if (_stopRequested || admissionExpired || !_sendQueue.Writer.TryWrite(pending))
        {
          return CreateReceipt(
            TransportSendCertainty.NotSubmitted,
            pending.Sequence,
            0,
            admissionExpired && !_stopRequested
              ? ProtocolErrorCode.RateLimited
              : ProtocolErrorCode.TransportClosed);
        }

        _pendingSendAdmissions--;
        queued = true;
        byteLease = null;
        rawLease = null;
        slotAcquired = false;
      }

      return await pending.Completion.Task.ConfigureAwait(false);
    }
    catch (OperationCanceledException)
    {
      return CreateReceipt(
        TransportSendCertainty.NotSubmitted,
        new PacketSequence(NextSendSequence()),
        0,
        cancellationToken.IsCancellationRequested
          ? null
          : _stop.IsCancellationRequested
          ? ProtocolErrorCode.TransportClosed
          : ProtocolErrorCode.RateLimited);
    }
    finally
    {
      rawLease?.Dispose();
      byteLease?.Dispose();
      if (slotAcquired)
      {
        _sendFrameSlots.Release();
      }

      CompleteSend(queued);
    }
  }

  /// <summary>
  /// Requests socket shutdown without waiting for <see cref="RunAsync"/>. This is required when
  /// a response must be fully submitted before a close-after-response transition is requested.
  /// </summary>
  public void RequestClose(string reason)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(reason);
    lock (_lifecycleGate)
    {
      if (_disposed)
      {
        return;
      }
    }

    RequestStop(reason);
  }

  /// <summary>
  /// Requests local shutdown and waits for receive, send, and active SendAsync callers before
  /// releasing semaphores, the assembler, and the stop source. Repeated callers share the same
  /// completion result.
  /// </summary>
  public async ValueTask DisposeAsync()
  {
    bool runStarted;
    bool alreadyDisposed;
    lock (_lifecycleGate)
    {
      if (_disposed)
      {
        alreadyDisposed = true;
        runStarted = false;
      }
      else
      {
        alreadyDisposed = false;
        _disposed = true;
        runStarted = _runStarted;
      }
    }

    if (alreadyDisposed)
    {
      await _disposeCompleted.Task.ConfigureAwait(false);
      return;
    }

    try
    {
      try
      {
        RequestStop("LocalShutdown");
      }
      finally
      {
        if (runStarted)
        {
          await _runCompleted.Task.ConfigureAwait(false);
        }
        else
        {
          _socket.Dispose();
        }

        await _sendsCompleted.Task.ConfigureAwait(false);
        _assembler.Dispose();
        _sendFrameSlots.Dispose();
        _stop.Dispose();
      }

      _disposeCompleted.TrySetResult(true);
    }
    catch (Exception exception)
    {
      _disposeCompleted.TrySetException(exception);
      throw;
    }
  }

  private async Task ReceiveLoopAsync(CancellationToken cancellationToken)
  {
    // Reserve the partial-frame scratch buffer, socket input, and maximum assembled frame
    // storage before allocating. Serial dispatch keeps its current frame inside this bound.
    int receiveCapacity = _options.MaximumReceiveBufferedBytes + _options.ReceiveBufferBytes +
      PacketFrameCodec.MaximumFrameBytes;
    using ConnectionByteBudget.Lease rawLease =
      await ConnectionByteBudget.SharedRaw.AcquireAsync(
        receiveCapacity, cancellationToken).ConfigureAwait(false);
    try {
      await ReceiveFramesAsync(cancellationToken).ConfigureAwait(false);
    } finally {
      _assembler.Dispose();
    }
  }

  private async Task ReceiveFramesAsync(CancellationToken cancellationToken)
  {
    byte[] receiveBuffer = new byte[_options.ReceiveBufferBytes];
    long lastProgress = _timeProvider.GetTimestamp();
    while (!cancellationToken.IsCancellationRequested)
    {
      using CancellationTokenSource receiveDeadline =
        CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
      UpdateReceiveDeadline(receiveDeadline, lastProgress, _assembler.HasPartialFrame);

      try
      {
        int bytesReceived = await _receiveBytes.Invoke(
          receiveBuffer,
          receiveDeadline.Token).ConfigureAwait(false);
        // Check the previous partial frame before new bytes can reset its progress timestamp.
        UpdateReceiveDeadline(receiveDeadline, lastProgress, _assembler.HasPartialFrame);
        if (bytesReceived < 0 || bytesReceived > receiveBuffer.Length)
        {
          throw new InvalidOperationException("The receive adapter returned an invalid byte count.");
        }

        if (bytesReceived == 0)
        {
          if (_assembler.HasPartialFrame)
          {
            throw new PacketFrameFormatException(
              PacketFrameErrorCode.TruncatedBody,
              "The peer closed the connection while a Version4 frame was incomplete.");
          }

          return;
        }

        lastProgress = _timeProvider.GetTimestamp();
        bool hasPartialFrame = _assembler.HasPartialFrameAfterAppend(
          receiveBuffer.AsSpan(0, bytesReceived));
        UpdateReceiveDeadline(receiveDeadline, lastProgress, hasPartialFrame);
        int receiveOffset = 0;
        while (receiveOffset < bytesReceived) {
          receiveDeadline.Token.ThrowIfCancellationRequested();
          receiveOffset += _assembler.AppendAvailable(
            receiveBuffer.AsSpan(receiveOffset, bytesReceived - receiveOffset));
          while (_assembler.TryReadFrame(out RawPacketFrame? frame)) {
            UpdateReceiveDeadline(receiveDeadline, lastProgress, hasPartialFrame);
            long sequence = NextReceiveSequence();
            var inbound = new InboundRawFrame(
              _epoch,
              new PacketSequence(sequence),
              _timeProvider.GetUtcNow(),
              frame!);
            await _onFrame(inbound, receiveDeadline.Token).ConfigureAwait(false);
            UpdateReceiveDeadline(receiveDeadline, lastProgress, hasPartialFrame);
          }
        }

        UpdateReceiveDeadline(receiveDeadline, lastProgress, hasPartialFrame);
      }
      catch (OperationCanceledException) when (
        !cancellationToken.IsCancellationRequested && receiveDeadline.IsCancellationRequested)
      {
        throw new PacketFrameFormatException(
          PacketFrameErrorCode.ReceiveProgressTimeout,
          "The peer stopped making progress while a packet frame was incomplete.");
      }
    }
  }

  private void UpdateReceiveDeadline(
      CancellationTokenSource deadline,
      long lastProgress,
      bool hasPartialFrame) {
    deadline.Token.ThrowIfCancellationRequested();
    if (!hasPartialFrame)
    {
      deadline.CancelAfter(Timeout.InfiniteTimeSpan);
      return;
    }

    TimeSpan remaining = _options.ReceiveNoProgressDeadline -
      _timeProvider.GetElapsedTime(lastProgress);
    if (remaining <= TimeSpan.Zero)
    {
      throw new PacketFrameFormatException(
        PacketFrameErrorCode.ReceiveProgressTimeout,
        "The peer stopped making progress while a packet frame was incomplete.");
    }

    deadline.CancelAfter(remaining);
  }

  private ValueTask<int> ReceiveSocketBytesAsync(
    Memory<byte> buffer,
    CancellationToken cancellationToken)
  {
    return _socket.ReceiveAsync(buffer, SocketFlags.None, cancellationToken);
  }

  private async Task SendLoopAsync(CancellationToken cancellationToken)
  {
    try
    {
      await foreach (PendingSend pending in _sendQueue.Reader.ReadAllAsync(cancellationToken)
        .ConfigureAwait(false))
      {
        SendReceipt receipt;
        try
        {
          receipt = await SendFrameAsync(pending, cancellationToken).ConfigureAwait(false);
        }
        catch
        {
          SendReceipt uncertainReceipt = CreateReceipt(
            TransportSendCertainty.OutcomeUnknown,
            pending.Sequence,
            pending.BytesSubmitted,
            ProtocolErrorCode.TransportFailure);
          pending.Completion.TrySetResult(uncertainReceipt);
          Volatile.Write(ref _lastSendCertainty, (int)uncertainReceipt.Certainty);
          pending.Release(_sendFrameSlots);
          RequestStop("SendFailure");
          throw;
        }

        pending.Completion.TrySetResult(receipt);
        Volatile.Write(ref _lastSendCertainty, (int)receipt.Certainty);
        pending.Release(_sendFrameSlots);
        if (receipt.Certainty is TransportSendCertainty.Failed or TransportSendCertainty.OutcomeUnknown)
        {
          RequestStop("SendFailure");
          break;
        }
      }
    }
    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
    {
    }
    finally
    {
      // Close admission before the final drain so no sender can enqueue behind the reader.
      try
      {
        RequestStop(_closeReason ?? "LocalShutdown");
      }
      finally
      {
        while (_sendQueue.Reader.TryRead(out PendingSend? pending))
        {
          SendReceipt receipt = CreateReceipt(
            TransportSendCertainty.NotSubmitted,
            pending.Sequence,
            0,
            ProtocolErrorCode.TransportClosed);
          pending.Completion.TrySetResult(receipt);
          pending.Release(_sendFrameSlots);
        }
      }
    }
  }

  private async Task<SendReceipt> SendFrameAsync(
    PendingSend pending,
    CancellationToken cancellationToken)
  {
    // Verify elapsed admission time as well as cancellation: a delayed timer callback must
    // not let an expired queued frame begin writing. Claimed writes reset on progress.
    pending.AdmissionDeadline.CancelAfter(Timeout.InfiniteTimeSpan);
    if (pending.AdmissionDeadline.IsCancellationRequested ||
      cancellationToken.IsCancellationRequested ||
      HasSendAdmissionExpired(pending.AdmissionStarted))
    {
      ProtocolErrorCode? errorCode = null;
      if (cancellationToken.IsCancellationRequested)
      {
        errorCode = ProtocolErrorCode.TransportClosed;
      }
      else if (!pending.CallerCancellation.IsCancellationRequested)
      {
        errorCode = ProtocolErrorCode.RateLimited;
      }

      return CreateReceipt(
        TransportSendCertainty.NotSubmitted,
        pending.Sequence,
        0,
        errorCode);
    }

    int bytesSent = 0;
    bool socketSendInFlight = false;
    long lastProgress = _timeProvider.GetTimestamp();
    using CancellationTokenSource sendDeadline =
      CancellationTokenSource.CreateLinkedTokenSource(
        cancellationToken,
        pending.AdmissionDeadline.Token);
    sendDeadline.CancelAfter(_options.SendDeadline);
    try
    {
      while (bytesSent < pending.FrameBytes.Length)
      {
        CheckSendProgressDeadline(sendDeadline, lastProgress);
        socketSendInFlight = true;
        int sent = await _socket.SendAsync(
          pending.FrameBytes.AsMemory(bytesSent),
          SocketFlags.None,
          sendDeadline.Token).ConfigureAwait(false);
        socketSendInFlight = false;
        if (sent <= 0)
        {
          throw new SocketException((int)SocketError.ConnectionReset);
        }

        bytesSent += sent;
        pending.RecordBytesSubmitted(sent);
        if (bytesSent < pending.FrameBytes.Length) {
          // Account for submitted bytes before rejecting a late partial write. A completed
          // frame remains LocallySent even if cancellation raced with the socket result.
          CheckSendProgressDeadline(sendDeadline, lastProgress);
          lastProgress = _timeProvider.GetTimestamp();
          sendDeadline.CancelAfter(_options.SendDeadline);
        }
      }

      return CreateReceipt(
        TransportSendCertainty.LocallySent,
        pending.Sequence,
        bytesSent,
        null);
    }
    catch (OperationCanceledException) when (sendDeadline.IsCancellationRequested)
    {
      if (bytesSent == 0
          && !socketSendInFlight
          && pending.CallerCancellation.IsCancellationRequested
          && !cancellationToken.IsCancellationRequested)
      {
        return CreateReceipt(
          TransportSendCertainty.NotSubmitted,
          pending.Sequence,
          0,
          null);
      }

      TransportSendCertainty certainty = GetSendFailureCertainty(
        bytesSent,
        socketSendInFlight);
      ProtocolErrorCode errorCode = cancellationToken.IsCancellationRequested
        ? ProtocolErrorCode.TransportClosed
        : ProtocolErrorCode.TransportFailure;
      return CreateReceipt(certainty, pending.Sequence, bytesSent, errorCode);
    }
    catch (TimeoutException)
    {
      TransportSendCertainty certainty = GetSendFailureCertainty(
        bytesSent,
        socketSendInFlight);
      return CreateReceipt(
        certainty,
        pending.Sequence,
        bytesSent,
        ProtocolErrorCode.TransportFailure);
    }
    catch (SocketException)
    {
      TransportSendCertainty certainty = GetSendFailureCertainty(
        bytesSent,
        socketSendInFlight);
      return CreateReceipt(
        certainty,
        pending.Sequence,
        bytesSent,
        ProtocolErrorCode.TransportFailure);
    }
    catch (ObjectDisposedException)
    {
      TransportSendCertainty certainty = GetSendFailureCertainty(
        bytesSent,
        socketSendInFlight);
      return CreateReceipt(
        certainty,
        pending.Sequence,
        bytesSent,
        ProtocolErrorCode.TransportClosed);
    }
  }

  private void CheckSendProgressDeadline(CancellationTokenSource deadline, long lastProgress) {
    deadline.Token.ThrowIfCancellationRequested();
    if (_timeProvider.GetElapsedTime(lastProgress) >= _options.SendDeadline) {
      throw new TimeoutException("The current frame stopped making sending progress.");
    }
  }

  private static TransportSendCertainty GetSendFailureCertainty(
    int bytesSent,
    bool socketSendInFlight)
  {
    return bytesSent == 0 && !socketSendInFlight
      ? TransportSendCertainty.Failed
      : TransportSendCertainty.OutcomeUnknown;
  }

  private bool HasSendAdmissionExpired(long admissionStarted) {
    return _timeProvider.GetElapsedTime(admissionStarted) >= _options.SendDeadline;
  }

  private SendReceipt CreateReceipt(
    TransportSendCertainty certainty,
    PacketSequence sequence,
    int bytesSubmitted,
    ProtocolErrorCode? errorCode)
  {
    return new SendReceipt(certainty, _epoch, sequence, bytesSubmitted, errorCode);
  }

  private long NextReceiveSequence()
  {
    long sequence = Interlocked.Increment(ref _receiveSequence);
    if (sequence <= 0)
    {
      throw new InvalidOperationException("The inbound packet sequence reached its maximum value.");
    }

    return sequence;
  }

  private long NextSendSequence()
  {
    long sequence = Interlocked.Increment(ref _sendSequence);
    if (sequence <= 0)
    {
      throw new InvalidOperationException("The outbound packet sequence reached its maximum value.");
    }

    return sequence;
  }

  private void SetCloseReason(string reason)
  {
    Interlocked.CompareExchange(ref _closeReason, reason, null);
  }

  private void RequestStop(string reason)
  {
    SetCloseReason(reason);
    lock (_lifecycleGate)
    {
      _stopRequested = true;
      _sendQueue.Writer.TryComplete();
      if (_activeSends == 0)
      {
        _sendsCompleted.TrySetResult(true);
      }
    }

    try
    {
      if (!_stop.IsCancellationRequested)
      {
        _stop.Cancel();
      }
    }
    catch (ObjectDisposedException)
    {
    }
    finally
    {
      // A throwing cancellation callback must not prevent the socket from closing.
      ShutdownSocket();
    }
  }

  private void CompleteSend(bool queued)
  {
    lock (_lifecycleGate)
    {
      if (!queued)
      {
        _pendingSendAdmissions--;
      }

      _activeSends--;
      if (_stopRequested && _activeSends == 0)
      {
        _sendsCompleted.TrySetResult(true);
      }
    }
  }

  private void ShutdownSocket()
  {
    try
    {
      _socket.Shutdown(SocketShutdown.Both);
    }
    catch (SocketException)
    {
    }
    catch (ObjectDisposedException)
    {
    }
  }

  private async Task NotifyClosedAsync()
  {
    if (_onClosed is null)
    {
      return;
    }

    var closed = new ConnectionClosed(
      _epoch,
      _closeReason ?? "LocalShutdown");
    await _onClosed(closed).ConfigureAwait(false);
  }

  private sealed class PendingSend
  {
    private readonly ConnectionByteBudget.Lease _byteLease;
    private readonly ConnectionByteBudget.Lease _rawLease;
    private int _bytesSubmitted;

    public PendingSend(
      byte[] frameBytes,
      PacketSequence sequence,
      ConnectionByteBudget.Lease byteLease,
      ConnectionByteBudget.Lease rawLease,
      CancellationTokenSource admissionDeadline,
      long admissionStarted,
      CancellationToken callerCancellation)
    {
      FrameBytes = frameBytes;
      Sequence = sequence;
      _byteLease = byteLease;
      _rawLease = rawLease;
      AdmissionDeadline = admissionDeadline;
      AdmissionStarted = admissionStarted;
      CallerCancellation = callerCancellation;
    }

    public byte[] FrameBytes { get; }
    public PacketSequence Sequence { get; }
    public CancellationTokenSource AdmissionDeadline { get; }
    public long AdmissionStarted { get; }
    public CancellationToken CallerCancellation { get; }
    public int BytesSubmitted => Volatile.Read(ref _bytesSubmitted);
    public TaskCompletionSource<SendReceipt> Completion { get; } =
      new(TaskCreationOptions.RunContinuationsAsynchronously);

    public void RecordBytesSubmitted(int bytes)
    {
      Interlocked.Add(ref _bytesSubmitted, bytes);
    }

    public void Release(SemaphoreSlim slots)
    {
      _rawLease.Dispose();
      _byteLease.Dispose();
      slots.Release();
    }
  }
}
