namespace Terraria.Dome.Simulation.WorldModel;

public readonly record struct TileRegionProbeResult(
  int TileCount,
  bool ReachedLimit,
  int LavaTileCount,
  int ShroomTileCount,
  int IceTileCount,
  int SandTileCount,
  int RockTileCount);
