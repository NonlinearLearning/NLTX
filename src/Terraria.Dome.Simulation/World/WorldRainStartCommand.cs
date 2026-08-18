namespace Terraria.Dome.Simulation.WorldModel;

public readonly record struct WorldRainStartCommand(
  int DurationTicks,
  float Strength,
  long Sequence)
{
  public bool IsValid => DurationTicks > 0 && float.IsFinite(Strength) &&
    Strength is > 0.0f and <= 1.0f && Sequence >= 0;
}
