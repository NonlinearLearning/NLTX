namespace Terraria.Dome.Simulation.WorldModel;

public readonly record struct WorldInvasionProgressCommand(int Amount, long Sequence)
{
  public bool IsValid => Amount > 0 && Sequence >= 0;
}
