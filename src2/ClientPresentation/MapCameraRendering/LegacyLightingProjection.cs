namespace NLTX.ClientPresentation.MapCameraRendering;

public static class LegacyLightingProjection
{
  public static bool TryRead(
    LegacyLightingCacheComponent component,
    int x,
    int y,
    out LightMapSample sample)
  {
    ArgumentNullException.ThrowIfNull(component);
    return component.TryRead(x, y, out sample);
  }
}
