namespace EntityEcs.Components;

public struct LiquidComponent
{
  public LiquidKind? DominantLiquidKind;
  public LiquidKind InLiquid;
  public bool IsHoneyWet;
  public bool IsLavaWet;
  public bool IsShimmerWet;
  public bool IsWet;
  public byte LiquidTimer;
  public long? ResolvedAtTick;

  public bool AnyWet => InLiquid != LiquidKind.Nano || IsWet || IsLavaWet ||
    IsHoneyWet || IsShimmerWet;
  public bool HasAnyLiquidContact => AnyWet;
  public bool HasNonWaterContact => InLiquid is LiquidKind.Lava or LiquidKind.Honey or
    LiquidKind.Shimmer || IsLavaWet || IsHoneyWet || IsShimmerWet;
  public bool HasNonWaterLiquidContact => HasNonWaterContact;
  public byte WetTickCount
  {
    get => LiquidTimer;
    set => LiquidTimer = value;
  }

}

public enum LiquidKind : byte
{
  Nano,
  Water,
  Shimmer,
  Honey,
  Lava,
}
