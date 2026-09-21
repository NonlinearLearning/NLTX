using System.Numerics;

namespace NLTX.ClientPresentation.MapCameraRendering;

public sealed class DrillDebugDrawProjection
{
  public DrillDebugDrawSnapshot Project(Vector2 point, RgbaColor color)
  {
    if (!float.IsFinite(point.X) || !float.IsFinite(point.Y))
    {
      throw new ArgumentOutOfRangeException(nameof(point));
    }

    return new DrillDebugDrawSnapshot(point, color);
  }
}
