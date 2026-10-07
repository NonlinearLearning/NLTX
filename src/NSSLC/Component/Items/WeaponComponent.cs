namespace Terraria.Items;

/// <summary>
/// 保存武器伤害、击退、使用时长、弹药和发射参数。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Item。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Item.cs。</para>
/// <para>
/// 主要源成员：useAnimation（第 134 行）； useTime（第 136 行）； damage（第 156 行）； knockBack（第 158 行）； shoot（第
/// 226 行）； shootSpeed（第 228 行）； useAmmo（第 234 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/baseline/。</para>
/// <para>拆分依据文件：Version4物品容器与经济事务组件设计.md。</para>
/// <para>依据位置：第 116 行。</para>
/// </remarks>
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
