namespace Terraria.Player;

public sealed class PlayerCombatDetectionStateComponent
{
  public bool DangerSense { get; internal set; }

  public bool LoveStruck { get; internal set; }

  public bool Stinky { get; internal set; }

  public bool ResistCold { get; internal set; }

  public bool Electrified { get; internal set; }

  public bool DryadWard { get; internal set; }

  public bool Panic { get; internal set; }

  internal void ResetEffects()
  {
    DangerSense = false;
    LoveStruck = false;
    Stinky = false;
    ResistCold = false;
    Electrified = false;
    DryadWard = false;
    Panic = false;
  }
}
