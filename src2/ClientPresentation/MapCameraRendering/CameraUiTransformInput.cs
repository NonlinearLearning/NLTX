using System.Numerics;

namespace NLTX.ClientPresentation.MapCameraRendering;

public readonly record struct CameraUiTransformInput(
  float UiScaleWanted,
  float UiScaleUsed,
  Vector2 CameraPosition,
  Vector2 CameraSize,
  Vector2 Zoom,
  Vector2 Translation,
  Vector2 Pan,
  float Lerp,
  int LerpTimer,
  int LerpTimeToggle);
