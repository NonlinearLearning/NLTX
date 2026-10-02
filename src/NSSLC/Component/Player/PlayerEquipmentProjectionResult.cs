namespace Terraria.Player;

public readonly record struct PlayerEquipmentProjectionResult(
  bool Applied,
  long EquipmentRevision,
  int MissingItemMetadataCount,
  PlayerEquipmentProjectionRejectionReason RejectionReason);
