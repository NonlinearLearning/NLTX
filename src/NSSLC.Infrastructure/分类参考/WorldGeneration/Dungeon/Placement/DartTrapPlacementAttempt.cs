using Terraria.WorldGeneration.Dungeon.Bounds;

namespace Terraria.WorldGeneration.Dungeon.Placement;

public readonly record struct DartTrapPlacementAttempt(
  int DirectionX,
  int XPush,
  int X,
  int Y,
  DungeonTilePoint Position,
  float T);
