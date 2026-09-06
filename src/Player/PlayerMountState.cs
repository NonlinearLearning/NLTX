namespace Terraria.Player;

public sealed class PlayerMountState
{
  public ContentId<MountDefinition>? MountType { get; set; }

  public bool IsActive { get; set; }

  public int FlightRemainingTicks { get; set; }

  public float Fatigue { get; set; }

  public float MaximumFatigue { get; set; }

  public int AbilityCharge { get; set; }

  public int AbilityCooldownRemainingTicks { get; set; }

  public int AbilityDurationRemainingTicks { get; set; }

  public bool IsAbilityCharging { get; set; }

  public bool IsAbilityActive { get; set; }

  public bool IsAimingAbility { get; set; }

  public bool IsDismountLocked { get; set; }

  public bool IsDismountRequested { get; set; }

  public int WalkingGraceRemainingTicks { get; set; }

  public bool UsesSuperCartRules { get; set; }

  public bool IsMounted => IsActive && MountType.HasValue;

  public bool CanUseAbility => IsMounted && AbilityCooldownRemainingTicks == 0;

  public bool IsMinecart { get; internal set; }

  public bool DismountsOnItemUse { get; internal set; }
}
