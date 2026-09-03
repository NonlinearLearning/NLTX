namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct LegacyErrorWorldAxisDirection(int X, int Y)
{
  public bool IsValid => ((X is -1 or 1) && Y == 0) || ((Y is -1 or 1) && X == 0);
}
