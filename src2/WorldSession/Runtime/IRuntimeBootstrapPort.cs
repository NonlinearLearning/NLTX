namespace Terraria.WorldSession.Runtime;

public interface IRuntimeBootstrapPort
{
  RuntimeBuildIdentity ReadBuildIdentity();

  AnnouncementPolicy ReadAnnouncementPolicy();

  WorldGenerationRequest ReadLaunchSeed();
}
