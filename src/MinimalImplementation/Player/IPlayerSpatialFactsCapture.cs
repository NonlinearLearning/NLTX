using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Identity;

namespace MinimalImplementation.Server.Player;

/// <summary>
/// A detached capture from authoritative collider/mount/stool/reach owners. Capture and Validate
/// are read-only. SourceRevision and Validate must cover every dependency, including equipment,
/// mount size delegates and runtime reach boosts. Missing facts return no capture.
/// </summary>
public interface IPlayerSpatialFactsCapture {
  SessionEpoch Epoch { get; }
  EntityHandle PlayerHandle { get; }
  CommitVersion PlayerVersion { get; }
  PlayerSourceRevision SourceRevision { get; }
  PlayerSpatialSourceFacts Facts { get; }
  OwnerCommitResult Validate();
}
