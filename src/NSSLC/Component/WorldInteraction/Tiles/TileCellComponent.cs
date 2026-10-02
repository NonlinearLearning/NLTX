namespace Terraria.WorldInteraction.Tiles;

public sealed class TileCellComponent
{
  public ushort TileType { get; internal set; }

  public bool IsActive { get; internal set; }

  public ushort WallType { get; internal set; }

  public byte LiquidAmount { get; internal set; }

  public LiquidKind LiquidKind { get; internal set; }

  // Version4 Tile.sTileHeader 的兼容形状。
  // 具体 header 位语义仍需与 TileSignalTopologyComponent 整合。
  public ushort StructuralHeader { get; internal set; }

  // Version4 bTileHeader、bTileHeader2、bTileHeader3 的兼容形状。
  public byte Header1 { get; internal set; }

  public byte Header2 { get; internal set; }

  public byte Header3 { get; internal set; }
}
