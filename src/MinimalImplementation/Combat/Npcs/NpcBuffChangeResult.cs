using MinimalImplementation.Contracts;

namespace MinimalImplementation.Combat.Npcs;

public sealed record NpcBuffChangeResult(
  OwnerCommitResult Commit,
  bool Changed,
  bool ShouldSynchronize,
  NpcBuffView? Snapshot);
