using System;

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
    if (localNpcCooldownTicks < -2)
    {
      throw new ArgumentOutOfRangeException(nameof(localNpcCooldownTicks));
    }

    if (staticNpcCooldownTicks < -1)
    {
      throw new ArgumentOutOfRangeException(nameof(staticNpcCooldownTicks));
    }

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

  public bool WritesLocalNpcImmunity =>
    UsesLocalNpcImmunity && LocalNpcCooldownTicks != -2;

  public bool UsesStaticNpcImmunityRegistry =>
    UsesStaticNpcImmunity;
}
