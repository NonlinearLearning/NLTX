namespace Terraria.Items;

public struct WeaponComponent
{
  public WeaponComponent(
    int damage,
    float knockback,
    int useAnimationTicks,
    int useTimeTicks,
    int projectileType,
    float projectileSpeed)
  {
    Damage = damage;
    Knockback = knockback;
    UseAnimationTicks = useAnimationTicks;
    UseTimeTicks = useTimeTicks;
    ProjectileType = projectileType;
    ProjectileSpeed = projectileSpeed;
  }

  public int Damage;
  public float Knockback;
  public int UseAnimationTicks;
  public int UseTimeTicks;
  public int ProjectileType;
  public float ProjectileSpeed;
}
