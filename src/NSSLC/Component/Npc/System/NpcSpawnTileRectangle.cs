namespace Terraria.Npc;

public readonly record struct NpcSpawnTileRectangle(
  int X,
  int Y,
  int Width,
  int Height)
{
  public int Left => X;

  public int Top => Y;

  public int Right => X + Width;

  public int Bottom => Y + Height;

  public bool Contains(int x, int y)
  {
    return x >= Left && x < Right && y >= Top && y < Bottom;
  }
}
