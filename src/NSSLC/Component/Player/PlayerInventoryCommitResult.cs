namespace Terraria.Player;

public readonly record struct PlayerInventoryCommitResult(
  bool Applied,
  PlayerInventoryCommitTarget Target,
  int AcceptedStack,
  int RemainingStack,
  PlayerInventoryCommitRejectionReason RejectionReason)
{
  public bool IsPartial =>
    Applied &&
    RemainingStack > 0;

  public static PlayerInventoryCommitResult Rejected(
    int remainingStack,
    PlayerInventoryCommitRejectionReason rejectionReason)
  {
    return new PlayerInventoryCommitResult(
      Applied: false,
      Target: default,
      AcceptedStack: 0,
      RemainingStack: Math.Max(0, remainingStack),
      RejectionReason: rejectionReason);
  }
}
