namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct TrapDoorValidationResult(
  bool IsValid,
  bool ShouldKill,
  int OriginX,
  int OriginY,
  int Width,
  int Height,
  int Style,
  bool HasSolidAnchor);
