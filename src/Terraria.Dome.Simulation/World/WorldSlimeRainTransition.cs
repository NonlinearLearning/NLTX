namespace Terraria.Dome.Simulation.WorldModel;

public readonly record struct WorldSlimeRainTransition(
  long TickNumber,
  int PreviousRainTicks,
  int CurrentRainTicks,
  int PreviousCooldownTicks,
  int CurrentCooldownTicks,
  int PreviousWarningTicks,
  int CurrentWarningTicks,
  bool Started,
  bool Stopped,
  bool WarningPublished,
  bool CooldownStarted,
  bool CooldownEnded)
{
  public long Sequence { get; init; }
}
