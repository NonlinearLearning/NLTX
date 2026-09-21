namespace Terraria.WorldGeneration.Components;

public readonly record struct UndergroundDesertRectangle(
  int X,
  int Y,
  int Width,
  int Height)
{
  public bool IsEmpty => Width == 0 && Height == 0;

  public bool HasPositiveArea => Width > 0 && Height > 0;

  public int Right => X + Width;

  public int Bottom => Y + Height;

  public bool Contains(TilePosition position)
  {
    return HasPositiveArea &&
      position.X >= X &&
      position.X < Right &&
      position.Y >= Y &&
      position.Y < Bottom;
  }

  public bool Contains(UndergroundDesertRectangle other)
  {
    return HasPositiveArea &&
      other.HasPositiveArea &&
      other.X >= X &&
      other.Y >= Y &&
      other.Right <= Right &&
      other.Bottom <= Bottom;
  }

  public bool Intersects(UndergroundDesertRectangle other)
  {
    return HasPositiveArea &&
      other.HasPositiveArea &&
      X < other.Right &&
      other.X < Right &&
      Y < other.Bottom &&
      other.Y < Bottom;
  }
}
