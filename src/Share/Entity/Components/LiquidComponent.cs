namespace EntityEcs.Components;

//液体组件
public struct LiquidComponent
{
  public LiquidKind InLiquid;
  public byte LiquidTimer;
  public bool AnyWet => (InLiquid != LiquidKind.Nano);

  public LiquidComponent()
  {
    InLiquid = LiquidKind.Nano;
    LiquidTimer = 0;
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
