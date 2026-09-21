namespace Terraria.Dome.Simulation.WorldGeneration;

public sealed record PotValidationResult(
  bool IsValid,
  bool ShouldKill,
  int OriginX,
  int OriginY,
  int StyleBand,
  bool UsedType653BottomSlope,
  bool StyleFrameDeferred,
  bool DestructionDeferred);
