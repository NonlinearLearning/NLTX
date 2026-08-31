using Terraria.Dome.Simulation;
using Terraria.Dome.Simulation.Projectile.Definitions;

namespace Terraria.Dome.Server.Replication;

public static class ProjectileNetworkImportancePolicy
{
  public static bool IsImportant(ProjectileReplicationSnapshot projectile)
  {
    return projectile.IsNetworkImportant ||
      projectile.ProjectileType == 12 ||
      LegacyPetProjectileRegistry.IsPet(projectile.ProjectileType) ||
      projectile.LegacyAiStyle == 11;
  }
}
