using System.Numerics;

namespace NLTX.ClientPresentation.MapCameraRendering;

public readonly record struct DrawCommand(
  string ResourceToken,
  Vector2 Position,
  SceneScanRectangle? DestinationRectangle,
  SceneScanRectangle? SourceRectangle,
  RgbaColor Color,
  float Rotation,
  Vector2 Origin,
  Vector2 Scale,
  int ShaderHandle,
  bool IgnorePlayerRotation,
  bool UseDestinationRectangle,
  SceneScanRectangle? NullRectangle,
  uint FrameLease);
