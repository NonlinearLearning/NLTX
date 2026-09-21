namespace Terraria.Player;

public enum PlayerInputRejectionReason : byte
{
  None,
  InvalidPlayerSlot,
  UnknownPlayer,
  InactivePlayer,
  DuplicatePlayer,
  InvalidFacing,
}
