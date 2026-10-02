namespace Terraria.Player;

public enum PlayerCoinMergeRejectionReason : byte
{
  None,
  EmptyCommand,
  InvalidSourceSlot,
  ItemLookupFailed,
  SourceIsNotUpgradeableCoin,
  CommitPortRejected,
  DuplicateCommand,
  RecursionLimitExceeded,
}
