using System.Collections.Concurrent;

namespace Terraria.NonAuthoritative.Diagnostics;

public sealed class CallTrackingSinkAdapter : IDisposable
{
  private readonly ConcurrentQueue<string> _logQueue = new();
  private readonly ConcurrentDictionary<string, byte> _loggedMethods =
    new(StringComparer.Ordinal);
  private readonly ICallTrackingOutput _output;
  private Timer? _flushTimer;
  private int _disposed;

  public CallTrackingSinkAdapter(ICallTrackingOutput output)
  {
    _output = output ?? throw new ArgumentNullException(nameof(output));
  }

  public void StartPeriodicFlush(TimeSpan interval)
  {
    ObjectDisposedException.ThrowIf(_disposed != 0, this);
    if (interval <= TimeSpan.Zero)
    {
      throw new ArgumentOutOfRangeException(nameof(interval));
    }

    _flushTimer?.Dispose();
    _flushTimer = new Timer(
      static state => ((CallTrackingSinkAdapter)state!).Flush(),
      this,
      interval,
      interval);
  }

  public bool Record(string methodName)
  {
    ObjectDisposedException.ThrowIf(_disposed != 0, this);
    if (string.IsNullOrWhiteSpace(methodName))
    {
      throw new ArgumentException("A method name is required.", nameof(methodName));
    }

    if (!_loggedMethods.TryAdd(methodName, 0))
    {
      return false;
    }

    _logQueue.Enqueue(methodName);
    return true;
  }

  public int Flush()
  {
    ObjectDisposedException.ThrowIf(_disposed != 0, this);
    int flushed = 0;
    while (_logQueue.TryDequeue(out string? methodName))
    {
      _output.Write(methodName);
      flushed++;
    }

    return flushed;
  }

  public void Dispose()
  {
    if (Interlocked.Exchange(ref _disposed, 1) != 0)
    {
      return;
    }

    _flushTimer?.Dispose();
    _flushTimer = null;
  }
}
