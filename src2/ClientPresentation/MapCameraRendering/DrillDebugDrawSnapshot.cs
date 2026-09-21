using System.Numerics;

namespace NLTX.ClientPresentation.MapCameraRendering;

public readonly record struct DrillDebugDrawSnapshot(
  Vector2 Point,
  RgbaColor Color);
