namespace Terraria.Player;

public readonly record struct PlayerContainerRelationResult(
  bool Applied,
  PlayerContainerRelationSlot Slot,
  PlayerContainerRef CurrentContainer,
  VoidVaultState CurrentVoidVaultState,
  PlayerContainerRelationRejectionReason RejectionReason)
{
  public static PlayerContainerRelationResult Rejected(
    PlayerContainerRelationSlot slot,
    PlayerContainerRef currentContainer,
    VoidVaultState currentVoidVaultState,
    PlayerContainerRelationRejectionReason reason)
  {
    return new PlayerContainerRelationResult(
      Applied: false,
      Slot: slot,
      CurrentContainer: currentContainer,
      CurrentVoidVaultState: currentVoidVaultState,
      RejectionReason: reason);
  }
}
