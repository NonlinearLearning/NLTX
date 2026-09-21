namespace Terraria.ExternalPlatformBoundaries.RuntimeComposition.Platform;

public sealed class PlatformExecutionLifecycleSystem
{
  private readonly MainPlatformExecutionBoundaryAdapter _adapter;

  public PlatformExecutionLifecycleSystem(MainPlatformExecutionBoundaryAdapter adapter)
  {
    _adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
  }

  public PlatformExecutionResult Start(bool isWindows)
  {
    return _adapter.Acquire(isWindows);
  }

  public PlatformExecutionResult Stop(bool isWindows)
  {
    return _adapter.Release(isWindows);
  }
}
