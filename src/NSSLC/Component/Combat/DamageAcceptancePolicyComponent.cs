namespace Terraria.Combat;

public struct DamageAcceptancePolicyComponent
{
  public DamageAcceptancePolicyComponent(
    bool rejectAllDamage,
    bool rejectHostileDamage,
    bool rejectTrapDamage,
    bool isImmortal)
  {
    RejectAllDamage = rejectAllDamage;
    RejectHostileDamage = rejectHostileDamage;
    RejectTrapDamage = rejectTrapDamage;
    IsImmortal = isImmortal;
  }

  public bool RejectAllDamage;
  public bool RejectHostileDamage;
  public bool RejectTrapDamage;
  public bool IsImmortal;

  public bool CanAcceptAnyDamage => !RejectAllDamage;
}
