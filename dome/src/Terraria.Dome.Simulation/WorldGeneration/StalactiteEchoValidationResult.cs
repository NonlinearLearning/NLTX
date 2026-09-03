namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct StalactiteEchoValidationResult(
  bool IsValid,
  bool ShouldKill,
  int OriginY,
  int Height,
  bool HangsFromTop,
  int Style);
