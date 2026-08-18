namespace Terraria.Dome.Simulation.WorldModel;

public readonly record struct WorldInvasionStartCommand(int Type, int Size, long Sequence)
{
  public bool IsValid => Type is >= 1 and <= 4 && Size > 0 && Sequence >= 0;
}
