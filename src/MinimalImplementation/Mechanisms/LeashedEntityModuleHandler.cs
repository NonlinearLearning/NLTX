using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Protocol;
using LeashedEntityMessage =
  MinimalImplementation.Server.Protocol.Version4ModulePayloads.LeashedEntityMessage;

namespace MinimalImplementation.Server.Mechanisms;

/// <summary>
/// Applies module 13 actions against the P06 registry and returns a broadcastable change.
/// </summary>
public sealed class LeashedEntityModuleHandler
{
  public const ushort ModuleId = 13;

  private readonly ILeashedEntityCommitPort _commitPort;
  private readonly long _worldScope;

  public LeashedEntityModuleHandler(ILeashedEntityCommitPort commitPort, long worldScope)
  {
    _commitPort = commitPort ?? throw new ArgumentNullException(nameof(commitPort));
    if (worldScope <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(worldScope));
    }
    _worldScope = worldScope;
  }

  public LeashedEntityModuleDispatchResult Handle(
    InboundPacketEnvelope envelope,
    byte authenticatedPlayerSlot,
    LeashedEntityMessage message,
    MechanismSectionCoordinate section,
    IReadOnlyCollection<SessionEpoch> activeSessionEpochs,
    CommitVersion commitVersion)
  {
    ArgumentNullException.ThrowIfNull(envelope);
    ArgumentNullException.ThrowIfNull(message);
    ArgumentNullException.ThrowIfNull(activeSessionEpochs);

    if (envelope.MessageId != 82 || envelope.ModuleId != ModuleId ||
        !string.Equals(envelope.Stage, "Active", StringComparison.Ordinal) ||
        authenticatedPlayerSlot == byte.MaxValue || !commitVersion.IsValid ||
        envelope.WorldScope != _worldScope)
    {
      return LeashedEntityModuleDispatchResult.InvalidEnvelope;
    }

    if (!activeSessionEpochs.Contains(envelope.Epoch))
    {
      return LeashedEntityModuleDispatchResult.SessionUnavailable;
    }

    if (envelope.ExpectedRevision is long expectedRevision &&
        expectedRevision != _commitPort.CurrentRevision)
    {
      return LeashedEntityModuleDispatchResult.StaleRevision(_commitPort.CurrentRevision);
    }

    try
    {
      LeashedEntityCommitResult commit = _commitPort.Commit(message, section, commitVersion);
      LeashedEntityChangeResult change = commit.Change;
      SessionEpoch[] recipients = change.Kind == LeashedEntityChangeKind.NoOp
        ? Array.Empty<SessionEpoch>()
        : activeSessionEpochs.Distinct().ToArray();
      return new LeashedEntityModuleDispatchResult(
        change.Kind == LeashedEntityChangeKind.NoOp
          ? LeashedEntityModuleDispatchStatus.NoOp
          : LeashedEntityModuleDispatchStatus.Applied,
        commitVersion,
        commit.RegistryRevision,
        change,
        recipients,
        null);
    }
    catch (ArgumentException exception)
    {
      return LeashedEntityModuleDispatchResult.Rejected(
        LeashedEntityModuleDispatchStatus.InvalidPayload,
        exception.Message);
    }
    catch (InvalidOperationException exception)
    {
      return LeashedEntityModuleDispatchResult.Rejected(
        LeashedEntityModuleDispatchStatus.Conflict,
        exception.Message);
    }
  }
}

public enum LeashedEntityModuleDispatchStatus
{
  Applied,
  NoOp,
  SessionUnavailable,
  InvalidEnvelope,
  InvalidPayload,
  StaleRevision,
  Conflict
}

public sealed record LeashedEntityModuleDispatchResult(
  LeashedEntityModuleDispatchStatus Status,
  CommitVersion? CommitVersion,
  long? RegistryRevision,
  LeashedEntityChangeResult? Change,
  IReadOnlyList<SessionEpoch> Recipients,
  string? Detail)
{
  public static LeashedEntityModuleDispatchResult InvalidEnvelope { get; } = new(
    LeashedEntityModuleDispatchStatus.InvalidEnvelope,
    null,
    null,
    null,
    Array.Empty<SessionEpoch>(),
    "The module 13 envelope is not an active, current-world request.");

  public static LeashedEntityModuleDispatchResult SessionUnavailable { get; } = new(
    LeashedEntityModuleDispatchStatus.SessionUnavailable,
    null,
    null,
    null,
    Array.Empty<SessionEpoch>(),
    "The authenticated sender is not in the active session set.");

  public static LeashedEntityModuleDispatchResult StaleRevision(long currentRevision) =>
    new(
      LeashedEntityModuleDispatchStatus.StaleRevision,
      null,
      currentRevision,
      null,
      Array.Empty<SessionEpoch>(),
      "The module 13 request expected a different registry revision; " +
      $"current revision is {currentRevision}.");

  public static LeashedEntityModuleDispatchResult Rejected(
    LeashedEntityModuleDispatchStatus status,
    string detail) =>
    new(status, null, null, null, Array.Empty<SessionEpoch>(), detail);
}
