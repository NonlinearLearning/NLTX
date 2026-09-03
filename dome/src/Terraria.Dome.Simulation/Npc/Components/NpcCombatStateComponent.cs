using System;

namespace Terraria.Dome.Simulation.Npc.Components;

public struct NpcCombatStateComponent
{
  public NpcCombatStateComponent(
    int damage,
    int defense,
    int maximumHealth,
    float takenDamageMultiplier = 1.0f,
    float knockBackResist = 1.0f,
    bool coldDamage = false,
    bool trapImmune = false,
    bool lavaImmune = false,
    bool doesNotTakeDamage = false,
    bool doesNotTakeDamageFromHostiles = false,
    bool immortal = false,
    bool chaseable = true,
    bool friendly = false,
    bool reflectsProjectiles = false)
  {
    if (damage < 0 || defense < 0 || maximumHealth <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(damage));
    }

    if (!float.IsFinite(takenDamageMultiplier) || takenDamageMultiplier < 1.0f ||
        !float.IsFinite(knockBackResist) || knockBackResist < 0.0f)
    {
      throw new ArgumentOutOfRangeException(nameof(takenDamageMultiplier));
    }

    Damage = damage;
    Defense = defense;
    MaximumHealth = maximumHealth;
    TakenDamageMultiplier = takenDamageMultiplier;
    KnockBackResist = knockBackResist;
    ColdDamage = coldDamage;
    TrapImmune = trapImmune;
    LavaImmune = lavaImmune;
    DoesNotTakeDamage = doesNotTakeDamage;
    DoesNotTakeDamageFromHostiles = doesNotTakeDamageFromHostiles;
    Immortal = immortal;
    Chaseable = chaseable;
    Friendly = friendly;
    FriendlyRegen = 0;
    ReflectsProjectiles = reflectsProjectiles;
    JustHit = false;
    BaseDamage = damage;
    BaseDefense = defense;
    BaseLifeMax = maximumHealth;
  }

  public int Damage;
  public int Defense;
  public int MaximumHealth;
  public int BaseDamage;
  public int BaseDefense;
  public int BaseLifeMax;
  public float TakenDamageMultiplier;
  public float KnockBackResist;
  public bool ColdDamage;
  public bool TrapImmune;
  public bool LavaImmune;
  public bool DoesNotTakeDamage;
  public bool DoesNotTakeDamageFromHostiles;
  public bool Immortal;
  public bool Chaseable;
  public bool Friendly;
  public int FriendlyRegen;
  public bool ReflectsProjectiles;
  public bool JustHit;
}
