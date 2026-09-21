namespace Terraria.Dome.Simulation.WorldModel;

public readonly record struct WorldLanternNightTransition(
  long TickNumber,
  bool WasScheduled,
  bool IsScheduled,
  bool WasActive,
  bool IsActive,
  bool ScheduleConsumed,
  bool Started,
  bool Stopped)
{
  public long Sequence { get; init; }
}
