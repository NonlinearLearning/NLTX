using System;

namespace Terraria.Projectile;

public static class ProjectileAnimationStateSystem
{
  public static void Reset(ref ProjectileAnimationStateComponent state)
  {
    state.Reset();
  }

  public static void SetFrame(
    ref ProjectileAnimationStateComponent state,
    int frame)
  {
    ValidateNonNegative(frame, nameof(frame));
    state.Frame = frame;
  }

  public static void SetFrameCounter(
    ref ProjectileAnimationStateComponent state,
    int frameCounter)
  {
    ValidateNonNegative(frameCounter, nameof(frameCounter));
    state.FrameCounter = frameCounter;
  }

  public static bool AdvanceFrameCounter(
    ref ProjectileAnimationStateComponent state,
    int frameCounterLimit)
  {
    if (frameCounterLimit <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(frameCounterLimit));
    }

    state.FrameCounter++;
    if (state.FrameCounter < frameCounterLimit)
    {
      return false;
    }

    state.FrameCounter = 0;
    return true;
  }

  private static void ValidateNonNegative(int value, string parameterName)
  {
    if (value < 0)
    {
      throw new ArgumentOutOfRangeException(parameterName);
    }
  }
}
