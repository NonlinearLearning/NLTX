namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct LogicTileValidationResult(
  bool IsValid,
  bool ShouldKill,
  bool HasAlignedFrame,
  bool HasRequiredLogicSupport);
