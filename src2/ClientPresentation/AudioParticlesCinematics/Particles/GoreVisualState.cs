using System.Numerics;

namespace NLTX.ClientPresentation.AudioParticlesCinematics.Particles;

public readonly record struct GoreVisualState(
  int GoreTime,
  Vector2 Position,
  Vector2 Velocity,
  float Rotation,
  float Scale,
  int Alpha,
  int Type,
  float Light,
  bool Active,
  bool Sticky,
  int TimeLeft,
  bool BehindTiles,
  byte FrameCounter,
  int Frame);
