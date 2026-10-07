using System;
using Terraria.WorldInteraction.Wiring;

namespace Terraria.Teleportation;

// Bridges Wiring intents to the Teleportation boundary without owning movement.
public sealed class WiringTeleportTransitionAdapter : IWiringTraversalCommitPort
{
  private readonly IWiringTraversalCommitPort _innerPort;
  private readonly TeleportTransitionCommitSystem _teleportSystem;
  private readonly IWiringTeleportSnapshotProvider _snapshotProvider;
  private readonly int _cooldownTicks;

  public WiringTeleportTransitionAdapter(
    IWiringTraversalCommitPort innerPort,
    TeleportTransitionCommitSystem teleportSystem,
    IWiringTeleportSnapshotProvider snapshotProvider,
    int cooldownTicks)
  {
    ArgumentNullException.ThrowIfNull(innerPort);
    ArgumentNullException.ThrowIfNull(teleportSystem);
    ArgumentNullException.ThrowIfNull(snapshotProvider);
    if (cooldownTicks < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(cooldownTicks));
    }

    _innerPort = innerPort;
    _teleportSystem = teleportSystem;
    _snapshotProvider = snapshotProvider;
    _cooldownTicks = cooldownTicks;
  }

  public WiringTraversalCommitResult CommitPump(
    in PumpTransferCommand command)
  {
    return _innerPort.CommitPump(in command);
  }

  public WiringTraversalCommitResult CommitTeleport(
    in WiringTeleportCommand command)
  {
    if (!command.IsValid)
    {
      return WiringTraversalCommitResult.Rejected("invalid-teleport-command");
    }

    if (!_snapshotProvider.TryGetSnapshot(in command, out TeleportTransitionSnapshot snapshot))
    {
      return WiringTraversalCommitResult.Rejected("teleport-snapshot-unavailable");
    }

    TeleportTransitionRequest request = new(
      CommandId: command.CommandId,
      SourceEndpoint: snapshot.SourceEndpoint.Endpoint,
      DestinationEndpoint: snapshot.DestinationEndpoint.Endpoint,
      Subject: command.Subject,
      SubjectKind: snapshot.Subject.Kind,
      SourcePosition: command.Source,
      DestinationPosition: command.Destination,
      Source: TeleportSource.Mechanism,
      CooldownTicks: _cooldownTicks,
      ExpectedSourceRevision: snapshot.SourceEndpoint.Revision,
      ExpectedDestinationRevision: snapshot.DestinationEndpoint.Revision,
      ExpectedSubjectRevision: snapshot.Subject.Revision,
      BlockPlayerTeleportation: command.BlockPlayerTeleportation);

    TeleportCommitResult result = _teleportSystem.Commit(
      in request,
      in snapshot);
    return result.Status switch
    {
      TeleportCommitStatus.Accepted =>
        WiringTraversalCommitResult.Accepted(changed: true),
      TeleportCommitStatus.Duplicate =>
        WiringTraversalCommitResult.Duplicate,
      TeleportCommitStatus.Unknown =>
        WiringTraversalCommitResult.Unknown(
          result.FailureReason ?? result.Reason.ToString()),
      _ => WiringTraversalCommitResult.Rejected(
        result.FailureReason ?? result.Reason.ToString()),
    };
  }
}
