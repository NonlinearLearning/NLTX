namespace Terraria.Dome.Simulation.WorldModel;

public readonly record struct WorldMeteorScheduleCommand(long Sequence)
{
  public bool IsValid => Sequence >= 0;
}
