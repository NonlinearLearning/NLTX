namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct Tile3x1ValidationResult(
  bool IsValid,
  bool RequiresBreakabilityCheck,
  int OriginX,
  int OriginY);
