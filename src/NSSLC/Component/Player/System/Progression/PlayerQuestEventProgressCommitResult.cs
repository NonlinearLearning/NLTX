namespace Terraria.Player.Progression;

public readonly record struct PlayerQuestEventProgressCommitResult(
  PlayerQuestEventProgressCommitStatus Status)
{
  public bool IsCommitted => Status == PlayerQuestEventProgressCommitStatus.Committed;
}
