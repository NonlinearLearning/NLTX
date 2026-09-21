namespace NLTX.ClientPresentation.MapCameraRendering;

public readonly record struct MapInvalidationCommand(
  SceneScanRectangle Area,
  uint SourceRevision);
