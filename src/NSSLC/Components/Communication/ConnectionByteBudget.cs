namespace Terraria.Communication;

internal sealed class ConnectionByteBudget
{
  internal static ConnectionByteBudget SharedRaw { get; } =
    new(PacketTcpConnectionOptions.MaximumTransportRawBytes);

  private readonly object _gate = new();
  private readonly int _maximumBytes;
  private TaskCompletionSource<bool> _changed = CreateSignal();
  private int _usedBytes;

  public ConnectionByteBudget(int maximumBytes)
  {
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maximumBytes);
    _maximumBytes = maximumBytes;
  }

  public async ValueTask<Lease> AcquireAsync(int bytes, CancellationToken cancellationToken)
  {
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(bytes);
    if (bytes > _maximumBytes)
    {
      throw new ArgumentOutOfRangeException(
        nameof(bytes),
        "The requested allocation exceeds the byte budget.");
    }

    while (true)
    {
      Task changedTask;
      lock (_gate)
      {
        cancellationToken.ThrowIfCancellationRequested();
        if (_maximumBytes - _usedBytes >= bytes)
        {
          _usedBytes += bytes;
          return new Lease(this, bytes);
        }

        changedTask = _changed.Task;
      }

      await changedTask.WaitAsync(cancellationToken).ConfigureAwait(false);
    }
  }

  private static TaskCompletionSource<bool> CreateSignal() =>
    new(TaskCreationOptions.RunContinuationsAsynchronously);

  private void Release(int bytes)
  {
    TaskCompletionSource<bool> changed;
    lock (_gate)
    {
      _usedBytes -= bytes;
      changed = _changed;
      _changed = CreateSignal();
    }

    changed.TrySetResult(true);
  }

  internal sealed class Lease : IDisposable
  {
    private ConnectionByteBudget? _owner;
    private readonly int _bytes;

    internal Lease(ConnectionByteBudget owner, int bytes)
    {
      _owner = owner;
      _bytes = bytes;
    }

    public void Dispose()
    {
      Interlocked.Exchange(ref _owner, null)?.Release(_bytes);
    }
  }
}
