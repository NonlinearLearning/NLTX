namespace Terraria.NonAuthoritative.WorldStorage.Tiles;

public sealed class TileCellFrameBitComponent
{
  public TileCellFrameBitComponent(
    ushort tileHeader,
    byte tileHeader1,
    byte tileHeader2,
    byte tileHeader3,
    short frameX,
    short frameY)
  {
    TileHeader = tileHeader;
    TileHeader1 = tileHeader1;
    TileHeader2 = tileHeader2;
    TileHeader3 = tileHeader3;
    FrameX = frameX;
    FrameY = frameY;
  }

  public ushort TileHeader { get; }

  public byte TileHeader1 { get; }

  public byte TileHeader2 { get; }

  public byte TileHeader3 { get; }

  public short FrameX { get; }

  public short FrameY { get; }
}
