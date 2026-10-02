namespace Terraria.Presentation.CombatText;

public sealed class CombatTextPalette
{
  public CombatTextPalette()
  {
    DamagedFriendly = new CombatTextColor(255, 80, 90);
    DamagedFriendlyCritical = new CombatTextColor(255, 100, 30);
    DamagedHostile = new CombatTextColor(255, 160, 80);
    DamagedHostileCritical = new CombatTextColor(255, 100, 30);
    OthersDamagedHostile = DamagedHostile.Scale(0.4f);
    OthersDamagedHostileCritical = DamagedHostileCritical.Scale(0.4f);
    HealLife = new CombatTextColor(100, 255, 100);
    HealMana = new CombatTextColor(100, 100, 255);
    LifeRegen = new CombatTextColor(255, 60, 70);
    LifeRegenNegative = new CombatTextColor(255, 140, 40);
  }

  public static CombatTextPalette Default { get; } = new();

  public CombatTextColor DamagedFriendly { get; }

  public CombatTextColor DamagedFriendlyCritical { get; }

  public CombatTextColor DamagedHostile { get; }

  public CombatTextColor DamagedHostileCritical { get; }

  public CombatTextColor HealLife { get; }

  public CombatTextColor HealMana { get; }

  public CombatTextColor LifeRegen { get; }

  public CombatTextColor LifeRegenNegative { get; }

  public CombatTextColor OthersDamagedHostile { get; }

  public CombatTextColor OthersDamagedHostileCritical { get; }

  public CombatTextColor Get(CombatTextColorRole role)
  {
    return role switch
    {
      CombatTextColorRole.DamagedFriendly => DamagedFriendly,
      CombatTextColorRole.DamagedFriendlyCritical => DamagedFriendlyCritical,
      CombatTextColorRole.DamagedHostile => DamagedHostile,
      CombatTextColorRole.DamagedHostileCritical => DamagedHostileCritical,
      CombatTextColorRole.OthersDamagedHostile => OthersDamagedHostile,
      CombatTextColorRole.OthersDamagedHostileCritical => OthersDamagedHostileCritical,
      CombatTextColorRole.HealLife => HealLife,
      CombatTextColorRole.HealMana => HealMana,
      CombatTextColorRole.LifeRegen => LifeRegen,
      CombatTextColorRole.LifeRegenNegative => LifeRegenNegative,
      _ => throw new ArgumentOutOfRangeException(nameof(role))
    };
  }
}
