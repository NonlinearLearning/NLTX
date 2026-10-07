namespace Terraria.WorldInteraction.Tiles;

/// <summary>
/// 保存单个方块的材质、墙、液体及压缩结构标记。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Tile。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Tile.cs。</para>
/// <para>
/// 主要源成员：type（第 8 行）； wall（第 10 行）； liquid（第 12 行）； sTileHeader（第 14 行）； bTileHeader（第 16 行）；
/// bTileHeader2（第 18 行）； bTileHeader3（第 20 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-1/design/。</para>
/// <para>
/// 拆分依据文件：2026-09-05-version4-world-interaction-and-structures-component-design.md。
/// </para>
/// <para>依据位置：第 104 行。</para>
/// </remarks>
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
