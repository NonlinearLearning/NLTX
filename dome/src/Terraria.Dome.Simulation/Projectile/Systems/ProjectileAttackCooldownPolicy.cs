using Terraria.Dome.Simulation.Components;

namespace Terraria.Dome.Simulation.Projectile.Systems;

public static class ProjectileAttackCooldownPolicy
{
  public static bool CaresForAttackCooldown(
    bool usesOwnerMeleeHitCooldown,
    bool isNpcProjectile,
    bool isTrap,
    int ownerValue)
  {
    return usesOwnerMeleeHitCooldown &&
      ProjectileOwnershipPolicy.OwnedBySomeone(
        new ProjectileOwnerComponent(new PlayerHandle(ownerValue)),
        isNpcProjectile,
        isTrap) &&
      ownerValue < 255;
  }
}
