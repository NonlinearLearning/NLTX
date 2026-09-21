namespace Terraria.Items.InventoryContainers;

public sealed class QuickStackCommitResult
{
  private QuickStackCommitResult(bool committed, string? failureReason)
  {
    Committed = committed;
    FailureReason = failureReason;
  }

  public bool Committed { get; }

  public string? FailureReason { get; }

  public static QuickStackCommitResult Accepted()
  {
    return new QuickStackCommitResult(true, null);
  }

  public static QuickStackCommitResult Rejected(string reason)
  {
    return new QuickStackCommitResult(false, reason);
  }
}
