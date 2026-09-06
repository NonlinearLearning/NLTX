namespace Terraria.Projectile;

public struct ProjectileHitImmunityPolicyComponent
{
  public ProjectileHitImmunityPolicyComponent(
    bool usesLocalNpcImmunity = false,
    bool usesStaticNpcImmunity = false,
    int localNpcCooldownTicks = -2,
    int staticNpcCooldownTicks = -1,
    bool appliesOnSingleHit = false,
    bool usesOwnerMeleeCooldown = false,
    bool copiesOwnerCooldownOnSpawn = false)
  {
    UsesLocalNpcImmunity = usesLocalNpcImmunity;
    UsesStaticNpcImmunity = usesStaticNpcImmunity;
    LocalNpcCooldownTicks = localNpcCooldownTicks;
    StaticNpcCooldownTicks = staticNpcCooldownTicks;
    AppliesOnSingleHit = appliesOnSingleHit;
    UsesOwnerMeleeCooldown = usesOwnerMeleeCooldown;
    CopiesOwnerCooldownOnSpawn = copiesOwnerCooldownOnSpawn;
  }

  public bool UsesLocalNpcImmunity;
  public bool UsesStaticNpcImmunity;
  public int LocalNpcCooldownTicks;
  public int StaticNpcCooldownTicks;
  public bool AppliesOnSingleHit;
  public bool UsesOwnerMeleeCooldown;
  public bool CopiesOwnerCooldownOnSpawn;
}
