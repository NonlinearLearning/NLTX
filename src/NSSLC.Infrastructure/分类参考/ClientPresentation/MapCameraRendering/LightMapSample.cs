namespace NLTX.ClientPresentation.MapCameraRendering;

public readonly record struct LightMapSample(
  RgbaColor Color,
  byte Mask,
  uint Revision);
