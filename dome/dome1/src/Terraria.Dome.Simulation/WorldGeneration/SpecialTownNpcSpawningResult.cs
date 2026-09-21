namespace Terraria.Dome.Simulation.WorldGeneration;

public sealed record SpecialTownNpcSpawningResult(
  bool IsAllowed,
  int NpcType,
  int MushroomTileCount,
  int MushroomTileThreshold,
  bool UsedTruffleRule,
  bool RoomScanDeferred);
