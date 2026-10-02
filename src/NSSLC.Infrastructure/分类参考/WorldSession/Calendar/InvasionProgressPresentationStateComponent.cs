using System;

namespace Terraria.WorldSession.Calendar;

public sealed class InvasionProgressPresentationStateComponent
{
  public InvasionProgressPresentationStateComponent(
    int progressIcon = 0,
    int progress = 0,
    int progressMax = 0,
    int progressWave = 0,
    int displayFramesRemaining = 0,
    float alpha = 0.0f)
  {
    ProgressIcon = progressIcon;
    Progress = progress;
    ProgressMax = progressMax;
    ProgressWave = progressWave;
    DisplayFramesRemaining = displayFramesRemaining;
    Alpha = alpha;
    Validate();
  }

  public int ProgressIcon { get; internal set; }

  public int Progress { get; internal set; }

  public int ProgressMax { get; internal set; }

  public int ProgressWave { get; internal set; }

  public int DisplayFramesRemaining { get; internal set; }

  public float Alpha { get; internal set; }

  public void Validate()
  {
    ArgumentOutOfRangeException.ThrowIfNegative(ProgressIcon);

    if (Progress < -1)
    {
      throw new ArgumentOutOfRangeException(nameof(Progress));
    }

    ArgumentOutOfRangeException.ThrowIfNegative(ProgressMax);
    ArgumentOutOfRangeException.ThrowIfNegative(ProgressWave);
    ArgumentOutOfRangeException.ThrowIfNegative(DisplayFramesRemaining);

    if (!float.IsFinite(Alpha) || Alpha < 0.0f || Alpha > 1.0f)
    {
      throw new ArgumentOutOfRangeException(nameof(Alpha));
    }
  }
}
