namespace Terraria.Dome.Simulation.WorldGeneration;

public sealed record TreeSettingsValidationResult(
  bool IsSupported,
  bool ShouldKill,
  ushort TreeType,
  int BelowType,
  bool GroundValid,
  bool HasLeftTree,
  bool HasRightTree,
  short OriginalFrameX,
  short OriginalFrameY,
  bool FrameMutationDeferred,
  bool DestructionDeferred);
