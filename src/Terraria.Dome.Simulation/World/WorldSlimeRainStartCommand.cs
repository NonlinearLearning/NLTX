namespace Terraria.Dome.Simulation.WorldModel;

public readonly record struct WorldSlimeRainStartCommand(
  int DurationTicks,
  long Sequence)
{
  public bool IsValid => DurationTicks > 0 && Sequence >= 0;
}
