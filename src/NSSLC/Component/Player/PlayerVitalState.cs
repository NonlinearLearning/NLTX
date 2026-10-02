namespace Terraria.Player;

public sealed class PlayerVitalState
{
  public int Life { get; set; }

  public int BaseLifeMaximum { get; set; }

  public int EffectiveLifeMaximum { get; set; }

  public int Mana { get; set; }

  public int BaseManaMaximum { get; set; }

  public int EffectiveManaMaximum { get; set; }

  public int Defense { get; set; }

  public bool IsLifeDepleted => Life <= 0;

  public float LifeFraction => EffectiveLifeMaximum <= 0 ? 0 : (float)Life / EffectiveLifeMaximum;

  public float ManaFraction => EffectiveManaMaximum <= 0 ? 0 : (float)Mana / EffectiveManaMaximum;
}
