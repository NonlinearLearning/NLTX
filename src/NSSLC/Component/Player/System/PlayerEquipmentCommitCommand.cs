namespace Terraria.Player;

public readonly record struct PlayerEquipmentCommitCommand(
  Guid CommandId,
  PlayerEquipmentSlotKind SlotKind,
  int SlotIndex,
  ItemEntityRef Item,
  ItemEntityRef? ExpectedCurrentItem = null,
  long ExpectedRevision = -1);
