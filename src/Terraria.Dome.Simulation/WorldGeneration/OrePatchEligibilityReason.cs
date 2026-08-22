namespace Terraria.Dome.Simulation.WorldGeneration;

public enum OrePatchEligibilityReason
{
  Eligible,
  OutOfBounds,
  NoSolidGroundBeforeSurface,
  GroundNotGrass,
  GroundWallPresent,
  SupportInactive,
  SupportDungeon,
  SupportCloud,
  SupportSand,
  SupportWallMissing
}
