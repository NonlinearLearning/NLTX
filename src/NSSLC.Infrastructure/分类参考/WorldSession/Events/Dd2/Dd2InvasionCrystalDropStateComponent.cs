using System;

namespace Terraria.WorldSession.Events.Dd2;

public sealed class Dd2InvasionCrystalDropStateComponent
{
  public Dd2InvasionCrystalDropStateComponent(
    int lastWave = 0,
    int toDrop = 0,
    int alreadyDropped = 0)
  {
    LastWave = lastWave;
    ToDrop = toDrop;
    AlreadyDropped = alreadyDropped;
    Validate();
  }

  public int LastWave { get; internal set; }

  public int ToDrop { get; internal set; }

  public int AlreadyDropped { get; internal set; }

  public void Validate()
  {
    ArgumentOutOfRangeException.ThrowIfNegative(LastWave);
    ArgumentOutOfRangeException.ThrowIfNegative(ToDrop);
    ArgumentOutOfRangeException.ThrowIfNegative(AlreadyDropped);

    if (AlreadyDropped > ToDrop)
    {
      throw new ArgumentException(
        "Already dropped crystals cannot exceed the crystals to drop.",
        nameof(AlreadyDropped));
    }
  }
}
