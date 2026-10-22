namespace Terraria.Player.Movement;

// status: implemented-partial
// componentId: PLAYER.COMP.GRAVITY_AND_WATER_TRAVERSAL_STATE
// source-members: P08-1303..P08-1307
// crossSubsystemOwner: integration-review
/// <summary>
/// 保存玩家水面行走、强制重力方向和重力控制能力。
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
/// <para>
/// 用途：装备和控制流程登记水面行走与重力控制事实，再由移动系统应用相应运动规则。
/// </para>
/// <para>
/// 当前实现：只保存能力和控制状态。ForcedGravity 的数值含义由移动流程约定，具体重力参数和碰撞处理由外部流程负责。
/// </para>
/// <para>
/// 原始分级记录（2026-10-08）：C105，L2（通用可选玩法）。分级理由：区域、游泳、呼吸、熔岩耐受、风力及表面状态属于额外环境玩法。
/// </para>
/// </remarks>
public sealed class PlayerGravityAndWaterTraversalStateComponent
{
  /// <summary>
  /// 第一种水面行走能力是否生效。
  /// </summary>
  public bool WaterWalk { get; internal set; }

  /// <summary>
  /// 第二种水面行走能力是否生效。
  /// </summary>
  public bool WaterWalk2 { get; internal set; }

  // C03 owns gravity and movement parameters; this component owns direction and control facts.
  /// <summary>
  /// 强制重力方向或控制值，沿用原版规则语义。
  /// </summary>
  public int ForcedGravity { get; internal set; }

  /// <summary>
  /// 第一种重力控制能力是否生效。
  /// </summary>
  public bool GravControl { get; internal set; }

  /// <summary>
  /// 第二种重力控制能力是否生效。
  /// </summary>
  public bool GravControl2 { get; internal set; }
}
