namespace Terraria.Items;

public readonly record struct TileCoordinates(int X, int Y)
{
  public static TileCoordinates Origin => new(0, 0);
}
