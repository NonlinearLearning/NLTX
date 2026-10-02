namespace Terraria.Items.InventoryContainers;

public sealed class TransferCommitResult
{
  private TransferCommitResult(bool committed, bool duplicate, string? failureReason)
  {
    Committed = committed;
    Duplicate = duplicate;
    FailureReason = failureReason;
  }

  public bool Committed { get; }

  public bool Duplicate { get; }

  public string? FailureReason { get; }

  public static TransferCommitResult Accepted()
  {
    return new TransferCommitResult(committed: true, duplicate: false, failureReason: null);
  }

  public static TransferCommitResult AlreadyCommitted()
  {
    return new TransferCommitResult(committed: false, duplicate: true, failureReason: null);
  }

  public static TransferCommitResult Rejected(string failureReason)
  {
    if (string.IsNullOrWhiteSpace(failureReason))
    {
      throw new ArgumentException("A transfer rejection requires a reason.", nameof(failureReason));
    }

    return new TransferCommitResult(committed: false, duplicate: false, failureReason);
  }
}
