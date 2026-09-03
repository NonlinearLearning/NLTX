namespace Terraria.Projectile;

public struct ProjectileDamageComponent
{
  public ProjectileDamageComponent(
    int current,
    int original,
    float knockback,
    int armorPenetration,
    int criticalChance)
  {
    Current = current;
    Original = original;
    Knockback = knockback;
    ArmorPenetration = armorPenetration;
    CriticalChance = criticalChance;
  }

  public int Current;
  public int Original;
  public float Knockback;
  public int ArmorPenetration;
  public int CriticalChance;
}
