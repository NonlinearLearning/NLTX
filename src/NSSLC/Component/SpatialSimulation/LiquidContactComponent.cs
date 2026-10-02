using System;

using EntityEcs.Components;

namespace Terraria.SpatialSimulation.Components;

// status: proposed
// componentId: SPATIAL.COMP.LIQUID_CONTACT
// crossSubsystemOwner: integration-review
public struct LiquidContactComponent
{
  public long? ResolvedAtTick;
  public bool IsWet;
  public bool IsLavaWet;
  public bool IsHoneyWet;
  public bool IsShimmerWet;
  public byte WetTickCount;
  public LiquidKind? DominantLiquidKind;

  public bool HasAnyLiquidContact =>
    IsWet || IsLavaWet || IsHoneyWet || IsShimmerWet;

  public bool HasNonWaterLiquidContact =>
    IsLavaWet || IsHoneyWet || IsShimmerWet;

  public void Replace(
    long tick,
    bool isWet,
    bool isLavaWet,
    bool isHoneyWet,
    bool isShimmerWet,
    LiquidKind? dominantLiquidKind = null)
  {
    if (tick < 0)
    {
      throw new ArgumentOutOfRangeException(
        nameof(tick),
        tick,
        "Tick must be non-negative.");
    }

    if (dominantLiquidKind.HasValue &&
      !Enum.IsDefined(dominantLiquidKind.Value))
    {
      throw new ArgumentOutOfRangeException(
        nameof(dominantLiquidKind),
        dominantLiquidKind,
        "Liquid kind must be a defined value.");
    }

    bool hadContinuousContact =
      HasAnyLiquidContact &&
      ResolvedAtTick.HasValue &&
      ResolvedAtTick.Value + 1 == tick;

    IsWet = isWet;
    IsLavaWet = isLavaWet;
    IsHoneyWet = isHoneyWet;
    IsShimmerWet = isShimmerWet;

    bool hasContact = HasAnyLiquidContact;
    DominantLiquidKind = hasContact ? dominantLiquidKind : null;
    WetTickCount = hasContact
      ? hadContinuousContact
        ? (byte)Math.Min(byte.MaxValue, WetTickCount + 1)
        : (byte)1
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
    DominantLiquidKind = null;
  }
}
