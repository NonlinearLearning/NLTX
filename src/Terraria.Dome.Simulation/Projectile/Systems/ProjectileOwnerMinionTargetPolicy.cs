namespace Terraria.Dome.Simulation.Projectile.Systems;

public static class ProjectileOwnerMinionTargetPolicy
{
  public static bool CanResolve(
    PlayerHandle owner,
    bool isMinion,
    NpcHandle? target,
    bool targetActive,
    bool targetChaseable,
    int targetHealth)
  {
    return owner.IsValid && isMinion && target.HasValue && target.Value.IsValid &&
      targetActive && targetChaseable && targetHealth > 0;
  }
}
