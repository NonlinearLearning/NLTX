namespace Terraria.WorldStorage;

public struct TileCellState
{
  public short FrameX;
  public short FrameY;
  public byte Header;
  public byte Header2;
  public byte Header3;
  public bool IsCheckingLiquid;
  public uint LastLiquidChangedRevision;
  public byte LiquidAmount;
  public byte LiquidType;
  public bool ShouldSkipLiquid;
  public ushort TileHeader;
  public ushort Type;
  public ushort Wall;

  public bool HasActiveLiquidWorkItem => IsCheckingLiquid && LiquidAmount != 0;
  public bool IsLiquidEmpty => LiquidAmount == 0;
  public bool IsLiquidFull => LiquidAmount == byte.MaxValue;
}
