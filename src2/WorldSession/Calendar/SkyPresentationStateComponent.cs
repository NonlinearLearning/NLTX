using System;

namespace Terraria.WorldSession.Calendar;

public sealed class SkyPresentationStateComponent
{
  public SkyPresentationStateComponent(int starCount = 0)
  {
    StarCount = starCount;
    Validate();
  }

  public int StarCount { get; internal set; }

  public void Validate()
  {
    ArgumentOutOfRangeException.ThrowIfNegative(StarCount);
  }
}
