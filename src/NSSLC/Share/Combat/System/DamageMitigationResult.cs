namespace Terraria.Share.Combat;

/// <summary>Deterministic damage result after offensive scaling and target mitigation.</summary>
public readonly record struct DamageMitigationResult(
    int IncomingDamage,
    int AcceptedDamage,
    float Reduction,
    int AfterDefenseDamage = 0,
    int AfterResistanceDamage = 0,
    int ShieldAbsorbedDamage = 0,
    int LifeDamage = 0,
    int ReflectedDamage = 0,
    int LifeSteal = 0,
    bool Critical = false)
{
  public int AfterShieldDamage => LifeDamage;

  public bool HasEffect => AcceptedDamage > 0 || ShieldAbsorbedDamage > 0;

  public bool IsShieldOnly => AcceptedDamage == 0 && ShieldAbsorbedDamage > 0;

  public bool IsBlocked => !HasEffect;
}
