namespace Terraria.Player;

public static class PlayerDamageEligibilityQuery
{
  public static PlayerDamageEligibilityResult Evaluate(
    in PlayerDamageEligibilityInput input,
    PlayerDodgeAndImmunityStateComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);
    return Evaluate(in input, component.ShadowDodge);
  }

  public static PlayerDamageEligibilityResult Evaluate(
    in PlayerDamageEligibilityInput input,
    bool shadowDodgeActive)
  {

    if (input.SourceId == Guid.Empty)
    {
      return Rejected(PlayerDamageEligibilityRejectionReason.EmptySource);
    }

    if (input.DamageAmount <= 0)
    {
      return Rejected(PlayerDamageEligibilityRejectionReason.NonPositiveDamage);
    }

    if (input.HasGeneralImmunity)
    {
      return Rejected(PlayerDamageEligibilityRejectionReason.GeneralImmunity);
    }

    if (input.SourceCooldownActive)
    {
      return Rejected(PlayerDamageEligibilityRejectionReason.SourceCooldown);
    }

    return new PlayerDamageEligibilityResult(
      IsEligible: true,
      UsesShadowDodge: input.Dodgeable && shadowDodgeActive,
      RejectionReason: PlayerDamageEligibilityRejectionReason.None);
  }

  private static PlayerDamageEligibilityResult Rejected(
    PlayerDamageEligibilityRejectionReason reason)
  {
    return new PlayerDamageEligibilityResult(
      IsEligible: false,
      UsesShadowDodge: false,
      RejectionReason: reason);
  }
}
