namespace Terraria.Player;

public sealed class PlayerUseComponent
{
  public int AnimationRemainingTicks { get; set; }

  public int AnimationDurationTicks { get; set; }

  public int UseRemainingTicks { get; set; }

  public int UseDurationTicks { get; set; }

  public int ToolTime { get; set; }

  public int ReuseDelayRemainingTicks { get; set; }

  public bool HasPendingReuse { get; set; }

  public bool IsChanneling { get; set; }

  public bool IsUseDelayed { get; set; }

  // Compatibility projection only; it is not a Projectile entity identity.
  public LegacyProjectileSlot? HeldProjectile { get; set; }

  public bool LastUseAttemptSucceeded { get; set; }
}
