namespace NLTX.ClientPresentation.MapCameraRendering;

public readonly record struct TileAnimationDefinition(
  TileAnimationKey Key,
  int FrameCount,
  int TicksPerFrame);
