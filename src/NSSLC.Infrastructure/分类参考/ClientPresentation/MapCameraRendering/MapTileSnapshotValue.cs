namespace NLTX.ClientPresentation.MapCameraRendering;

public readonly record struct MapTileSnapshotValue(
  ushort Type,
  byte Light,
  byte Color,
  bool IsChanged,
  bool UpdateQueued);
