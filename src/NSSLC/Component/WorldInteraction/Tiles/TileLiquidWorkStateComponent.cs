namespace Terraria.WorldInteraction.Tiles;

/// <summary>
/// 保存方块液体检查、跳过和内容变更状态。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Tile。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Tile.cs。</para>
/// <para>主要源成员：bTileHeader3（第 20 行）。</para>
/// <para>重组说明：检查和跳过液体的位标记独立表达；LastLiquidChangedRevision 是新增版本。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-P01-liquid-wiring-spatial-death-teleport-component-design.md。
/// </para>
/// <para>依据位置：第 1563 行。</para>
/// </remarks>
public sealed class TileLiquidWorkStateComponent
{
  public bool IsCheckingLiquid { get; internal set; }

  public uint LastLiquidChangedRevision { get; internal set; }

  public bool ShouldSkipLiquid { get; internal set; }
}
