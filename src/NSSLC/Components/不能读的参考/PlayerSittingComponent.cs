using System.Numerics;

namespace Terraria.Player;

// Stores the independently owned portion of a player's sitting relation.
// Tile/frame eligibility, ExtraSeatInfo and stack-manager effects remain external.
/// <summary>
/// 保存玩家坐下时的座位锚点、朝向和显示偏移关系。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.GameContent.PlayerSittingHelper。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria.GameContent/PlayerSittingHelper.cs。</para>
/// <para>
/// 主要源成员：isSitting（第 10 行）； details（第 12 行）； offsetForSeat（第 14 行）； sittingIndex（第 16 行）。
/// </para>
/// <para>重组说明：锚点和朝向来自坐下目标判定流程；原帮助类状态按活动组件重组。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-P04-player-lifecycle-interaction-component-design.md。
/// </para>
/// <para>依据位置：第 489 行。</para>
/// <para>
/// 用途：坐下流程登记座位所在方块和要求的朝向，表现流程读取座位偏移与叠放位置。
/// </para>
/// <para>
/// 当前实现：只保存坐下关系，StackIndex 默认 -1。座位方块资格、座位叠放分配和实际交互效果由外部流程负责。
/// </para>
/// <para>
/// 原始分级记录（2026-10-08）：C143，L2（通用可选玩法）。分级理由：统计金币损失、坐下、床睡眠和休息加成是死亡重生以外的玩法。
/// </para>
/// </remarks>
public sealed class PlayerSittingComponent
{
  /// <summary>
  /// 座位所在方块锚点；未关联时可为空。
  /// </summary>
  public TileCoordinate? AnchorTile { get; set; }

  /// <summary>
  /// 座位要求的玩家朝向。
  /// </summary>
  public DirectionKind RequiredFacing { get; set; }

  // RestSeatFeatures is the existing NLTX equivalent for the confirmed toilet flag.
  /// <summary>
  /// 座位特征位集，包括原版已确认的马桶标记。
  /// </summary>
  public RestSeatFeatures SeatFeatures { get; set; }

  /// <summary>
  /// 坐下时使用的二维座位偏移。
  /// </summary>
  public Vector2 SeatOffset { get; set; }

  /// <summary>
  /// 座位叠放索引，默认 -1 表示尚未分配。
  /// </summary>
  public int StackIndex { get; set; } = -1;
}
