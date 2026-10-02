namespace NLTX.ClientPresentation.MapCameraRendering;

public readonly record struct MapLoadResult(
  bool Success,
  MapPersistenceSnapshot? Snapshot,
  MapPersistenceFailureKind FailureKind,
  int Attempts,
  string? ErrorType);
