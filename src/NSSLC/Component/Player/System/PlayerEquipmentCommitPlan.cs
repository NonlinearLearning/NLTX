namespace Terraria.Player;

public readonly record struct PlayerEquipmentCommitPlan(
  Guid CommandId,
  PlayerEquipmentSlotKind SlotKind,
  int SlotIndex,
  ItemEntityRef PreviousItem,
  ItemEntityRef NextItem,
  long RevisionBefore,
  long RevisionAfter,
  bool EffectRebuildRequired);
