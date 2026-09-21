namespace Terraria.Dome.Simulation.WorldModel;

public readonly record struct WorldProgressionTransition(
  long TickNumber,
  WorldEventKind EventKind,
  bool Started,
  bool Stopped)
{
  public long Sequence { get; init; }
}
