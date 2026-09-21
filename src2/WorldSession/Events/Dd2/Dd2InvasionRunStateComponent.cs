using System;

namespace Terraria.WorldSession.Events.Dd2;

public sealed class Dd2InvasionRunStateComponent
{
  public Dd2InvasionRunStateComponent(
    bool lostThisRun = false,
    bool wonThisRun = false,
    bool ongoing = false,
    int difficulty = 0)
  {
    LostThisRun = lostThisRun;
    WonThisRun = wonThisRun;
    Ongoing = ongoing;
    Difficulty = difficulty;
    Validate();
  }

  public bool LostThisRun { get; internal set; }

  public bool WonThisRun { get; internal set; }

  public bool Ongoing { get; internal set; }

  public int Difficulty { get; internal set; }

  public void Validate()
  {
    ArgumentOutOfRangeException.ThrowIfNegative(Difficulty);

    if (LostThisRun && WonThisRun)
    {
      throw new ArgumentException(
        "A DD2 run cannot be both won and lost.",
        nameof(WonThisRun));
    }
  }
}
