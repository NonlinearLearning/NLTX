using System;
using System.Threading;
using Terraria.NonAuthoritative.Persistence;

namespace Terraria.NonAuthoritative.WorldStorage;

/// <summary>
/// Shares the legacy WorldFile lock with transformations and snapshot-save coordinators.
/// </summary>
public sealed class WorldStorageIoGate : IWorldStorageIoGate
{
  private const int CancellationPollMilliseconds = 50;
  private readonly SemaphoreSlim? _semaphore;
  private readonly object? _synchronizationRoot;

  public WorldStorageIoGate()
  {
    _semaphore = new SemaphoreSlim(1, 1);
  }

  /// <summary>
  /// Uses the host's legacy I/O lock so migrated operations also exclude legacy file operations.
  /// </summary>
  public WorldStorageIoGate(object synchronizationRoot)
  {
    _synchronizationRoot = synchronizationRoot ??
      throw new ArgumentNullException(nameof(synchronizationRoot));
  }

  public IDisposable Enter(CancellationToken cancellationToken)
  {
    if (_semaphore is not null)
    {
      _semaphore.Wait(cancellationToken);
      return new SemaphoreLease(_semaphore);
    }

    object synchronizationRoot = _synchronizationRoot!;
    while (!Monitor.TryEnter(synchronizationRoot, CancellationPollMilliseconds))
    {
      cancellationToken.ThrowIfCancellationRequested();
    }

    if (cancellationToken.IsCancellationRequested)
    {
      Monitor.Exit(synchronizationRoot);
      cancellationToken.ThrowIfCancellationRequested();
    }

    return new MonitorLease(synchronizationRoot);
  }

  private sealed class SemaphoreLease(SemaphoreSlim semaphore) : IDisposable
  {
    private SemaphoreSlim? _semaphore = semaphore;

    public void Dispose()
    {
      SemaphoreSlim? current = Interlocked.Exchange(ref _semaphore, null);
      current?.Release();
    }
  }

  private sealed class MonitorLease(object synchronizationRoot) : IDisposable
  {
    private object? _synchronizationRoot = synchronizationRoot;

    public void Dispose()
    {
      object? current = Interlocked.Exchange(ref _synchronizationRoot, null);
      if (current is not null)
      {
        Monitor.Exit(current);
      }
    }
  }
}
