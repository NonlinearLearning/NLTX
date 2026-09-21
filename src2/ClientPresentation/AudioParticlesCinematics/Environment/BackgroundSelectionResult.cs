namespace NLTX.ClientPresentation.AudioParticlesCinematics.Environment;

public readonly record struct BackgroundSelectionResult(
  IReadOnlyList<int> LayerIds,
  float ParallaxOffset);
