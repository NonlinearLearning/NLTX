using System.Numerics;

namespace NLTX.ClientPresentation.AudioParticlesCinematics.Particles;

public readonly record struct RainVisualState(
  Vector2 Position,
  Vector2 Velocity,
  float Alpha,
  bool Active,
  bool Kill,
  int Type);
