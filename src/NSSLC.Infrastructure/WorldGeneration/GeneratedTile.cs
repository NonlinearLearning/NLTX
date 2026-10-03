namespace NSSLC.WorldGeneration;

/// <summary>A value copy of one generated cell, independent of the legacy Tile API.</summary>
public readonly record struct GeneratedTile(
  ushort Type,
  ushort Wall,
  byte Liquid,
  short FrameX,
  short FrameY,
  ushort TileHeader,
  byte Header,
  byte Header2,
  byte Header3) {
  public bool Active => (TileHeader & 32) != 0;
  public int LiquidType => (Header & 96) >> 5;
}
