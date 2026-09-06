namespace Terraria.WorldStorage;

public struct TileLiquidStateComponent
{
  public byte Amount;
  public byte Type;

  public bool IsEmpty => Amount == 0;
}
