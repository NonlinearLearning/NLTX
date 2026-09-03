using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct LegacyWallFrameEvaluationResult(
  bool IsApplicable,
  WallFrameCoordinate Target,
  WorldTile Projected,
  int NeighborMask,
  int FrameLookupIndex,
  int RandomDrawCount,
  bool ClearedWallPaintAndCoating,
  string? SkipReason);
