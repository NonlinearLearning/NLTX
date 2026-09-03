namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct HousingRoomMinimumSizeDecision(
  int RoomTileCount,
  int MinimumRoomTileCount,
  bool CanSpawn,
  bool IsTooSmall);
