namespace Terraria.ExternalPlatformBoundaries.Social.Provider;

public sealed class SocialProviderRegistryAdapter : ISocialProviderRegistryPort
{
  private readonly List<ISocialProviderModule> _modules;
  private readonly ISocialJoinRequestTickSource? _tickSource;
  private IDisposable? _tickSubscription;
  private SocialProviderRegistrySnapshot _snapshot;
  private bool _isInitialized;
  private bool _hasStopped;

  public SocialProviderRegistryAdapter(
    IEnumerable<ISocialProviderModule> modules,
    ISocialJoinRequestTickSource? tickSource = null)
  {
    ArgumentNullException.ThrowIfNull(modules);
    _modules = modules.ToList();
    if (_modules.Any(module => module is null))
    {
      throw new ArgumentException("Provider modules cannot contain null values.", nameof(modules));
    }

    _tickSource = tickSource;
  }

  public SocialProviderRegistrySnapshot Snapshot => _snapshot;

  public SocialProviderLifecycleResult Initialize(SocialProviderMode mode)
  {
    if (_isInitialized)
    {
      return SocialProviderLifecycleResult.AlreadyInitialized;
    }

    if (_hasStopped)
    {
      return SocialProviderLifecycleResult.AlreadyStopped;
    }

    var initializedModules = new List<ISocialProviderModule>();
    try
    {
      SocialProviderCapabilities capabilities = SocialProviderCapabilities.None;
      foreach (ISocialProviderModule module in _modules)
      {
        SocialProviderLifecycleResult result = module.Initialize();
        if (result != SocialProviderLifecycleResult.Initialized)
        {
          ShutdownModules(initializedModules);
          PublishFailure(mode);
          return SocialProviderLifecycleResult.InitializationFailed;
        }

        initializedModules.Add(module);
        capabilities |= module.Capabilities;
      }

      _tickSubscription = _tickSource?.Subscribe(OnJoinRequestTick);
      _isInitialized = true;
      _snapshot = new SocialProviderRegistrySnapshot(
        mode,
        IsInitialized: true,
        Generation: _snapshot.Generation + 1,
        capabilities);
      return SocialProviderLifecycleResult.Initialized;
    }
    catch
    {
      ShutdownModules(initializedModules);
      _tickSubscription?.Dispose();
      _tickSubscription = null;
      PublishFailure(mode);
      return SocialProviderLifecycleResult.InitializationFailed;
    }
  }

  public SocialProviderLifecycleResult Shutdown()
  {
    if (!_isInitialized)
    {
      return SocialProviderLifecycleResult.AlreadyStopped;
    }

    _tickSubscription?.Dispose();
    _tickSubscription = null;

    bool failed = false;
    for (int index = _modules.Count - 1; index >= 0; index--)
    {
      try
      {
        if (_modules[index].Shutdown() != SocialProviderLifecycleResult.ShutdownCompleted)
        {
          failed = true;
        }
      }
      catch
      {
        failed = true;
      }
    }

    _isInitialized = false;
    _hasStopped = true;
    _snapshot = _snapshot with
    {
      IsInitialized = false,
      Capabilities = SocialProviderCapabilities.None,
      Generation = _snapshot.Generation + 1
    };
    return failed
      ? SocialProviderLifecycleResult.ShutdownFailed
      : SocialProviderLifecycleResult.ShutdownCompleted;
  }

  private static void ShutdownModules(IReadOnlyList<ISocialProviderModule> modules)
  {
    for (int index = modules.Count - 1; index >= 0; index--)
    {
      try
      {
        modules[index].Shutdown();
      }
      catch
      {
        // Cleanup continues so every initialized module gets a shutdown attempt.
      }
    }
  }

  private void OnJoinRequestTick()
  {
    if (!_isInitialized)
    {
      return;
    }
  }

  private void PublishFailure(SocialProviderMode mode)
  {
    _snapshot = new SocialProviderRegistrySnapshot(
      mode,
      IsInitialized: false,
      Generation: _snapshot.Generation + 1,
      SocialProviderCapabilities.None);
  }
}
