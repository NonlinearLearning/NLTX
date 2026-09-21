namespace Terraria.Player;

public sealed class PlayerMiscCounterComponent
{
  public int MiscCounter { get; internal set; }

  internal void ResetForLifecycle()
  {
    MiscCounter = 0;
  }
}
