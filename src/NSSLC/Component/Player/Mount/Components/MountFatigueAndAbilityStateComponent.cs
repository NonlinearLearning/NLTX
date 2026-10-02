namespace Terraria.Player.Mount;

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
