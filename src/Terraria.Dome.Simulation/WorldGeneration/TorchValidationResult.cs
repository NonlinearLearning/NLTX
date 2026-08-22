namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct TorchValidationResult(
  bool IsValid,
  bool ShouldKill,
  bool CanDown,
  bool CanLeft,
  bool CanRight,
  bool CanAttachToWall,
  short SuggestedFrameX);
