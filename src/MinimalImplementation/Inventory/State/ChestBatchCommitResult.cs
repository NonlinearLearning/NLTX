using MinimalImplementation.Contracts;

namespace MinimalImplementation.Server.Inventory.State;

public sealed record ChestBatchCommitResult(
  OwnerCommitStatus Status,
  IReadOnlyList<ChestSnapshot> Chests,
  ProtocolErrorCode? ErrorCode,
  string? Detail)
{
  public bool IsApplied => Status == OwnerCommitStatus.Applied;

  internal static ChestBatchCommitResult Applied(IReadOnlyCollection<ChestSnapshot> chests) =>
    new(OwnerCommitStatus.Applied, Array.AsReadOnly(chests.ToArray()), null, null);

  internal static ChestBatchCommitResult From(OwnerCommitResult result) =>
    new(result.Status, Array.Empty<ChestSnapshot>(), result.ErrorCode, result.Detail);
}
