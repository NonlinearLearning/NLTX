namespace NLTX.ClientPresentation.MapCameraRendering;

public readonly record struct MapPersistenceResult(
  bool Success,
  MapPersistenceFailureKind FailureKind,
  int Attempts,
  string? ErrorType);
