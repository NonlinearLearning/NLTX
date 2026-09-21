namespace Terraria.Player.Progression;

public enum PlayerProgressionCommitStatus : byte
{
  Committed,
  AlreadyConsumed,
  RejectedInvalidToken,
  RejectedUnknownUpgrade,
}
