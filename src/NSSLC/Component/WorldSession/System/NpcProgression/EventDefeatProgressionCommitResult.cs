using Terraria.WorldSession.NpcProgression.Events;

namespace Terraria.WorldSession.NpcProgression;

public readonly record struct EventDefeatProgressionCommitResult(
  NpcProgressionCommitStatus Status,
  EventDefeatProgressionKind EventKind)
{
  public bool IsCommitted => Status == NpcProgressionCommitStatus.Committed;

  public bool IsFirstClear => IsCommitted;
}
