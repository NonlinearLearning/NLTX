namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct TileBreakabilitySurvivalResult(
  bool ShouldSurvive,
  bool IsChestLike,
  bool IsDisplayDoll,
  bool IsHatRack,
  int OriginX,
  int OriginY);
