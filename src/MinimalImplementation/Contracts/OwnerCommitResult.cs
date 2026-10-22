namespace MinimalImplementation.Contracts;

public sealed record OwnerCommitResult(
  OwnerCommitStatus Status,
  CommitVersion? Version,
  ProtocolErrorCode? ErrorCode,
  string? Detail)
{
  public static OwnerCommitResult Applied(CommitVersion version) =>
    new(OwnerCommitStatus.Applied, version, null, null);

  public static OwnerCommitResult Rejected(ProtocolErrorCode code, string detail) =>
    new(OwnerCommitStatus.Rejected, null, code, detail);
}
