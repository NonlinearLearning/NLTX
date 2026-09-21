namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct UnderwaterPlantValidationResult(
  bool IsValid,
  bool ShouldKill,
  short NormalizedFrameX,
  short NormalizedFrameY,
  ushort SupportType,
  bool SupportIsSand);
