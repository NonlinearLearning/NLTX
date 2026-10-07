namespace Terraria.Player;

public enum PlayerSpawnPacket12Status : byte
{
  Applied,
  RejectedUnsupportedMode,
  RejectedUnauthenticatedSender,
  RejectedInvalidPlayerSlot,
  RejectedInvalidCoordinateRange,
  RejectedMismatchedCoordinates,
  RejectedInvalidDeathCount,
  RejectedInvalidTeam,
  RejectedInvalidSpawnContext,
  RejectedInvalidTimeSnapshot,
}
