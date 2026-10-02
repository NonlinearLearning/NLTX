using System.Numerics;

namespace NLTX.ClientPresentation.MapCameraRendering;

public sealed class MapOverlaySystem
{
  public void ApplyTransform(
    MapOverlayComponent component,
    Vector2 translation,
    SceneScanRectangle clipArea,
    float opacity)
  {
    ArgumentNullException.ThrowIfNull(component);
    if (!float.IsFinite(translation.X) || !float.IsFinite(translation.Y))
    {
      throw new ArgumentOutOfRangeException(nameof(translation));
    }

    if (!float.IsFinite(opacity) || opacity < 0f || opacity > 1f)
    {
      throw new ArgumentOutOfRangeException(nameof(opacity));
    }

    if (clipArea.Width <= 0 || clipArea.Height <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(clipArea));
    }

    component.ReplaceTransform(translation, clipArea, opacity);
  }

  public void AddPing(MapOverlayComponent component, MapPingProjection ping)
  {
    ArgumentNullException.ThrowIfNull(component);
    component.AddPing(ping);
  }

  public void AddPylon(MapOverlayComponent component, MapPylonProjection pylon)
  {
    ArgumentNullException.ThrowIfNull(component);
    component.AddPylon(pylon);
  }

  public void ExpirePings(MapOverlayComponent component, MapClockSnapshot clock)
  {
    ArgumentNullException.ThrowIfNull(component);
    component.ExpirePings(clock);
  }
}
