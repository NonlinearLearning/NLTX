namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct SignValidationResult(
  bool IsValid,
  bool ShouldKill,
  int OriginX,
  int OriginY,
  int AttachmentStyle,
  bool HasAttachment);
