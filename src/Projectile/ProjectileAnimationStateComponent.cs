using System;

namespace Terraria.Projectile;

public struct ProjectileAnimationStateComponent
{
  public ProjectileAnimationStateComponent(
    int frame = 0,
    int frameCounter = 0)
  {
    ValidateNonNegative(frame, nameof(frame));
    ValidateNonNegative(frameCounter, nameof(frameCounter));
    Frame = frame;
    FrameCounter = frameCounter;
  }

  public int Frame;
  public int FrameCounter;

  public void Reset()
  {
    Frame = 0;
    FrameCounter = 0;
  }

  private static void ValidateNonNegative(int value, string parameterName)
  {
    if (value < 0)
    {
      throw new ArgumentOutOfRangeException(parameterName);
    }
  }
}
