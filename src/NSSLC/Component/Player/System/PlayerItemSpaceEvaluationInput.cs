namespace Terraria.Player;

public readonly record struct PlayerItemSpaceEvaluationInput(
  PlayerItemSpaceCandidate Candidate,
  IReadOnlyList<PlayerItemSpaceSlotSnapshot> InventorySlots,
  IReadOnlyList<PlayerItemSpaceSlotSnapshot> VoidVaultSlots,
  bool IsVoidVaultEnabled,
  bool CanVoidVaultAccept);
