namespace EntityEcs.Components;

public struct LiquidContactStateComponent
{
  public long? ResolvedAtTick;
  public bool IsWet;
  public bool IsLavaWet;
  public bool IsHoneyWet;
  public bool IsShimmerWet;
  public int WetTickCount;
  public LiquidKind? DominantLiquidType;

  public bool HasAnyLiquidContact => IsWet || IsLavaWet ||
    IsHoneyWet || IsShimmerWet;
}
