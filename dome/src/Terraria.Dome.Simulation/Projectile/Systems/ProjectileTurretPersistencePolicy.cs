using Terraria.Dome.Simulation;
using Terraria.Dome.Simulation.Projectile.Definitions;

namespace Terraria.Dome.Simulation.Projectile.Systems;

public static class ProjectileTurretPersistencePolicy
{
  public static bool ShouldPersist(
    int projectileType,
    bool eventActive,
    bool isDd2Summon = false)
  {
    return LegacySentryPersistenceRegistry.ShouldPersist(
      projectileType,
      eventActive,
      isDd2Summon);
  }

  public static bool CanWipe(
    int projectileType,
    PlayerHandle owner,
    PlayerHandle localPlayer,
    bool isSentry,
    bool eventActive,
    bool isDd2Summon = false)
  {
    return projectileType >= 0 && isSentry && owner.IsValid && localPlayer.IsValid &&
      owner == localPlayer &&
      !ShouldPersist(projectileType, eventActive, isDd2Summon);
  }
}
