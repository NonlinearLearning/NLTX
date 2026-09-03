namespace Terraria.Dome.Simulation.WorldGeneration;

public sealed record StinkbugBlockerValidationResult(
  bool IsValid,
  bool ShouldKill,
  int OriginalStyle,
  int SuggestedStyle,
  bool SwappedHorizontalStyle,
  bool OrientationDeferred);
