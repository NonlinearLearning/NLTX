namespace Terraria.Player;

public sealed class PlayerStickyBreakStateComponent
{
  public int StickyBreak { get; internal set; }

  internal void ResetForLifecycle()
  {
    StickyBreak = 0;
  }
}
