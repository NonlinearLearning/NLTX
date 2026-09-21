using System.Numerics;

namespace NLTX.ClientPresentation.AudioParticlesCinematics.Particles;

public readonly record struct DustVisualState(
  Vector2 Position,
  Vector2 Velocity,
  float Rotation,
  float Scale,
  int Alpha,
  int Type,
  float Light,
  bool FullBright,
  bool Active,
  int Frame,
  bool FirstFrame,
  uint Color,
  bool HasShaderData);
