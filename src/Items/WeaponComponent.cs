namespace Terraria.Items;

public struct WeaponComponent
{
  public WeaponComponent(
    int damage,
    float knockback,
    int useAnimationTicks,
    int useTimeTicks,
    int projectileType,
    float projectileSpeed,
    int ammoCategoryId = 0,
    bool consumesAmmo = false)
  {
    Damage = damage;
    Knockback = knockback;
    UseAnimationTicks = useAnimationTicks;
    UseTimeTicks = useTimeTicks;
    ProjectileType = projectileType;
    ProjectileSpeed = projectileSpeed;
    AmmoCategoryId = ammoCategoryId;
    ConsumesAmmo = consumesAmmo;
  }

  public int Damage;
  public float Knockback;
  public int UseAnimationTicks;
  public int UseTimeTicks;
  public int ProjectileType;
  public float ProjectileSpeed;
  public int AmmoCategoryId;
  public bool ConsumesAmmo;

  public bool CanSpawnProjectile => ProjectileType > 0;
}
