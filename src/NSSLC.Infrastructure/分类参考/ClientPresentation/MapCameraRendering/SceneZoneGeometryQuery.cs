using System.Numerics;

namespace NLTX.ClientPresentation.MapCameraRendering;

public static class SceneZoneGeometryQuery
{
  public static SceneScanRectangle GetZoneScanArea(
    Vector2 center,
    SceneScanThresholds thresholds)
  {
    int x = checked((int)MathF.Floor(center.X)) - thresholds.ZoneScanWidth / 2;
    int y = checked((int)MathF.Floor(center.Y)) - thresholds.ZoneScanHeight / 2;
    return new SceneScanRectangle(
      x - thresholds.ZoneScanPadding,
      y - thresholds.ZoneScanPadding,
      thresholds.ZoneScanWidth + thresholds.ZoneScanPadding * 2,
      thresholds.ZoneScanHeight + thresholds.ZoneScanPadding * 2);
  }

  public static bool IsBelowSurface(
    float worldY,
    float surfaceY)
  {
    return worldY >= surfaceY;
  }
}
