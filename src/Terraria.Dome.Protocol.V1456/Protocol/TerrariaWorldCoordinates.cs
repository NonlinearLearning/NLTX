namespace Terraria.Dome.Protocol.V1456.Protocol;

public static class TerrariaWorldCoordinates
{
  public const float PixelsPerTile = 16.0f;

  public static float ToPixels(float tileCoordinate)
  {
    return tileCoordinate * PixelsPerTile;
  }

  public static float FromPixels(float pixelCoordinate)
  {
    return pixelCoordinate / PixelsPerTile;
  }
}
