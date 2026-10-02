namespace Terraria.Player;

public sealed class PlayerManaActivityComponent
{
  public bool ManaSick { get; internal set; }

  public float ManaSickReduction { get; internal set; }

  public int AfkCounter { get; internal set; }

  public int AfkCounterForKiting { get; internal set; }

  internal void ResetEffects()
  {
    ManaSick = false;
    ManaSickReduction = 0f;
  }

  internal void ResetForLifecycle()
  {
    ResetEffects();
    AfkCounter = 0;
    AfkCounterForKiting = 0;
  }
}
