namespace Terraria.Player.Interaction;

// status: implemented
// componentId: PLAYER.COMP.TILE_TARGETING_AND_RANGE_STATE
// source-members: P08-1140, P08-1141, P08-1152
// crossSubsystemOwner: integration-review
/// <summary>
/// 保存玩家最近交互范围和相邻方块标记。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>主要源成员：lastTileRangeX（第 1913 行）； lastTileRangeY（第 1915 行）； adjTile（第 1939 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P08-player-environment-armor-component-design.md。</para>
/// <para>依据位置：第 74 行。</para>
/// </remarks>
public sealed class PlayerTileTargetingAndRangeStateComponent
{
  public int LastTileRangeX { get; internal set; }

  public int LastTileRangeY { get; internal set; }

  public bool[] AdjacentTiles { get; } = Array.Empty<bool>();
}
