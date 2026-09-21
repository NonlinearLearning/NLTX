using System;

namespace Terraria.WorldSession.Environment.Celebration;

public sealed class MysticFairyEventStateComponent
{
  public MysticFairyEventStateComponent(
    bool canAttemptSpawn = false,
    int delayUntilNextAttempt = 0)
  {
    CanAttemptSpawn = canAttemptSpawn;
    DelayUntilNextAttempt = delayUntilNextAttempt;
    Validate();
  }

  public bool CanAttemptSpawn { get; internal set; }

  public int DelayUntilNextAttempt { get; internal set; }

  public void Validate()
  {
    ArgumentOutOfRangeException.ThrowIfNegative(DelayUntilNextAttempt);
  }
}
