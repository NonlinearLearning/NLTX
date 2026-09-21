using System.Numerics;

namespace NLTX.ClientPresentation.AudioParticlesCinematics.Particles;

public readonly record struct CloudVisualState(
  Vector2 Position,
  float Scale,
  float Rotation,
  float RotationSpeed,
  float ScaleSpeed,
  bool Active,
  int SpriteDirection,
  int Type,
  int Width,
  int Height,
  float Alpha,
  bool Kill,
  bool HasCameraCenter,
  Vector2 LastCameraCenter);
