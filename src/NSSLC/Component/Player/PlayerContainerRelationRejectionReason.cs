namespace Terraria.Player;

public enum PlayerContainerRelationRejectionReason : byte
{
  None,
  EmptyCommand,
  DuplicateCommand,
  InvalidSlot,
  EmptyContainer,
  InvalidVoidVaultState,
}
