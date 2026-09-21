namespace NLTX.ClientPresentation.AudioParticlesCinematics.Animation;

public static class CageAnimationQuery
{
  public static CageAnimationStepResult AdvanceFrame(
    int currentFrame,
    int frameCount,
    int ticksSinceAdvance,
    int ticksPerFrame)
  {
    if (frameCount <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(frameCount));
    }

    if (ticksSinceAdvance < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(ticksSinceAdvance));
    }

    if (ticksPerFrame <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(ticksPerFrame));
    }

    if ((uint)currentFrame >= (uint)frameCount)
    {
      throw new ArgumentOutOfRangeException(nameof(currentFrame));
    }

    if (ticksSinceAdvance < ticksPerFrame)
    {
      return new CageAnimationStepResult(currentFrame, ticksSinceAdvance + 1, false);
    }

    return new CageAnimationStepResult((currentFrame + 1) % frameCount, 0, true);
  }
}
