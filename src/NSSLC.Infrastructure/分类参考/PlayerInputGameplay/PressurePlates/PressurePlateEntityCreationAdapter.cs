namespace NLTX.PlayerInputGameplay.PressurePlates;

public interface IPressurePlateEntityCreationAdapter
{
  void EnsureEntity(PressurePlateCoordinate coordinate);
}

public sealed class PressurePlateEntityCreationLock
{
  private readonly object _syncRoot = new();

  public IDisposable Acquire()
  {
    Monitor.Enter(_syncRoot);
    return new ReleaseHandle(_syncRoot);
  }

  private sealed class ReleaseHandle : IDisposable
  {
    private object? _syncRoot;

    public ReleaseHandle(object syncRoot)
    {
      _syncRoot = syncRoot;
    }

    public void Dispose()
    {
      var syncRoot = Interlocked.Exchange(ref _syncRoot, null);
      if (syncRoot is not null)
      {
        Monitor.Exit(syncRoot);
      }
    }
  }
}
