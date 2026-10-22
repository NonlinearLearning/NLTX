using System.Numerics;

namespace Terraria.Player;

// Stores the independently owned portion of a player's sleeping relation.
// Bed eligibility, rotation, stack-manager and network effects remain external.
/// <summary>
/// 保存玩家睡眠时的床位锚点、朝向、叠放位置和睡眠计时。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.GameContent.PlayerSleepingHelper。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria.GameContent/PlayerSleepingHelper.cs。</para>
/// <para>
/// 主要源成员：isSleeping（第 13 行）； sleepingIndex（第 15 行）； timeSleeping（第 17 行）； visualOffsetOfBedBase（第
/// 19 行）。
/// </para>
/// <para>重组说明：锚点和朝向来自睡眠目标判定流程。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-P04-player-lifecycle-interaction-component-design.md。
/// </para>
/// <para>依据位置：第 490 行。</para>
/// <para>
/// 用途：睡眠流程登记床位关联与睡眠进度，休息和表现流程读取计时和床位显示偏移。
/// </para>
/// <para>
/// 当前实现：只保存关系和计数，StackIndex 默认 -1。床位资格、计时推进、叠放分配、旋转和网络效果由外部流程负责。
/// </para>
/// <para>
/// 原始分级记录（2026-10-08）：C144，L2（通用可选玩法）。分级理由：统计金币损失、坐下、床睡眠和休息加成是死亡重生以外的玩法。
/// </para>
/// </remarks>
public sealed class PlayerSleepingComponent
{
  /// <summary>
  /// 床位所在方块锚点；未关联时可为空。
  /// </summary>
  public TileCoordinate? AnchorTile { get; set; }

  /// <summary>
  /// 睡眠时要求的玩家朝向。
  /// </summary>
  public DirectionKind RequiredFacing { get; set; }

  /// <summary>
  /// 床位叠放索引，默认 -1 表示尚未分配。
  /// </summary>
  public int StackIndex { get; set; } = -1;

  /// <summary>
  /// 外部睡眠流程维护的睡眠持续计数。
  /// </summary>
  public int TimeSleeping { get; set; }

  /// <summary>
  /// 睡眠时使用的床位视觉偏移。
  /// </summary>
  public Vector2 BedVisualOffset { get; set; }
}
