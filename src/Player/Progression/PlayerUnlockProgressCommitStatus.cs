namespace Terraria.Player.Progression;

public enum PlayerUnlockProgressCommitStatus : byte
{
  Committed,
  AlreadyUnlocked,
  RejectedInvalidToken,
  RejectedUnknownProgression,
}
