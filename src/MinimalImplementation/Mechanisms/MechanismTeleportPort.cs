using MinimalImplementation.Contracts;

namespace MinimalImplementation.Server.Mechanisms;

/// <summary>
/// Describes a teleport request without owning player position or transport state.
/// </summary>
public sealed record MechanismTeleportRequest(
  SessionEpoch Epoch,
  EntityUuid Player,
  byte PlayerSlot,
  short TargetX,
  short TargetY,
  byte PylonType);

public enum MechanismTeleportPortStatus
{
  Applied,
  Rejected,
  NotReady,
  Conflict
}

public sealed record MechanismTeleportPortResult(
  MechanismTeleportPortStatus Status,
  CommitVersion? CommitVersion,
  PylonTeleportFailure? Failure,
  string? Detail)
{
  public static MechanismTeleportPortResult Applied(CommitVersion commitVersion) =>
    new(MechanismTeleportPortStatus.Applied, commitVersion, null, null);

  public static MechanismTeleportPortResult Rejected(
    PylonTeleportFailure failure,
    string detail) =>
    new(MechanismTeleportPortStatus.Rejected, null, failure, detail);

  public static MechanismTeleportPortResult NotReady(string detail) =>
    new(MechanismTeleportPortStatus.NotReady, null, null, detail);

  public static MechanismTeleportPortResult Conflict(string detail) =>
    new(MechanismTeleportPortStatus.Conflict, null, null, detail);
}

/// <summary>
/// Explicit P02/P03 boundary for a pylon teleport. Implementations own player movement commit.
/// </summary>
public interface IMechanismTeleportPort
{
  MechanismTeleportPortResult Submit(MechanismTeleportRequest request);
}
