namespace NLTX.ClientPresentation.MapCameraRendering;

public readonly record struct SceneVisualInput(
  float AirLightDecay,
  float SolidLightDecay,
  float OutsideWeatherEffectIntensity,
  string? StrongBlizzardSound,
  string? InsideBlizzardSound,
  bool SkipTransitions);
