namespace Terraria.Combat;

public readonly record struct DamageEligibility(
  bool IsEligible,
  DamageRejectionReason RejectionReason)
{
  public static DamageEligibility Eligible => new(true, DamageRejectionReason.None);
}
