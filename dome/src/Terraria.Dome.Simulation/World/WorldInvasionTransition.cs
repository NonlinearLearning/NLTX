using Terraria.Dome.Simulation.WorldModel.Systems;

namespace Terraria.Dome.Simulation.WorldModel;

public readonly record struct WorldInvasionTransition(
  long TickNumber,
  int PreviousType,
  int CurrentType,
  int PreviousSize,
  int CurrentSize,
  double PreviousPosition,
  double CurrentPosition,
  WorldInvasionClearFlag? ClearFlag,
  bool Started,
  bool Progressed,
  bool Completed)
{
  public long Sequence { get; init; }
}
