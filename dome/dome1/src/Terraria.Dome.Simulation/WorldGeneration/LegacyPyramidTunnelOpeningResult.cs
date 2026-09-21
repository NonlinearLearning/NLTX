namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct LegacyPyramidTunnelOpeningResult(
  int Direction,
  int StartX,
  int StartY,
  int TunnelHeight,
  int InitialDelay,
  int CommandsVisited,
  int ClearedPyramidTileCount,
  int WallWriteCount,
  int SandConversionCount,
  bool NoTunnelMode)
{
  public bool NoTunnelBoundaryReached => NoTunnelMode;
}
