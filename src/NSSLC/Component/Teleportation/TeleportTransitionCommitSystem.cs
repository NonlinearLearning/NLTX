using System;
using System.Collections.Generic;

namespace Terraria.Teleportation;

// Commits only the teleport intent and cooldown. Movement, section and network
// effects stay behind ITeleportCommitPort until their owning domain is verified.
public sealed class TeleportTransitionCommitSystem
{
  private readonly HashSet<Guid> _acceptedCommandIds = [];
  private readonly ITeleportCommitPort _commitPort;
  private readonly TeleportCooldownSystem _cooldown;

  public TeleportTransitionCommitSystem(
    ITeleportCommitPort commitPort,
    TeleportCooldownSystem cooldown)
  {
    ArgumentNullException.ThrowIfNull(commitPort);
    ArgumentNullException.ThrowIfNull(cooldown);
    _commitPort = commitPort;
    _cooldown = cooldown;
  }

  public TeleportCooldownSnapshot CooldownSnapshot => _cooldown.Snapshot();

  public TeleportCommitResult Commit(
    in TeleportTransitionRequest request,
    in TeleportTransitionSnapshot currentSnapshot)
  {
    if (request.CommandId == Guid.Empty)
    {
      return TeleportCommitResult.Rejected(
        request.CommandId,
        TeleportCommitReason.EmptyCommand);
    }

    if (_acceptedCommandIds.Contains(request.CommandId))
    {
      return TeleportCommitResult.Duplicate(request.CommandId);
    }

    if (currentSnapshot.Cooldown != _cooldown.Snapshot())
    {
      return TeleportCommitResult.Rejected(
        request.CommandId,
        TeleportCommitReason.StaleRevision);
    }

    TeleportEligibility eligibility = TeleportEligibilityQuery.Evaluate(
      in request,
      in currentSnapshot);
    if (!eligibility.IsEligible)
    {
      return TeleportCommitResult.Rejected(
        request.CommandId,
        eligibility.Reason);
    }

    TeleportCommitPortResult portResult = _commitPort.Commit(
      in request,
      in currentSnapshot);
    if (portResult.Status == TeleportCommitPortStatus.Rejected)
    {
      return TeleportCommitResult.Rejected(
        request.CommandId,
        TeleportCommitReason.CommitPortRejected,
        portResult.FailureReason);
    }

    if (portResult.Status == TeleportCommitPortStatus.Unknown)
    {
      return TeleportCommitResult.Unknown(
        request.CommandId,
        portResult.FailureReason);
    }

    if (!_cooldown.Arm(
      request.CooldownTicks,
      request.Source,
      request.StartedAtTick))
    {
      return TeleportCommitResult.Rejected(
        request.CommandId,
        TeleportCommitReason.CooldownActive);
    }

    _acceptedCommandIds.Add(request.CommandId);
    return TeleportCommitResult.Accepted(request.CommandId);
  }

  public void ResetForLifecycle()
  {
    _acceptedCommandIds.Clear();
    _cooldown.Reset();
  }
}
