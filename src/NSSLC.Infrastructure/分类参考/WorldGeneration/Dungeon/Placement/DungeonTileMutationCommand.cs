using Terraria.WorldGeneration.Dungeon.Bounds;

namespace Terraria.WorldGeneration.Dungeon.Placement;

public enum DungeonTileMutationKind : byte
{
  Tile = 0,
  Wall = 1,
  Liquid = 2,
  Structure = 3,
  Wire = 4
}

public readonly record struct DungeonTileMutationCommand(
  DungeonTilePoint Position,
  DungeonTileMutationKind Kind,
  int Value,
  int ExpectedRevision);
