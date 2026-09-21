namespace Terraria.EntityLifecycleAttribution;

public sealed class WorldSignState
{
  internal WorldSignState(TileCoordinate coordinate)
  {
    Coordinate = coordinate;
  }

  public TileCoordinate Coordinate { get; }

  public string Text { get; internal set; } = string.Empty;

  public bool IsActive { get; internal set; } = true;
}
