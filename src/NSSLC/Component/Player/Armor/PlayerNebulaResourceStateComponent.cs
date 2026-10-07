namespace Terraria.Player.Armor;

// status: implemented-isolated-core
// crossSubsystemOwner: integration-review for Resource, Combat, Buff, network, and persistence
/// <summary>
/// 保存星云套装的生命、魔力、伤害等级和魔力计数。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>
/// 主要源成员：nebulaLevelLife（第 590 行）； nebulaLevelMana（第 592 行）； nebulaManaCounter（第 594 行）；
/// nebulaLevelDamage（第 596 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P07-player-mobility-component-design.md。</para>
/// <para>依据位置：第 279 行。</para>
/// </remarks>
public sealed class PlayerNebulaResourceStateComponent
{
  public int LifeLevel { get; set; }

  public int ManaLevel { get; set; }

  public int NebulaManaCounter { get; set; }

  public int DamageLevel { get; set; }
}
