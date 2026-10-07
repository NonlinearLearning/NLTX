using System.Numerics;

namespace Terraria.Player;

// Stores the independently owned portion of a player's sleeping relation.
// Bed eligibility, rotation, stack-manager and network effects remain external.
/// <summary>
/// 保存玩家睡眠锚点、朝向、睡眠时间和床偏移。
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
/// </remarks>
public sealed class PlayerSleepingComponent
{
  public TileCoordinate? AnchorTile { get; set; }

  public DirectionKind RequiredFacing { get; set; }

  public int StackIndex { get; set; } = -1;

  public int TimeSleeping { get; set; }

  public Vector2 BedVisualOffset { get; set; }
}
