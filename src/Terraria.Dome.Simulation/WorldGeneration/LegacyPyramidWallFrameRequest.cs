namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct LegacyPyramidWallFrameRequest(
  WallFrameCoordinate Center,
  WallFrameCoordinate Target,
  bool ResetFrame,
  string Source,
  int SourceLine);
