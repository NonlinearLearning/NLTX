namespace Terraria.Dome.Simulation.Npc.Components;

public struct NpcRangedAttackState
{
  public NpcRangedAttackState(
    int projectileType,
    int damage,
    float projectileSpeed,
    int cooldownTicks)
  {
    ProjectileType = projectileType;
    Damage = damage;
    ProjectileSpeed = projectileSpeed;
    CooldownTicks = cooldownTicks;
    RemainingCooldownTicks = 0;
  }

  public int ProjectileType;
  public int Damage;
  public float ProjectileSpeed;
  public int CooldownTicks;
  public int RemainingCooldownTicks;
}
