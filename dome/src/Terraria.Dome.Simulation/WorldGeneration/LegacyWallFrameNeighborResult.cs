namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct LegacyWallFrameNeighborResult(
  int X,
  int Y,
  ushort OriginalCenterWallType,
  ushort EffectiveCenterWallType,
  LegacyWallFrameNeighborMask NeighborMask,
  bool ShowInvisibleWalls,
  bool ShouldFrame,
  bool WasCenterWallTypeNormalized);
