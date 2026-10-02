namespace Terraria.Player;

public readonly record struct PlayerCoinMergeResult(
  bool Applied,
  int AppliedStepCount,
  int TerminalSlotIndex,
  PlayerCoinMergeRejectionReason RejectionReason)
{
  public static PlayerCoinMergeResult Rejected(
    PlayerCoinMergeRejectionReason rejectionReason,
    int terminalSlotIndex = -1)
  {
    return new PlayerCoinMergeResult(
      Applied: false,
      AppliedStepCount: 0,
      TerminalSlotIndex: terminalSlotIndex,
      RejectionReason: rejectionReason);
  }
}
