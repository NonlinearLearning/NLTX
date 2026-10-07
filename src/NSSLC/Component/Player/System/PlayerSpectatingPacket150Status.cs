namespace Terraria.Player;

public enum PlayerSpectatingPacket150Status : byte
{
  Applied,
  Ignored,
  RejectedUnsupportedMode,
  RejectedUnauthenticatedSender,
  RejectedInvalidPlayerSlot,
  RejectedInvalidTargetSlot,
  RejectedTargetIdentityMismatch,
}
