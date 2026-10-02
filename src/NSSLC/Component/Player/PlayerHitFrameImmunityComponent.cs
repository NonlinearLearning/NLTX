namespace Terraria.Player;

public sealed class PlayerHitFrameImmunityComponent
{
  public bool Immune { get; internal set; }

  public bool ImmuneNoBlink { get; internal set; }

  public int ImmuneTime { get; internal set; }

  public int TimeSinceLastImmuneGet { get; internal set; }

  public int ImmuneStrikes { get; internal set; }

  internal void ResetForLifecycle()
  {
    Immune = false;
    ImmuneNoBlink = false;
    ImmuneTime = 0;
    TimeSinceLastImmuneGet = 0;
    ImmuneStrikes = 0;
  }
}
