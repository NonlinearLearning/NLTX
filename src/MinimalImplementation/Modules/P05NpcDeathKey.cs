using MinimalImplementation.Contracts;

namespace MinimalImplementation.Server.Modules;

/// <summary>Identity of one committed death, adapted from P05 without retaining an NPC.</summary>
public readonly record struct P05NpcDeathKey(
    EntityUuid NpcId, long WorldScope, long Generation, CommitVersion SourceCommitVersion) {
  public bool IsValid => NpcId.IsValid && WorldScope > 0 && Generation > 0
      && SourceCommitVersion.IsValid;
}
