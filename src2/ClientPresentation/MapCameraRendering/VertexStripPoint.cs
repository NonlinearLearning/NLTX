using System.Numerics;

namespace NLTX.ClientPresentation.MapCameraRendering;

public readonly record struct VertexStripPoint(
  Vector2 Position,
  RgbaColor Color,
  Vector3 TexCoord);
