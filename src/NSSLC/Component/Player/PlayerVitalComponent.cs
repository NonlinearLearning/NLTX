namespace Terraria.Player;

public sealed class PlayerVitalComponent
{
  public int Life { get; set; } = 100;

  public int BaseLifeMaximum { get; set; } = 100;

  public int EffectiveLifeMaximum { get; set; } = 100;

  public int Mana { get; set; }

  public int BaseManaMaximum { get; set; }

  public int EffectiveManaMaximum { get; set; }

  // Final ownership remains under review with CombatAndStatus.
  public int Defense { get; set; }
}
