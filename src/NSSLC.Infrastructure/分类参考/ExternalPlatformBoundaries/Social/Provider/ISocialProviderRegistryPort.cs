namespace Terraria.ExternalPlatformBoundaries.Social.Provider;

public interface ISocialProviderRegistryPort
{
  SocialProviderLifecycleResult Initialize(SocialProviderMode mode);

  SocialProviderLifecycleResult Shutdown();

  SocialProviderRegistrySnapshot Snapshot { get; }
}
