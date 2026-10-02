namespace Terraria.Player;

public enum PlayerDefenseInteractionRejectionReason : byte
{
  None,
  EmptyCommand,
  DuplicateCommand,
  NoChange,
}
