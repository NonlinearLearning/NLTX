using Terraria.WorldGeneration.Dungeon.Bounds;

namespace Terraria.WorldGeneration.Dungeon.Placement;

public readonly record struct WirePlacementAttempt(
  DungeonTilePoint Position,
  int DirX,
  int DirY,
  int Steps);
