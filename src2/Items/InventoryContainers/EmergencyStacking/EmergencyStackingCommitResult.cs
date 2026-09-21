namespace Terraria.Items.InventoryContainers;

public sealed class EmergencyStackingCommitResult
{
  private EmergencyStackingCommitResult(
    bool committed,
    bool duplicate,
    bool rolledBack,
    int amount,
    string? failureReason)
  {
    Committed = committed;
    Duplicate = duplicate;
    RolledBack = rolledBack;
    Amount = amount;
    FailureReason = failureReason;
  }

  public bool Committed { get; }

  public bool Duplicate { get; }

  public bool RolledBack { get; }

  public int Amount { get; }

  public string? FailureReason { get; }

  public static EmergencyStackingCommitResult Accepted(int amount)
  {
    return new EmergencyStackingCommitResult(true, false, false, amount, null);
  }

  public static EmergencyStackingCommitResult AlreadyCommitted()
  {
    return new EmergencyStackingCommitResult(false, true, false, 0, null);
  }

  public static EmergencyStackingCommitResult Rejected(
    string reason,
    bool rolledBack = false,
    int amount = 0)
  {
    if (string.IsNullOrWhiteSpace(reason))
    {
      throw new ArgumentException("A commit rejection requires a reason.", nameof(reason));
    }

    return new EmergencyStackingCommitResult(false, false, rolledBack, amount, reason);
  }
}
