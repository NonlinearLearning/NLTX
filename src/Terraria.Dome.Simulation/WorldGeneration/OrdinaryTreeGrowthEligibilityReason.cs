namespace Terraria.Dome.Simulation.WorldGeneration;

public enum OrdinaryTreeGrowthEligibilityReason
{
  Eligible,
  OutOfBounds,
  LiquidAboveGround,
  GroundInactive,
  GroundShape,
  GroundUnsuitable,
  WallUnsuitable,
  NoSuitableNeighbor
}
