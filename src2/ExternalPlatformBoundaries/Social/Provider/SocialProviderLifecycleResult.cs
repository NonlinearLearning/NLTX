namespace Terraria.ExternalPlatformBoundaries.Social.Provider;

public enum SocialProviderLifecycleResult
{
  Initialized,
  AlreadyInitialized,
  InitializationFailed,
  ShutdownCompleted,
  ShutdownFailed,
  AlreadyStopped
}
