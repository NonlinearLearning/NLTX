namespace MinimalImplementation.Server.Player;

public readonly record struct PlayerImmunities(int GeneralTicks, int BrainDodgeAnimationTicks)
{
  public static PlayerImmunities Empty => new(0, 0);

  public PlayerHurtCooldowns HurtCooldowns { get; init; }

  public int ShadowDodgeCooldownTicks { get; init; }

  public PlayerImmunities AdvanceTick()
  {
    return new PlayerImmunities(
      GeneralTicks > 0 ? GeneralTicks - 1 : 0,
      BrainDodgeAnimationTicks > 0 ? BrainDodgeAnimationTicks - 1 : 0) {
      HurtCooldowns = HurtCooldowns.AdvanceTick(),
      ShadowDodgeCooldownTicks = ShadowDodgeCooldownTicks > 0
        ? ShadowDodgeCooldownTicks - 1 : 0
    };
  }

  public PlayerImmunities ApplyNinjaDodge(int longInvincibilityTicks)
  {
    return SetImmuneTimeForAllTypes(longInvincibilityTicks);
  }

  public PlayerImmunities ApplyShadowDodge(int longInvincibilityTicks)
  {
    return SetImmuneTimeForAllTypes(longInvincibilityTicks);
  }

  public PlayerImmunities ApplyBrainDodge()
  {
    return ApplyBrainDodge(80);
  }

  public PlayerImmunities ApplyBrainDodge(int longInvincibilityTicks) {
    return SetImmuneTimeForAllTypes(longInvincibilityTicks) with {
      BrainDodgeAnimationTicks = 300
    };
  }

  private PlayerImmunities SetImmuneTimeForAllTypes(int ticks) {
    System.ArgumentOutOfRangeException.ThrowIfNegative(ticks);
    return this with {
      GeneralTicks = ticks,
      HurtCooldowns = PlayerHurtCooldowns.ForAllTypes(ticks)
    };
  }
}
