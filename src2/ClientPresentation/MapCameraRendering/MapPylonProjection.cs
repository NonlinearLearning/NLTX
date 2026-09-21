namespace NLTX.ClientPresentation.MapCameraRendering;

public readonly record struct MapPylonProjection(
  Guid PylonId,
  SceneScanRectangle Border,
  RgbaColor Color);
