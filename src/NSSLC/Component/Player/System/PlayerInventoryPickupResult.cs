namespace Terraria.Player;

public readonly record struct PlayerInventoryPickupResult(
  bool Applied,
  int RemainingStack,
  PlayerInventoryCommitResult Inventory,
  bool CoinMergeAttempted,
  PlayerCoinMergeResult CoinMerge,
  bool EffectsApplied,
  PlayerInventoryPickupRejectionReason RejectionReason)
{
  public bool IsPartial =>
    Applied &&
    RemainingStack > 0;

  public static PlayerInventoryPickupResult Rejected(
    int remainingStack,
    PlayerInventoryCommitResult inventory,
    PlayerInventoryPickupRejectionReason rejectionReason)
  {
    return new PlayerInventoryPickupResult(
      Applied: false,
      RemainingStack: Math.Max(0, remainingStack),
      Inventory: inventory,
      CoinMergeAttempted: false,
      CoinMerge: default,
      EffectsApplied: false,
      RejectionReason: rejectionReason);
  }
}
