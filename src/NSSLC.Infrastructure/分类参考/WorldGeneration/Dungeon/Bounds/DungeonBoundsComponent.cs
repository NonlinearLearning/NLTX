namespace Terraria.WorldGeneration.Dungeon.Bounds;

public sealed class DungeonBoundsComponent
{
  public DungeonBoundsComponent(int worldWidth, int worldHeight)
  {
    if (worldWidth <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(worldWidth));
    }

    if (worldHeight <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(worldHeight));
    }

    WorldWidth = worldWidth;
    WorldHeight = worldHeight;
  }

  public int WorldWidth { get; }

  public int WorldHeight { get; }

  public int Left { get; private set; }

  public int Right { get; private set; }

  public int Top { get; private set; }

  public int Bottom { get; private set; }

  public int Width => Right - Left;

  public int Height => Bottom - Top;

  public DungeonTilePoint Center => new(
    Left + Width / 2,
    Top + Height / 2);

  public DungeonBoundsRectangle Hitbox => new(Left, Top, Width, Height);

  internal void Replace(int left, int right, int top, int bottom)
  {
    Left = left;
    Right = right;
    Top = top;
    Bottom = bottom;
  }
}
