namespace Terraria.Player;

public enum PlayerRestPacket13Status : byte
{
  Applied,
  IgnoredSelfEcho,
  RejectedUnsupportedMode,
  RejectedUnauthenticatedSender,
  RejectedInvalidSenderSlot,
}
