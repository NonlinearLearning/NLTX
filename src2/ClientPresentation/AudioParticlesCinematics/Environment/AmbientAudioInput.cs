using System.Numerics;

namespace NLTX.ClientPresentation.AudioParticlesCinematics.Environment;

public readonly record struct AmbientAudioInput(
  float Wind,
  float Rain,
  Vector2 WaterfallPosition,
  float WaterfallStrength,
  Vector2 LavafallPosition,
  float LavafallStrength,
  Vector2 LavaPosition,
  float LavaStrength);
