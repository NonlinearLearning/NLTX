namespace NLTX.ClientPresentation.MapCameraRendering;

public readonly record struct LightMapCellInput(
  int X,
  int Y,
  RgbaColor Color,
  byte Mask);
