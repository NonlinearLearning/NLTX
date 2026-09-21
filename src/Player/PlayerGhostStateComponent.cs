namespace Terraria.Player;

public sealed class PlayerGhostStateComponent
{
  public bool Ghost { get; internal set; }

  public int GhostFrame { get; internal set; }

  public int GhostFrameCounter { get; internal set; }

  public bool PvpDeath { get; internal set; }

  internal void ResetForLifecycle()
  {
    Ghost = false;
    GhostFrame = 0;
    GhostFrameCounter = 0;
    PvpDeath = false;
  }
}
