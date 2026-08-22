namespace Terraria.Dome.Simulation.WorldGeneration;

public sealed record RoomBoundaryValidationResult(
  bool IsAllowed,
  bool ShouldStop,
  bool TooCloseToWorldEdge,
  bool RoomOutOfBounds,
  bool RoomTileLimitReached,
  bool RoomSizeLimitReached,
  int RoomTileCount,
  int RoomWidth,
  int RoomHeight,
  bool RecursiveRoomScanDeferred);
