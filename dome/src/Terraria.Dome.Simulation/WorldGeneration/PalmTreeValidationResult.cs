namespace Terraria.Dome.Simulation.WorldGeneration;

public sealed record PalmTreeValidationResult(
  bool IsSupported,
  bool ShouldKill,
  ushort CurrentType,
  ushort NormalizedAboveType,
  short OriginalFrameX,
  short SuggestedFrameX,
  bool RequiresRandomFrameSelection,
  bool RequiresRecursiveFraming,
  bool DestructionDeferred);
