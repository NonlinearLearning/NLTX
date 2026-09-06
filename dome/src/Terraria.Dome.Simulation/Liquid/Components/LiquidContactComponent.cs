using System;

namespace Terraria.Dome.Simulation.Liquid.Components;

public struct LiquidContactComponent
{
  public long? ResolvedAtTick;
  public bool IsWet;
  public bool IsLavaWet;
  public bool IsHoneyWet;
  public bool IsShimmerWet;
  public byte WetTickCount;
  public LiquidType? DominantLiquidType;

  public bool HasAnyLiquidContact => IsWet || IsLavaWet || IsHoneyWet || IsShimmerWet;

  public bool HasNonWaterLiquidContact => IsLavaWet || IsHoneyWet || IsShimmerWet;

  public void Replace(
    long tick,
    bool isWet,
    bool isLavaWet,
    bool isHoneyWet,
    bool isShimmerWet,
    LiquidType? dominantLiquidType = null)
  {
    if (tick < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(tick));
    }

    if (dominantLiquidType.HasValue && !Enum.IsDefined(dominantLiquidType.Value))
    {
      throw new ArgumentOutOfRangeException(nameof(dominantLiquidType));
    }

    bool hadContact = HasAnyLiquidContact && ResolvedAtTick.HasValue &&
      ResolvedAtTick.Value + 1 == tick;
    IsWet = isWet;
    IsLavaWet = isLavaWet;
    IsHoneyWet = isHoneyWet;
    IsShimmerWet = isShimmerWet;
    DominantLiquidType = HasAnyLiquidContact ? dominantLiquidType : null;
    WetTickCount = HasAnyLiquidContact
      ? hadContact ? (byte)Math.Min(byte.MaxValue, WetTickCount + 1) : (byte)1
      : (byte)0;
    ResolvedAtTick = tick;
  }

  public void Clear()
  {
    ResolvedAtTick = null;
    IsWet = false;
    IsLavaWet = false;
    IsHoneyWet = false;
    IsShimmerWet = false;
    WetTickCount = 0;
    DominantLiquidType = null;
  }
}
