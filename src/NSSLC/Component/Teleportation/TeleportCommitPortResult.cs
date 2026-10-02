namespace Terraria.Teleportation;

public readonly record struct TeleportCommitPortResult(
  TeleportCommitPortStatus Status,
  string? FailureReason)
{
  public static TeleportCommitPortResult Accepted =>
    new(TeleportCommitPortStatus.Accepted, null);

  public static TeleportCommitPortResult Rejected(string reason)
  {
    return new(TeleportCommitPortStatus.Rejected, reason);
  }

  public static TeleportCommitPortResult Unknown(string reason)
  {
    return new(TeleportCommitPortStatus.Unknown, reason);
  }
}
