namespace Terraria.Player.Mount;

/// <summary>
/// 保存坐骑疲劳、能力蓄力、持续时间和冷却。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Mount。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Mount.cs。</para>
/// <para>
/// 主要源成员：_fatigue（第 343 行）； _fatigueMax（第 345 行）； _abilityCooldown（第 351 行）； _abilityDuration（第
/// 353 行）； _aiming（第 357 行）； AbilityCharging（第 584 行）； AbilityActive（第 586 行）； AbilityCharge（第
/// 588 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P03-mount-vehicle-component-design.md。</para>
/// <para>依据位置：第 85 行。</para>
/// </remarks>
public sealed class MountFatigueAndAbilityStateComponent
{
  public MountFatigueAndAbilityStateComponent()
  {
    Fatigue = 0f;
    MaximumFatigue = 0f;
    IsAbilityCharging = false;
    AbilityCharge = 0;
    AbilityCooldownRemainingTicks = 0;
    AbilityDurationRemainingTicks = 0;
    IsAbilityActive = false;
    IsAimingAbility = false;
  }

  public float Fatigue { get; internal set; }

  public float MaximumFatigue { get; internal set; }

  public bool IsAbilityCharging { get; internal set; }

  public int AbilityCharge { get; internal set; }

  public int AbilityCooldownRemainingTicks { get; internal set; }

  public int AbilityDurationRemainingTicks { get; internal set; }

  public bool IsAbilityActive { get; internal set; }

  public bool IsAimingAbility { get; internal set; }

  internal void Reset()
  {
    Fatigue = 0f;
    MaximumFatigue = 0f;
    IsAbilityCharging = false;
    AbilityCharge = 0;
    AbilityCooldownRemainingTicks = 0;
    AbilityDurationRemainingTicks = 0;
    IsAbilityActive = false;
    IsAimingAbility = false;
  }
}
