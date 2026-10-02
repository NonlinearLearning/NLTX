namespace Terraria.Player;

public enum PlayerEquipmentCommitRejectionReason : byte
{
  None,
  EmptyCommand,
  InvalidSlotKind,
  InvalidSlotIndex,
  InvalidExpectedRevision,
  StaleExpectedRevision,
  UnexpectedCurrentItem,
  ItemAlreadyEquipped,
  NoChange,
  DuplicateCommand,
}
