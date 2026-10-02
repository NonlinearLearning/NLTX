namespace Terraria.WorldSession.Components;

public readonly record struct WorldClockSnapshotValue(
  bool DayTime,
  double Time,
  int MoonPhase,
  long ClockRevision);
