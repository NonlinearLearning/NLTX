namespace Terraria.Teleportation;

public readonly record struct TeleportEligibility(
  bool IsEligible,
  TeleportCommitReason Reason)
{
  public static TeleportEligibility Eligible =>
    new(true, TeleportCommitReason.None);

  public static TeleportEligibility Rejected(TeleportCommitReason reason)
  {
    return new(false, reason);
  }
}
