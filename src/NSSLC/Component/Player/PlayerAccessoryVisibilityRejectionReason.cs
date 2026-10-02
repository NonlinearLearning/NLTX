namespace Terraria.Player;

public enum PlayerAccessoryVisibilityRejectionReason : byte
{
  None,
  EmptyCommand,
  DuplicateCommand,
  InvalidState,
}
