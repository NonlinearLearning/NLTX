namespace NLTX.ClientPresentation.MapCameraRendering;

public sealed class SceneVisualProjectionComponent
{
  public float AirLightDecay { get; private set; }

  public float SolidLightDecay { get; private set; }

  public float OutsideWeatherEffectIntensity { get; private set; }

  public string? StrongBlizzardSound { get; private set; }

  public string? InsideBlizzardSound { get; private set; }

  public bool SkipTransitions { get; private set; }

  public uint Revision { get; private set; }

  internal void Replace(SceneVisualInput input)
  {
    AirLightDecay = input.AirLightDecay;
    SolidLightDecay = input.SolidLightDecay;
    OutsideWeatherEffectIntensity = input.OutsideWeatherEffectIntensity;
    StrongBlizzardSound = input.StrongBlizzardSound;
    InsideBlizzardSound = input.InsideBlizzardSound;
    SkipTransitions = input.SkipTransitions;
    Revision++;
  }
}
