namespace Terraria.WorldInteraction.Tiles;

/// <summary>
/// 保存方块的四种电线、致动器及致动状态。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Tile。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Tile.cs。</para>
/// <para>主要源成员：sTileHeader（第 14 行）。</para>
/// <para>重组说明：四种电线、致动器和致动状态从原压缩头标记拆出。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/non-authoritative/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-non-authoritative-P04-world-tiles-storage-component-design.md。
/// </para>
/// <para>依据位置：第 128 行。</para>
/// </remarks>
public sealed class TileSignalTopologyComponent
{
  public bool HasWire1 { get; internal set; }
  public bool HasWire2 { get; internal set; }
  public bool HasWire3 { get; internal set; }
  public bool HasWire4 { get; internal set; }
  public bool HasActuator { get; internal set; }
  public bool IsActuated { get; internal set; }
}
