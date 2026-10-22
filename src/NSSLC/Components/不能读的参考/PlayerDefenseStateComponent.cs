namespace Terraria.Player;

// status: implemented-isolated-core
// source-members: P09-1402, P09-1403, P09-1404, P09-1405
// crossSubsystemOwner: shield command, combat resolution, and effect reset remain integration-review
/// <summary>
/// 保存玩家可举盾能力、举盾状态和盾牌招架计时。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>
/// 主要源成员：hasRaisableShield（第 2461 行）； shieldRaised（第 2463 行）； shieldParryTimeLeft（第 2465 行）；
/// shield_parry_cooldown（第 2467 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-P09-player-inventory-equipment-component-design.md。
/// </para>
/// <para>依据位置：第 14 行。</para>
/// <para>
/// 用途：盾牌操作流程登记是否举盾及招架窗口，战斗流程读取窗口和冷却决定本次防御效果。
/// </para>
/// <para>
/// 当前实现：只有内部可写属性。输入响应、招架判定、计时推进和效果重置由外部盾牌与战斗流程负责。
/// </para>
/// <para>
/// 原始分级记录（2026-10-08）：C120，L2（通用可选玩法）。分级理由：穿甲、减伤、暴击、武器倍率、攻速、招架及装备闪避是基础扣血之外的规则。
/// </para>
/// </remarks>
public sealed class PlayerDefenseStateComponent
{
  /// <summary>
  /// 是否具备可举起的盾牌。
  /// </summary>
  public bool HasRaisableShield { get; internal set; }

  /// <summary>
  /// 盾牌当前是否处于举起状态。
  /// </summary>
  public bool ShieldRaised { get; internal set; }

  /// <summary>
  /// 盾牌招架窗口的剩余计时。
  /// </summary>
  public int ShieldParryTimeLeft { get; internal set; }

  /// <summary>
  /// 盾牌招架再次可用前的冷却计时。
  /// </summary>
  public int ShieldParryCooldown { get; internal set; }
}
