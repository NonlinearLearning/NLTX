namespace NLTX.ClientPresentation.MapCameraRendering;

public static class LightMapQuery
{
  public static bool TryRead(
    LightMapCacheComponent component,
    int x,
    int y,
    out LightMapSample sample)
  {
    ArgumentNullException.ThrowIfNull(component);
    return component.TryRead(x, y, out sample);
  }
}
