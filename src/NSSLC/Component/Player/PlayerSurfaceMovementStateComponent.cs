namespace Terraria.Player;

public sealed class PlayerSurfaceMovementStateComponent
{
  public bool SandStorm { get; internal set; }

  public bool Sticky { get; internal set; }

  public bool Slippy { get; internal set; }

  public bool Slippy2 { get; internal set; }

  internal void ResetEffects()
  {
    SandStorm = false;
    Sticky = false;
    Slippy = false;
    Slippy2 = false;
  }

  internal void ResetForLifecycle()
  {
    ResetEffects();
  }
}
