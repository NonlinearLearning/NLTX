namespace Terraria.Dome.Simulation.Projectile.Definitions;

public static class LegacyProjectileCombatPolicy
{
  public const int StormLightningLiquidDamageRadius = 500;

  public static bool IsDamageDodgeable(int projectileType, int damage)
  {
    if (damage == 9999 &&
        (projectileType is 871 or 872 or 873 or 874 or 919 or 923 or 924))
    {
      return false;
    }

    return true;
  }
}
