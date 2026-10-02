namespace Terraria.Player;

public readonly record struct PlayerEquipmentCommitResult(
  bool Applied,
  PlayerEquipmentSlotKind SlotKind,
  int SlotIndex,
  ItemEntityRef PreviousItem,
  ItemEntityRef CurrentItem,
  long Revision,
  bool EffectRebuildRequired,
  PlayerEquipmentCommitRejectionReason RejectionReason)
{
  public static PlayerEquipmentCommitResult Rejected(
    PlayerEquipmentSlotKind slotKind,
    int slotIndex,
    ItemEntityRef currentItem,
    long revision,
    PlayerEquipmentCommitRejectionReason rejectionReason)
  {
    return new PlayerEquipmentCommitResult(
      Applied: false,
      SlotKind: slotKind,
      SlotIndex: slotIndex,
      PreviousItem: currentItem,
      CurrentItem: currentItem,
      Revision: revision,
      EffectRebuildRequired: false,
      RejectionReason: rejectionReason);
  }
}
