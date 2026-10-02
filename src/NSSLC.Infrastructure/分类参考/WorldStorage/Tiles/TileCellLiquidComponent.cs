namespace Terraria.NonAuthoritative.WorldStorage.Tiles;

public sealed class TileCellLiquidComponent
{
  public TileCellLiquidComponent(byte liquidType, byte amount)
  {
    if (liquidType > 3)
    {
      throw new ArgumentOutOfRangeException(nameof(liquidType));
    }

    LiquidType = liquidType;
    Amount = amount;
  }

  public byte LiquidType { get; }

  public byte Amount { get; }
}
