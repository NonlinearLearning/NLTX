namespace Terraria.Player;

public sealed class PlayerAttackSpeedModifierComponent
{
  public float MeleeSpeed { get; internal set; } = 1f;

  public float SummonerWeaponSpeedBonus { get; internal set; }

  internal void ResetEffects()
  {
    MeleeSpeed = 1f;
    SummonerWeaponSpeedBonus = 0f;
  }
}
