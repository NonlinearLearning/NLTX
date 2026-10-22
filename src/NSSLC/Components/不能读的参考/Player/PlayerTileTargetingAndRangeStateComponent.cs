namespace Terraria.Player.Interaction;

// status: implemented
// componentId: PLAYER.COMP.TILE_TARGETING_AND_RANGE_STATE
// source-members: P08-1140, P08-1141, P08-1152
// crossSubsystemOwner: integration-review
/// <summary>
/// 保存玩家最近的方块交互范围和邻近方块类型标记。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>主要源成员：lastTileRangeX（第 1913 行）； lastTileRangeY（第 1915 行）； adjTile（第 1939 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P08-player-environment-armor-component-design.md。</para>
/// <para>依据位置：第 74 行。</para>
/// <para>
/// 用途：方块交互与环境查询流程记录范围，邻近方块标记可供制作条件和周边能力读取。
/// </para>
/// <para>
/// 当前实现：两个范围属性可内部写入，AdjacentTiles 初始化为空数组且没有替换或扩容入口；完整邻近扫描和可用容量需由实际整合流程提供。
/// </para>
/// <para>
/// 原始分级记录（2026-10-08）：C102，L2（通用可选玩法）。分级理由：挖掘、放置、相邻方块、范围和交互锁是可选建造玩法。
/// </para>
/// </remarks>
public sealed class PlayerTileTargetingAndRangeStateComponent
{
  /// <summary>
  /// 最近记录的水平方块交互范围。
  /// </summary>
  public int LastTileRangeX { get; internal set; }

  /// <summary>
  /// 最近记录的垂直方块交互范围。
  /// </summary>
  public int LastTileRangeY { get; internal set; }

  /// <summary>
  /// 按方块类型索引的邻近标记数组；当前初始化为空数组。
  /// </summary>
  public bool[] AdjacentTiles { get; } = Array.Empty<bool>();
}
