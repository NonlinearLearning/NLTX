using System.Numerics;

namespace NLTX.ClientPresentation.AudioParticlesCinematics.Environment;

public readonly record struct AmbientAudioCommand(
  AmbientAudioKind Kind,
  Vector2 Position,
  float Strength);
