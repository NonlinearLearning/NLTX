namespace Terraria.NpcTownBestiary;

public readonly record struct BestiaryPlayerBounds(int Left, int Top, int Width, int Height)
{
  public bool Intersects(BestiaryPlayerBounds other)
  {
    return Left < other.Left + other.Width &&
      Left + Width > other.Left &&
      Top < other.Top + other.Height &&
      Top + Height > other.Top;
  }
}
