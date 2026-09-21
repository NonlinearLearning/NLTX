namespace Terraria.Player;

public sealed class PlayerDebuffRecoveryStateComponent
{
  public bool Chilled { get; internal set; }

  public bool Dazed { get; internal set; }

  public bool Frozen { get; internal set; }

  public bool Stoned { get; internal set; }

  public bool Ichor { get; internal set; }

  public bool Webbed { get; internal set; }

  public bool Tipsy { get; internal set; }

  public bool NoBuilding { get; internal set; }

  public bool CrimsonRegen { get; internal set; }

  public bool GhostHeal { get; internal set; }

  public bool GhostHurt { get; internal set; }

  internal void ResetEffects()
  {
    Chilled = false;
    Dazed = false;
    Frozen = false;
    Stoned = false;
    Ichor = false;
    Webbed = false;
    Tipsy = false;
    NoBuilding = false;
    CrimsonRegen = false;
    GhostHeal = false;
    GhostHurt = false;
  }

  internal void ResetForLifecycle()
  {
    ResetEffects();
  }
}
