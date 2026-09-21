namespace NLTX.ClientPresentation.MapCameraRendering;

public readonly record struct LightingSample(
  RgbaColor Color,
  byte Mask,
  float Brightness,
  uint FrameRevision);
