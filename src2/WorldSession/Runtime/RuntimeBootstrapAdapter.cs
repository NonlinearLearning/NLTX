namespace Terraria.WorldSession.Runtime;

public sealed class RuntimeBootstrapAdapter : IRuntimeBootstrapPort
{
  private readonly RuntimeBuildIdentity _buildIdentity;
  private readonly AnnouncementPolicy _announcementPolicy;
  private readonly WorldGenerationRequest _launchSeed;

  public RuntimeBootstrapAdapter(
    RuntimeBuildIdentity buildIdentity,
    AnnouncementPolicy announcementPolicy,
    WorldGenerationRequest launchSeed)
  {
    _buildIdentity = buildIdentity;
    _announcementPolicy = announcementPolicy;
    _launchSeed = launchSeed;
  }

  public RuntimeBuildIdentity ReadBuildIdentity()
  {
    return _buildIdentity;
  }

  public AnnouncementPolicy ReadAnnouncementPolicy()
  {
    return _announcementPolicy;
  }

  public WorldGenerationRequest ReadLaunchSeed()
  {
    return _launchSeed;
  }
}
