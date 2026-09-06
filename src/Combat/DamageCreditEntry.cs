namespace Terraria.Combat;

public struct DamageCreditEntry
{
  public DamageCreditEntry(
    CombatContributorId contributor,
    int appliedDamage)
  {
    Contributor = contributor;
    AppliedDamage = appliedDamage;
  }

  public CombatContributorId Contributor;
  public int AppliedDamage;
}
