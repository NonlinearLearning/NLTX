namespace Terraria.Player;

// status: implemented-isolated-core
// source-members: P09-1402, P09-1403, P09-1404, P09-1405
// crossSubsystemOwner: shield command, combat resolution, and effect reset remain integration-review
/// <summary>
/// 保存玩家举盾、招架窗口和招架冷却。
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
/// </remarks>
public sealed class PlayerDefenseStateComponent
{
  public bool HasRaisableShield { get; internal set; }

  public bool ShieldRaised { get; internal set; }

  public int ShieldParryTimeLeft { get; internal set; }

  public int ShieldParryCooldown { get; internal set; }
}
