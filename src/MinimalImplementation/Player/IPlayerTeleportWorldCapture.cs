using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Identity;

namespace MinimalImplementation.Server.Player;

/// <summary>
/// Read-only facts from the currently loaded persistence document and matching published world.
/// DualDungeonsSeed must come from the present world.spawn-points section, never its Empty
/// fallback. Missing document/section returns no capture. Validate guards the document-to-world
/// binding as well as its epoch/version; it performs no publication, loading or other writes.
/// </summary>
public interface IPlayerTeleportWorldCapture {
  WorldScopeId WorldScope { get; }
  ulong WorldEpoch { get; }
  CommitVersion WorldVersion { get; }
  bool DualDungeonsSeed { get; }
  OwnerCommitResult Validate();
}
