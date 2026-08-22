namespace Terraria.Dome.Simulation.WorldModel;

public readonly record struct WorldSlimeRainStopCommand(
  int CooldownTicks,
  long Sequence,
  bool Announce = true)
{
  public bool IsValid => CooldownTicks >= 0 && Sequence >= 0;
}
