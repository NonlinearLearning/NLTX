namespace Terraria.Player;

public enum PlayerLoadoutSwitchRejectionReason
{
  None,
  EmptyCommand,
  DuplicateCommand,
  SwitchingBlocked,
  InvalidTargetIndex,
  SameLoadout,
  InvalidCurrentLoadout,
  InvalidLoadoutState,
}
