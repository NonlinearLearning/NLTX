using System.Numerics;

namespace NLTX.ClientPresentation.MapCameraRendering;

public readonly record struct EntityShadowSnapshot(
  Guid EntityId,
  Vector2 Position,
  float Rotation,
  float Opacity,
  RgbaColor Color,
  uint SourceRevision);
