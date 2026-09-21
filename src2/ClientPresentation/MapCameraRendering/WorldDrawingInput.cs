namespace NLTX.ClientPresentation.MapCameraRendering;

public readonly record struct WorldDrawingInput(
  RgbaColor HorizonColor,
  float HorizonBlend,
  string? ParticleToken);
