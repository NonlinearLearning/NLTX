namespace Terraria.WorldSession.NpcProgression.Invasion;

public readonly record struct InvasionWaveProgressCommitResult(
  InvasionWaveProgressCommitStatus Status,
  InvasionWaveProgressSyncView View)
{
  public bool IsCommitted => Status == InvasionWaveProgressCommitStatus.Committed;
}
