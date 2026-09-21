namespace Terraria.ExternalPlatformBoundaries.RuntimeComposition.Platform;

public sealed class MainPlatformExecutionBoundaryAdapter
{
  public const uint EsContinuous = 2147483648u;

  public const uint EsSystemRequired = 1u;

  private readonly IPlatformExecutionStatePort _port;
  private uint _previousExecutionState;
  private bool _isHeld;

  public MainPlatformExecutionBoundaryAdapter(IPlatformExecutionStatePort port)
  {
    _port = port ?? throw new ArgumentNullException(nameof(port));
  }

  public PlatformExecutionSnapshot Snapshot =>
    new(_isHeld, _previousExecutionState);

  public PlatformExecutionResult Acquire(bool isWindows)
  {
    if (!isWindows)
    {
      return PlatformExecutionResult.Unsupported;
    }

    if (_isHeld)
    {
      return PlatformExecutionResult.AlreadyHeld;
    }

    uint previousExecutionState = _port.SetExecutionState(
      EsContinuous | EsSystemRequired);
    if (previousExecutionState == 0)
    {
      return PlatformExecutionResult.NativeFailure;
    }

    _previousExecutionState = previousExecutionState;
    _isHeld = true;
    return PlatformExecutionResult.Acquired;
  }

  public PlatformExecutionResult AcquireForCurrentPlatform()
  {
    return Acquire(OperatingSystem.IsWindows());
  }

  public PlatformExecutionResult Release(bool isWindows)
  {
    if (!isWindows)
    {
      return PlatformExecutionResult.Unsupported;
    }

    if (!_isHeld)
    {
      return PlatformExecutionResult.NotHeld;
    }

    uint restoredExecutionState = _port.SetExecutionState(_previousExecutionState);
    if (restoredExecutionState == 0)
    {
      return PlatformExecutionResult.NativeFailure;
    }

    _previousExecutionState = 0;
    _isHeld = false;
    return PlatformExecutionResult.Released;
  }

  public PlatformExecutionResult ReleaseForCurrentPlatform()
  {
    return Release(OperatingSystem.IsWindows());
  }
}
