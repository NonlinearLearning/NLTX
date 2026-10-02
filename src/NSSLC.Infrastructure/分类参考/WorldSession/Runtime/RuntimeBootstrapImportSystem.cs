using Terraria.WorldSession.Components;

namespace Terraria.WorldSession.Runtime;

public sealed class RuntimeBootstrapImportSystem
{
  public WorldGenerationRequest Import(
    IRuntimeBootstrapPort port,
    RuntimeBootstrapAndWorldRuleStateComponent state,
    int mapDelayTicks,
    SecretSeedFlags secretSeedFlags)
  {
    ArgumentNullException.ThrowIfNull(port);
    ArgumentNullException.ThrowIfNull(state);
    _ = port.ReadBuildIdentity();
    _ = port.ReadAnnouncementPolicy();
    WorldGenerationRequest request = port.ReadLaunchSeed();
    state.Commit(mapDelayTicks, secretSeedFlags);
    return request;
  }
}
