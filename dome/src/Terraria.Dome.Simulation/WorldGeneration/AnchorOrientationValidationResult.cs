namespace Terraria.Dome.Simulation.WorldGeneration;

public sealed record AnchorOrientationValidationResult(
  bool IsValid,
  bool ShouldKill,
  int OriginalStyle,
  int SuggestedStyle,
  bool UsedWallFallback,
  bool ConsideredSolidDeferred);
