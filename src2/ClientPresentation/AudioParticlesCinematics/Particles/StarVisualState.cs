using System.Numerics;

namespace NLTX.ClientPresentation.AudioParticlesCinematics.Particles;

public readonly record struct StarVisualState(
  Vector2 Position,
  float Scale,
  float Rotation,
  int Type,
  float Twinkle,
  float TwinkleSpeed,
  float RotationSpeed,
  bool Falling,
  bool Hidden,
  Vector2 FallSpeed,
  int FallTime,
  Vector2 Velocity,
  float FadeIn);
