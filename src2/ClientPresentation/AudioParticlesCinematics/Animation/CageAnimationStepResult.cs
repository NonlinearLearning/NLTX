namespace NLTX.ClientPresentation.AudioParticlesCinematics.Animation;

public readonly record struct CageAnimationStepResult(
  int Frame,
  int TicksSinceAdvance,
  bool Advanced);
