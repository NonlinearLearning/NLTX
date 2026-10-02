namespace Terraria.Player;

public sealed class PlayerSurvivalNeedComponent
{
  public bool NoItems { get; internal set; }

  public bool Hungry { get; internal set; }

  public bool Starving { get; internal set; }

  public bool HeartyMeal { get; internal set; }

  internal void ResetEffects()
  {
    NoItems = false;
    Hungry = false;
    Starving = false;
    HeartyMeal = false;
  }
}
