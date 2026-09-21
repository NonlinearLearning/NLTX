namespace Terraria.WorldStorage;

public readonly record struct LiquidBufferCheckingIntent(
  LiquidBufferCheckingIntentKind Kind,
  TileCoordinate? Coordinate)
{
  public bool HasCoordinate => Coordinate.HasValue;

  public static LiquidBufferCheckingIntent None =>
    new(LiquidBufferCheckingIntentKind.None, null);

  public static LiquidBufferCheckingIntent Set(TileCoordinate coordinate)
  {
    return new(LiquidBufferCheckingIntentKind.Set, coordinate);
  }

  public static LiquidBufferCheckingIntent Clear(TileCoordinate coordinate)
  {
    return new(LiquidBufferCheckingIntentKind.Clear, coordinate);
  }
}
