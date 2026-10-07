namespace Terraria.Player.Movement;

// status: implemented-partial
// componentId: PLAYER.COMP.GRAVITY_AND_WATER_TRAVERSAL_STATE
// source-members: P08-1303..P08-1307
// crossSubsystemOwner: integration-review
/// <summary>
/// 保存玩家水上行走和重力控制能力。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>
/// 主要源成员：waterWalk（第 2250 行）； waterWalk2（第 2252 行）； forcedGravity（第 2254 行）； gravControl（第 2256
/// 行）； gravControl2（第 2258 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P08-player-environment-armor-component-design.md。</para>
/// <para>依据位置：第 92 行。</para>
/// </remarks>
public sealed class PlayerGravityAndWaterTraversalStateComponent
{
  public bool WaterWalk { get; internal set; }

  public bool WaterWalk2 { get; internal set; }

  // C03 owns gravity and movement parameters; this component owns direction and control facts.
  public int ForcedGravity { get; internal set; }

  public bool GravControl { get; internal set; }

  public bool GravControl2 { get; internal set; }
}
