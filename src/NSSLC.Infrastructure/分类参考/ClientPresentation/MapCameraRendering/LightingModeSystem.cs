namespace NLTX.ClientPresentation.MapCameraRendering;

public sealed class LightingModeSystem
{
  public void Apply(
    LightingCoordinatorComponent component,
    LightingMode mode,
    float globalBrightness,
    int offScreenTileBudget)
  {
    ArgumentNullException.ThrowIfNull(component);
    if (!float.IsFinite(globalBrightness) || globalBrightness < 0f || globalBrightness > 1f)
    {
      throw new ArgumentOutOfRangeException(nameof(globalBrightness));
    }

    if (offScreenTileBudget < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(offScreenTileBudget));
    }

    component.Configure(mode, globalBrightness, offScreenTileBudget);
  }
}
