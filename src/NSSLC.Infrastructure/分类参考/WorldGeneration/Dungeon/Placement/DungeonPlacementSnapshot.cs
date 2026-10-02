using Terraria.WorldGeneration.Dungeon.Bounds;

namespace Terraria.WorldGeneration.Dungeon.Placement;

public readonly record struct DungeonPlacementSnapshot(
  DungeonTilePoint Position,
  int OccupancyRevision,
  bool Occupied,
  bool InProtectedBounds);
