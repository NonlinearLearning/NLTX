using System.Numerics;

namespace NLTX.ClientPresentation.MapCameraRendering;

public readonly record struct CameraTransformSnapshot(
  Vector2 UnscaledPosition,
  Vector2 UnscaledSize,
  Vector2 ScaledPosition,
  Vector2 ScaledSize,
  Vector2 Translation,
  Matrix4x4 TransformationMatrix,
  uint Revision);
