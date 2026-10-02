namespace Terraria.Player.Mount;

public enum MountAdapterCommitStatus
{
  Accepted = 0,
  Rejected = 1,
  Unknown = 2,
}

public readonly record struct MountAdapterCommitResult(
  MountAdapterCommitStatus Status,
  bool Changed,
  string? FailureReason)
{
  public bool Succeeded => Status == MountAdapterCommitStatus.Accepted;

  public static MountAdapterCommitResult Accepted(bool changed)
  {
    return new MountAdapterCommitResult(
      MountAdapterCommitStatus.Accepted,
      changed,
      null);
  }

  public static MountAdapterCommitResult Rejected(string reason)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(reason);
    return new MountAdapterCommitResult(
      MountAdapterCommitStatus.Rejected,
      false,
      reason);
  }

  public static MountAdapterCommitResult Unknown(string reason)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(reason);
    return new MountAdapterCommitResult(
      MountAdapterCommitStatus.Unknown,
      false,
      reason);
  }
}
