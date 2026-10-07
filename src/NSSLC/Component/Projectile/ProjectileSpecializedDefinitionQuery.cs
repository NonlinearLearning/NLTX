namespace Terraria.Projectile;

/// <summary>
/// Version4 projectile rules whose values do not depend on world or entity state.
/// This query exposes rule data; it does not implement specialized AI behavior.
/// </summary>
public static class ProjectileSpecializedDefinitionQuery
{
  public const int StormLightningLiquidDamageRadius = 500;

  public const float MinimumWindStrengthToFlyKite = 0.2f;

  public static bool IsWhipType(int projectileType)
  {
    return projectileType is
      847 or 841 or 848 or 849 or 912 or 913 or 914 or 915 or 952 or 1028 or 1029 or 1030 or
      1031 or 1032 or 1033 or 1034 or 1035 or 1104;
  }

  public static bool IsAdd2Turret(int projectileType)
  {
    return projectileType is
      663 or 665 or 667 or 677 or 678 or 679 or 688 or 689 or 690 or 691 or 692 or 693;
  }
}
