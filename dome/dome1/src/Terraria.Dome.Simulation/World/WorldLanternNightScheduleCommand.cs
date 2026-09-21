namespace Terraria.Dome.Simulation.WorldModel;

public readonly record struct WorldLanternNightScheduleCommand(long Sequence)
{
  public bool IsValid => Sequence >= 0;
}
