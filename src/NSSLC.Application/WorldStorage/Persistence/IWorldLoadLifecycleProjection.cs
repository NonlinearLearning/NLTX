namespace Terraria.NonAuthoritative.Persistence;

/// <summary>Projects lifecycle-owned load flags to a host's compatibility surface.</summary>
/// <remarks>
/// When the load gate is being raised, the projection must publish it before any other fallible
/// work and leave it raised if a later projection step fails. When the gate is being released,
/// publish it only after the associated session and flags are visible to the host. A gate inherited
/// from an earlier incomplete reset remains raised when a new unpublished attempt fails; a new
/// successful publication or completed reset may release it after settling.
/// </remarks>
public interface IWorldLoadLifecycleProjection
{
  WorldStorageOperationResult Project(
    LoadedWorldSession session,
    bool isGeneratingOrLoadingWorld,
    bool loadFailed,
    bool worldBackup,
    bool worldCleared);

  /// <summary>
  /// Commits a published candidate after host finalization has succeeded. A host may stage the
  /// candidate as the provisional active session from <see cref="Project"/> while the load gate is
  /// raised, so legacy and session projections agree during settling. This method then validates
  /// and retires the previous session; if that commit fails, a later lifecycle projection must
  /// restore the previous session before the failed candidate is disposed. Hosts that do not own
  /// an active-session exchange can keep the default no-op implementation.
  /// </summary>
  WorldStorageOperationResult CommitPublishedSession(LoadedWorldSession session)
  {
    return WorldStorageOperationResult.Success;
  }
}
