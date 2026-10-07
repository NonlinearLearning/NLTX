namespace Terraria.Player.Combat;

// status: implemented-partial
// componentId: PLAYER.COMP.ARMOR_SET_AND_TURRET_STATE
// source-members: P08-1284..P08-1299, P08-1302
// excluded-members: P08-1300..P08-1301 (existing PlayerAbilityComponent owner)
// crossSubsystemOwner: integration-review
/// <summary>
/// 保存套装激活、炮塔加成和套装冷却状态。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>
/// 主要源成员：setSolar（第 2212 行）； setVortex（第 2214 行）； setNebula（第 2216 行）； nebulaCD（第 2218 行）；
/// setStardust（第 2220 行）； setForbidden（第 2222 行）； setForbiddenCooldownLocked（第 2224 行）；
/// setChlorophyte（第 2226 行）； setSquireT3（第 2228 行）； setHuntressT3（第 2230 行）； setApprenticeT3（第
/// 2232 行）； setMonkT3（第 2234 行）； setSquireT2（第 2236 行）； setHuntressT2（第 2238 行）；
/// setApprenticeT2（第 2240 行）； setMonkT2（第 2242 行）； vortexStealthActive（第 2248 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P08-player-environment-armor-component-design.md。</para>
/// <para>依据位置：第 91 行。</para>
/// </remarks>
public sealed class PlayerArmorSetAndTurretStateComponent
{
  public bool SetSolar { get; internal set; }

  public bool SetVortex { get; internal set; }

  public bool SetNebula { get; internal set; }

  public int NebulaCooldown { get; internal set; }

  public bool SetStardust { get; internal set; }

  public bool SetForbidden { get; internal set; }

  public bool SetForbiddenCooldownLocked { get; internal set; }

  public bool SetChlorophyte { get; internal set; }

  public bool SetSquireTierThree { get; internal set; }

  public bool SetHuntressTierThree { get; internal set; }

  public bool SetApprenticeTierThree { get; internal set; }

  public bool SetMonkTierThree { get; internal set; }

  public bool SetSquireTierTwo { get; internal set; }

  public bool SetHuntressTierTwo { get; internal set; }

  public bool SetApprenticeTierTwo { get; internal set; }

  public bool SetMonkTierTwo { get; internal set; }

  public bool VortexStealthActive { get; internal set; }
}
