namespace Terraria.Player;

public sealed class PlayerWeaponCritModifierComponent
{
  public int MeleeCrit { get; internal set; } = 4;

  public int MagicCrit { get; internal set; } = 4;

  public int RangedCrit { get; internal set; } = 4;

  public int RevolverCritChanceBonus { get; internal set; }

  internal void ResetEffects()
  {
    MeleeCrit = 4;
    MagicCrit = 4;
    RangedCrit = 4;
    RevolverCritChanceBonus = 0;
  }
}
