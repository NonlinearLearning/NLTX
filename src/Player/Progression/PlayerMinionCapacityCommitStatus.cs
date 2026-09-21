namespace Terraria.Player.Progression;

public enum PlayerMinionCapacityCommitStatus : byte
{
  Committed,
  AlreadyApplied,
  RejectedInvalidCommand,
  RejectedInvalidDelta,
  RejectedCapacityExceeded,
}
