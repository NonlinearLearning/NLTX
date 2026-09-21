namespace NLTX.ClientPresentation.AudioParticlesCinematics.Animation;

public sealed class CageAnimationSystem
{
  public CageAnimationStepResult Advance(
    CageBirdAnimationVisualsComponent component,
    CageAnimationKey key,
    int frameCount,
    int ticksPerFrame)
  {
    return AdvanceFrame(component, key, frameCount, ticksPerFrame);
  }

  public CageAnimationStepResult Advance(
    CageAquaticAnimationVisualsComponent component,
    CageAnimationKey key,
    int frameCount,
    int ticksPerFrame)
  {
    return AdvanceFrame(component, key, frameCount, ticksPerFrame);
  }

  public CageAnimationStepResult Advance(
    CageMammalAnimationVisualsComponent component,
    CageAnimationKey key,
    int frameCount,
    int ticksPerFrame)
  {
    return AdvanceFrame(component, key, frameCount, ticksPerFrame);
  }

  public CageAnimationStepResult Advance(
    CageSmallCritterAnimationVisualsComponent component,
    CageAnimationKey key,
    int frameCount,
    int ticksPerFrame)
  {
    return AdvanceFrame(component, key, frameCount, ticksPerFrame);
  }

  public CageAnimationProjection Project(
    CageAnimationKey key,
    CageAnimationFrameState state)
  {
    return new CageAnimationProjection(key, state.Frame, state.Mode);
  }

  private static CageAnimationStepResult AdvanceFrame(
    CageBirdAnimationVisualsComponent component,
    CageAnimationKey key,
    int frameCount,
    int ticksPerFrame)
  {
    component.TryGet(key, out CageAnimationFrameState state);
    CageAnimationStepResult result = CageAnimationQuery.AdvanceFrame(
      state.Frame,
      frameCount,
      state.TicksSinceAdvance,
      ticksPerFrame);
    component.Set(key, new CageAnimationFrameState(
      result.Frame,
      result.TicksSinceAdvance,
      state.Mode));
    return result;
  }

  private static CageAnimationStepResult AdvanceFrame(
    CageAquaticAnimationVisualsComponent component,
    CageAnimationKey key,
    int frameCount,
    int ticksPerFrame)
  {
    component.TryGet(key, out CageAnimationFrameState state);
    CageAnimationStepResult result = CageAnimationQuery.AdvanceFrame(
      state.Frame,
      frameCount,
      state.TicksSinceAdvance,
      ticksPerFrame);
    component.Set(key, new CageAnimationFrameState(
      result.Frame,
      result.TicksSinceAdvance,
      state.Mode));
    return result;
  }

  private static CageAnimationStepResult AdvanceFrame(
    CageMammalAnimationVisualsComponent component,
    CageAnimationKey key,
    int frameCount,
    int ticksPerFrame)
  {
    component.TryGet(key, out CageAnimationFrameState state);
    CageAnimationStepResult result = CageAnimationQuery.AdvanceFrame(
      state.Frame,
      frameCount,
      state.TicksSinceAdvance,
      ticksPerFrame);
    component.Set(key, new CageAnimationFrameState(
      result.Frame,
      result.TicksSinceAdvance,
      state.Mode));
    return result;
  }

  private static CageAnimationStepResult AdvanceFrame(
    CageSmallCritterAnimationVisualsComponent component,
    CageAnimationKey key,
    int frameCount,
    int ticksPerFrame)
  {
    component.TryGet(key, out CageAnimationFrameState state);
    CageAnimationStepResult result = CageAnimationQuery.AdvanceFrame(
      state.Frame,
      frameCount,
      state.TicksSinceAdvance,
      ticksPerFrame);
    component.Set(key, new CageAnimationFrameState(
      result.Frame,
      result.TicksSinceAdvance,
      state.Mode));
    return result;
  }
}
