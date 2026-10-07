namespace Terraria.Player.Armor;

// status: implemented-isolated-core
// crossSubsystemOwner: integration-review for Buff, Combat, Spatial, network, and persistence
/// <summary>
/// 保存日耀盾数量、恢复计数和冲刺消耗状态。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>
/// 主要源成员：solarShields（第 577 行）； solarCounter（第 579 行）； solarDashing（第 586 行）；
/// solarDashConsumedFlare（第 588 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P07-player-mobility-component-design.md。</para>
/// <para>依据位置：第 273 行。</para>
/// </remarks>
public sealed class PlayerSolarArmorStateComponent
{
  public int ShieldCount { get; set; }

  public int SolarCounter { get; set; }

  public bool IsSolarDashing { get; set; }

  public bool SolarDashConsumedFlare { get; set; }
}
