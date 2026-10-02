namespace Terraria.WorldGeneration.Dungeon.Bounds;

public readonly record struct DungeonBoundsRectangle(int X, int Y, int Width, int Height)
{
  public int Right => X + Width;

  public int Bottom => Y + Height;

  public bool IsEmpty => Width <= 0 || Height <= 0;

  public bool Contains(DungeonTilePoint point)
  {
    return !IsEmpty &&
      point.X >= X &&
      point.X < Right &&
      point.Y >= Y &&
      point.Y < Bottom;
  }

  public bool Intersects(DungeonBoundsRectangle other)
  {
    return !IsEmpty &&
      !other.IsEmpty &&
      X < other.Right &&
      Right > other.X &&
      Y < other.Bottom &&
      Bottom > other.Y;
  }
}
