namespace NLTX.ClientPresentation.MapCameraRendering;

public readonly record struct SpriteBatchSubmissionResult(
  bool Submitted,
  int CommandCount,
  uint FrameLease);
