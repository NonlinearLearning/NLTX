namespace Terraria.Combat;

public static class DamageResolutionSystem
{
  public static DamageResult Resolve(
    in DamageRequest request,
    ref HealthComponent health,
    ref ImmunityComponent immunity,
    HitCooldownComponent cooldowns,
    DamageContributionComponent contributions)
  {
    DamageEligibility eligibility = DamageEligibilityQuery.Evaluate(
      request,
      immunity,
      cooldowns);
    if (!eligibility.IsEligible)
    {
      return new DamageResult(
        false,
        eligibility.RejectionReason,
        0,
        false,
        request.Attribution);
    }

    int finalDamage = Math.Max(1, request.Amount - request.Defense);
    if (request.Critical)
    {
      finalDamage *= 2;
    }

    health.Current = Math.Max(0, health.Current - finalDamage);
    cooldowns.Arm(request.Target, request.CooldownTicks);
    contributions.AddDamage(request.Attribution.Source, finalDamage);

    return new DamageResult(
      true,
      DamageRejectionReason.None,
      finalDamage,
      health.Current == 0,
      request.Attribution);
  }
}
