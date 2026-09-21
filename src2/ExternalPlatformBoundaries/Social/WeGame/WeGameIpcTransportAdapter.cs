using System.IO;
using System.Text;

namespace Terraria.ExternalPlatformBoundaries.Social.WeGame;

public sealed class WeGameIpcTransportAdapter : IWeGameIpcTransportPort, IDisposable
{
  private static readonly Encoding Utf8 = new UTF8Encoding(
    encoderShouldEmitUTF8Identifier: false,
    throwOnInvalidBytes: true);

  private readonly object _listLock = new();
  private List<byte[]> _producer = new();
  private List<byte[]> _consumer = new();
  private readonly List<byte> _totalData = new();
  private readonly WeGameIpcTransportOptions _options;
  private readonly IWeGameIpcPipe? _pipeStream;
  private CancellationTokenSource? _cancelTokenSrc;
  private bool _pipeBrokenFlag;
  private bool _isOpen;
  private int _generation;

  public WeGameIpcTransportAdapter(
    WeGameIpcTransportOptions options,
    IWeGameIpcPipe? pipeStream = null)
  {
    _options = options ?? throw new ArgumentNullException(nameof(options));
    _pipeStream = pipeStream;
  }

  public WeGameIpcTransportStatus Status
  {
    get
    {
      lock (_listLock)
      {
        return new WeGameIpcTransportStatus(
          _isOpen,
          _pipeBrokenFlag,
          _producer.Count,
          _options.BufferSize,
          _generation);
      }
    }
  }

  public IpcTransportResult Open()
  {
    lock (_listLock)
    {
      if (_isOpen)
      {
        return IpcTransportResult.AlreadyOpen;
      }

      _cancelTokenSrc?.Dispose();
      _cancelTokenSrc = new CancellationTokenSource();
      _pipeBrokenFlag = false;
      _isOpen = true;
      _generation++;
      return IpcTransportResult.Opened;
    }
  }

  public IpcTransportResult AcceptReadChunk(ReadOnlySpan<byte> chunk, bool isMessageComplete)
  {
    lock (_listLock)
    {
      if (!_isOpen)
      {
        return IpcTransportResult.NotOpen;
      }

      if (_pipeBrokenFlag)
      {
        return IpcTransportResult.Broken;
      }

      if (chunk.Length > 0)
      {
        _totalData.AddRange(chunk.ToArray());
      }

      if (_totalData.Count > _options.MaxFrameBytes)
      {
        _totalData.Clear();
        _pipeBrokenFlag = true;
        return IpcTransportResult.OversizedFrame;
      }

      if (isMessageComplete)
      {
        _producer.Add(_totalData.ToArray());
        _totalData.Clear();
      }

      return IpcTransportResult.Buffered;
    }
  }

  public IReadOnlyList<WeGameIpcFrame> DrainCompleteFrames()
  {
    List<byte[]> frames;
    lock (_listLock)
    {
      (_producer, _consumer) = (_consumer, _producer);
      frames = _consumer.ToList();
      _consumer.Clear();
    }

    var result = new List<WeGameIpcFrame>(frames.Count);
    foreach (byte[] frame in frames)
    {
      result.Add(new WeGameIpcFrame(frame));
    }

    return result;
  }

  public async ValueTask<IpcTransportResult> SendAsync(
    string payload,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(payload);

    CancellationToken token;
    IWeGameIpcPipe? pipe;
    lock (_listLock)
    {
      if (!_isOpen)
      {
        return IpcTransportResult.NotOpen;
      }

      if (_pipeBrokenFlag)
      {
        return IpcTransportResult.Broken;
      }

      pipe = _pipeStream;
      token = _cancelTokenSrc?.Token ?? CancellationToken.None;
    }

    if (pipe is null)
    {
      return IpcTransportResult.Unavailable;
    }

    using var linkedCancellation = CancellationTokenSource.CreateLinkedTokenSource(
      token,
      cancellationToken);
    try
    {
      byte[] bytes = Utf8.GetBytes(payload);
      await pipe.WriteAsync(bytes, linkedCancellation.Token).ConfigureAwait(false);
      return IpcTransportResult.Submitted;
    }
    catch (OperationCanceledException)
    {
      return IpcTransportResult.Canceled;
    }
    catch (IOException)
    {
      MarkBroken();
      return IpcTransportResult.Broken;
    }
    catch (InvalidOperationException)
    {
      MarkBroken();
      return IpcTransportResult.Broken;
    }
    catch
    {
      return IpcTransportResult.Failed;
    }
  }

  public IpcTransportResult Close()
  {
    CancellationTokenSource? cancellation;
    lock (_listLock)
    {
      if (!_isOpen)
      {
        return IpcTransportResult.AlreadyClosed;
      }

      _isOpen = false;
      cancellation = _cancelTokenSrc;
      _cancelTokenSrc = null;
      _totalData.Clear();
      _producer.Clear();
      _consumer.Clear();
    }

    cancellation?.Cancel();
    cancellation?.Dispose();
    if (_pipeStream is not null)
    {
      _pipeStream.DisposeAsync().AsTask().GetAwaiter().GetResult();
    }

    return IpcTransportResult.Closed;
  }

  public void Dispose()
  {
    Close();
  }

  private void MarkBroken()
  {
    lock (_listLock)
    {
      _pipeBrokenFlag = true;
      _isOpen = false;
    }
  }
}
