using MinimalImplementation.Contracts;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace MinimalImplementation.Server.Mechanisms;

/// <summary>
/// Handles the P06-owned module 7 request boundary without mutating player position.
/// </summary>
public sealed class TeleportPylonModuleHandler
{
  public const ushort ModuleId = 7;

  private readonly IMechanismTeleportPort _teleportPort;

  public TeleportPylonModuleHandler(IMechanismTeleportPort teleportPort)
  {
    _teleportPort = teleportPort ?? throw new ArgumentNullException(nameof(teleportPort));
  }

  public MechanismTeleportDispatchResult Handle(
    InboundPacketEnvelope envelope,
    EntityUuid authenticatedPlayer,
    byte authenticatedPlayerSlot,
    Packet82TeleportPylonData packet,
    IReadOnlyCollection<SessionEpoch> activeSessionEpochs)
  {
    ArgumentNullException.ThrowIfNull(envelope);
    ArgumentNullException.ThrowIfNull(packet);
    ArgumentNullException.ThrowIfNull(activeSessionEpochs);

    if (envelope.MessageId != 82 || envelope.ModuleId != ModuleId ||
        !string.Equals(envelope.Stage, "Active", StringComparison.Ordinal) ||
        !authenticatedPlayer.IsValid || authenticatedPlayerSlot == byte.MaxValue)
    {
      return MechanismTeleportDispatchResult.InvalidEnvelope;
    }

    if (!activeSessionEpochs.Contains(envelope.Epoch))
    {
      return MechanismTeleportDispatchResult.SessionUnavailable;
    }

    if (packet.Action is 0 or 1)
    {
      return MechanismTeleportDispatchResult.ServerAuthoredAction;
    }

    if (packet.Action != 2 || packet.X < 0 || packet.Y < 0 || packet.PylonType > 10)
    {
      return MechanismTeleportDispatchResult.InvalidPayload;
    }

    MechanismTeleportRequest request = new(
      envelope.Epoch,
      authenticatedPlayer,
      authenticatedPlayerSlot,
      packet.X,
      packet.Y,
      packet.PylonType);
    MechanismTeleportPortResult result = _teleportPort.Submit(request);
    return result.Status switch
    {
      MechanismTeleportPortStatus.Applied when result.CommitVersion is CommitVersion version =>
        MechanismTeleportDispatchResult.Applied(version),
      MechanismTeleportPortStatus.Applied => MechanismTeleportDispatchResult.Conflict(
        "The teleport port reported success without a committed version."),
      MechanismTeleportPortStatus.Rejected => MechanismTeleportDispatchResult.Rejected(
        result.Failure,
        result.Detail ?? "The pylon teleport request was rejected."),
      MechanismTeleportPortStatus.NotReady => MechanismTeleportDispatchResult.NotReady(
        result.Detail ?? "The teleport owner is not ready."),
      MechanismTeleportPortStatus.Conflict => MechanismTeleportDispatchResult.Conflict(
        result.Detail ?? "The teleport owner rejected the current revision."),
      _ => throw new ArgumentOutOfRangeException()
    };
  }
}

public enum MechanismTeleportDispatchStatus
{
  Applied,
  SessionUnavailable,
  InvalidEnvelope,
  InvalidPayload,
  ServerAuthoredAction,
  Rejected,
  NotReady,
  Conflict
}

public sealed record MechanismTeleportDispatchResult(
  MechanismTeleportDispatchStatus Status,
  CommitVersion? CommitVersion,
  PylonTeleportFailure? Failure,
  string? Detail)
{
  public static MechanismTeleportDispatchResult Applied(CommitVersion commitVersion) =>
    new(MechanismTeleportDispatchStatus.Applied, commitVersion, null, null);

  public static MechanismTeleportDispatchResult Rejected(
    PylonTeleportFailure? failure,
    string detail) =>
    new(MechanismTeleportDispatchStatus.Rejected, null, failure, detail);

  public static MechanismTeleportDispatchResult NotReady(string detail) =>
    new(MechanismTeleportDispatchStatus.NotReady, null, null, detail);

  public static MechanismTeleportDispatchResult Conflict(string detail) =>
    new(MechanismTeleportDispatchStatus.Conflict, null, null, detail);

  public static MechanismTeleportDispatchResult InvalidEnvelope { get; } = new(
    MechanismTeleportDispatchStatus.InvalidEnvelope,
    null,
    null,
    "The module 7 envelope is not an active authenticated request.");

  public static MechanismTeleportDispatchResult InvalidPayload { get; } = new(
    MechanismTeleportDispatchStatus.InvalidPayload,
    null,
    null,
    "The module 7 request must use action 2 and a valid Version4 pylon target.");

  public static MechanismTeleportDispatchResult ServerAuthoredAction { get; } = new(
    MechanismTeleportDispatchStatus.ServerAuthoredAction,
    null,
    null,
    "Module 7 actions 0 and 1 are server-authored projections, not client requests.");

  public static MechanismTeleportDispatchResult SessionUnavailable { get; } = new(
    MechanismTeleportDispatchStatus.SessionUnavailable,
    null,
    null,
    "The authenticated sender is not in the active session set.");
}
