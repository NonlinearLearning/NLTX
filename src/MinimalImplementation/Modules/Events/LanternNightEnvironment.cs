namespace MinimalImplementation.Server.Modules.Events;

public sealed record LanternNightEnvironment(
    bool DayTime,
    bool MeteorPending,
    bool BloodMoon,
    bool PumpkinMoon,
    bool SnowMoon,
    bool InvasionActive,
    bool MoonLordCountdownActive,
    bool BossOrEaterOfWorldsActive,
    bool MoonLordDefeated) {
  public bool CanStart => !MeteorPending && !BloodMoon && !PumpkinMoon && !SnowMoon
      && !InvasionActive && !MoonLordCountdownActive && !BossOrEaterOfWorldsActive;
}
