using System.Numerics;

namespace NLTX.ClientPresentation.MapCameraRendering;

public readonly record struct MapPingProjection(
  Guid PingId,
  Vector2 Position,
  long CreatedMilliseconds,
  long ExpiresMilliseconds,
  RgbaColor Color);
