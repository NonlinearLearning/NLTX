namespace Terraria.NonAuthoritative.Persistence;

public readonly record struct WorldTileHeaderCoreValue
{
  public WorldTileHeaderCoreValue(
    bool active,
    ushort tileType,
    bool wall,
    ushort wallType,
    TileLiquidKind liquid,
    byte liquidAmount,
    bool wire1,
    bool wire2,
    bool wire3,
    byte slope,
    ushort runLength)
  {
    if (!Enum.IsDefined(liquid))
    {
      throw new ArgumentOutOfRangeException(nameof(liquid));
    }

    if (slope > 7)
    {
      throw new ArgumentOutOfRangeException(nameof(slope), "Slope must fit the Header2 three-bit field.");
    }

    if (liquid == TileLiquidKind.None && liquidAmount != 0)
    {
      throw new ArgumentException("A liquid amount requires a liquid kind.", nameof(liquidAmount));
    }

    Active = active;
    TileType = tileType;
    Wall = wall;
    WallType = wallType;
    Liquid = liquid;
    LiquidAmount = liquidAmount;
    Wire1 = wire1;
    Wire2 = wire2;
    Wire3 = wire3;
    Slope = slope;
    RunLength = runLength;
  }

  public bool Active { get; }

  public ushort TileType { get; }

  public bool Wall { get; }

  public ushort WallType { get; }

  public TileLiquidKind Liquid { get; }

  public byte LiquidAmount { get; }

  public bool Wire1 { get; }

  public bool Wire2 { get; }

  public bool Wire3 { get; }

  public byte Slope { get; }

  public ushort RunLength { get; }

  internal bool RequiresHeader2 =>
    Wire1 || Wire2 || Wire3 || Slope != 0 || WallType > byte.MaxValue;
}
