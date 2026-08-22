namespace Terraria.Dome.Simulation.WorldGeneration;

public sealed record TreeFrameValidationResult(
  bool IsSupported,
  bool ShouldKill,
  short OriginalFrameX,
  short OriginalFrameY,
  short SuggestedFrameX,
  short SuggestedFrameY,
  int NormalizedGroundType,
  bool HasLeftTree,
  bool HasRightTree,
  bool FrameMutationDeferred,
  bool DestructionDeferred);
