using Terraria.WorldGeneration.Dungeon.Bounds;

namespace Terraria.WorldGeneration.Dungeon.Placement;

public readonly record struct BoulderPlacementAttempt(
  DungeonTilePoint Position,
  int YPush,
  int RequiredHeight,
  int BestType);
