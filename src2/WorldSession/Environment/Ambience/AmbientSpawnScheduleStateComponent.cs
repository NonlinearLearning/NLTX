using System;

namespace Terraria.WorldSession.Environment.Ambience;

public sealed class AmbientSpawnScheduleStateComponent
{
  public AmbientSpawnScheduleStateComponent(int updatesUntilNextAttempt = 0)
  {
    UpdatesUntilNextAttempt = updatesUntilNextAttempt;
    Validate();
  }

  public int UpdatesUntilNextAttempt { get; internal set; }

  public void Validate()
  {
    ArgumentOutOfRangeException.ThrowIfNegative(UpdatesUntilNextAttempt);
  }
}
