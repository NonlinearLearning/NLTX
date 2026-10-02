using System;

namespace Terraria.DeathPenaltyAndRevenge;

// Owns marker identity, lifecycle commits, and the explicit game clock.
public sealed class RevengeRegistrySystem
{
  private readonly RevengeMarkerRegistryComponent _registry;
  private readonly RevengeMarkerIdAllocator _idAllocator;
  private readonly RevengeClockState _clock;

  public RevengeRegistrySystem(
    RevengeMarkerRegistryComponent registry,
    RevengeMarkerIdAllocator idAllocator,
    RevengeClockState clock)
  {
    _registry = registry ?? throw new ArgumentNullException(nameof(registry));
    _idAllocator = idAllocator ?? throw new ArgumentNullException(nameof(idAllocator));
    _clock = clock ?? throw new ArgumentNullException(nameof(clock));
  }

  public int CurrentGameTime => _clock.CurrentTick;

  public uint Revision => _registry.Revision;

  public RevengeMarkerSnapshot? Capture(
    RevengeTargetSnapshotComponent target,
    int expiresAtGameTime)
  {
    ArgumentNullException.ThrowIfNull(target);
    RevengeMarkerId markerId = _idAllocator.Allocate();
    RevengeMarkerSnapshot candidate = new(
      markerId,
      target,
      expiresAtGameTime,
      forceExpire: false,
      respawnAttemptLocked: false);
    return _registry.TryRegister(candidate, out RevengeMarkerSnapshot committed)
      ? committed
      : null;
  }

  public RevengeRegistryCommitResult CommitDecision(
    RevengeMarkerId markerId,
    uint expectedMarkerRevision,
    RevengeRespawnDecision decision)
  {
    if (!_registry.TryGet(markerId, out RevengeMarkerSnapshot? current) ||
      current is null)
    {
      return RevengeRegistryCommitResult.MissingMarker(_registry.Revision);
    }

    if (current.Revision != expectedMarkerRevision)
    {
      return RevengeRegistryCommitResult.Stale(_registry.Revision, current);
    }

    if (decision.RemovesMarker)
    {
      return _registry.TryUnregister(
          markerId,
          expectedMarkerRevision,
          out RevengeMarkerSnapshot? removed)
        ? RevengeRegistryCommitResult.AcceptedMarker(
          _registry.Revision,
          removed ?? current)
        : RevengeRegistryCommitResult.Stale(_registry.Revision, current);
    }

    if (decision.Kind == RevengeRespawnDecisionKind.NoAction)
    {
      return RevengeRegistryCommitResult.AcceptedMarker(
        _registry.Revision,
        current);
    }

    bool forceExpire = current.ForceExpire ||
      decision.Kind == RevengeRespawnDecisionKind.ForceExpire;
    bool respawnLocked = decision.Kind switch
    {
      RevengeRespawnDecisionKind.UnlockAttempt => false,
      RevengeRespawnDecisionKind.ForceExpire => true,
      RevengeRespawnDecisionKind.RequestSpawn => true,
      _ => current.RespawnAttemptLocked,
    };
    if (forceExpire == current.ForceExpire &&
      respawnLocked == current.RespawnAttemptLocked)
    {
      return RevengeRegistryCommitResult.AcceptedMarker(
        _registry.Revision,
        current);
    }

    RevengeMarkerSnapshot replacement = current.WithLifecycle(
      forceExpire,
      respawnLocked);
    return _registry.TryReplace(
        replacement,
        expectedMarkerRevision,
        out RevengeMarkerSnapshot committed)
      ? RevengeRegistryCommitResult.AcceptedMarker(
        _registry.Revision,
        committed)
      : RevengeRegistryCommitResult.Stale(_registry.Revision, current);
  }

  public RevengeRegistryCommitResult Remove(
    RevengeMarkerId markerId,
    uint expectedMarkerRevision)
  {
    if (!_registry.TryGet(markerId, out RevengeMarkerSnapshot? current) ||
      current is null)
    {
      return RevengeRegistryCommitResult.MissingMarker(_registry.Revision);
    }

    return _registry.TryUnregister(
        markerId,
        expectedMarkerRevision,
        out RevengeMarkerSnapshot? removed)
      ? RevengeRegistryCommitResult.AcceptedMarker(
        _registry.Revision,
        removed ?? current)
      : RevengeRegistryCommitResult.Stale(_registry.Revision, current);
  }

  public int AdvanceClock()
  {
    return _clock.Advance();
  }

  public void Reset()
  {
    _registry.Clear();
    _clock.Reset();
  }
}
