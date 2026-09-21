namespace Terraria.ExternalPlatformBoundaries.Social.Provider;

public sealed class SocialRegistryLifecycleSystem
{
  private readonly ISocialProviderRegistryPort _registry;

  public SocialRegistryLifecycleSystem(ISocialProviderRegistryPort registry)
  {
    _registry = registry ?? throw new ArgumentNullException(nameof(registry));
  }

  public SocialProviderLifecycleResult Start(SocialProviderMode mode)
  {
    return _registry.Initialize(mode);
  }

  public SocialProviderLifecycleResult Stop()
  {
    return _registry.Shutdown();
  }
}
