namespace Terraria.Player;

public sealed class PlayerManaRegenModifierComponent
{
  public int ManaRegenBonus { get; internal set; }

  public float ManaRegenDelayBonus { get; internal set; }

  internal void Reset()
  {
    ManaRegenBonus = 0;
    ManaRegenDelayBonus = 0f;
  }
}
