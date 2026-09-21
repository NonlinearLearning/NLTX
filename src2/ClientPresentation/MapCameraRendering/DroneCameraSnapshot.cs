namespace NLTX.ClientPresentation.MapCameraRendering;

public readonly record struct DroneCameraSnapshot(
  Guid? TrackedProjectileId,
  int LastTrackedType,
  uint Revision);
