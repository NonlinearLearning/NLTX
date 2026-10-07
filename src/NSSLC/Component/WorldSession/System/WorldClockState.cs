namespace Terraria.WorldSession.Components;

public sealed class WorldClockState
{
  public bool DayTime = true;
  public double Time = 13500.0;
  public int MoonPhase;
  public long ClockRevision;

  public bool HasValidMoonPhase => MoonPhase is >= 0 and <= 7;
}
