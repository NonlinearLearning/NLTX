namespace Terraria.Player;

public sealed class PlayerCombatDefenseModifierComponent
{
  public int ArmorPenetration { get; internal set; }

  public int MeleeArmorPenetration { get; internal set; }

  public int StatDefense { get; internal set; }

  public bool NoKnockback { get; internal set; }

  internal void ResetEffects()
  {
    ArmorPenetration = 0;
    MeleeArmorPenetration = 0;
    StatDefense = 0;
    NoKnockback = false;
  }
}
