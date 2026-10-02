using Terraria.WorldSession.NpcProgression.Boss;

namespace Terraria.WorldSession.NpcProgression;

public readonly record struct BossDefeatProgressionCommitResult(
  NpcProgressionCommitStatus Status,
  BossDefeatProgressionKind BossKind)
{
  public bool IsCommitted => Status == NpcProgressionCommitStatus.Committed;

  public bool IsFirstClear => IsCommitted;
}
