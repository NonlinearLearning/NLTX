using MinimalImplementation.Contracts;

namespace MinimalImplementation.Combat.Npcs;

/// <summary>
/// Read-only motion and tile queries over one captured world snapshot. All NPC AI families
/// in a batch share this exact instance and its epoch/version. The adapter must not replace its
/// captured snapshot or mutate NPC/World owners while a query is in progress.
/// </summary>
public interface INpcTickMotionSnapshot : INpcMotionQuery, INpcFighterMotionQuery {
  ulong WorldEpoch { get; }

  CommitVersion WorldCommitVersion { get; }
}
