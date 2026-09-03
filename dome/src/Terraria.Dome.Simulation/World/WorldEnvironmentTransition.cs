namespace Terraria.Dome.Simulation.WorldModel;

public readonly record struct WorldEnvironmentTransition(
  long TickNumber,
  bool RainStarted,
  bool RainStopped,
  int RainTimeTicks,
  float RainStrength,
  float PreviousMaximumRainStrength,
  float WindTarget,
  float WindCurrent)
{
  public long Sequence { get; init; }
}
