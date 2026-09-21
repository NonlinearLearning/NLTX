using System;

namespace Terraria.WorldSession.Presentation;

public sealed class CreditsRollPresentationStateComponent
{
  public const int MaximumRemainingFrames = 28800;

  public CreditsRollPresentationStateComponent(int remainingFrames = 0)
  {
    RemainingFrames = remainingFrames;
    Validate();
  }

  public int RemainingFrames { get; internal set; }

  public void Validate()
  {
    if (RemainingFrames is < 0 or > MaximumRemainingFrames)
    {
      throw new ArgumentOutOfRangeException(nameof(RemainingFrames));
    }
  }
}
