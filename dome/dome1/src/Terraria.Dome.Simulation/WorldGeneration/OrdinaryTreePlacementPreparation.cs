namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct OrdinaryTreePlacementPreparation(
  int OriginX,
  int GroundY,
  int Height)
{
  public bool IsPrepared => Height > 0;
}
