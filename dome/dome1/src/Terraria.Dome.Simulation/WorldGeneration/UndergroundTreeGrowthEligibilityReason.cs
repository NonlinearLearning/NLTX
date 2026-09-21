namespace Terraria.Dome.Simulation.WorldGeneration;

public enum UndergroundTreeGrowthEligibilityReason
{
  Eligible,
  OutOfBounds,
  HeightOutOfRange,
  GroundInactive,
  GroundType,
  GroundShape,
  NoSuitableNeighbor,
  CanopyBlocked
}
