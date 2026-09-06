namespace Terraria.Player;

public sealed class PlayerItemUseState
{
  public int AnimationRemainingTicks { get; set; }

  public int AnimationDurationTicks { get; set; }

  public int UseRemainingTicks { get; set; }

  public int UseDurationTicks { get; set; }

  public int ReuseDelayRemainingTicks { get; set; }

  public bool IsChanneling { get; set; }

  public bool HasPendingReuse { get; set; }

  public LegacyProjectileSlot? HeldProjectileSlot { get; set; }

  public ItemUseMode AlternateUseMode { get; set; }

  public bool IsUseDelayed { get; set; }

  public bool LastUseAttemptSucceeded { get; set; }

  public bool IsUsingItem => AnimationRemainingTicks > 0 || UseRemainingTicks > 0;

  public bool IsReadyForUse => !IsUsingItem && ReuseDelayRemainingTicks <= 0;
}
