using System.Numerics;

namespace Terraria.Player;

// Stores the independently owned portion of a player's sitting relation.
// Tile/frame eligibility, ExtraSeatInfo and stack-manager effects remain external.
/// <summary>
/// 保存玩家坐下的锚点、朝向、座位偏移和堆叠顺序。
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
/// </remarks>
public sealed class PlayerSittingComponent
{
  public TileCoordinate? AnchorTile { get; set; }

  public DirectionKind RequiredFacing { get; set; }

  // RestSeatFeatures is the existing NLTX equivalent for the confirmed toilet flag.
  public RestSeatFeatures SeatFeatures { get; set; }

  public Vector2 SeatOffset { get; set; }

  public int StackIndex { get; set; } = -1;
}
