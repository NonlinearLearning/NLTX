namespace Terraria.Player.Progression;

public readonly record struct PlayerProgressionCommitResult(
  PlayerProgressionCommitStatus Status,
  PlayerConsumedProgressionUpgrade Upgrade)
{
  public bool IsCommitted => Status == PlayerProgressionCommitStatus.Committed;
}
