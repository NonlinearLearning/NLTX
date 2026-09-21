namespace Terraria.ExternalPlatformBoundaries.Social.Provider;

public interface ISocialProviderModule
{
  string Name { get; }

  SocialProviderCapabilities Capabilities { get; }

  SocialProviderLifecycleResult Initialize();

  SocialProviderLifecycleResult Shutdown();
}
