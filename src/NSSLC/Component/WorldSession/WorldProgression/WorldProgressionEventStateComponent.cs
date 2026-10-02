using System;

namespace Terraria.WorldProgression.Components;

public sealed class WorldProgressionEventStateComponent
{
  public bool ShadowOrbSmashed { get; private set; }

  public int ShadowOrbCount { get; private set; }

  public int AltarCount { get; private set; }

  public void Replace(bool shadowOrbSmashed, int shadowOrbCount, int altarCount)
  {
    ArgumentOutOfRangeException.ThrowIfNegative(shadowOrbCount);
    ArgumentOutOfRangeException.ThrowIfNegative(altarCount);

    ShadowOrbSmashed = shadowOrbSmashed;
    ShadowOrbCount = shadowOrbCount;
    AltarCount = altarCount;
  }

  public void Reset()
  {
    ShadowOrbSmashed = false;
    ShadowOrbCount = 0;
    AltarCount = 0;
  }
}
