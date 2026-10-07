namespace Terraria.Player;

public static class PlayerDamageMitigationQuery
{
  public static int Evaluate(in PlayerDamageMitigationInput input)
  {
    int defense = Math.Max(0, input.Defense);
    int baseDamage = Math.Max(1, input.DamageAmount - defense);
    int criticalDamage = input.Critical
      ? SaturatingDouble(baseDamage)
      : baseDamage;
    float endurance = Math.Clamp(input.Endurance, 0f, 0.999999f);
    int reducedDamage = (int)(criticalDamage * (1f - endurance));
    return Math.Max(1, reducedDamage);
  }

  private static int SaturatingDouble(int value)
  {
    return value > int.MaxValue / 2 ? int.MaxValue : value * 2;
  }
}
