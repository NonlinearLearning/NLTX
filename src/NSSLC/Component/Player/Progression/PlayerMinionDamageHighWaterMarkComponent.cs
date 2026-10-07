namespace Terraria.Player.Progression;

// status: locally-verified-core; integration-blocked; parity-not-run
// source-members: highestStormTigerGemOriginalDamage, highestAbigailCounterOriginalDamage
/// <summary>
/// 保存玩家特定召唤物已记录的最高原始伤害。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>
/// 主要源成员：highestStormTigerGemOriginalDamage（第 874 行）； highestAbigailCounterOriginalDamage（第 884
/// 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-P05-player-progression-pets-minions-component-design.md。
/// </para>
/// <para>依据位置：第 216 行。</para>
/// </remarks>
public sealed class PlayerMinionDamageHighWaterMarkComponent
{
  public int HighestStormTigerGemOriginalDamage { get; internal set; }

  public int HighestAbigailCounterOriginalDamage { get; internal set; }
}
