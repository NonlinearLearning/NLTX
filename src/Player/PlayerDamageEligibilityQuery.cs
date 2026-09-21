namespace Terraria.Player;

public static class PlayerDamageEligibilityQuery
{
  public static PlayerDamageEligibilityResult Evaluate(
    in PlayerDamageEligibilityInput input,
    PlayerDodgeAndImmunityStateComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);

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
      UsesShadowDodge: component.ShadowDodge,
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
