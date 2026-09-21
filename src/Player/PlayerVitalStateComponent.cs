namespace Terraria.Player;

public sealed class PlayerVitalStateComponent
{
  public int StatLifeMax { get; internal set; } = 100;

  public int StatLifeMax2 { get; internal set; } = 100;

  public int StatLife { get; internal set; } = 100;

  public int StatMana { get; internal set; }

  public int StatManaMax { get; internal set; } = 20;

  public int StatManaMax2 { get; internal set; } = 20;

  internal void ResetEffects()
  {
    StatLifeMax2 = StatLifeMax;
    StatManaMax2 = StatManaMax;
  }

  internal void ResetForLifecycle()
  {
    StatLifeMax = 100;
    StatLifeMax2 = 100;
    StatLife = 100;
    StatMana = 0;
    StatManaMax = 20;
    StatManaMax2 = 20;
  }
}
