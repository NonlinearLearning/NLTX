namespace Terraria.Combat;

public static class DamageEligibilityQuery
{
  public static DamageEligibility Evaluate(
    in DamageRequest request,
    in ImmunityComponent immunity,
    HitCooldownComponent cooldowns)
  {
    if (request.Amount <= 0)
    {
      return new DamageEligibility(false, DamageRejectionReason.NonPositiveAmount);
    }

    if (immunity.RemainingTicks > 0)
    {
      return new DamageEligibility(false, DamageRejectionReason.GeneralImmunity);
    }

    if (cooldowns.GetRemaining(request.Target) > 0)
    {
      return new DamageEligibility(false, DamageRejectionReason.SourceTargetCooldown);
    }

    return DamageEligibility.Eligible;
  }
}
