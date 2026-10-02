namespace Terraria.Player.Progression;

public enum PlayerQuestEventProgressCommitStatus : byte
{
  Committed,
  AlreadyApplied,
  RejectedInvalidToken,
  RejectedInvalidScore,
}
