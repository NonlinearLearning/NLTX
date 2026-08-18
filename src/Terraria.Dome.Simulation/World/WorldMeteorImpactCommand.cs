namespace Terraria.Dome.Simulation.WorldModel;

public readonly record struct WorldMeteorImpactCommand(
  int X,
  int Y,
  long Sequence)
{
  public bool IsValid => X >= 0 && Y >= 0 && Sequence >= 0;
}
