namespace Terraria.WorldSession.Events.BossTracking;

public sealed class BossEncounterOutcomeStateComponent
{
  public BossEncounterOutcomeStateComponent(bool wasKilled = false)
  {
    WasKilled = wasKilled;
  }

  public bool WasKilled { get; internal set; }
}
