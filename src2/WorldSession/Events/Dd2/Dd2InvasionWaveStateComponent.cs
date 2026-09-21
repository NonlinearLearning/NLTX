using System;

namespace Terraria.WorldSession.Events.Dd2;

public sealed class Dd2InvasionWaveStateComponent
{
  public Dd2InvasionWaveStateComponent(int timeLeftUntilSpawningBegins = 0)
  {
    TimeLeftUntilSpawningBegins = timeLeftUntilSpawningBegins;
    Validate();
  }

  public int TimeLeftUntilSpawningBegins { get; internal set; }

  public void Validate()
  {
    ArgumentOutOfRangeException.ThrowIfNegative(TimeLeftUntilSpawningBegins);
  }
}
