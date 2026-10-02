namespace Terraria.Player;

public sealed class PlayerLuckPotionStateComponent
{
  public byte LuckPotion { get; internal set; }

  internal void ResetEffects()
  {
    LuckPotion = 0;
  }
}
