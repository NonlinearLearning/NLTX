namespace Terraria.Items.InventoryContainers;

public sealed class EmergencyStackingMutationResult
{
  private EmergencyStackingMutationResult(bool committed, bool rolledBack, string? failureReason)
  {
    Committed = committed;
    RolledBack = rolledBack;
    FailureReason = failureReason;
  }

  public bool Committed { get; }

  public bool RolledBack { get; }

  public string? FailureReason { get; }

  public static EmergencyStackingMutationResult Accepted()
  {
    return new EmergencyStackingMutationResult(true, false, null);
  }

  public static EmergencyStackingMutationResult Rejected(string reason, bool rolledBack = false)
  {
    if (string.IsNullOrWhiteSpace(reason))
    {
      throw new ArgumentException("A mutation rejection requires a reason.", nameof(reason));
    }

    return new EmergencyStackingMutationResult(false, rolledBack, reason);
  }
}
