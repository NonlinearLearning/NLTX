using Terraria.Relationships;

namespace Terraria.Combat;

/// <summary>Committed result returned after target life was changed.</summary>
public readonly record struct DamageAppliedReceipt(
    DamageRequest Request,
    int AcceptedDamage,
    LifeComponent TargetLifeBefore,
    LifeComponent TargetLifeAfter,
    DamageMitigationResult Mitigation,
    long Tick)
{
  public EntityRuntimeHandle Source => Request.Source;

  public EntityRuntimeHandle Target => Request.Target;

  public DamageType DamageType => Request.Payload.Type;

  public DamageRelation Relation => Request.Payload.Relation;

  public DamageCause Cause => Request.Cause;

  public bool HasDamage => AcceptedDamage > 0;

  public bool HasEffect => Mitigation.HasEffect;

  public bool IsShieldOnly => Mitigation.IsShieldOnly;

  public int RawDamage => Mitigation.IncomingDamage;

  public int AfterDefenseDamage => Mitigation.AfterDefenseDamage;

  public int AfterResistanceDamage => Mitigation.AfterResistanceDamage;

  public int ShieldAbsorbedDamage => Mitigation.ShieldAbsorbedDamage;

  public int LifeDamage => Mitigation.LifeDamage;

  public int ReflectedDamage => Mitigation.ReflectedDamage;

  public int LifeSteal => Mitigation.LifeSteal;

  public bool Critical => Mitigation.Critical;
}
