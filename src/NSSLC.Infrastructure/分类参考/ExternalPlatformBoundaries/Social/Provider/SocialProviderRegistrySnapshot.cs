namespace Terraria.ExternalPlatformBoundaries.Social.Provider;

public readonly record struct SocialProviderRegistrySnapshot(
  SocialProviderMode Mode,
  bool IsInitialized,
  int Generation,
  SocialProviderCapabilities Capabilities)
{
  public bool AchievementsAvailable =>
    Capabilities.HasFlag(SocialProviderCapabilities.Achievements);

  public bool CloudAvailable => Capabilities.HasFlag(SocialProviderCapabilities.Cloud);

  public bool NetworkAvailable => Capabilities.HasFlag(SocialProviderCapabilities.Network);

  public bool JoinRequestsAvailable =>
    Capabilities.HasFlag(SocialProviderCapabilities.JoinRequests);
}
