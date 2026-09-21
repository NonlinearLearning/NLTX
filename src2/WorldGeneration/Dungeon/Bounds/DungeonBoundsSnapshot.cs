namespace Terraria.WorldGeneration.Dungeon.Bounds;

public readonly record struct DungeonBoundsSnapshot(
  int Left,
  int Right,
  int Top,
  int Bottom,
  DungeonBoundsRectangle Hitbox)
{
  public int Width => Right - Left;

  public int Height => Bottom - Top;

  public DungeonTilePoint Center => new(
    Left + Width / 2,
    Top + Height / 2);
}
