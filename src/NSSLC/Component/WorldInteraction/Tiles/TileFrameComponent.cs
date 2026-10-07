namespace Terraria.WorldInteraction.Tiles;

/// <summary>
/// 保存方块和墙的贴图帧坐标。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Tile。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Tile.cs。</para>
/// <para>
/// 主要源成员：bTileHeader2（第 18 行）； bTileHeader3（第 20 行）； frameX（第 22 行）； frameY（第 24 行）。
/// </para>
/// <para>重组说明：墙帧坐标从原压缩头信息解码后独立表达。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-1/design/。</para>
/// <para>
/// 拆分依据文件：2026-09-05-version4-world-interaction-and-structures-component-design.md。
/// </para>
/// <para>依据位置：第 142 行。</para>
/// </remarks>
public sealed class TileFrameComponent
{
  public short TileFrameX { get; internal set; }
  public short TileFrameY { get; internal set; }
  public short WallFrameX { get; internal set; }
  public short WallFrameY { get; internal set; }
}
