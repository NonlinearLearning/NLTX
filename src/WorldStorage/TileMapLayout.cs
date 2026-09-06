namespace Terraria.WorldStorage;

public readonly record struct TileMapLayout(
  int Width,
  int Height,
  int SectionWidthInTiles,
  int SectionHeightInTiles);
