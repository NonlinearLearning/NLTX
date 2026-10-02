namespace Terraria.EntityLifecycleAttribution;

public readonly record struct TileCoordinate(int X, int Y)
{
  public bool IsValid => X >= 0 && Y >= 0;
}
