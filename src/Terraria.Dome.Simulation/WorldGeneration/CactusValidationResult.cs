namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct CactusValidationResult(
  bool IsSupported,
  bool ShouldKill,
  int SupportX,
  int SupportY,
  int CactusHeight,
  bool HasSideBranch);
