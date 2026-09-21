namespace Terraria.Dome.Simulation.WorldGeneration;

public enum TreeProfileGrowthEligibilityReason
{
  Eligible,
  OutOfBounds,
  UnknownProfile,
  LiquidAboveGround,
  GroundInactive,
  GroundShape,
  GroundUnsuitable,
  WallUnsuitable,
  NoSuitableNeighbor
}
