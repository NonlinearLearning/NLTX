using Terraria.WorldGeneration.Dungeon.Bounds;
using Terraria.WorldGeneration.Dungeon.Styles;

namespace Terraria.WorldGeneration.Dungeon.Layout;

public readonly record struct DungeonControlLineSnapshot(
  int Index,
  int? Prev,
  int? Next,
  DungeonTilePoint Start,
  DungeonTilePoint End,
  DungeonTilePoint Center,
  DungeonStyleId Style,
  int ProgressionStage,
  float LineLength,
  bool CurveLine);
