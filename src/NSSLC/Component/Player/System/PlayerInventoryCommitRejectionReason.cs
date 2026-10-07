namespace Terraria.Player;

public enum PlayerInventoryCommitRejectionReason : byte
{
  None,
  EmptyCommand,
  EmptyItem,
  InvalidItemStack,
  CandidateMismatch,
  ItemAlreadyInInventory,
  DuplicateCommand,
  UniqueItemAlreadyPresent,
  ItemLookupFailed,
  NoSpace,
  CommitPortRejected,
  PickupRequiresWorldItemAdapter,
}
