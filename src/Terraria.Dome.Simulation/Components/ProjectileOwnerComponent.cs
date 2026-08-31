using Terraria.Dome.Simulation;

namespace Terraria.Dome.Simulation.Components;

public readonly record struct ProjectileOwnerComponent(PlayerHandle Owner);

public static class ProjectileOwnershipPolicy
{
  public static bool OwnedBySomeone(ProjectileOwnerComponent owner)
  {
    return OwnedBySomeone(owner, isNpcProjectile: false, isTrap: false);
  }

  public static bool OwnedBySomeone(
    ProjectileOwnerComponent owner,
    bool isNpcProjectile,
    bool isTrap)
  {
    return !isNpcProjectile && !isTrap && owner.Owner.IsValid;
  }
}
