namespace Terraria.Dome.Simulation.WorldModel;

public readonly record struct WorldMeteorTransition(
  long TickNumber,
  bool WasScheduled,
  bool IsScheduled,
  bool ScheduleStarted,
  bool ScheduleCleared,
  bool ImpactQueued)
{
  public long Sequence { get; init; }
}
