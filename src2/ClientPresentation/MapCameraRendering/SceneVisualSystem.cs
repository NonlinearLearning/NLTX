namespace NLTX.ClientPresentation.MapCameraRendering;

public sealed class SceneVisualSystem
{
  public void Apply(
    SceneVisualProjectionComponent component,
    SceneVisualInput input)
  {
    ArgumentNullException.ThrowIfNull(component);
    EnsureNormalized(input.AirLightDecay, nameof(input.AirLightDecay));
    EnsureNormalized(input.SolidLightDecay, nameof(input.SolidLightDecay));
    EnsureNormalized(
      input.OutsideWeatherEffectIntensity,
      nameof(input.OutsideWeatherEffectIntensity));
    component.Replace(input);
  }

  private static void EnsureNormalized(float value, string parameterName)
  {
    if (!float.IsFinite(value) || value < 0f || value > 1f)
    {
      throw new ArgumentOutOfRangeException(parameterName);
    }
  }
}
