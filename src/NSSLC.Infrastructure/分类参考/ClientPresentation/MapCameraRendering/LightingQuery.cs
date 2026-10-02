namespace NLTX.ClientPresentation.MapCameraRendering;

public static class LightingQuery
{
  public static bool TryRead(
    LightingCoordinatorComponent component,
    int x,
    int y,
    out LightingSample sample)
  {
    ArgumentNullException.ThrowIfNull(component);
    if (!component.TryReadActive(x, y, out RgbaColor color, out byte mask))
    {
      sample = default;
      return false;
    }

    float brightness = mask / 255f * component.GlobalBrightness;
    sample = new LightingSample(color, mask, brightness, component.ActiveFrameRevision);
    return true;
  }
}
