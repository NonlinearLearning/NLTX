namespace Terraria.Dome.Simulation.WorldGeneration;

public sealed record ChandelierValidationResult(
  bool IsValid,
  bool ShouldKill,
  int OriginX,
  int OriginY,
  int Width,
  int Height,
  int CheckedTiles,
  int InvalidTiles,
  bool HasSolidSupport,
  bool StyleFrameDeferred,
  bool DestructionDeferred);
