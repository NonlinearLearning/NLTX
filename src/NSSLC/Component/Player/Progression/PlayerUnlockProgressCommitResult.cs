namespace Terraria.Player.Progression;

public readonly record struct PlayerUnlockProgressCommitResult(
  PlayerUnlockProgressCommitStatus Status,
  PlayerUnlockProgressionKind Progression)
{
  public bool IsCommitted => Status == PlayerUnlockProgressCommitStatus.Committed;
}
