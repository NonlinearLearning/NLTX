using MinimalImplementation.Contracts;

namespace MinimalImplementation.Server.Inventory.State;

public sealed record ChestLeaseResult(
  OwnerCommitStatus Status,
  ChestSession? Session,
  ProtocolErrorCode? ErrorCode,
  string? Detail)
{
  public static ChestLeaseResult Acquired(ChestSession session) =>
    new(OwnerCommitStatus.Applied, session, null, null);

  public static ChestLeaseResult Rejected(ProtocolErrorCode code, string detail) =>
    new(OwnerCommitStatus.Rejected, null, code, detail);

  public static ChestLeaseResult Stale(string detail) =>
    new(OwnerCommitStatus.Stale, null, ProtocolErrorCode.StaleEntity, detail);

  public static ChestLeaseResult Conflict(string detail) =>
    new(OwnerCommitStatus.Conflict, null, ProtocolErrorCode.Conflict, detail);

  public static ChestLeaseResult Unavailable(string detail) =>
    new(OwnerCommitStatus.Unavailable, null, ProtocolErrorCode.NotReady, detail);
}
