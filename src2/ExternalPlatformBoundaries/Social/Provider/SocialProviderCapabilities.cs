namespace Terraria.ExternalPlatformBoundaries.Social.Provider;

[Flags]
public enum SocialProviderCapabilities
{
  None = 0,
  Achievements = 1,
  Cloud = 2,
  Network = 4,
  JoinRequests = 8
}
