namespace NLTX.ClientPresentation.MapCameraRendering;

public readonly record struct ShaderApplyResult(
  bool Applied,
  int ParameterCount,
  string? FailureReason);
